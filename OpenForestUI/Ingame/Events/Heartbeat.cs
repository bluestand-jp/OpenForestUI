using OpenForestUI.ChampSelect.Data.Config;
using OpenForestUI.Common.Controllers;
using OpenForestUI.Common.Events;
using OpenForestUI.Ingame.State;

namespace OpenForestUI.Ingame.Events
{
    public class HeartbeatEvent : LeagueEvent
    {
        public StateData stateData;
        //public FrontendConfig config;
        public HeartbeatEvent(StateData stateData)
        {
            this.eventType = "GameHeartbeat";
            this.stateData = stateData;
            //this.config = ConfigController.PickBan.frontend;
        }
    }
}
