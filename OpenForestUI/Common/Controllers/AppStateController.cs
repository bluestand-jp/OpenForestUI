using LCUSharp;
using LCUSharp.Websocket;
using OpenForestUI.ChampSelect.Data.LCU;
using OpenForestUI.ChampSelect.StateInfo;
using OpenForestUI.Common.Data.Provider;
using OpenForestUI.Common.Utils;
using OpenForestUI.Farsight;
using OpenForestUI.MVVM.ViewModel;
using OpenForestUI.OperatingSystem;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenForestUI.Common.Controllers
{
    class AppStateController : ITickable
    {
        public static EventHandler GameLoad, GameStop, ChampSelectStart, ChampSelectStop;
        public static EventHandler<Process> GameStart;
        public static List<Summoner> summoners = new();
        public static StringVersion LocalGameVersion;

        private static AppStateController _instance;

        public static AppStateController Instance
        {
            get => GetInstance();
            set => _instance = value;
        }

        public LeagueClientApi ClientAPI;

        private MainViewModel mainCtx;

        // Reliable "LCU client is connected" signal for the status resolver. The [Flags] LeagueState
        // enum cannot carry this — its Connected member is 0, so HasFlag(Connected) is always true and
        // Set/Unset(Connected) are no-ops — so connectedness is tracked explicitly here. Written from
        // the async connect / disconnect handlers, read on the tick thread (hence volatile).
        private volatile bool _clientConnected;

        // Last status pushed to the chip, so the tick resolver writes (and logs) only on a transition.
        // Touched only on the tick-timer thread.
        private ConnectionStatusViewModel _lastStatus;

        private static AppStateController GetInstance()
        {
            if (_instance == null)
            {
                _instance = new();
            }

            return _instance;
        }

        private AppStateController()
        {
            if (_instance != null)
            {
                return;
            }

            mainCtx = (MainViewModel)BroadcastController.Instance.Main.DataContext;
        }

        public void Init()
        {
            InitConnection();

            if (ConfigController.Component.PickBan.IsActive)
            {
                EnableChampSelect();
            }
            if (ConfigController.Component.Ingame.IsActive)
            {
                EnableIngame();
            }
        }

        public void EnableChampSelect()
        {
            ChampSelectStart += BroadcastController.Instance.PBController.OnEnterPickBan;
            ChampSelectStop += BroadcastController.Instance.PBController.OnPickBanExit;

            BroadcastController.Instance.PBController.Enable();
            Log.Info("PickBan Enabled");
        }

        public void DisableChampSelect()
        {
            if (BroadcastController.CurrentLeagueState.HasFlag(LeagueState.ChampSelect))
            {
                ConfigController.Component.PickBan.IsActive = true;
                Log.Warn("Tried disabling Champ Select while active");
            }
            ChampSelectStart -= BroadcastController.Instance.PBController.OnEnterPickBan;
            ChampSelectStop -= BroadcastController.Instance.PBController.OnPickBanExit;
            BroadcastController.Instance.PBController.Disable();
            Log.Info("PickBan Disabled");
        }

        public static void EnableIngame()
        {
            GameStart += BroadcastController.Instance.IGController.EnterIngame;
            Log.Info("Ingame Enabled");
        }

        public static void DisableIngame()
        {
            if (BroadcastController.CurrentLeagueState.HasFlag(LeagueState.InProgress))
            {
                ConfigController.Component.Ingame.IsActive = true;
                Log.Warn("Tried disabling Ingame while active");
            }
            GameStart -= BroadcastController.Instance.IGController.EnterIngame;
            Log.Info("Ingame Disabled");
        }

        private async void InitConnection()
        {
            ClientAPI = await ConnectToClient();
            ClientAPI.Disconnected += async (e, a) =>
            {
                State.LeagueDisconnected();
                _clientConnected = false;
                FlagsHelper.Unset(ref BroadcastController.CurrentLeagueState, LeagueState.Connected);
                Log.Info("Client Disconnected! Attempting to reconnect...");
                await ClientAPI.ReconnectAsync();
                State.LeagueConntected();
                FlagsHelper.Set(ref BroadcastController.CurrentLeagueState, LeagueState.Connected);
                _clientConnected = true;
                Log.Info("Client Reconnected!");
            };
        }

        private async Task<LeagueClientApi> ConnectToClient()
        {
            Log.Info("Connecting to League Client");
            Stopwatch stopwatch = Stopwatch.StartNew();
            LeagueClientApi api = await LeagueClientApi.ConnectAsync();
            api.EventHandler.Subscribe("/lol-gameflow/v1/gameflow-phase", ClientStateChanged);
            api.EventHandler.Subscribe("/lol-champ-select/v1/session", ChampSelectChanged);
            api.EventHandler.Subscribe("/lol-champ-select/vi/sfx-notifications", ChampSelectSFXChanged);
            stopwatch.Stop();

            _clientConnected = true;

            FlagsHelper.Set(ref BroadcastController.CurrentLeagueState, LeagueState.Connected);
            Log.Info($"Connected to League Client in {stopwatch.ElapsedMilliseconds} ms");

            State.LeagueConntected();

            string res = null;
            while (res is null)
            {
                res = await api.RequestHandler.GetResponseAsync<string>(HttpMethod.Get, "/lol-patch/v1/game-version");
            }
            LocalGameVersion = GetLocalGameVersion(res);
            LoadOffsets();

            return api;
        }

        private void ClientStateChanged(object sender, LeagueEvent e)
        {
            string eventType = e.Data.ToString();
            Log.Info($"League State: {eventType}");

            if (!eventType.Equals("ChampSelect") && BroadcastController.CurrentLeagueState.HasFlag(LeagueState.ChampSelect))
            {
                ChampSelectStop?.Invoke(this, EventArgs.Empty);
                return;
            }
            //Backup state change
            if (eventType.Equals("ChampSelect") && !BroadcastController.CurrentLeagueState.HasFlag(LeagueState.ChampSelect))
            {
                Log.Info("Starting champ select from client state change");
                ChampSelectStart?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ChampSelectChanged(object sender, LeagueEvent e)
        {
            if (ConfigController.Component.PickBan.IsActive)
            {
                if (BroadcastController.CurrentLeagueState.HasFlag(LeagueState.InProgress) || BroadcastController.CurrentLeagueState.HasFlag(LeagueState.PostGame))
                {
                    return;
                }
                if (!BroadcastController.CurrentLeagueState.HasFlag(LeagueState.ChampSelect))
                {
                    JArray myTeam = e.Data["myTeam"] as JArray;
                    JArray theirTeam = e.Data["theirTeam"] as JArray;
                    if (myTeam.Count == 0 && theirTeam.Count == 0)
                    {
                        return;
                    }

                    ChampSelectStart?.Invoke(this, EventArgs.Empty);
                }
                BroadcastController.Instance.PBController.ApplyNewState(e);
            }
        }

        private void ChampSelectSFXChanged(object sender, LeagueEvent e)
        {
            if (ConfigController.Component.PickBan.IsActive)
            {
                Log.Info($"SFX Change: {e.Data}");
            }
        }

        public static async Task CacheSummoners(Session session)
        {
            //Clear to reset summoners before caching
            summoners.Clear();

            List<Cell> blueTeam = session.myTeam;
            List<Cell> redTeam = session.theirTeam;

            Dictionary<Cell, Task<string>> jobs = FetchPlayersFromTeam(blueTeam);
            jobs = jobs.Concat(FetchPlayersFromTeam(redTeam)).ToDictionary(x => x.Key, x => x.Value);
            List<Task<string>> completedJobs = jobs.Values.ToList();
            while (completedJobs.Any())
            {
                Task<string> finished = await Task.WhenAny(completedJobs);
                summoners.Add(JsonConvert.DeserializeObject<Summoner>(await finished));
                completedJobs.Remove(finished);
            }

            Log.Verbose($"Cached {summoners.Count} summoners");
        }

        public static async Task<Timer> GetTimer()
        {
            try
            {
                return JsonConvert.DeserializeObject<Timer>(await Instance.ClientAPI.RequestHandler.GetJsonResponseAsync(HttpMethod.Get, $"/lol-champ-select/v1/session/timer"));
            }
            catch
            {
                return null;
            }
        }

        private static Dictionary<Cell, Task<string>> FetchPlayersFromTeam(List<Cell> team)
        {
            Dictionary<Cell, Task<string>> toFinish = new Dictionary<Cell, Task<string>>();
            team.ForEach(cell =>
            {
                if (cell.summonerId == 0)
                {
                    return;
                }

                try
                {
                    toFinish.Add(cell, Instance.ClientAPI.RequestHandler.GetJsonResponseAsync(HttpMethod.Get, $"lol-summoner/v1/summoners/{cell.summonerId}"));
                }
                catch (Exception)
                {
                    Log.Info("Could not fetch players for team. Is this not a custom game?");
                }

            });

            return toFinish;
        }

        private void LoadOffsets()
        {
            Log.Info($"Local Client Version {LocalGameVersion}");
            Log.Info($"Loading offsets for current patch");
            ConfigController.LoadOffsetConfig();
            FarsightController.GameOffsets = ConfigController.Farsight.GameOffsets;
            FarsightController.ObjectOffsets = ConfigController.Farsight.ObjectOffsets;
            Log.Info($"Using offsets {ConfigController.Farsight.OffsetVersion}");
        }

        private StringVersion GetLocalGameVersion(string rawPatch)
        {
            try
            {
                string[] patchComponents = rawPatch.Split('.');
                return StringVersion.Parse($"{patchComponents[0]}.{patchComponents[1]}.1");
            }
            catch
            {
                Log.Warn($"Could not determine local game version. Client reported invalid patch \"{rawPatch}\"");
                Log.Info("Reverting to DataDragon version");
                return StringVersion.Parse(DataDragon.version.Champion);
            }

        }

        public void DoTick()
        {
            ReconcileGameProcess();
            UpdateConnectionStatus();
        }

        // Self-heal a game that ended without a WMI process-stop event. If we still hold the game's
        // process handle but it has exited, run the same teardown TargetProcessStopped would have
        // (GameStop -> overlay GameEnd + drop from the tick list + chip back to Client Loaded). Without
        // this the chip stays stuck on "Connected" and the overlay keeps the dead game's last frame.
        // Nulling LeagueProcess first makes a later (or racing) WMI stop event a no-op, so GameStop
        // fires exactly once.
        private void ReconcileGameProcess()
        {
            Process proc = IngameController.LeagueProcess;
            if (proc != null && HasExited(proc))
            {
                IngameController.LeagueProcess = null;
                GameStop?.Invoke(this, EventArgs.Empty);
            }
        }

        // Single source of truth for the top-bar status chip. Resolved every tick (2x/s) from reliable
        // signals, so a missed lifecycle edge self-corrects within ~500ms instead of leaving the chip
        // stale. Writes (and logs) only on an actual transition.
        private void UpdateConnectionStatus()
        {
            if (mainCtx == null)
                return;

            ConnectionStatusViewModel target =
                  MockController.IsEnabled ? ConnectionStatusViewModel.MOCKING        // preview feed wins
                : !_clientConnected        ? ConnectionStatusViewModel.DISCONNECTED   // LCU API down
                : IsGameRunning()          ? ConnectionStatusViewModel.CONNECTED       // live game / spectating
                :                            ConnectionStatusViewModel.LCU;            // client up, no game
            // (ConnectionStatusViewModel.ISSUE is defined but intentionally not produced here yet.)

            if (ReferenceEquals(target, _lastStatus))
                return;

            _lastStatus = target;
            Log.Info($"Connection status: {target.TextContent}");
            mainCtx.ConnectionStatus = target;   // StateService setter marshals to the UI thread
        }

        private static bool IsGameRunning()
        {
            Process proc = IngameController.LeagueProcess;
            return proc != null && !HasExited(proc);
        }

        // HasExited returns cleanly (no throw) for a normally-exited same-session process; the catch is
        // only for an invalid/inaccessible handle, where we conservatively assume the game is still
        // running so a transient error never tears down a live game (WMI remains the primary stop path).
        private static bool HasExited(Process proc)
        {
            try { return proc.HasExited; }
            catch { return false; }
        }

        public static Summoner GetSummonerById(long id)
        {
            return summoners.SingleOrDefault(summoner => summoner.summonerId == id);
        }

    }

    public class StateControllerSettings
    {
        public bool PickBan;
        public bool Delay;
        public bool Ingame;
    }
}

