using OpenForestUI.Common.Events;

namespace OpenForestUI.ChampSelect.Events
{
    class ChampSelectEndEvent : LeagueEvent
    {
        public ChampSelectEndEvent()
        {
            eventType = "champSelectEnd";
        }
    }
}
