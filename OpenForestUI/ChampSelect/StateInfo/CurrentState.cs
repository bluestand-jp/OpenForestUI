using OpenForestUI.ChampSelect.Data.LCU;

namespace OpenForestUI.ChampSelect.StateInfo
{
    public class CurrentState
    {
        public bool isChampSelectActive;
        public Session session;

        public CurrentState(bool IsChampSelectActive, Session Session)
        {
            this.isChampSelectActive = IsChampSelectActive;
            this.session = Session;
        }
    }
}
