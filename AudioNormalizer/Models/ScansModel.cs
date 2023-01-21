using System.Collections.Generic;
using System.IO;
using System.Text;
using IPA.Utilities;
using Newtonsoft.Json;

namespace AudioNormalizer.Models
{
    public class ScansModel
    {
        private static readonly string ScanResultsDir = Path.Combine(UnityGame.UserDataPath, nameof(AudioNormalizer));
        private static readonly string ScanResultsPath = Path.Combine(ScanResultsDir, "scans.json");

        private const int CurrVersion = 2;

        [JsonProperty]
        private readonly int _version;
        [JsonProperty]
        private readonly Dictionary<string, Scan> _scans;

        public static ScansModel Load()
        {
            var model = JsonConvert.DeserializeObject<ScansModel>
              (File.ReadAllText(ScanResultsPath, Encoding.UTF8)) ?? new ScansModel();

            if (model._version != CurrVersion)
            {
                File.Delete(ScanResultsPath);
                model = new ScansModel();
            }

            return model;
        }

        public ScansModel()
        {
            _version = CurrVersion;
            _scans = new Dictionary<string, Scan>();
        }

        public void Save()
        {
            File.WriteAllText(ScanResultsPath, JsonConvert.SerializeObject(this), Encoding.UTF8);
        }

        public void Add(string levelId, float loudness, float peak)
        {
            _scans.Add(levelId, new Scan(loudness, peak));
        }

        public bool Contains(string levelId)
        {
            return _scans.ContainsKey(levelId);
        }

        public bool TryGet(string levelId, out Scan scan)
        {
            return _scans.TryGetValue(levelId, out scan);
        }

        public int Count()
        {
            return _scans.Count;
        }

        public void Remove(string levelId)
        {
            _scans.Remove(levelId);
        }
    }

    public class Scan
    {
        public readonly float Loudness;
        public readonly float Peak;

        public Scan(float loudness, float peak)
        {
            Loudness = loudness;
            Peak = peak;
        }
    }
}