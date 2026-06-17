using OpenForestUI.Common.Data.Provider;

namespace OpenForestUI.ChampSelect.Data.DTO
{
    public class Meta
    {
        public string cdn => DataDragon.version.CDN;
        public Version version = new Version();
    }
}
