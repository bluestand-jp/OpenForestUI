using OpenForestUI.Common.Data.Provider;

namespace OpenForestUI.ChampSelect.Data.DTO
{
    public class Version
    {
        public string champion => DataDragon.version.Champion;
        public string item => DataDragon.version.Item;
    }
}
