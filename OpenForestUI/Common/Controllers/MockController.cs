using OpenForestUI.Common.Data.Provider;
using OpenForestUI.Http;
using OpenForestUI.MVVM.ViewModel;
using OpenForestUI.OperatingSystem;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace OpenForestUI.Common.Controllers
{
    /// <summary>
    /// Dev / preview helper: pushes a canned "GameHeartbeat" to the Ingame overlay so the PRM bar
    /// (and the rest of the overlay) can be previewed + tuned in the real OBS browser source WITHOUT
    /// a live game. Toggled from Settings ("Use Mock").
    ///
    /// It reuses the overlay-harness mock-state fixture, overlays the operator's live team Details
    /// (name/tag/score/color/logo/region/seed/flag) onto it so the preview reflects PickBan > Details,
    /// wraps it in the heartbeat envelope, and broadcasts it ~2x/s over the same WebSocket the live
    /// game uses. The overlay already holds the real OverlayConfig (PrmScore enabled), so the bar
    /// renders. While Mock is on it has priority over the live feed (IngameController.Broadcast gates
    /// the live feed off) so the two never interleave. Transient: defaults OFF on each launch.
    /// </summary>
    public static class MockController
    {
        // The overlay-harness mock-state fixture the overlay renders, so the in-app preview matches it
        // 1:1. Resolved relative to the executable (source checkout or published build); override with
        // the OPENFORESTUI_MOCK_STATE environment variable.
        public static string MockStatePath = ResolveMockStatePath();

        private static string ResolveMockStatePath()
        {
            var env = Environment.GetEnvironmentVariable("OPENFORESTUI_MOCK_STATE");
            if (!string.IsNullOrEmpty(env)) return env;
            var dir = AppContext.BaseDirectory;
            for (int i = 0; i < 6 && dir != null; i++)
            {
                var candidate = Path.Combine(dir, "ocr-poc", "overlay-harness", "mock-state.json");
                if (File.Exists(candidate)) return candidate;
                dir = Directory.GetParent(dir)?.FullName;
            }
            return Path.Combine("ocr-poc", "overlay-harness", "mock-state.json");
        }

        private static Timer _timer;
        private static JObject _baseState;
        public static bool IsEnabled { get; private set; }

        /// <summary>Start/stop broadcasting the mock heartbeat. Reloads the fixture on each start.</summary>
        public static void SetEnabled(bool on)
        {
            if (on == IsEnabled) return;

            if (on)
            {
                if (!TryLoad()) return;          // leave IsEnabled=false so the toggle snaps back on failure
                IsEnabled = true;
                _timer = new Timer(_ => Push(), null, 0, 500);
                Log.Info("Mock overlay feed STARTED (Settings > Use Mock)");
            }
            else
            {
                IsEnabled = false;
                _timer?.Dispose();
                _timer = null;
                Log.Info("Mock overlay feed stopped");
            }
            // The title-bar chip reflects MOCKING via AppStateController's tick resolver (single source
            // of truth, keyed off IsEnabled), so there is no status to set/restore here.
        }

        private static bool TryLoad()
        {
            try
            {
                // tolerate a UTF-8 BOM (the harness fixture may carry one)
                var json = File.ReadAllText(MockStatePath).TrimStart('﻿').Trim();
                _baseState = JObject.Parse(json);
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn($"Mock state load failed ({MockStatePath}): {ex.Message}");
                return false;
            }
        }

        private static void Push()
        {
            try
            {
                if (_baseState == null) return;
                // Work on a fresh CLONE each push. ApplyFeatureGates REMOVES fields (mirroring the live
                // ShouldSerialize* gates); mutating _baseState directly would permanently strip them so
                // they'd never return when a toggle is switched back on. The clone keeps the fixture pristine.
                var state = (JObject)_baseState.DeepClone();
                // Overlay the operator's live team Details onto the fixture each push, so edits to
                // PickBan > Details (name/tag/score/color/logo/region/seed/flag) show up in the Mock
                // preview without toggling off/on. Champs, items and stats stay from the fixture.
                ApplyTeamIdentity(state);
                // Synthesize the InfoPage from the fixture roster so the Gold Tab / CS per Min toggles
                // preview under Mock (live builds it from gameState, which is empty during Mock).
                ApplyInfoPage(state);
                // Apply the SAME Ingame > Teams toggles the live feed applies via ShouldSerialize* in
                // ScoreboardConfig/FrontEndTeam, so Scoreboard / Team Names / Team Scores / Team Icons
                // hide & show in the Mock preview exactly as they do in a live game.
                ApplyFeatureGates(state);
                string payload = "{\"eventType\":\"GameHeartbeat\",\"stateData\":" + state.ToString(Formatting.None) + "}";
                EmbedIOServer.SocketServer?.SendMessageToAllAsync(payload);
            }
            catch (Exception ex) { Log.Warn($"Mock push failed: {ex.Message}"); }
        }

        // Mirrors State.UpdateScoreboard's config -> FrontEndTeam identity mapping, applied to the mock
        // JSON. Colors are emitted as "rgb(r,g,b)" (what the overlay's RGBStringToColor parses; the raw
        // fixture's hex value would not parse). Scoreboard stats and the roster are left untouched.
        private static void ApplyTeamIdentity(JObject state)
        {
            state["blueColor"] = TeamConfigViewModel.BlueTeam.Color.ToSerializedString();
            state["redColor"] = TeamConfigViewModel.RedTeam.Color.ToSerializedString();

            if (state["scoreboard"] is JObject sb)
            {
                PatchTeam(sb["BlueTeam"] as JObject, TeamConfigViewModel.BlueTeam);
                PatchTeam(sb["RedTeam"] as JObject, TeamConfigViewModel.RedTeam);
            }
        }

        private static void PatchTeam(JObject team, TeamConfigViewModel vm)
        {
            if (team == null || vm == null) return;
            team["Name"] = vm.NameTag;     // PRM bar shows Name as the team tag (matches State.UpdateScoreboard)
            team["Icon"] = vm.IconName;
            team["Score"] = vm.Score;
            team["Region"] = vm.Region;
            team["Seed"] = vm.Seed;
            team["Flag"] = vm.Flag;
        }

        // Mirror the live feed's ShouldSerialize* gates (ScoreboardConfig.cs / FrontEndTeam.cs) on the
        // mock JSON so the Ingame > Teams toggles behave under Mock exactly as in a live game. Removing
        // a field makes the overlay treat it as "off": absent GameTime hides the whole scoreboard (PRM
        // top bar + bottom board), absent Name/Region/Seed/Flag clears the team name, absent Icon
        // removes the logo, absent Score/SeriesGameCount drops the series score.
        private static void ApplyFeatureGates(JObject state)
        {
            // Gold Graph data gate (runtime "Gold Graph" toggle): off -> drop the data so the graph hides.
            if (!IngameController.CurrentSettings.GoldGraph) state.Remove("goldGraph");

            if (!(state["scoreboard"] is JObject sb)) return;

            var ig = ConfigController.Component.Ingame;
            bool scoreboard = ig.UseCustomScoreboard;   // master: GameTime/Players + per-team stat numbers
            bool names = ig.Teams.DoTeamNames;          // Name / Region / Seed / Flag
            bool icons = ig.Teams.DoTeamIcons;          // Icon
            bool scores = ig.Teams.DoTeamScores;        // Score + SeriesGameCount

            if (!scoreboard) { sb.Remove("GameTime"); sb.Remove("Players"); }
            if (!scores) sb.Remove("SeriesGameCount");

            GateTeam(sb["BlueTeam"] as JObject, scoreboard, names, icons, scores);
            GateTeam(sb["RedTeam"] as JObject, scoreboard, names, icons, scores);
        }

        private static void GateTeam(JObject team, bool scoreboard, bool names, bool icons, bool scores)
        {
            if (team == null) return;
            if (!names) { team.Remove("Name"); team.Remove("Region"); team.Remove("Seed"); team.Remove("Flag"); }
            if (!icons) team.Remove("Icon");
            if (!scores) team.Remove("Score");
            if (!scoreboard)
            {
                team.Remove("Kills"); team.Remove("Towers"); team.Remove("Gold");
                team.Remove("VoidGrubs"); team.Remove("Inhibitors"); team.Remove("Baron"); team.Remove("DragonCount");
            }
        }

        // Build the InfoPage side panel from the fixture roster so the "Gold Tab" / "CS per Min" toggles
        // preview under Mock (live builds it from gameState, which is empty during Mock). Mirrors
        // IngameController.UpdateInfoPage + PlayerTab.GetGoldTabs/GetCSPerMinTabs. PlayerGold wins if both on.
        private static void ApplyInfoPage(JObject state)
        {
            var cs = IngameController.CurrentSettings;
            bool gold = cs.PlayerGold;
            bool cspm = !gold && cs.CSPerMin;
            if (!gold && !cspm) { state.Remove("infoPage"); return; }

            var players = (state["scoreboard"] as JObject)?["Players"] as JArray;
            if (players == null || players.Count == 0) { state.Remove("infoPage"); return; }

            double gameTime = state.Value<double?>("gameTime") ?? 0;
            string ver = DataDragon.version.Champion ?? "";

            var rows = new List<(JObject p, double val)>();
            foreach (var p in players.OfType<JObject>())
            {
                double val = gold
                    ? (p.Value<double?>("Gold") ?? 0)
                    : (gameTime > 0 ? (p.Value<double?>("CreepScore") ?? 0) / (gameTime / 60.0) : 0);
                rows.Add((p, val));
            }
            double least = rows.Min(r => r.val);
            double most = rows.Max(r => r.val);

            var tabs = new JArray();
            foreach (var (p, val) in rows.OrderByDescending(r => r.val))
            {
                double cur = gold ? Math.Round(val) : Math.Round(val, 1);
                tabs.Add(new JObject
                {
                    ["PlayerName"] = MockPlayerName(p),
                    ["IconPath"] = $"Cache\\{ver}\\champion\\{p.Value<string>("ChampionID")}_square.png",
                    ["Values"] = new JObject
                    {
                        ["MinValue"] = gold ? Math.Max(0, least - 100) : least,
                        ["MaxValue"] = most,
                        ["CurrentValue"] = cur
                    },
                    ["ExtraInfo"] = new JArray { gold ? ((int)cur).ToString() : cur.ToString("0.0"), gold ? "gold" : "cspm", p.Value<string>("Team") ?? "ORDER" }
                });
            }

            state["infoPage"] = new JObject
            {
                ["Title"] = gold ? "Player Gold" : "CS/min",
                ["Order"] = "MaxToMin",
                ["Players"] = tabs
            };
        }

        // Anonymize the fixture roster for the Mock preview: blue (ORDER) = Player1..5, red (CHAOS) =
        // Player6..10, numbered in TOP/JUNGLE/MIDDLE/BOTTOM/UTILITY order within each side. Hides the
        // real summoner names baked into mock-state.json.
        private static readonly string[] POS_ORDER = { "TOP", "JUNGLE", "MIDDLE", "BOTTOM", "UTILITY" };
        private static string MockPlayerName(JObject p)
        {
            int idx = Array.IndexOf(POS_ORDER, p.Value<string>("Position") ?? "");
            int n = (idx >= 0 ? idx : 0) + 1 + (p.Value<string>("Team") == "CHAOS" ? 5 : 0);
            return "Player" + n;
        }
    }
}
