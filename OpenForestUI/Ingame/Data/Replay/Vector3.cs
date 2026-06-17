
using System.Text.Json.Serialization;

namespace OpenForestUI.Ingame.Data.Replay
{
    public class Vector3
    {
        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }

        [JsonPropertyName("z")]
        public double Z { get; set; }
    }
}
