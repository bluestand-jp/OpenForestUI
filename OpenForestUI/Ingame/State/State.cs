using OpenForestUI.Common;
using OpenForestUI.Common.Controllers;
using OpenForestUI.Common.Data.Provider;
using OpenForestUI.Farsight;
using OpenForestUI.Farsight.Object;
using OpenForestUI.Ingame.Data.Hub;
using OpenForestUI.Ingame.Data.Hub.Objectives;
using OpenForestUI.Ingame.Data.Provider;
using OpenForestUI.Ingame.Data.RIOT;
using OpenForestUI.Ingame.Events;
using OpenForestUI.MVVM.View;
using OpenForestUI.MVVM.ViewModel;
using OpenForestUI.OperatingSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json;

namespace OpenForestUI.Ingame.State
{
    class State
    {
        private IngameController controller;
        private bool ShowedChampionMemoryError = false;

        public StateData stateData;
        public List<RiotEvent> pastIngameEvents;
        // Set when the first non-empty /eventdata batch is recorded (UpdateEvents runs before
        // UpdateTeams in DoTick, so the teams may still be null at that point). The next tick —
        // once the teams exist — replays that historical batch through ApplyHistoricalBaseline so
        // a game observed mid-progress shows the correct tower / dragon counts instead of zero.
        private bool pendingHistoricalCatchup = false;
        public int lastGoldDifference;
        public double lastGoldUpdate;


        //Track past objectives
        private GameObject lastDragon;
        private GameObject lastHerald;
        private GameObject lastBaron;

        // Phase 3: replacement for snapshot.NextDragonType. Without memory reading
        // we cannot know the upcoming drake type — Riot only exposes that via the
        // in-game Dragon_Indicator_* placeholder. We populate this from the most
        // recent /eventdata DragonKill instead, so the scoreboard can still show
        // *something* (the last killed type) rather than blank.
        private string lastDragonType = "";

        // Phase 4: set by IngameController when a replay rewind is detected.
        // The next UpdateTeams call rebaselines per-player diff state to the
        // freshly-fetched values without firing events; otherwise items that
        // were in late-game slots (e.g. Zhonya's in slot 0) would falsely
        // register as build-up completions when the inventory rewinds back
        // to starter items (Doran's Ring in slot 0). The flag self-clears
        // after one rebaseline pass.
        public bool RewindOccurredThisTick = false;

        public Team blueTeam;
        public Team redTeam;


        //only need outer Turrets atm
        public Dictionary<string, Turret> turrets;

        public State(IngameController controller)
        {
            this.controller = controller;
            pastIngameEvents = new List<RiotEvent>();
            this.stateData = null;
            this.controller = controller;
            this.blueTeam = null;
            this.redTeam = null;
            this.lastGoldDifference = 0;
            this.lastGoldUpdate = 0;
            AppStateController.GameStop += (s, e) => { lastGoldDifference = 0; lastGoldUpdate = 0; };
        }

