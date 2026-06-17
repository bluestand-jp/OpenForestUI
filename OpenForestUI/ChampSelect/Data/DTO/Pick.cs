using OpenForestUI.Common.Data.DTO;

namespace OpenForestUI.ChampSelect.Data.DTO
{
    public class Pick : PickBan
    {
        public int id;
        public FrontEndSummonerSpell spell1;
        public FrontEndSummonerSpell spell2;
        public bool isActive = false;
        public string displayName = "";

        public Pick(int id)
        {
            this.id = id;
        }
    }
}
