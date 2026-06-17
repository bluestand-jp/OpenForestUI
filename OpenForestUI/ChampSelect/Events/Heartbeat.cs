using OpenForestUI.ChampSelect.Data.Config;
using OpenForestUI.Common.Events;

namespace OpenForestUI.ChampSelect.Events
{
    class Heartbeat : LeagueEvent
    {
        public PickBanConfig config;
        public Heartbeat(PickBanConfig config)
        {
            this.eventType = "heartbeat";
            this.config = config;
        }
    }
}