        public void UpdateTeams(List<Player> PlayerData)
        {
            if (PlayerData == null)
            {
                return;
            }

            List<Player> bluePlayers = PlayerData.Where(p => p.team == "ORDER").ToList();
            List<Player> redPlayers = PlayerData.Where(p => p.team != "ORDER").ToList();

            //Init data on first run
            bool firstRun = false;
            if (blueTeam == null)
            {
                blueTeam = new Team(0, bluePlayers);
                stateData.scoreboard.BlueTeam = new(TeamConfigViewModel.BlueTeam.NameTag, false);
                firstRun = true;
            }

            if (redTeam == null)
            {
                redTeam = new Team(1, redPlayers);
                stateData.scoreboard.RedTeam = new(TeamConfigViewModel.RedTeam.NameTag, true);
                firstRun = true;
            }

            if (firstRun)
            {
                Log.Info("Init Team data");
                blueTeam.UpdateIDs();
                redTeam.UpdateIDs();

                IngameTeamsView.InitPlayers(blueTeam);
                IngameTeamsView.InitPlayers(redTeam);

                // (Dragon SpawnTimer seed removed — ObjectiveSpawnClock derives all
                // countdowns from events + patch timings every tick.)

                // Seed per-player diff baselines from the very first /playerlist payload
                // so we don't fire spurious LevelUp / ItemCompleted events for the
                // starter inventory and current level on the first real diff tick.
                GetAllPlayers().ForEach(p =>
                {
                    p.LastLevel = p.level;
                    p.LastItemBySlot = SnapshotItemSlots(p.items);
                    p.LastCountBySlot = SnapshotItemCounts(p.items);
                });

                return;
            }

            // Phase 4: replay rewind rebaseline — IngameController.DoTick set this
            // flag in the time-scrolled-back branch. Treat the current tick like
            // a fresh init for the diff trackers, suppress event firing, then clear.
            if (RewindOccurredThisTick)
            {
                Log.Info("Rewind detected — rebaselining per-player diff state");
                GetAllPlayers().ForEach(p =>
                {
                    Player newP = PlayerData.FirstOrDefault(p2 => p2.summonerName.Equals(p.summonerName, StringComparison.Ordinal));
                    if (newP == null) return;
                    p.LastLevel = newP.level;
                    p.LastItemBySlot = SnapshotItemSlots(newP.items);
                    p.LastCountBySlot = SnapshotItemCounts(newP.items);
                    p.UpdateInfo(newP);
                });
                RewindOccurredThisTick = false;
                return;
            }

            //Update players
            GetAllPlayers().ForEach(p =>
            {
                //Get new player data
                Player newP = PlayerData.Where(p2 => p2.summonerName.Equals(p.summonerName, StringComparison.Ordinal)).FirstOrDefault();
                if (newP == null)
                {
                    return;
                }

                Team playerTeam = (newP.team == "ORDER") ? blueTeam : redTeam;

                //Check if player died with buff
                if (playerTeam.hasBaron && newP.isDead && !p.diedDuringBaron)
                {
                    Log.Verbose($"{p.championName} died with baron buff");
                    p.diedDuringBaron = true;
                }
                if (playerTeam.hasElder && newP.isDead && !p.diedDuringElder)
                {
                    Log.Verbose($"{p.championName} died with elder buff");
                    p.diedDuringElder = true;
                }

                //Level up Events — diff against LastLevel (not p.level), because Phase 4
                //resets LastLevel on rewind so we don't double-fire after seeking backwards.
                //
                //The Viego possession heuristic (skip events when alive and outside base)
                //was removed with the snapshot — we have no Position from /playerlist.
                //Acceptable degradation: Viego's possession buys will trigger ItemCompleted.
                if (p.LastLevel < 6 && newP.level >= 6)
                {
                    controller.OnLevelUp(new LevelUpEventArgs(p.id, 6));
                }
                else if (p.LastLevel < 11 && newP.level >= 11)
                {
                    controller.OnLevelUp(new LevelUpEventArgs(p.id, 11));
                }
                else if (p.LastLevel < 16 && newP.level >= 16)
                {
                    controller.OnLevelUp(new LevelUpEventArgs(p.id, 16));
                }

                //Item Purchase / build-up detection — (itemID, slot) pair-key diff.
                //Three cases trigger an ItemCompleted event:
                //  (a) New slot occupied that was previously empty           — fresh purchase
                //  (b) Same slot, different itemID                           — build-up completion
                //                                                              (e.g. Long Sword 1036 → B.F. Sword 1038
                //                                                              would be MISSED by a plain itemID set diff)
                //  (c) Same (itemID, slot) but count increased                — stackable rebuy
                //                                                              (Tear of the Goddess, Control Ward)
                if (newP.items != null)
                {
                    var prevBySlot = p.LastItemBySlot ?? new Dictionary<int, int>();
                    var prevCountBySlot = p.LastCountBySlot ?? new Dictionary<int, int>();
                    var validIds = CDragonItem.Full?.Select(it => it.ID).ToHashSet() ?? new HashSet<int>();

                    foreach (var item in newP.items)
                    {
                        if (!validIds.Contains(item.itemID))
                        {
                            continue;
                        }

                        bool fired = false;
                        if (!prevBySlot.TryGetValue(item.slot, out int prevItemId))
                        {
                            // (a) slot was empty
                            fired = true;
                        }
                        else if (prevItemId != item.itemID)
                        {
                            // (b) build-up completed in this slot
                            fired = true;
                        }
                        else if (prevCountBySlot.TryGetValue(item.slot, out int prevCount) && item.count > prevCount)
                        {
                            // (c) stackable count went up
                            fired = true;
                        }

                        if (fired)
                        {
                            controller.OnItemCompleted(new ItemCompletedEventArgs(
                                p.id,
                                DataDragon.Instance.GetItemById(item.itemID)));
                        }
                    }
                }

                //Update tracking baselines for the next tick's diff.
                p.LastLevel = newP.level;
                p.LastItemBySlot = SnapshotItemSlots(newP.items);
                p.LastCountBySlot = SnapshotItemCounts(newP.items);

                p.UpdateInfo(newP);
                //Note: p.farsightObject is no longer set — it stays null for the duration
                //of the game. LiveEventsDataProvider has guards (TryGetValue) for this.
            });

            //Update Teams
            GetBothTeams().ForEach(t =>
            {
                //Determine if the team still has baron
                if (t.hasBaron && t.players.All((p) => p.diedDuringBaron))
                {
                    t.hasBaron = false;
                    SetObjectiveData(stateData.backBaron, stateData.baron, 0);
                    controller.OnBaronEnd(null, EventArgs.Empty);
                    Log.Info("All Players died during baron");
                }

                //Determine if the team still has elder
                if (t.hasElder && t.players.All(p => p.diedDuringElder))
                {
                    t.hasElder = false;
                    SetObjectiveData(stateData.backBaron, stateData.dragon, 0);
                    controller.OnDragonEnd(null, EventArgs.Empty);
                    Log.Info("All Players died during elder");
                }

                // Refresh team kill count from per-player /playerlist scores
                // every tick. This is the authoritative source — see
                // UpdateKillsForTeam comment below.
                UpdateKillsForTeam(t);
            });
        }

