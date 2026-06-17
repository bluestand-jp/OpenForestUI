using System.Drawing;

namespace OpenForestUI.ChampSelect.Data.Config
{
    public class TeamConfig
    {
        public string name;
        public string nameTag;
        public int score;
        public string coach;
        public string color;
        // PRM top bar metadata (optional; default empty -> not shown).
        public string region;   // league badge text, e.g. "LFL" / "PRM"
        public string seed;     // standing/seed, e.g. "1" / "2"
        public string flag;     // ISO country code for the flag image, e.g. "fr" / "de"
        private Color _Color { get { return _Color; } set { _Color = value; color = RGBToString(value); } }

        public static string RGBToString(Color c)
        {
            return c.ToString().ToLower();
        }

        public static TeamConfig DefaultConfig(string TeamName, string c)
        {
            string nameTag = TeamName.Length >= 3 ? TeamName.Substring(0, 3) : "TeamName";
            return new TeamConfig() { name = TeamName, score = 0, coach = "G2 Grabz", color = c, nameTag = nameTag };
        }
    }
}
