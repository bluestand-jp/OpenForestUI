using OpenForestUI.Common.Events;

namespace OpenForestUI.ChampSelect.Events
{
    class ChampSelectStartEvent : LeagueEvent
    {
        public ChampSelectStartEvent()
        {
            eventType = "champSelectStart";
        }
    }
}
