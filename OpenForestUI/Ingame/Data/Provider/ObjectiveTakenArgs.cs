using OpenForestUI.Ingame.Data.Hub;

namespace OpenForestUI.Ingame.Data.Provider
{
    // Event args for objective-taken events (Dragon/Baron/Herald). Fired from the /eventdata
    // pipeline in State.cs and consumed by IngameController. (Formerly declared alongside the
    // now-removed LiveEventsDataProvider, whose LiveEvents API was dropped by Riot in patch 14.1.)
    public class ObjectiveTakenArgs
    {
        public string Type;
        public Team Team;
        public double GameTime;

        public ObjectiveTakenArgs(string Type, Team Team, double GameTime)
        {
            this.Type = Type;
            this.Team = Team;
            this.GameTime = GameTime;
        }
    }
}
