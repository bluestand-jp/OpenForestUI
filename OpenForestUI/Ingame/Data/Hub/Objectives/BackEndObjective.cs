namespace OpenForestUI.Ingame.Data.Hub.Objectives
{
    public class BackEndObjective
    {
        public float BlueStartGold = 0;
        public float RedStartGold = 0;

        public double DurationRemaining = -1;
        public double TakeGameTime = 0;

        public BackEndObjective(int gameTime)
        {
            TakeGameTime = gameTime;
        }
    }
}
