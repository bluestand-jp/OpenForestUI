using OpenForestUI.Common.Events;

namespace OpenForestUI.Ingame.Events
{
    public class BuffDespawn : LeagueEvent
    {
        public string objective;
        public int teamId;
        public BuffDespawn(string objective, int team)
        {
            eventType = "BuffDespawn";
            this.objective = objective;
            teamId = team;
        }
    }
}
