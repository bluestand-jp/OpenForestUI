using OpenForestUI.ChampSelect.StateInfo;
using OpenForestUI.Common.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenForestUI.ChampSelect.Events
{
    public class NewState : LeagueEvent
    {
        public StateData state;
        public NewState(StateData State)
        {
            this.eventType = "newState";
            this.state = State;
        }
    }
}
