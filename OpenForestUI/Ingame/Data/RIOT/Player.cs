using OpenForestUI.Common.Controllers;
using OpenForestUI.Farsight.Object;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenForestUI.Ingame.Data.RIOT
{
    public class Player
    {
        public int id;
        public string summonerName;
        // riotIdGameName / riotIdTagLine are exposed by /liveclientdata/playerlist
        // alongside summonerName (which is the full "Name#Tag" Riot ID). The
        // /liveclientdata/eventdata KillerName/VictimName fields use the bare
        // game-name form (no tag), so any code matching player to event must
        // compare against riotIdGameName, not summonerName.
        public string riotIdGameName;
        public string riotIdTagLine;
        public string championName;
        public string rawChampionName
        {
            set { championID = value.Split("_")[^1]; }
        }
        public string championID;
        public bool isDead;
        public IEnumerable<Item> items;
        public int level;
        public string position;
        public float respawnTimer;
        public RuneList runes;
        public Score scores;
        public SummonerList summonerSpells;
        public string team;

#nullable enable
        public GameObject? farsightObject;
#nullable disable


        public bool diedDuringBaron = false;
        public bool diedDuringElder = false;

        // Phase 3: tick-to-tick diff tracking for the Live Client Data API path
        // (replaces the snapshot-based comparison previously done against
        //  GameObject.Level / inventory). These are kept on the persistent Player
        // instance held in Team.players, not on the per-tick deserialized newP.
        //
        // LastLevel — the level we last *observed*, used to detect 6/11/16 milestones
        // crossing forward. Reset to current level on rewind (Phase 4).
        //
        // LastItemBySlot / LastCountBySlot — keyed by inventory slot (0..6). The
        // (itemID, slot) pair is the right key because /playerlist exposes raw
        // inventory including components (Long Sword, B.F. Sword, etc), so a simple
        // itemID set diff would miss build-up completions where one slot's item is
        // replaced by a more expensive one. Count tracking covers stackables
        // (Tear, Control Ward).
        public int LastLevel = 0;
        public Dictionary<int, int> LastItemBySlot = new();
        public Dictionary<int, int> LastCountBySlot = new();

        public Player()
        {
        }

        public float GetCSPerMinute()
        {
            return GetCSPerMinute(BroadcastController.Instance.IGController.gameData.gameTime);
        }

        public float GetCSPerMinute(double gameTime)
        {
            return (float)(scores.creepScore / (gameTime / 60));
        }

        public void UpdateInfo(Player p)
        {
            this.isDead = p.isDead;
            this.items = p.items;
            this.level = p.level;
            this.respawnTimer = p.respawnTimer;
            // CS always comes from /playerlist. The legacy LiveEvents API (port 34243),
            // which used to increment CS itself, was removed by Riot in patch 14.1.
            this.scores.Update(p.scores, true);
        }

        [ObsoleteAttribute("Does not work in custom games", true)]
        public void UpdateId(string position, int teamId)
        {
            int posId = 0;
            switch (position)
            {
                case "TOP":
                    posId = 0;
                    break;
                case "JUNGLE":
                    posId = 1;
                    break;
                case "MIDDLE":
                    posId = 2;
                    break;
                case "BOTTOM":
                    posId = 3;
                    break;
                case "UTILITY":
                    posId = 4;
                    break;
            }
            this.id = (teamId == 0 ? 0 : 5) + posId;
        }
    }
}
