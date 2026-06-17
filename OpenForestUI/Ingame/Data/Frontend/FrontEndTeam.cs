using OpenForestUI.Common.Controllers;
using OpenForestUI.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenForestUI.Ingame.Data.Frontend
{
    public class FrontEndTeam
    {
        private bool mapSide;

        #region TeamProperties
        public string Name;
        public string Icon;
        public int Score;
        #endregion

        #region Scoreboard
        public int Kills;
        public int Towers;
        public float Gold;
        public int PlatesDestroyed;
        public int VoidGrubs;
        public int Inhibitors;
        // Baron-takedown count and dragon count for the PRM top bar. These come from the top-bar
        // OCR (objective-monster kills aren't in the spectator/replay /eventdata). DragonCount is
        // separate from the Dragons list (which carries types for legacy pips and the rewind trim);
        // the PRM bar shows DragonCount so it works even when dragonsTaken is empty (no events).
        public int Baron;
        public int DragonCount;
        public List<string> Dragons { get { return mapSide ? BroadcastController.Instance.IGController.gameState.redTeam.dragonsTaken : BroadcastController.Instance.IGController.gameState.blueTeam.dragonsTaken; } }
        #endregion

        #region TeamMetadata (PRM top bar: region badge / seed / country flag)
        public string Region;
        public string Seed;
        public string Flag;
        #endregion

        public FrontEndTeam(string tag, bool mapSide)
        {
            this.mapSide = mapSide;

            this.Score = 0;
            this.Icon = TeamConfigViewModel.DefaultIconPath;
            this.Name = tag;

            this.Kills = 0;
            this.Towers = 0;
            this.Gold = 2500;
            this.PlatesDestroyed = 0;
        }

        #region SerializeConditions
        public bool ShouldSerializeIcon()
        {
            return IngameController.CurrentSettings.TeamIcons;
        }

        public bool ShouldSerializeScore()
        {
            return IngameController.CurrentSettings.TeamStats;
        }

        public bool ShouldSerializeName()
        {
            return IngameController.CurrentSettings.TeamNames;
        }

        public bool ShouldSerializeKills()
        {
            return ConfigController.Component.Ingame.UseCustomScoreboard;
        }

        public bool ShouldSerializeTowers()
        {
            return ConfigController.Component.Ingame.UseCustomScoreboard;
        }

        public bool ShouldSerializeGold()
        {
            return ConfigController.Component.Ingame.UseCustomScoreboard;
        }

        public bool ShouldSerializePlatesDestroyed()
        {
            return IngameController.CurrentSettings.TeamPlates;
        }

        public bool ShouldSerializeVoidGrubs()
        {
            return ConfigController.Component.Ingame.UseCustomScoreboard;
        }

        public bool ShouldSerializeInhibitors()
        {
            return ConfigController.Component.Ingame.UseCustomScoreboard;
        }

        public bool ShouldSerializeBaron()
        {
            return ConfigController.Component.Ingame.UseCustomScoreboard;
        }

        public bool ShouldSerializeDragonCount()
        {
            return ConfigController.Component.Ingame.UseCustomScoreboard;
        }

        public bool ShouldSerializeRegion()
        {
            return IngameController.CurrentSettings.TeamNames;
        }

        public bool ShouldSerializeSeed()
        {
            return IngameController.CurrentSettings.TeamNames;
        }

        public bool ShouldSerializeFlag()
        {
            return IngameController.CurrentSettings.TeamNames;
        }
        #endregion
    }
}