        // Objective-taken events are fired from /liveclientdata/eventdata. The legacy
        // LiveEvents API (port 34243) was removed by Riot in patch 14.1, so /eventdata
        // is now the sole driver of objective state mutation.
        private static bool ShouldFireObjectivesFromEventData()
        {
            return true;
        }

        private Team GetKillerTeam(string killerName)
        {
            if (string.IsNullOrEmpty(killerName)) return null;
            // KillerName from /eventdata is the bare riotIdGameName (no tag).
            if (blueTeam?.players?.Any(p => p.riotIdGameName != null && p.riotIdGameName.Equals(killerName, StringComparison.OrdinalIgnoreCase)) == true)
            {
                return blueTeam;
            }
            if (redTeam?.players?.Any(p => p.riotIdGameName != null && p.riotIdGameName.Equals(killerName, StringComparison.OrdinalIgnoreCase)) == true)
            {
                return redTeam;
            }
            return null;
        }

        private static string ExtractDragonType(string victimName)
        {
            if (string.IsNullOrEmpty(victimName)) return "";
            // VictimName format: "Dragon_Earth", "Dragon_Elder", etc.
            // Riot encodes terrain/elder types in the suffix after the underscore.
            var parts = victimName.Split('_');
            return parts.Length > 0 ? parts[parts.Length - 1] : "";
        }

        private static Dictionary<int, int> SnapshotItemSlots(IEnumerable<Item> items)
        {
            var result = new Dictionary<int, int>();
            if (items == null) return result;
            foreach (var item in items)
            {
                // Last write wins if the API ever sends duplicate slots — never
                // observed in practice but defends against an /playerlist quirk.
                result[item.slot] = item.itemID;
            }
            return result;
        }

        private static Dictionary<int, int> SnapshotItemCounts(IEnumerable<Item> items)
        {
            var result = new Dictionary<int, int>();
            if (items == null) return result;
            foreach (var item in items)
            {
                result[item.slot] = item.count;
            }
            return result;
        }

