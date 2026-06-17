using OpenForestUI.Common.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using static OpenForestUI.Common.Log;

namespace OpenForestUI.Common.Data.Config
{
    class ComponentConfig : JSONConfig
    {
        [JsonIgnore]
        public override string Name => "Component";

        public override string FileVersion { get => _fileVersion; set => _fileVersion = value; }

        [JsonIgnore]
        public static new string CurrentVersion => "1.6";

        public DataDragonConfig DataDragon;

        public PickBanConfig PickBan;

        public IngameComponentConfig Ingame;

        public ReplayConfig Replay;

        public PostGameConfig PostGame;

        public AppConfig App;


        public override string GETCurrentVersion()
        {
            return CurrentVersion;
        }

        public override string GETDefaultString()
        {
            return SerializeIndented(CreateDefault());
        }

        public override void RevertToDefault()
        {
            ComponentConfig def = CreateDefault();
            this.DataDragon = def.DataDragon;
            this.PickBan = def.PickBan;
            this.Ingame = def.Ingame;
            this.Replay = def.Replay;
            this.App = def.App;
            this.PostGame = def.PostGame;
            this.FileVersion = CurrentVersion;
        }

        private ComponentConfig CreateDefault()
        {
            return new()
            {
                DataDragon = new DataDragonConfig()
                {
                    MinimumGoldCost = 2000,
                    Region = "global",
                    Locale = "en_US",
                    Patch = "latest",
                    CDN = "https://ddragon.leagueoflegends.com/cdn",
                    CDragonRaw = "https://raw.communitydragon.org"
                },
                PickBan = new PickBanConfig()
                {
                    IsActive = true,
                    DelayValue = 300,
                    UseDelay = false,
                    DefaultBlueColor = "rgb(66, 133, 244)",
                    DefaultRedColor = "rgb(234, 67, 53)"
                },
                Ingame = new IngameComponentConfig()
                {
                    IsActive = true,
                    UseLiveEvents = true,
                    DoItemCompleted = true,
                    DoLevelUp = true,
                    UseCustomScoreboard = false,
                    UseMemoryReader = false,
                    SeriesGameCount = 3,
                    TournamentName = "",
                    Objectives = new IngameComponentConfig.ObjectiveConfig()
                    {
                        DoBaronKill = true,
                        DoDragonKill = true,
                        DoInhibitors = false,
                        DoObjectiveSpawnPopUp = false,
                        DoObjectiveKillPopUp = false,
                        // Functional since the ObjectiveSpawnClock rework (derived from
                        // raw kill events every tick) — on by default like the dragon timer.
                        UseCustomBaronTimer = true,
                        UseCustomDragonTimer = true,
                    },
                    Timings = new ObjectiveTimingsConfig(),
                    Teams = new IngameComponentConfig.TeamInfoConfig()
                    {
                        DoTeamIcons = false,
                        DoTeamNames = false,
                        DoTeamScores = false
                    }
                },
                PostGame = new()
                {
                    IsActive = false
                },
                Replay = new ReplayConfig()
                {
                    IsActive = true,
                    // Default OFF: on game start this sent observer HUD-toggle keys (O, U, N) to the
                    // LoL client, which closed the spectator detail scoreboard — the panel we OCR for
                    // exact per-player CS. Leaving it off keeps the operator's HUD layout intact.
                    // Re-enable via the Ingame > AutoInitUI toggle if a clean (HUD-off) view is wanted.
                    UseAutoInitUI = false
                },
                App = new AppConfig()
                {
                    LogLevel = LogLevel.Info,
                    CheckForUpdates = false,
                    UpdateRepositoryName = "",
                    UpdateRepositoryUrl = "",
                    LeagueInstall = new()
                    {
                        "C:\\",
                        "D:\\",
                        "E:\\",
                        "F:\\",
                        "C:\\Program Files",
                        "C:\\Program Files (x86)"
                    },
                    OffsetRepository = "",
                    OffsetPrefix = "Offsets-",
                    CheckForOffsets = false,
                    FrontendPort = 9001,
                }
            };
        }

        public override string GETJson()
        {
            return SerializeIndented(this);
        }

        public override bool UpdateConfigVersion(string oldVersion, string oldValues)
        {
            if (oldVersion.Equals("1.5"))
            {
                Task t = new(async () =>
                {
                    await Task.Delay(200);
                    //1.5 to 1.6

                    FileVersion = CurrentVersion;

                    App.FrontendPort = 9001;

                    JSONConfigProvider.Instance.WriteConfig(this);
                    Info($"Updated Component config from v1.5 to v{CurrentVersion}");
                });
                t.Start();
            }
            else
            {
                Log.Warn("Config too old to update");
            }
            return true;
        }

