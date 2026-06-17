using OpenForestUI.Common.Controllers;
using OpenForestUI.Common.Data.Provider;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenForestUI.Ingame.Data.Hub
{
    public class PlayerTab
    {
        public string PlayerName;
        public string IconPath;
        public ValueBar Values;
        public string[] ExtraInfo;

        // (GetEXPTabs removed: per-player XP is only observable via the memory reader;
        //  there is no Vanguard-safe source — see docs/feature-completion/DESIGN.md (b).)

        public static List<PlayerTab> GetGoldTabs()
        {
            var ret = new List<PlayerTab>();
            var gameTime = BroadcastController.Instance.IGController.gameData.gameTime;
            if (gameTime < 5)
                return ret;

            // Per-player gold from the item+score estimator (Team.EstimatePlayerGold).
            // Displayed as the player's gold (operator decision: estimate ≈ accurate
            // enough to present as-is; no memory reader available under Vanguard).
            var players = BroadcastController.Instance.IGController.gameState.GetAllPlayers();
            double leastGold = double.MaxValue;
            double mostGold = double.MinValue;
            players.ForEach(p =>
            {
                double gold = Team.EstimatePlayerGold(p, gameTime);
                if (gold > mostGold) mostGold = gold;
                if (gold < leastGold) leastGold = gold;
            });
            players.ForEach(p =>
            {
                int gold = (int)Math.Round(Team.EstimatePlayerGold(p, gameTime));
                ret.Add(new PlayerTab()
                {
                    PlayerName = p.summonerName,
                    IconPath = $"Cache\\{DataDragon.version.Champion}\\champion\\{p.championID}_square.png",
                    Values = new() { MinValue = Math.Max(0, leastGold - 100), MaxValue = mostGold, CurrentValue = gold },
                    ExtraInfo = new string[] { gold + "", "gold", p.team}
                });
            });

            return ret;
        }

        public static List<PlayerTab> GetCSPerMinTabs()
        {
            var ret = new List<PlayerTab>();
            if (BroadcastController.Instance.IGController.gameData.gameTime < 5)
                return ret;

            double leastCSperMin = 0;
            double mostCSperMin = 0;
            BroadcastController.Instance.IGController.gameState.GetAllPlayers().ForEach(p =>
            {
                var cspm = p.GetCSPerMinute();
                if (cspm > mostCSperMin)
                    mostCSperMin = cspm;
                if (cspm < mostCSperMin)
                    leastCSperMin = cspm;
            });
            BroadcastController.Instance.IGController.gameState.GetAllPlayers().ForEach(p =>
            {
                var cspm = p.GetCSPerMinute();
                ret.Add(new PlayerTab()
                {
                    PlayerName = p.summonerName,
                    IconPath = $"Cache\\{DataDragon.version.Champion}\\champion\\{p.championID}_square.png",
                    Values = new() { MinValue = leastCSperMin, MaxValue = mostCSperMin, CurrentValue = cspm },
                    ExtraInfo = new string[] { cspm + "", "cspm", p.team }
                });
            });

            return ret;
        }
    }

    public class ValueBar
    {
        public double MinValue;
        public double MaxValue;
        public double CurrentValue;
    }
}