        public void UpdateEvents(List<RiotEvent> allEvents)
        {
            if (allEvents == null)
            {
                return;
            }

            //Init Event List incase its empty.
            //
            //When we begin observing a game already MID-PROGRESS, this first non-empty batch
            //carries the entire event history (every TurretKilled / DragonKill since minute 0).
            //The old code discarded it (baseline + return), so counters driven purely by event
            //accumulation — tower count, dragons taken — started at 0 and never caught up.
            //(Kills are immune: UpdateKillsForTeam reads the authoritative /playerlist scores.)
            //
            //We cannot reconstruct here yet: UpdateEvents runs BEFORE UpdateTeams in DoTick, so
            //blueTeam/redTeam may still be null this tick. Keep the history + raise a flag; the
            //reconstruction runs on the next tick once the teams are initialized (below).
            if (pastIngameEvents.Count == 0)
            {
                pastIngameEvents = allEvents;
                pendingHistoricalCatchup = allEvents.Count > 0;
                return;
            }

            //Mid-game catch-up (runs once): rebuild cumulative state from the historical batch
            //recorded above, now that the teams exist.
            if (pendingHistoricalCatchup && blueTeam != null && redTeam != null)
            {
                ApplyHistoricalBaseline(pastIngameEvents);
                pendingHistoricalCatchup = false;
            }

            //Get new events — EventID dedup. Phase 4 will additionally clear
            //processedEventIds entries past the current playback time on rewind.
            List<RiotEvent> newEvents = new List<RiotEvent>();
            allEvents.ForEach(e =>
            {
                if (pastIngameEvents.Where(o => o.EventID == e.EventID).ToList().Count == 0)
                {
                    newEvents.Add(e);
                    if (Log.Instance.Level == Log.LogLevel.Verbose)
                    {
                        Log.Verbose($"New Event: {JsonSerializer.Serialize(e)}");
                    }
                }
            });

            //Save event data.
            //
            //Carry forward the synthetic objective markers (EventID == -1). The live objective
            //handlers (OnDragonTaken / OnBaronTaken / OnHeraldTaken) and the mid-game baseline
            //append ObjectiveKilled records so the replay-rewind trim can count drakes/barons
            //taken before a seek point. Riot's /eventdata batch never contains them, and we
            //replace the list wholesale each tick — so without carrying them they would survive
            //only a single tick and the rewind drake/baron count would collapse to 0. Real Riot
            //events always carry EventID >= 0, so -1 uniquely identifies our synthetic markers.
            var carriedObjectives = pastIngameEvents.Where(e => e.EventID == -1).ToList();
            pastIngameEvents = allEvents;
            pastIngameEvents.AddRange(carriedObjectives);

            newEvents.ForEach(e =>
            {
                switch (e.EventName)
                {
                    case "TurretKilled":
                        CreditTurretKill(e.TurretKilled);
                        break;
                    case "ChampionKill":
                        UpdateKillsForTeam(blueTeam);
                        UpdateKillsForTeam(redTeam);
                        break;
                    case "InhibKilled":
                        stateData.inhibitors.Inhibitors.Single(inhib => inhib.id == e.InhibKilled.Substring(9, 5)).timeLeft = 300;
                        CreditInhibKill(e.InhibKilled);
                        break;
                    case "HordeKill":
                        // Void Grubs. Credit the killer's team (like a kill, not a structure).
                        // Riot emits one HordeKill per grub the team secures; see SPEC §3 for
                        // the spectator-symmetry caveat. KillerName maps to a team via GetKillerTeam.
                        {
                            Team grubTeam = GetKillerTeam(e.KillerName);
                            if (grubTeam != null)
                            {
                                grubTeam.voidgrubs++;
                                Log.Info($"Void Grub to {grubTeam.teamName} (now {grubTeam.voidgrubs}) by {e.KillerName}");
                            }
                        }
                        break;
                    case "DragonKill":
                        // Track the most recent killed drake's type as our best guess
                        // for "next dragon" UI; without memory we cannot see the
                        // pre-spawn Dragon_Indicator placeholder. Riot encodes the
                        // type in VictimName like "Dragon_Earth" / "Dragon_Elder".
                        // Best-effort parse: take the suffix after the last underscore.
                        string drakeType = ExtractDragonType(e.VictimName);
                        if (!string.IsNullOrEmpty(drakeType))
                        {
                            lastDragonType = drakeType;
                        }
                        // Phase 5: Power Play fallback — when the LiveEvents API
                        // (port 34243) isn't connected we drive DragonTaken straight
                        // from /eventdata. For Elder, the hasElder check prevents
                        // double-firing if both pipelines happen to be active.
                        if (ShouldFireObjectivesFromEventData())
                        {
                            Team dragonKillerTeam = GetKillerTeam(e.KillerName);
                            if (dragonKillerTeam != null)
                            {
                                bool isElder = drakeType.Equals("Elder", StringComparison.OrdinalIgnoreCase);
                                if (!isElder || !dragonKillerTeam.hasElder)
                                {
                                    IngameController.DragonTaken?.Invoke(this,
                                        new ObjectiveTakenArgs(drakeType, dragonKillerTeam, e.EventTime));
                                }
                            }
                        }
                        break;
                    case "BaronKill":
                        // Phase 5: drive Hand of Baron Power Play from /eventdata when
                        // the LiveEvents API isn't supplying it. The hasBaron guard
                        // prevents double-firing if both pipelines deliver the kill.
                        if (ShouldFireObjectivesFromEventData())
                        {
                            Team baronKillerTeam = GetKillerTeam(e.KillerName);
                            if (baronKillerTeam != null && !baronKillerTeam.hasBaron)
                            {
                                // Match the side-effects LiveEventsDataProvider sets
                                // before invoking BaronTaken, so OnBaronTaken sees a
                                // consistent team state.
                                baronKillerTeam.hasBaron = true;
                                baronKillerTeam.players.ForEach(p => p.diedDuringBaron = false);
                                IngameController.BaronTaken?.Invoke(this,
                                    new ObjectiveTakenArgs("Baron", baronKillerTeam, e.EventTime));
                            }
                        }
                        break;
                    case "HeraldKill":
                        if (ShouldFireObjectivesFromEventData())
                        {
                            Team heraldKillerTeam = GetKillerTeam(e.KillerName);
                            if (heraldKillerTeam != null)
                            {
                                IngameController.HeraldTaken?.Invoke(this,
                                    new ObjectiveTakenArgs("Herald", heraldKillerTeam, e.EventTime));
                            }
                        }
                        break;
                    default:
                        // Discoverability: surface any event name we don't yet handle (each
                        // fires once as a new event). Helps confirm new Riot objective names
                        // (e.g. AtakhanKill) and the spectator HordeKill behavior.
                        Log.Info($"Unhandled /eventdata event: {e.EventName}");
                        break;
                }
            });

            //Drake / Herald spawn detection previously came from comparing
            //gameSnap.Dragon.ID to lastDragon.ID — the in-game placeholder
            //appearing/disappearing at spawn time. With no snapshot we lose this
            //precision; spawn timer countdowns in IngameController.DoTick already
            //fire OnObjectiveSpawn("Baron") at zero, so the most visible spawn
            //event is preserved. Drake/Herald initial spawns are still announced
            //via /eventdata kill events when they're killed.
        }

