using OpenForestUI.Ingame.Data.Hub;
using OpenForestUI.Ingame.Data.RIOT;
using System.Collections.Generic;
using System.Linq;

namespace OpenForestUI.Ingame.Data.Frontend
{
    /// <summary>
    /// One player's row in the PRM bottom comparison bar (Phase 2). All fields are EXACT
    /// from /liveclientdata/playerlist — except per-player gold, which this Vanguard-compatible
    /// fork can only estimate, so it is deliberately NOT included (strict-accuracy: the
    /// per-lane gold diff is hidden rather than approximated; team-total gold is OCR-exact and
    /// lives on the top bar). Icons are built frontend-side from DataDragon using ChampionID
    /// (the internal key, e.g. "Shen"/"MonkeyKing"), item IDs, and summoner-spell keys.
    /// </summary>
    public class PlayerScoreboardEntry
    {
        public string Team;          // "ORDER" | "CHAOS"
        public string Position;      // TOP/JUNGLE/MIDDLE/BOTTOM/UTILITY (raw from playerlist)
        public string Name;          // riotIdGameName
        public string ChampionID;    // DataDragon champion key (rawChampionName suffix)
        public string ChampionName;  // localized display name (fallback / tooltip)
        public int Level;
        public int Kills;
        public int Deaths;
        public int Assists;
        public int CreepScore;
        // Player gold from the item+score estimator (Team.EstimatePlayerGold). Shown as the
        // player's gold per operator decision (2026-06-12) — feeds the per-lane gold diff.
        public float Gold;
        public List<int> Items;      // item IDs, inventory-slot order (0..6); 0 = empty slot
        public List<string> Spells;  // two DataDragon summoner-spell keys (e.g. "SummonerFlash")

        public static PlayerScoreboardEntry From(Player p, double gameTime)
        {
            return new PlayerScoreboardEntry
            {
                Team = p.team,
                Position = p.position,
                Name = string.IsNullOrEmpty(p.riotIdGameName) ? p.summonerName : p.riotIdGameName,
                ChampionID = p.championID,
                ChampionName = p.championName,
                Level = p.level,
                Kills = p.scores?.kills ?? 0,
                Deaths = p.scores?.deaths ?? 0,
                Assists = p.scores?.assists ?? 0,
                CreepScore = p.scores?.creepScore ?? 0,
                Gold = Hub.Team.EstimatePlayerGold(p, gameTime),
                Items = BuildItems(p.items),
                Spells = new List<string>
                {
                    SpellKey(p.summonerSpells?.summonerSpellOne?.rawDisplayName),
                    SpellKey(p.summonerSpells?.summonerSpellTwo?.rawDisplayName),
                }
            };
        }

        // Inventory slots 0..6 in order; 0 for an empty slot so the frontend can lay out a fixed grid.
        private static List<int> BuildItems(IEnumerable<Item> items)
        {
            var byId = new int[7];
            if (items != null)
            {
                foreach (var it in items)
                {
                    if (it.slot >= 0 && it.slot < 7)
                        byId[it.slot] = it.itemID;
                }
            }
            return byId.ToList();
        }

        // "GeneratedTip_SummonerSpell_SummonerFlash_DisplayName" -> "SummonerFlash"
        // "GeneratedTip_SummonerSpell_S12_SummonerTeleportUpgrade_DisplayName" -> "S12_SummonerTeleportUpgrade"
        private static string SpellKey(string rawDisplayName)
        {
            if (string.IsNullOrEmpty(rawDisplayName))
                return "";
            const string prefix = "SummonerSpell_";
            const string suffix = "_DisplayName";
            int start = rawDisplayName.IndexOf(prefix);
            if (start < 0) return "";
            start += prefix.Length;
            int end = rawDisplayName.LastIndexOf(suffix);
            if (end <= start) end = rawDisplayName.Length;
            return rawDisplayName.Substring(start, end - start);
        }
    }
}
