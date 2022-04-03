using Newtonsoft.Json;

namespace BSReplayGain.Models {
    public struct RGScan {
        [JsonProperty("gain")]
        public readonly float Gain { get; }
        [JsonProperty("peak")]
        public readonly float Peak { get; }
        
        public RGScan(float gain, float peak) {
            Gain = gain;
            Peak = peak;
        }
    }
}