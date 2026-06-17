using OpenForestUI.Common.Controllers;

namespace OpenForestUI.MVVM.Core.Services
{
    /// <summary>
    /// Forwards to the static <c>ConfigController</c>. The config objects are plain POCOs (no
    /// INotifyPropertyChanged), so callers raise their own change notifications; this service only
    /// owns read/write + persistence.
    /// </summary>
    internal class ConfigService : IConfigService
    {
        public bool PickBanActive
        {
            get => ConfigController.Component.PickBan.IsActive;
            set => ConfigController.Component.PickBan.IsActive = value;
        }

        public bool IngameActive
        {
            get => ConfigController.Component.Ingame.IsActive;
            set => ConfigController.Component.Ingame.IsActive = value;
        }

        public bool PostGameActive
        {
            get => ConfigController.Component.PostGame.IsActive;
            set => ConfigController.Component.PostGame.IsActive = value;
        }

        public void Save() => ConfigController.UpdateConfigFile(ConfigController.Component);
    }
}
