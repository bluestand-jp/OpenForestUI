using OpenForestUI.Common.Data.RIOT;
using OpenForestUI.Common.Events;
using OpenForestUI.Ingame.Data.RIOT;

namespace OpenForestUI.Ingame.Events
{
    public class ItemCompleted : LeagueEvent
    {
        public int playerId;
        public ItemData itemData;

        public ItemCompleted(int playerId, ItemData itemData)
        {
            this.eventType = "ItemCompleted";
            this.playerId = playerId;
            this.itemData = itemData;
        }

    }
}
