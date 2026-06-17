using OpenForestUI.Common;
using OpenForestUI.Common.Controllers;
using OpenForestUI.Ingame.Data.RIOT;
using OpenForestUI.MVVM.ViewModel;
using OpenForestUI.OperatingSystem;
using System.Collections.Generic;
using System.Linq;

namespace OpenForestUI.Ingame.Data.Hub
{
    // How the team's gold value was obtained. Drives frontend display:
    // Estimated = CS/kills heuristic (legacy default); Exact/Stale = OCR sidecar value;
    // Unknown = no trustworthy value -> frontend hides (strict-accuracy policy).
    public enum GoldConfidence { Estimated, Exact, Stale, Unknown }

    public class Team
    {
        public int id;
        public string teamName;
        public string color;

        public List<Player> players;
        public int towers;
        public int kills;
        public int platesDestroyed;
        // Void Grubs (Riot "Horde") this team has killed, and inhibitors this team
        // has destroyed. Counted from /eventdata (HordeKill / InhibKilled) like towers.
        public int voidgrubs;
        public int inhibsDestroyed;

        public bool hasBaron;
        public bool hasElder;

        public int baronTimer = 0;
        public int elderTimer = 0;

        public List<string> dragonsTaken;

        private double lastGoldCalculated = -1;
        private float lastGoldValue = 2500;

        // OCR-sourced authoritative team gold (Stage 3, set each tick by OcrGoldController).
        // When set, GetGold returns it verbatim instead of the estimate. Confidence is carried
        // to the frontend so it can hide the value when not trustworthy.
        public float? ExternalGold;
        public GoldConfidence Confidence = GoldConfidence.Estimated;

        // OCR-sourced objective counts (Stage 3b, set each tick by OcrGoldController.ApplyObjectives).
        // Objective-MONSTER kills (grub/baron/dragon) are never in the spectator/replay /eventdata,
        // so the top-bar HUD OCR is the only count source. null => fall back to the event-counted
        // value in State.UpdateScoreboard (OcrX ?? eventValue). Towers fall back to the TurretKilled
        // count; dragons to dragonsTaken.Count; grubs to voidgrubs; baron has no event count (->0).
        public int? OcrGrubs, OcrBaron, OcrDragons, OcrTowers;

        public Team(int teamId, List<Player> players)
        {
            this.id = teamId;
            this.teamName = (id == 0) ? "ORDER" : "CHAOS";
            this.players = players;
            this.color = (id == 0) ? TeamConfigViewModel.BlueTeam.Color.ToSerializedString() : TeamConfigViewModel.RedTeam.Color.ToSerializedString();
            towers = 0;
            platesDestroyed = 0;
            voidgrubs = 0;
            inhibsDestroyed = 0;
            hasBaron = false;
            hasElder = false;
            dragonsTaken = new List<string>();
        }

        public int GetDragonsTaken()
        {
            return dragonsTaken.Count;
        }

        public void UpdateIDs()
        {
            for (int i = 0; i < players.Count; i++)
            {
                var teamComponent = id == 0 ? 0 : 5;
                players.ElementAt(i).id = teamComponent + i;
            }
        }

        public float GetGold(double gameTime)
        {
            // OCR sidecar value takes precedence (bypasses the per-gameTime cache so a
            // mid-tick update is never masked). null => fall through to the estimate.
            if (ExternalGold.HasValue)
                return ExternalGold.Value;
            if (gameTime == lastGoldCalculated)
                return lastGoldValue;
            // Vanguard-compatible fork: /liveclientdata/playerlist does not expose
            // per-player totalGold, so we estimate from the fields it does expose
            // (scores: creepScore / kills / assists) plus the game's standard
            // passive income curve. Accurate enough for a broadcast-side
            // gold-difference indicator (~±5-10%); ignores shutdowns, streaks,
            // tower/objective gold, and item-sale refunds.
            lastGoldValue = players.Select(p => EstimatePlayerGold(p, gameTime)).Sum();
            lastGoldCalculated = gameTime;
            return lastGoldValue;
        }

        // Per-player gold estimate. Constants are patch-26 averages; tweak here
        // if Riot changes the income curve and the displayed gold drifts.
        private const float STARTING_GOLD = 500f;
        private const float PASSIVE_GOLD_START_TIME = 110f;   // seconds; 1:50
        private const float PASSIVE_GOLD_PER_SECOND = 2.0f;
        private const float GOLD_PER_CS = 21f;
        private const float GOLD_PER_KILL = 300f;
        private const float GOLD_PER_ASSIST = 150f;

        // Combine two signals to estimate per-player total gold earned:
        //
        // (1) Sum of equipped items' prices (itemsSpent). Exact and observable
        //     from /liveclientdata/playerlist; this is the lower bound of "gold
        //     earned so far" — you can't own items you didn't buy. Built-up
        //     items (Long Sword → BF Sword) count at their final price since
        //     components are consumed during build.
        //
        // (2) Score-based estimate (passive + CS + kills + assists). Smooths
        //     out the gap between purchases — items_sum jumps in chunks
        //     whereas the score-based signal grows continuously.
        //
        // total_gold = items_spent + cash_held, and cash_held ≥ 0. Whichever
        // signal is higher is the better lower bound; neither is added to the
        // other (that would double-count, since the score-based estimate
        // already represents earned = items + cash).
        //
        // Public: also feeds the per-player Gold Tab and the PRM bottom-bar lane
        // gold diff — displayed as the gold value (operator decision 2026-06-12:
        // the estimate is accurate enough to present as the player's gold).
        public static float EstimatePlayerGold(Player p, double gameTime)
        {
            if (p == null) return STARTING_GOLD;

            float itemsSpent = 0f;
            if (p.items != null)
            {
                foreach (var item in p.items)
                {
                    // Trinkets and starter wards have price 0; counts default
                    // to 1 for non-stackables.
                    itemsSpent += item.price * System.Math.Max(1, item.count);
                }
            }

            if (p.scores == null)
            {
                return System.Math.Max(STARTING_GOLD, itemsSpent);
            }

            float passive = (float)System.Math.Max(0, gameTime - PASSIVE_GOLD_START_TIME) * PASSIVE_GOLD_PER_SECOND;
            float estimatedEarned = STARTING_GOLD
                                  + passive
                                  + p.scores.creepScore * GOLD_PER_CS
                                  + p.scores.kills * GOLD_PER_KILL
                                  + p.scores.assists * GOLD_PER_ASSIST;

            return System.Math.Max(estimatedEarned, itemsSpent);
        }
    }
}