        /// <summary>
        /// Credit a TurretKilled event to the team that DESTROYED the turret. The turret id marks
        /// the OWNING side: a Chaos/red-side turret falling is a BLUE takedown; an Order/blue-side
        /// turret is a RED takedown.
        ///
        /// Riot's Live Client Data feed switched the team marker in the turret id from "T1"/"T2"
        /// to "TOrder"/"TChaos" (observed e.g. "Turret_TChaos_L1_P2_..."), so the legacy
        /// Contains("T1")/Contains("T2") test silently matched nothing and tower counts stuck at 0.
        /// We accept BOTH spellings. ("Order"/"Chaos" are distinctive enough not to false-match the
        /// lane/position segments like "L1"/"P2".) Used by both the live path and the mid-game
        /// baseline so they stay identical.
        /// </summary>
        private void CreditTurretKill(string turretId)
        {
            string tk = turretId ?? "";
            if (tk.Contains("Chaos") || tk.Contains("T2"))
            {
                blueTeam.towers++;
            }
            else if (tk.Contains("Order") || tk.Contains("T1"))
            {
                redTeam.towers++;
            }
        }

        /// <summary>
        /// Credit an InhibKilled event to the DESTROYING team. The inhib id marks the OWNING side
        /// (e.g. "Inhib_TChaos_L1_..."): a Chaos/red-side inhib falling is a BLUE takedown, an
        /// Order/blue-side inhib is a RED takedown. Same Order/Chaos (and legacy T1/T2) handling as
        /// CreditTurretKill.
        /// </summary>
        private void CreditInhibKill(string inhibId)
        {
            string ik = inhibId ?? "";
            if (ik.Contains("Chaos") || ik.Contains("T2"))
            {
                blueTeam.inhibsDestroyed++;
            }
            else if (ik.Contains("Order") || ik.Contains("T1"))
            {
                redTeam.inhibsDestroyed++;
            }
        }

        /// <summary>
        /// Reconstruct cumulative scoreboard state from the historical /eventdata batch captured
        /// on the first observed tick. Used when broadcasting a game already in progress so tower
        /// and dragon counts reflect what happened before we started observing — instead of zero.
        ///
        /// Mutates counters ONLY. It deliberately does NOT fire the transient broadcast events
        /// (objective-taken banners, power-play popups) for things that happened in the past.
        ///
        /// Intentionally NOT reconstructed here:
        ///  - hasBaron / hasElder buffs: short-lived (≤3 min) and self-correct on expiry; faithfully
        ///    reconstructing the live countdown would require approximating the gold-at-take anchor,
        ///    which violates the fork's strict-accuracy policy (prefer hiding to approximating). A
        ///    buff icon may simply be absent if we start observing mid-buff — an honest gap.
        ///  - platesDestroyed: only emitted by the LiveEvents API (OnTurretPlateDestroyed); /eventdata
        ///    carries no plate events, so there is nothing to reconstruct from here.
        ///  - inhibitor / objective spawn timers: display-only and self-correct on the next live event.
        ///
        /// Replay rewind: each reconstructed drake also gets a synthetic ObjectiveKilled "Dragon"
        /// marker (below), which UpdateEvents' carry-forward keeps alive across the per-tick list
        /// overwrite. So a backward seek trims dragonsTaken to the correct historical count instead
        /// of collapsing it to 0 — for both pre-observation drakes and ones seen live.
        /// </summary>
        private void ApplyHistoricalBaseline(List<RiotEvent> history)
        {
            if (history == null || history.Count == 0)
            {
                return;
            }

            // Synthetic ObjectiveKilled markers, appended AFTER the loop. `history` aliases
            // pastIngameEvents, so adding to it mid-enumeration would throw. UpdateEvents' carry-
            // forward (EventID == -1) then keeps these alive across the per-tick list overwrite,
            // so the replay-rewind drake-trim counts the pre-observation drakes correctly.
            var reconstructedObjectives = new List<RiotEvent>();

            foreach (var e in history)
            {
                switch (e.EventName)
                {
                    case "TurretKilled":
                        CreditTurretKill(e.TurretKilled);
                        break;
                    case "InhibKilled":
                        CreditInhibKill(e.InhibKilled);
                        break;
                    case "HordeKill":
                        Team grubTeamBase = GetKillerTeam(e.KillerName);
                        if (grubTeamBase != null) grubTeamBase.voidgrubs++;
                        break;
                    case "DragonKill":
                        string drakeType = ExtractDragonType(e.VictimName);
                        if (!string.IsNullOrEmpty(drakeType))
                        {
                            lastDragonType = drakeType;   // best-guess "last drake" for the UI
                        }
                        // null => killer name did not match a known player; skip rather than
                        // mis-attribute (a missed drake is conservative; a wrong one is not).
                        Team dragonTeam = GetKillerTeam(e.KillerName);
                        if (dragonTeam != null)
                        {
                            dragonTeam.dragonsTaken.Add(drakeType);
                            // Mirror the live OnDragonTaken bookkeeping so a backward seek trims
                            // dragonsTaken to the correct count (see UpdateEvents carry-forward).
                            reconstructedObjectives.Add(new ObjectiveKilled("Dragon", dragonTeam.teamName, e.EventTime));
                        }
                        break;
                    default:
                        break;
                }
            }

            pastIngameEvents.AddRange(reconstructedObjectives);

            Log.Info($"Mid-game baseline from {history.Count} historical events — " +
                     $"towers blue:{blueTeam.towers} red:{redTeam.towers}, " +
                     $"dragons blue:{blueTeam.dragonsTaken.Count} red:{redTeam.dragonsTaken.Count}");
        }

