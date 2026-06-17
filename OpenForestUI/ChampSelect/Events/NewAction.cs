using OpenForestUI.Common.Events;
using static OpenForestUI.ChampSelect.StateInfo.StateData;

namespace OpenForestUI.ChampSelect.Events
{
    class NewActionEvent : LeagueEvent
    {
        public CurrentAction action;
        public NewActionEvent(CurrentAction action)
        {
            eventType = "newAction";
            this.action = action;
        }
    }
}
