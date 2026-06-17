using OpenForestUI.Common;
using OpenForestUI.Common.Controllers;
using System.Collections.Generic;

namespace OpenForestUI.Ingame.Data.Frontend
{
    public class ScoreboardConfig
    {
        public FrontEndTeam BlueTeam;
        public FrontEndTeam RedTeam;
        public double GameTime;
        public int SeriesGameCount;
        public string TournamentName;
        // PRM bottom comparison bar (Phase 2): full 10-player roster. Only serialized when the
        // custom scoreboard is on (the PRM overlay path).
        public List<PlayerScoreboardEntry> Players;

        public bool ShouldSerializeGameTime()
        {
            return ConfigController.Component.Ingame.UseCustomScoreboard;
        }

        public bool ShouldSerializeSeriesGameCount()
        {
            return IngameController.CurrentSettings.TeamStats;
        }

        public bool ShouldSerializeTournamentName()
        {
            return !string.IsNullOrEmpty(TournamentName);
        }

        public bool ShouldSerializePlayers()
        {
            return ConfigController.Component.Ingame.UseCustomScoreboard && Players != null && Players.Count > 0;
        }
    }
}