        public void UpdateScoreboard()
        {
            stateData.scoreboard.GameTime = stateData.gameTime;
            stateData.scoreboard.SeriesGameCount = ConfigController.Component.Ingame.SeriesGameCount;
            stateData.scoreboard.TournamentName = ConfigController.Component.Ingame.TournamentName;

            stateData.nextDragon.SpawnTimer = stateData.dragon.SpawnTimer;
            // lastDragonType is populated from /eventdata DragonKill events.
            // Empty string before any dragon kill — frontend renders a default icon.
            stateData.nextDragon.Element = lastDragonType;


            //Do not support Herald for now
            stateData.nextBaron.SpawnTimer = stateData.baron.SpawnTimer;
            stateData.nextBaron.Element = "Baron";

            // In OCR-objective mode the /eventdata counts are NOT a valid fallback: in spectator/
            // replay they are the whole-game FINAL totals (and over-count), so when an OCR value is
            // momentarily absent (gate re-locking after a seek / warmup) reverting to them flashes
            // impossible numbers (grub 8, tower 14). Fall back to 0 instead — once OCR has locked,
            // ApplyObjectives holds the last good value, so 0 only shows before the very first lock.
            bool ocrObj = IngameController.CurrentSettings.UseOcrObjectives;
            Data.Frontend.FrontEndTeam currentTeam = stateData.scoreboard.BlueTeam;
            currentTeam.Name = TeamConfigViewModel.BlueTeam.NameTag;
            currentTeam.Icon = TeamConfigViewModel.BlueTeam.IconName;
            currentTeam.Kills = blueTeam.kills;
            currentTeam.Towers = blueTeam.OcrTowers ?? (ocrObj ? 0 : blueTeam.towers);
            currentTeam.Gold = blueTeam.GetGold(stateData.gameTime);
            currentTeam.Score = TeamConfigViewModel.BlueTeam.Score;
            currentTeam.PlatesDestroyed = blueTeam.platesDestroyed;
            currentTeam.VoidGrubs = blueTeam.OcrGrubs ?? (ocrObj ? 0 : blueTeam.voidgrubs);
            currentTeam.Inhibitors = blueTeam.inhibsDestroyed;
            currentTeam.Baron = blueTeam.OcrBaron ?? 0;
            currentTeam.DragonCount = blueTeam.OcrDragons ?? (ocrObj ? 0 : blueTeam.dragonsTaken.Count);
            currentTeam.Region = TeamConfigViewModel.BlueTeam.Region;
            currentTeam.Seed = TeamConfigViewModel.BlueTeam.Seed;
            currentTeam.Flag = TeamConfigViewModel.BlueTeam.Flag;

            currentTeam = stateData.scoreboard.RedTeam;
            currentTeam.Name = TeamConfigViewModel.RedTeam.NameTag;
            currentTeam.Icon = TeamConfigViewModel.RedTeam.IconName;
            currentTeam.Kills = redTeam.kills;
            currentTeam.Towers = redTeam.OcrTowers ?? (ocrObj ? 0 : redTeam.towers);
            currentTeam.Gold = redTeam.GetGold(stateData.gameTime);
            currentTeam.Score = TeamConfigViewModel.RedTeam.Score;
            currentTeam.PlatesDestroyed = redTeam.platesDestroyed;
            currentTeam.VoidGrubs = redTeam.OcrGrubs ?? (ocrObj ? 0 : redTeam.voidgrubs);
            currentTeam.Inhibitors = redTeam.inhibsDestroyed;
            currentTeam.Baron = redTeam.OcrBaron ?? 0;
            currentTeam.DragonCount = redTeam.OcrDragons ?? (ocrObj ? 0 : redTeam.dragonsTaken.Count);
            currentTeam.Region = TeamConfigViewModel.RedTeam.Region;
            currentTeam.Seed = TeamConfigViewModel.RedTeam.Seed;
            currentTeam.Flag = TeamConfigViewModel.RedTeam.Flag;

            // Phase 2: full per-player roster for the bottom comparison bar (EXACT from
            // /playerlist; per-player gold intentionally omitted — see PlayerScoreboardEntry).
            var roster = new System.Collections.Generic.List<Data.Frontend.PlayerScoreboardEntry>();
            if (blueTeam.players != null)
                roster.AddRange(blueTeam.players.Select(p => Data.Frontend.PlayerScoreboardEntry.From(p, stateData.gameTime)));
            if (redTeam.players != null)
                roster.AddRange(redTeam.players.Select(p => Data.Frontend.PlayerScoreboardEntry.From(p, stateData.gameTime)));
            stateData.scoreboard.Players = roster;

        }

