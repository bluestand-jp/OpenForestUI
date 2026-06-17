using OpenForestUI.ChampSelect.Data.Config;
using OpenForestUI.Common;
using OpenForestUI.Common.Controllers;
using OpenForestUI.Common.Data.Config;
using OpenForestUI.MVVM.Core;
using OpenForestUI.MVVM.View;
using OpenForestUI.OperatingSystem;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace OpenForestUI.MVVM.ViewModel
{
    public class TeamConfigViewModel : ObservableObject
    {
        public TeamConfig ConfigReference;

        private string _name;
        private int _score;
        private string _coach;
        private Color _color;
        private SolidColorBrush _colorBrush;

        private string _iconName;
        private string _nameTag;
        private string _region;
        private string _seed;
        private string _flag;

        public string MapSide;
        public string Name { get { return _name; } set { _name = value; OnPropertyChanged(); } }
        public int Score { get { return _score; } set { _score = value; OnPropertyChanged(); } }
        public string Coach { get { return _coach; } set { _coach = value; OnPropertyChanged(); } }
        public Color Color { get { return _color; } set { _color = value; OnPropertyChanged(); UpdateColorConfig(); } }
        public SolidColorBrush ColorBrush { get { return _colorBrush; } set { _colorBrush = value; OnPropertyChanged(); } }
        public string IconName
        {
            get
            {
                return _iconName;
            }
            set
            {
                _iconName = value;
                OnPropertyChanged();
                OnPropertyChanged("IconNameFull");
                OnPropertyChanged("ShowIconReset");
            }
        }
        [JsonIgnore]
        public string IconNameFull { get { return Path.Combine(Directory.GetCurrentDirectory(), IconName); } }
        public bool ShowIconReset { get { return IconName != DefaultIconPath; } }
        public string NameTag { get { return _nameTag; } set { _nameTag = value; OnPropertyChanged(); } }
        // PRM top bar metadata (region badge / seed / country flag code).
        public string Region { get { return _region; } set { _region = value; OnPropertyChanged(); } }
        public string Seed { get { return _seed; } set { _seed = value; OnPropertyChanged(); } }
        public string Flag { get { return _flag; } set { _flag = value; OnPropertyChanged(); } }
        public ObservableCollection<string> Teams { get { return JSONConfigProvider.Instance.TeamConfigs; } }

        #region Static
        public static TeamConfigViewModel BlueTeam = new ();
        public static TeamConfigViewModel RedTeam = new ();
        public static string DefaultIconPath = Path.Combine("Cache", "TeamIcons", "Default.png");
        #endregion
        public TeamConfigViewModel(TeamConfig config)
        {
            this.ConfigReference = config;
        }

        public TeamConfigViewModel() { }

        public void Init(TeamConfig config, string mapSide)
        {
            this.ConfigReference = config;
            this.Name = ConfigReference.name;

            this.MapSide = mapSide;

            var cfgPath = Teams.SingleOrDefault(path => path.Equals(this.Name));
            if(cfgPath == null)
            {
                this.NameTag = ConfigReference.nameTag;
                this.Score = ConfigReference.score;
                this.Coach = ConfigReference.coach;
                this.IconName = DefaultIconPath;
                this.Region = ConfigReference.region;
                this.Seed = ConfigReference.seed;
                this.Flag = ConfigReference.flag;

            } else
            {
                var cfg = new ExtendedTeamConfig(this.Name);
                var res = JSONConfigProvider.Instance.ReadTeam(cfg);
                this.NameTag = cfg.Config.nameTag;
                this.Score = cfg.Config.score;
                this.Coach = cfg.Config.coach;
                this.IconName = cfg.IconLocation;
                this.Region = cfg.Config.region;
                this.Seed = cfg.Config.seed;
                this.Flag = cfg.Config.flag;
            }

            this.Color = mapSide == "blue" ? ConfigController.Component.PickBan.DefaultBlueColor.ToColor() : ConfigController.Component.PickBan.DefaultRedColor.ToColor();
        }

        private void UpdateColorConfig()
        {
            if (ConfigReference == null)
                return;

            ColorBrush = new SolidColorBrush(Color);
            ConfigReference.color = Color.ToSerializedString();

        }

    }
}
