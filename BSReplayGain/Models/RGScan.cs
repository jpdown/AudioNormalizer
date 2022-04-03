using Newtonsoft.Json;

namespace BSReplayGain.Models {
    public struct RGScan {
        [JsonProperty("loudness")]
        public readonly float Loudness { get; }
        [JsonProperty("peak")]
        public readonly float Peak { get; }
        
        public RGScan(float loudness, float peak) {
            Loudness = loudness;
            Peak = peak;
        }
    }
}