        public void UpdateTeamColors()
        {
            //Nothing to update if not ingame
            if (blueTeam == null || redTeam == null)
            {
                return;
            }

            blueTeam.color = TeamConfigViewModel.BlueTeam.Color.ToSerializedString();
            redTeam.color = TeamConfigViewModel.RedTeam.Color.ToSerializedString();
        }

        #region Getters
        public Team GetTeam(string TeamName)
        {
            if (TeamName.Equals("Order", StringComparison.OrdinalIgnoreCase))
            {
                return blueTeam;
            }

            if (TeamName.Equals("Chaos", StringComparison.OrdinalIgnoreCase))
            {
                return redTeam;
            }

            return null;
        }

        // Team gold difference (blue − red) over game time, sampled each tick by
        // IngameController.DoTick AFTER OCR injection — so each point is OCR-exact
        // when the sidecar has a lock and the item+score estimate otherwise (both
        // via Team.GetGold's priority). Replaces the dead per-player goldHistory
        // path (its only writer was the removed LiveEvents API / memory reader).
        private readonly SortedList<double, float> teamGoldDiffHistory = new();
        // Guards teamGoldDiffHistory. Writers run on the tick timer's ThreadPool threads
        // (DoTick is async void on a 500ms System.Timers.Timer — ticks can overlap when the
        // Live Client API stalls) and on game start/stop event threads (ResetState / final
        // heartbeat); the reader (GetGoldGraph) runs inside Newtonsoft serialization of
        // stateData on whichever thread broadcasts. Unsynchronized, a concurrent RemoveAt/
        // Clear during the read loop throws inside SendEventToAllAsync — outside DoTick's
        // try/catch, which in an async void method kills the process.
        private readonly object goldGraphLock = new();
        private const double GOLD_GRAPH_SAMPLE_INTERVAL = 5.0;   // seconds of game time

        /// <summary>Record one gold-diff sample (called once per tick). Skips paused /
        /// non-advancing time; backward seeks are handled by TrimGoldDiffHistory.</summary>
        public void RecordGoldDiff(double gameTime, float diff)
        {
            if (gameTime <= 0)
            {
                return;
            }
            lock (goldGraphLock)
            {
                if (teamGoldDiffHistory.Count > 0)
                {
                    double last = teamGoldDiffHistory.Keys[teamGoldDiffHistory.Count - 1];
                    if (gameTime < last + GOLD_GRAPH_SAMPLE_INTERVAL)
                    {
                        return;
                    }
                }
                teamGoldDiffHistory[gameTime] = diff;
            }
        }

        /// <summary>Drop samples at/after the seek target after a backward seek (replay).</summary>
        public void TrimGoldDiffHistory(double seekTargetGameTime)
        {
            lock (goldGraphLock)
            {
                while (teamGoldDiffHistory.Count > 0 &&
                       teamGoldDiffHistory.Keys[teamGoldDiffHistory.Count - 1] >= seekTargetGameTime)
                {
                    teamGoldDiffHistory.RemoveAt(teamGoldDiffHistory.Count - 1);
                }
            }
        }

        public void ClearGoldDiffHistory()
        {
            lock (goldGraphLock)
            {
                teamGoldDiffHistory.Clear();
            }
        }

