using OpenForestUI.Common.Data.Config;
using OpenForestUI.Common.Data.RIOT;
using OpenForestUI.Farsight;
using OpenForestUI.Http;
using OpenForestUI.Ingame.Data.Hub;
using OpenForestUI.Ingame.Data.Provider;
using OpenForestUI.Ingame.Data.RIOT;
using OpenForestUI.Ingame.Events;
using OpenForestUI.Ingame.State;
using OpenForestUI.MVVM.ViewModel;
using OpenForestUI.OperatingSystem;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace OpenForestUI.Common.Controllers
{
    class IngameController : ITickable
    {
        public static IngameDataProvider LoLDataProvider = new();
        public static CurrentSettings CurrentSettings = new();
#nullable enable
        public static Process? LeagueProcess;
#nullable disable
        public static bool IsPaused;

        // What kind of game we're attached to. Decided once per game, when GameFound flips true.
        // Spectator and Replay are supported; Live aborts because /playerlist gives only the
        // local player without a memory reader.
        public static PlaybackMode CurrentPlaybackMode = PlaybackMode.Spectator;

        // Power Play buff durations — patch 26.x (2026-05) reference values.
        // Hand of Baron lasts 3:00 and Aspect of the Dragon lasts 2:30.
        // Centralised so a future Riot tuning is a one-line change rather than a grep.
        public const double BARON_BUFF_DURATION = 180.0;
        public const double ELDER_BUFF_DURATION = 150.0;

        public State gameState;
        public GameMetaData gameData;

        // Derives all objective spawn countdowns from raw events each tick (reset per game).
        private readonly ObjectiveSpawnClock spawnClock = new();

        private static string TargetProcessName = "League of Legends";
        private static ProcessEventWatcher ProcessEventWatcher { get; } = new ProcessEventWatcher();
        private static bool GameFound = false;

        public static EventHandler BaronEnd, DragonEnd;
        public static EventHandler<ObjectiveTakenArgs> DragonTaken, BaronTaken, HeraldTaken;

        public IngameController()
        {
            this.gameState = new State(this);
            this.gameData = new GameMetaData();

            AppStateController.GameStop += OnGameStop;

            BaronTaken += OnBaronTaken;
            BaronEnd += OnBaronEnd;
            DragonTaken += OnDragonTaken;
            DragonEnd += OnDragonEnd;
            HeraldTaken += OnHeraldTaken;
            
        }

        // Single funnel for EVERY overlay broadcast from the live game. While the Mock preview feed is
        // ON this stays silent, giving Mock priority so the two feeds never interleave (that interleave
        // is the "UI breaks when Mock is toggled mid-game" bug). The instant Mock is OFF, the next tick's
        // heartbeat flows again — a seamless, gap-free handoff in both directions (Mock <-> live game).
        private static void Broadcast(OpenForestUI.Common.Events.LeagueEvent ev)
        {
            if (MockController.IsEnabled)
                return;
            EmbedIOServer.SocketServer?.SendEventToAllAsync(ev);
        }

        public async void DoTick()
        {
            //Check if ingame and get game meta data
            var newGameData = await LoLDataProvider.GetGameData();

            //Discard late rejected responses by API
            if (!BroadcastController.CurrentLeagueState.HasFlag(LeagueState.InProgress) || newGameData == null || LeagueProcess == null)
            {
                Log.Verbose("game reponse invalid and discarded");
                return;
            }
                

            //Wait until the game has been found
            if (!GameFound)
            {
                GameFound = true;
                CurrentPlaybackMode = await LoLDataProvider.DetectPlaybackMode();
                if (CurrentPlaybackMode == PlaybackMode.Live)
                {
                    // Tournament UI: don't abort on Live. /playerlist only exposes
                    // the local player's data so the overlay degrades to whatever
                    // info is visible from that one perspective, but we'd rather
                    // show what we can than refuse to run. The LoadGame path below
                    // proceeds regardless of mode.
                    Log.Warn("Live mode detected — overlay features will be limited without memory reader");
                }
                LoadGame(newGameData);
            }

            //Replay-mode time override: /replay/playback is the canonical clock.
            //gameTime from /gamestats also tracks playback during a .rofl, but
            //playback.time is more precise around seeks and exposes the paused flag.
            if (CurrentPlaybackMode == PlaybackMode.Replay)
            {
                var playback = await ReplayDataProvider.GetPlaybackInfo();
                if (playback != null)
                {
                    newGameData.gameTime = playback.time;
                }
            }

            #region GameTime
            //Check if game is paused/unpaused
            double timeDiff = newGameData.gameTime - gameData.gameTime;
            Log.Verbose($"Tick: {newGameData.gameTime}. Update duration: {timeDiff}");

            if(timeDiff == 0 || newGameData.gameTime == 0)
            {
                if(!gameState.stateData.gamePaused)
                {
                    SetGamePauseState(true);
                }
                return;
            }
            if(gameState.stateData.gamePaused)
            {
                SetGamePauseState(false);
            }
            var backDragon = gameState.stateData.backDragon;
            var backBaron = gameState.stateData.backBaron;


            // Dragon/Baron/Herald spawn countdowns are derived from raw kill events every
            // tick by ObjectiveSpawnClock (called after UpdateEvents below) — no seeded
            // decrementing state, so rewinds and mid-game joins need no special-casing.
            // Capture the time direction here for its zero-crossing (pop-up) logic.
            bool timeAdvanced = newGameData.gameTime > gameData.gameTime;

            // Replay seek (backward, or a large forward jump) breaks the OCR gates' continuity:
            // the sidecar's gold gate is monotonic (stays stuck at the pre-seek max) and the count
            // gates hold stale values — so the overlay shows pre-seek numbers, the event-count
            // fallback's whole-game totals (grub 8 / tower 14), or the coarse API-floor CS. Reset
            // the OCR sidecar + held values so every metric re-locks at the new timeline. Live play
            // advances by ~one tick interval, so any backward move or a >5s jump is a seek.
            if ((CurrentSettings.UseOcrGold || CurrentSettings.UseOcrCs || CurrentSettings.UseOcrObjectives)
                && (newGameData.gameTime < gameData.gameTime || timeDiff > 5))
            {
                OcrGoldController.SignalSeekReset();
            }

            // Gold-graph trim must run on EVERY backward seek — not only when tracked
            // objective events exist (the gate below). A seek before the first dragon/baron
            // would otherwise keep "future" samples on the graph and stall RecordGoldDiff's
            // monotonic interval check until playback re-passed the stale last key.
            if (newGameData.gameTime < gameData.gameTime)
            {
                gameState.TrimGoldDiffHistory(newGameData.gameTime);
            }

            //Check for time scrolled back
            if (newGameData.gameTime < gameData.gameTime && gameState.pastIngameEvents.Count != 0)
            {
                Log.Info("Scrolled back in timeline, reverting state");
                Log.Info(newGameData.gameTime);

                // Phase 4: tell State.UpdateTeams to rebaseline player diff trackers
                // on this tick instead of computing a (now-meaningless) diff against
                // late-game inventory/levels.
                gameState.RewindOccurredThisTick = true;
                // Filter to events BEFORE the seek TARGET (newGameData.gameTime), not the old
                // seek-from position. gameData is not reassigned to newGameData until later in
                // DoTick, so here gameData.gameTime is still the old (larger) time — using it kept
                // every event between the target and the old position, so the drake/baron trim
                // below counted them and RemoveRange became a no-op, leaving dragonsTaken stuck at
                // the pre-seek over-count. newGameData.gameTime is "now" after the seek — the same
                // value the spawn-timer math further down already uses.
                gameState.pastIngameEvents = gameState.pastIngameEvents.Where((e) => e.EventTime < newGameData.gameTime).ToList();
                // (The old per-player goldHistory/csHistory rollback lived here: both
                // histories were dead — their only writers were the memory reader / the
                // LiveEvents API Riot removed in patch 14.1 — and the csHistory.Last()
                // fallback reset every player's CS to 0 on each backward seek. The
                // gold-graph history trim now runs above, outside this event-gated branch.)

                Log.Info("Rolling back Objectives");
                //Check if Elder died in roll back period
                if (backDragon.DurationRemaining - timeDiff > ELDER_BUFF_DURATION)
                {
                    backDragon.DurationRemaining = 0;
                    gameState.blueTeam.hasElder = false;
                    gameState.redTeam.hasElder = false;
                    OnDragonEnd(null, EventArgs.Empty);
                }

                //Check if Baron died in roll back period
                if (backBaron.DurationRemaining - timeDiff > BARON_BUFF_DURATION)
                {
                    backBaron.DurationRemaining = 0;
                    gameState.blueTeam.hasBaron = false;
                    gameState.redTeam.hasBaron = false;
                    gameState.GetAllPlayers().ForEach(p => p.diedDuringBaron = false);
                    OnBaronEnd(null, EventArgs.Empty);
                }

                var drakesTaken = gameState.pastIngameEvents.Where(e => e.EventName == "ObjectiveKilled" && ((ObjectiveKilled)e).ObjectiveName == "Dragon");
                int blueDrakesTaken = drakesTaken.Count(e => ((ObjectiveKilled)e).TeamName.Equals("Order", StringComparison.OrdinalIgnoreCase));
                int redDrakesTaken = drakesTaken.Count(e => ((ObjectiveKilled)e).TeamName.Equals("Chaos", StringComparison.OrdinalIgnoreCase));

                //Remove all drakes taken after this point in time
                gameState.blueTeam.dragonsTaken.RemoveRange(blueDrakesTaken, gameState.blueTeam.dragonsTaken.Count - blueDrakesTaken);
                gameState.redTeam.dragonsTaken.RemoveRange(redDrakesTaken, gameState.redTeam.dragonsTaken.Count - redDrakesTaken);

                // (The old rewind timer recompute lived here. It also had an inverted
                // elapsed-time sign — 420 - (kill - now) = 420 + elapsed — which the
                // ObjectiveSpawnClock derivation removes by construction.)
            }
            #endregion

            #region Objectives
            //Update remaining time for objectives incase either team has them

            if (backDragon.DurationRemaining > 0)
            {
                gameState.SetObjectiveData(backDragon, gameState.stateData.dragon, backDragon.DurationRemaining - timeDiff);
                Log.Verbose($"Elder Time left: {backDragon.DurationRemaining:C0}");
                if (backDragon.DurationRemaining <= 0)
                {
                    OnDragonEnd(null, EventArgs.Empty);
                    gameState.blueTeam.hasElder = gameState.redTeam.hasElder = false;
                    gameState.GetAllPlayers().ForEach(p => p.diedDuringElder = false);
                }
            }


            if (backBaron.DurationRemaining > 0)
            {
                gameState.SetObjectiveData(backBaron, gameState.stateData.baron, backBaron.DurationRemaining - timeDiff);
                Log.Verbose($"Baron Time left: {backBaron.DurationRemaining:C0}");
                if (backBaron.DurationRemaining <= 0)
                {
                    backBaron.DurationRemaining = 0;
                    OnBaronEnd(null, EventArgs.Empty);
                    gameState.blueTeam.hasBaron = gameState.redTeam.hasBaron = false;
                    gameState.GetAllPlayers().ForEach(p => p.diedDuringBaron = false);
                }
            }

            //Update inhibitors      
            gameState.stateData.inhibitors.Inhibitors.ForEach(inhib => {
                inhib.timeLeft = Math.Max(0, inhib.timeLeft - timeDiff);
                //Make sure to reset inhibs incase of scrollback
                if (inhib.timeLeft > 300)
                    inhib.timeLeft = 0;
            });

            #endregion

            //Update Meta Data
            gameData = newGameData;
            gameState.stateData.gameTime = gameData.gameTime;

            //Update State — Phase 3: State methods consume Live Client Data API only.
            //If memory reader is opt-in enabled (FarsightController.ShouldRun), we still
            //run CreateSnapshot so users that flip the flag can see Farsight load,
            //but its output is no longer plumbed into State.
            try
            {
                if (FarsightController.ShouldRun)
                {
                    BroadcastController.Instance.MemoryController.CreateSnapshot(gameState.stateData.gameTime);
                }

                var events = LoLDataProvider.GetEventData().Result;
                var players = LoLDataProvider.GetPlayerData().Result;

                gameState.UpdateTurrets(events);
                gameState.UpdateEvents(events);
                gameState.UpdateTeams(players);

                // Objective spawn clock: derive the dragon/baron/herald countdowns from the
                // raw kill events + patch timings. remaining == 0 means "spawned (presumed
                // alive)", which the frontend timer already renders; zero-crossings fire the
                // spawn pop-ups (Elder named explicitly when a team has soul point).
                var timings = ConfigController.Component.Ingame.Timings ?? ObjectiveTimingsConfig.Defaults;
                var clockTick = spawnClock.Recompute(newGameData.gameTime, timeAdvanced, gameState.pastIngameEvents,
                    gameState.blueTeam?.GetDragonsTaken() ?? 0, gameState.redTeam?.GetDragonsTaken() ?? 0, timings);
                gameState.stateData.dragon.SpawnTimer = clockTick.DragonRemaining;
                gameState.stateData.baron.SpawnTimer = clockTick.BaronRemaining;
                if (clockTick.BaronSpawned)
                    OnObjectiveSpawn("Baron");
                if (clockTick.HeraldSpawned)
                    OnObjectiveSpawn("Herald");
                if (clockTick.DragonSpawned)
                    OnObjectiveSpawn(clockTick.NextDragonIsElder ? "Elder" : "Dragon");
                // Stage 3: overlay OCR'd HUD values (top-bar reader sidecar) onto the teams before
                // the scoreboard is built. Consumers share the one sidecar:
                //   * gold       — when UseOcrGold (exact team gold; falls back to the estimate).
                //   * objectives — grub/baron/dragon/tower: objective-monster kills are NOT in the
                //                  spectator OR replay /eventdata (verified live + .rofl), so the
                //                  top-bar HUD OCR is the only count source in BOTH modes.
                //   * cs         — exact per-player CS (vs the API's multiples of 10).
                bool replay = CurrentPlaybackMode == PlaybackMode.Replay;
                if (CurrentSettings.UseOcrGold || CurrentSettings.UseOcrCs || CurrentSettings.UseOcrObjectives || replay)
                {
                    OcrGoldController.EnsureStarted();
                    if (CurrentSettings.UseOcrGold)
                        OcrGoldController.ApplyTo(gameState.blueTeam, gameState.redTeam);
                    if (CurrentSettings.UseOcrObjectives)
                        OcrGoldController.ApplyObjectives(gameState.blueTeam, gameState.redTeam);
                    // Per-player CS from the detail scoreboard (exact, vs API's multiples of 10).
                    // After UpdateTeams set creepScore from /playerlist; the floor-match gate inside
                    // ApplyCs only overrides when the OCR rounds to that API value. Runs before the
                    // gold sample + UpdateScoreboard so the finer CS feeds both.
                    if (CurrentSettings.UseOcrCs)
                        OcrGoldController.ApplyCs(gameState.blueTeam, gameState.redTeam);
                }
                else if (OcrGoldController.IsRunning)
                {
                    OcrGoldController.Stop();
                    OcrGoldController.ClearInjection(gameState.blueTeam, gameState.redTeam);
                }
                // Gold graph: one (gameTime, blue−red) sample per tick, AFTER OCR injection so
                // each point is OCR-exact when locked and the item+score estimate otherwise
                // (Team.GetGold priority). RecordGoldDiff enforces the sample interval and
                // ignores non-advancing time; backward seeks were trimmed above. Requires BOTH
                // rosters: in Live (non-spectator) mode /playerlist only exposes the local
                // player, so one team's sum is 0 and the diff would be garbage — skip.
                if (!gameState.stateData.gamePaused
                    && gameState.blueTeam?.players?.Count > 0 && gameState.redTeam?.players?.Count > 0)
                {
                    double sampleTime = gameState.stateData.gameTime;
                    gameState.RecordGoldDiff(sampleTime,
                        gameState.blueTeam.GetGold(sampleTime) - gameState.redTeam.GetGold(sampleTime));
                }
                gameState.UpdateScoreboard();
                UpdateInfoPage();
            }
            catch (Exception canceled)
            {
                Log.Warn($"Could not update State:\n{canceled.Source} -> {canceled.Message}\n Stacktrace:\n{canceled.StackTrace}");
            }

            //Update frontend
            Broadcast(new HeartbeatEvent(gameState.stateData));
        }

        private void UpdateInfoPage()
        {
            if (!CurrentSettings.SideGraph)
            {
                return;
            }

            // A titled page with ZERO tabs must not be sent: the frontend indexes
            // Players[0] and the resulting TypeError aborts the whole heartbeat handler
            // (freezing the gold graph and soul-point checks). Empty happens for any
            // tab during the first 5s of a game.
            // (EXP tab removed: per-player XP has no Vanguard-safe source.)
            if (CurrentSettings.PlayerGold)
            {
                SetInfoPageIfAny("Player Gold", PlayerTab.GetGoldTabs());
                return;
            }
            if (CurrentSettings.CSPerMin)
            {
                SetInfoPageIfAny("CS/min", PlayerTab.GetCSPerMinTabs());
                return;
            }
        }

        private void SetInfoPageIfAny(string title, List<PlayerTab> tabs)
        {
            gameState.stateData.infoPage = (tabs != null && tabs.Count > 0)
                ? new InfoSidePage(title, PlayerOrder.MaxToMin, tabs)
                : null;
        }

        private void LoadGame(GameMetaData gameData)
        {
            this.gameData = gameData;
            this.gameData.gameTime = 0;
            spawnClock.Reset();   // new game: forget the previous game's countdown state
            Log.Info("Game Loaded");
            Broadcast(new GameStart());
            AppStateController.GameLoad?.Invoke(this, EventArgs.Empty);
        }

        private void SetGamePauseState(bool PauseState)
        {
            gameState.stateData.gamePaused = PauseState;
            IsPaused = PauseState;
            Log.Info(PauseState? "Game Paused" : "Game Resumed");
            Broadcast(PauseState? new GameUnpause(gameData.gameTime): new GamePause(gameData.gameTime));
        }

        public void EnterIngame(object sender, Process p)
        {
            var Instance = BroadcastController.Instance;
            if(Instance.ToTick.Contains(this))
            {
                return;
            }
            Instance.ToTick.Add(this);
            FlagsHelper.Set( ref BroadcastController.CurrentLeagueState, LeagueState.InProgress);
            InitGameState();
        }

        public void InitGameState()
        {
            Log.Info("Init Game State");
            this.gameState.ResetState();
            GameFound = false;
        }

        #region Process
        //Following adapted from https://github.com/Johannes-Schneider/GoldDiff/blob/master/GoldDiff/App.xaml.cs
        public void StartWaitingForTargetProcess()
        {
            ProcessEventWatcher.ProcessStarted += ProcessEventWatcher_OnProcessStarted;
            ProcessEventWatcher.ProcessStopped += ProcessEventWatcher_OnProcessStopped;

            var processes = Process.GetProcessesByName(TargetProcessName);
            if (processes.Length > 0)
            {
                LeagueProcess = processes[0];
                TargetProcessStarted(LeagueProcess);
            }
        }

        private void ProcessEventWatcher_OnProcessStarted(object sender, ProcessEventArguments e)
        {
            if (LeagueProcess != null)
            {
                return;
            }

            try
            {
                var newProcess = Process.GetProcessById(e.ProcessId);
                if (newProcess != null && !newProcess.ProcessName.Equals(TargetProcessName))
                {
                    return;
                }

                LeagueProcess = newProcess;
                TargetProcessStarted(LeagueProcess);
            }
            catch 
            {
                // ignored
            }
        }

        private void ProcessEventWatcher_OnProcessStopped(object sender, ProcessEventArguments e)
        {
            if (LeagueProcess == null || LeagueProcess.Id != e.ProcessId)
            {
                return;
            }

            LeagueProcess = null;
            TargetProcessStopped();
        }

        private void TargetProcessStarted(Process p)
        {
            Log.Info($"Target process ({TargetProcessName}) detected.");
            AppStateController.GameStart?.Invoke(this, p);
        }

        private void TargetProcessStopped()
        {
            AppStateController.GameStop?.Invoke(this, EventArgs.Empty);
        }

        public void OnExit()
        {
            ProcessEventWatcher.Dispose();
        }

        #endregion

        #region Events
        public void OnDragonTaken(object sender, ObjectiveTakenArgs e)
        {
            e.Team.dragonsTaken.Add(e.Type);

            // (SpawnTimer is no longer seeded here — ObjectiveSpawnClock derives it from
            // the DragonKill event itself on the same tick, elder rule included.)

            if (e.Type.Equals("Elder", StringComparison.OrdinalIgnoreCase))
            {
                gameState.stateData.backDragon.TakeGameTime = gameData.gameTime;
                gameState.stateData.backDragon.BlueStartGold = gameState.blueTeam.GetGold(gameData.gameTime);
                gameState.stateData.backDragon.RedStartGold = gameState.redTeam.GetGold(gameData.gameTime);
                gameState.SetObjectiveData(gameState.stateData.backDragon, gameState.stateData.dragon, ELDER_BUFF_DURATION);
                e.Team.hasElder = true;
            }

            gameState.pastIngameEvents.Add(new ObjectiveKilled("Dragon", e.Team.teamName, gameData.gameTime));
            OnObjectiveKilled(e.Type, e.Team.teamName);
        }

        public void OnBaronTaken(object sender, ObjectiveTakenArgs e)
        {
            gameState.stateData.backBaron.TakeGameTime = gameData.gameTime;
            gameState.stateData.backBaron.BlueStartGold = gameState.blueTeam.GetGold(gameData.gameTime);
            gameState.stateData.backBaron.RedStartGold = gameState.redTeam.GetGold(gameData.gameTime);
            gameState.SetObjectiveData(gameState.stateData.backBaron, gameState.stateData.baron, BARON_BUFF_DURATION);
            // (SpawnTimer derived by ObjectiveSpawnClock from the BaronKill event; the old
            // hardcoded 420 here was a stale constant — patch 26.x respawn is 6:00.)
            e.Team.hasBaron = true;

            gameState.pastIngameEvents.Add(new ObjectiveKilled("Baron", e.Team.teamName, gameData.gameTime));
            OnObjectiveKilled("Baron", e.Team.teamName);
        }

        public void OnHeraldTaken(object sender, ObjectiveTakenArgs e)
        {
            gameState.pastIngameEvents.Add(new ObjectiveKilled("Herald", e.Team.teamName, gameData.gameTime));
            OnObjectiveKilled("Herald", e.Team.teamName);
        }

        public void OnBaronEnd(object sender, EventArgs e)
        {
            Log.Info("Baron ended. Resetting baron status for all players");
            gameState.GetBothTeams().ForEach(t => t.players.ForEach(p => p.diedDuringBaron = false));
        }

        public void OnDragonEnd(object sender, EventArgs e)
        {
            Log.Info("Elder dragon ended. Resetting baron status for all players");
            gameState.GetBothTeams().ForEach(t => t.players.ForEach(p => p.diedDuringElder = false));
        }

        private void OnGameStop(object sender, EventArgs e)
        {
            FlagsHelper.Unset(ref BroadcastController.CurrentLeagueState, LeagueState.InProgress);
            //GameInfoPage.ClearPlayers();
            OcrGoldController.Stop();   // stop the OCR sidecar with the game (re-warms next game)
            BroadcastController.Instance.ToTick.Remove(this);
            gameState.stateData.scoreboard.GameTime = -1;
            gameState.stateData.gameTime = -1;
            Broadcast(new HeartbeatEvent(gameState.stateData));
            Broadcast(new GameEnd());
            Log.Info("Game ended");
        }

        public void OnLevelUp(LevelUpEventArgs e)
        {
            if (!CurrentSettings.LevelUp)
                return;
            Log.Info("Player " + e.playerId + " lvl up");
            Broadcast(new PlayerLevelUp(e.playerId, e.level));
        }

        public void OnItemCompleted(ItemCompletedEventArgs e)
        {
            if (!CurrentSettings.LevelUp)
                return;
            Log.Info("Player " + e.playerId + " finished Item " + e.itemData.itemID);
            Log.Info(JsonConvert.SerializeObject(new ItemCompleted(e.playerId, e.itemData), Formatting.Indented));
            Broadcast(new ItemCompleted(e.playerId, e.itemData));
        }

        public void OnObjectiveSpawn(string objectiveName)
        {
            Log.Info($"{objectiveName} spawned");
            if (!CurrentSettings.ObjectiveSpawnPopUp)
                return;
            Broadcast(new ObjectiveSpawnSimple(objectiveName));
        }

        public void OnObjectiveKilled(string objectiveName, string teamName)
        {
            Log.Info($"{objectiveName} killed by {teamName}");
            if (!CurrentSettings.ObjectiveKillPopUp)
                return;
            Broadcast(new ObjectiveKilledSimple(objectiveName, teamName));
        }

        #endregion
    }

    #region EventArgs
    public class LevelUpEventArgs : EventArgs
    {
        public int playerId;
        public int level;

        public LevelUpEventArgs(int playerId, int level)
        {
            this.playerId = playerId;
            this.level = level;
        }
    }

    public class ItemCompletedEventArgs : EventArgs
    {
        public int playerId;
        public ItemData itemData;

        public ItemCompletedEventArgs(int playerId, ItemData itemData)
        {
            this.playerId = playerId;
            this.itemData = itemData;
        }
    }

    #endregion

    public class CurrentSettings
    {
        public bool Baron => ConfigController.Component.Ingame.Objectives.DoBaronKill;
        public bool Elder => ConfigController.Component.Ingame.Objectives.DoDragonKill;
        public bool Inhibs => ConfigController.Component.Ingame.Objectives.DoInhibitors;
        public bool Items => ConfigController.Component.Ingame.DoItemCompleted;
        public bool LevelUp => ConfigController.Component.Ingame.DoLevelUp;
        public bool TeamNames => ConfigController.Component.Ingame.Teams.DoTeamNames;
        public bool TeamIcons => ConfigController.Component.Ingame.Teams.DoTeamIcons;
        public bool TeamStats => ConfigController.Component.Ingame.Teams.DoTeamScores;

        public bool ObjectiveSpawnPopUp => ConfigController.Component.Ingame.Objectives.DoObjectiveSpawnPopUp;
        public bool ObjectiveKillPopUp => ConfigController.Component.Ingame.Objectives.DoObjectiveKillPopUp;

        public bool CS = false;
        public bool CSPerMin = false;
        public bool PlayerGold = false;
        public bool TeamPlates = false;

        // Gold Graph data gate. Defaults to the overlay config's GoldGraph.Enabled (so the
        // graph works out of the box when the visual is enabled); the UI tile overrides for
        // the session. Data is the OCR-exact team gold when locked, the estimate otherwise.
        private bool? goldGraphOverride = null;
        public bool GoldGraph
        {
            get => goldGraphOverride ?? ConfigController.Ingame?.GoldGraph?.Enabled ?? false;
            set => goldGraphOverride = value;
        }

        // Stage 3: read exact team gold via the OCR sidecar (top-bar reader) instead of the
        // CS/kills estimate. TEST DEFAULT = true (auto-launches the Python sidecar when a game
        // runs). Set false to use the legacy estimate. (A GUI toggle can be added later.)
        public bool UseOcrGold = true;

        // Per-player CS from the detail-scoreboard OCR (exact, vs the API's multiples of 10).
        // ON by default: it needs the observer's detail scoreboard open at default position,
        // but is SAFE when that's not the case — the floor-match gate in OcrGoldController.ApplyCs
        // only overrides a CS when the OCR rounds to the authoritative API value, otherwise the
        // API value (multiples of 10) stands. So worst case it's a no-op, never wrong.
        public bool UseOcrCs = true;

        // Objective counts (grub/baron/dragon/tower) from the top-bar OCR. ON by default: objective-
        // monster kills never reach /eventdata in spectator/replay, so this HUD OCR is the only count
        // source. Per-counter median gate + hold-last-good in OcrGoldController.ApplyObjectives.
        public bool UseOcrObjectives = true;

        public bool SideGraph => CS || CSPerMin || PlayerGold;
    }
}