        public override void UpdateValues(string readValues)
        {
            ComponentConfig Cfg = JsonConvert.DeserializeObject<ComponentConfig>(readValues);
            this.DataDragon = Cfg.DataDragon;
            this.PickBan = Cfg.PickBan;
            this.Ingame = Cfg.Ingame;
            this.Replay = Cfg.Replay;
            this.PostGame = Cfg.PostGame;
            this.App = Cfg.App;
            this.FileVersion = Cfg.FileVersion;
        }

    }

    public class DataDragonConfig
    {
        public int MinimumGoldCost;
        public string Region;
        public string Locale;
        public string CDN;
        public string CDragonRaw;
        public string Patch;
    }

    public class PickBanConfig
    {
        public bool IsActive;
        public bool UseDelay;
        public int DelayValue;
        public string DefaultBlueColor;
        public string DefaultRedColor;
    }

    public class IngameComponentConfig
    {
        public bool IsActive;
        public bool UseLiveEvents;
        public bool DoLevelUp;
        public bool DoItemCompleted;
        public int SeriesGameCount;
        public ObjectiveConfig Objectives;
        public TeamInfoConfig Teams;
        public bool UseCustomScoreboard;
        // PRM top bar: tournament banner text (e.g. "EMEA Masters 2026 Spring | ..."). Empty -> hidden.
        public string TournamentName;

        // Vanguard-compatible fork: memory reader (Farsight) defaults off.
        // Enable only on environments where ReadProcessMemory works (no Vanguard).
        public bool UseMemoryReader;

        public class ObjectiveConfig
        {
            public bool DoBaronKill;
            public bool DoDragonKill;
            public bool DoInhibitors;
            public bool DoObjectiveSpawnPopUp;
            public bool DoObjectiveKillPopUp;
            public bool UseCustomDragonTimer;
            public bool UseCustomBaronTimer;
        }

        // Objective spawn/respawn timings in SECONDS of game time. These are PATCH DATA,
        // not code: Riot retunes them between seasons (Baron moved 20:00 -> 25:00 -> back
        // to 20:00 in patch 26.1), so they live in Component.json where a season change
        // is a config edit. Defaults verified against the patch 26.x wiki (2026-06).
        public ObjectiveTimingsConfig Timings;

        public class TeamInfoConfig
        {
            public bool DoTeamNames;
            public bool DoTeamIcons;
            public bool DoTeamScores;
        }
    }

    /// <summary>
    /// Objective spawn/respawn timings (seconds of game time). Used by ObjectiveSpawnClock
    /// to derive every countdown from raw kill events. Older Component.json files won't
    /// contain this section — use-sites must fall back to <see cref="Defaults"/>.
    /// </summary>
    public class ObjectiveTimingsConfig
    {
        public double FirstDragonSpawn = 300;    //  5:00
        public double DragonRespawn = 300;       //  5:00
        public double ElderRespawn = 360;        //  6:00 (after a team reaches soul point)
        public double FirstHeraldSpawn = 900;    // 15:00 (patch 26.x; single spawn)
        public double HeraldDespawn = 1185;      // 19:45 (Baron replaces it at 20:00)
        public double FirstBaronSpawn = 1200;    // 20:00 (patch 26.1; was 25:00 in season 2025)
        public double BaronRespawn = 360;        //  6:00 (patch 26.x; was 7:00 historically)

        [JsonIgnore]
        public static readonly ObjectiveTimingsConfig Defaults = new();
    }

    public class PostGameConfig
    {
        public bool IsActive;
    }

    public class ReplayConfig
    {
        public bool IsActive;
        public bool UseAutoInitUI;
    }

    public class AppConfig
    {
        public LogLevel LogLevel;
        public bool CheckForUpdates;
        public string UpdateRepositoryUrl;
        public string UpdateRepositoryName;
        public List<string> LeagueInstall;
        public bool CheckForOffsets;
        public string OffsetRepository;
        public string OffsetPrefix;
        public int FrontendPort;

        [JsonIgnore]
        public StringVersion Version => GetSimpleLocalVersion();

        private StringVersion GetSimpleLocalVersion()
        {
            string longVersion = FileVersionInfo.GetVersionInfo("OpenForestUI.exe").FileVersion;
            return StringVersion.TryParse(
                FileVersionInfo.GetVersionInfo("OpenForestUI.exe")
                .FileVersion.Remove(longVersion.LastIndexOf('.')),
                out StringVersion version) ? version! : StringVersion.Zero;
        }
    }
}