        public Dictionary<double, float> GetGoldGraph()
        {
            Dictionary<double, float> outList = new Dictionary<double, float>();

            // Snapshot under the lock; the downsampling below then runs race-free.
            KeyValuePair<double, float>[] samples;
            lock (goldGraphLock)
            {
                samples = teamGoldDiffHistory.ToArray();
            }

            int dataPoints = samples.Length;
            if (dataPoints < 2)
            {
                return new Dictionary<double, float>() {
                    {0, 0},
                    {1, 0}
                };
            }

            // Downsample: keep first/last, >500g moves, or >15s gaps (legacy rule).
            for (int i = 0; i < dataPoints; i++)
            {
                double time = samples[i].Key;
                float goldDiff = samples[i].Value;
                if (i == 0 || i == dataPoints - 1 || Math.Abs(outList.Last().Value - goldDiff) > 500 || time - outList.Last().Key > 15)
                {
                    outList.TryAdd(time, goldDiff);
                }
            }

            //limit outlist size to 1000 to prevent crashing due to too many data points
            if (outList.Count > 1000)
            {
                Dictionary<double, float> newOutList = new Dictionary<double, float>();
                int step = outList.Count / 1000;
                for (int i = 0; i < outList.Count; i += step)
                {
                    newOutList.Add(outList.Keys.ElementAt(i), outList.Values.ElementAt(i));
                }
                outList = newOutList;
            }

            return outList;
        }

        public void UpdateKillsForTeam(Team t)
        {
            // Sum per-player kills from /playerlist scores. Empirically more
            // accurate than counting /eventdata ChampionKill events: /eventdata
            // misses some kills (observed 8 actual vs 5 events for Blue at one
            // sample). /playerlist.scores.kills tracks the in-client scoreboard
            // exactly and updates continuously, including correct rewind behavior.
            t.kills = t.players?.Sum(p => p?.scores?.kills ?? 0) ?? 0;
        }

        public List<Player> GetAllPlayers()
        {
            List<Player> bluePlayers = blueTeam?.players;
            List<Player> redPlayers = redTeam?.players;
            if (bluePlayers == null && redPlayers == null)
            {
                return new List<Player>();
            }
            if (bluePlayers == null)
            {
                return redPlayers;
            }
            if (redPlayers == null)
            {
                return bluePlayers;
            }
            return bluePlayers.Concat(redPlayers).ToList();
        }

        public List<Team> GetBothTeams()
        {
            return new List<Team>() { blueTeam, redTeam };
        }
        #endregion

        #region Setters
        public void SetObjectiveData(BackEndObjective back, FrontEndObjective front, double time)
        {
            //Generate text version of time for frontend
            back.DurationRemaining = time;
            TimeSpan t = TimeSpan.FromSeconds(time);
            front.DurationRemaining = t.ToString(@"mm\:ss");

            //Gold differences since objective was taken
            float originalDiff = back.BlueStartGold - back.RedStartGold;
            float currentDiff = stateData.blueGold - stateData.redGold;

            //Check if blue has the objective to determine in which direction the gold difference should go
            bool blueHasObjective = (front.Type == Objective.ObjectiveType.Baron) ? blueTeam.hasBaron : blueTeam.hasElder;

            //Difference between the gold gained inverted based on if Blue or Red team has the objective
            front.GoldDifference = (currentDiff - originalDiff) * (blueHasObjective ? 1 : -1);
        }

        public void ResetState()
        {
            this.blueTeam = null;
            this.redTeam = null;
            this.stateData = new StateData();
            this.stateData.inhibitors = new InhibitorInfo();
            this.pastIngameEvents = new List<RiotEvent>();
            this.pendingHistoricalCatchup = false;
            ClearGoldDiffHistory();

            this.turrets = new Dictionary<string, Turret>();
            this.lastDragonType = "";
            this.RewindOccurredThisTick = false;
            Log.Info("Game State reset");
        }

        internal void UpdateTurrets(List<RiotEvent> allEvents)
        {
            // Snapshot.Turrets gave us live Position/Health for every standing
            // tower. /eventdata only tells us when a turret is destroyed.
            //
            // Without the position/health stream the plate-credit logic in
            // LiveEventsDataProvider.OnPlateDestroy can no longer compute distances
            // (farsightObject is null), so it short-circuits via TryGetValue
            // returning false on this empty dictionary.
            //
            // For the user-facing scoreboard the existing TurretKilled-driven
            // counters in UpdateEvents (blueTeam.towers / redTeam.towers) are
            // sufficient. We intentionally leave the `turrets` dictionary empty
            // — its only consumer that requires populated entries is dead code
            // under the no-memory-reader configuration.
            //
            // The events parameter is reserved for forward compatibility (e.g.
            // tracking destroyed-turret names if the overlay ever wants to mark
            // standing-only icons).
        }
        #endregion
    }
}
