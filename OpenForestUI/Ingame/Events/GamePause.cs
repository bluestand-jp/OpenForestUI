using OpenForestUI.Common.Events;

namespace OpenForestUI.Ingame.Events
{
    public class GamePause : LeagueEvent
    {
        public double GameTime;
        public GamePause(double gameTime)
        {
            this.eventType = "GamePause";
            this.GameTime = gameTime;
        }
    }
}
