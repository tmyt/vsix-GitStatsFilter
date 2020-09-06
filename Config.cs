using System.IO;
using Newtonsoft.Json;

namespace GitStatsFilter
{
    class Config
    {
        public string? TargetBranch { get; set; }

        public static Config Load(string rootDir)
        {
            var text = ReadString(ConfigPath(rootDir));
            return text == null ? new Config() : JsonConvert.DeserializeObject<Config>(text);
        }

        public void Save(string rootDir)
        {
            var text = JsonConvert.SerializeObject(this);
            WriteString(ConfigPath(rootDir), text);
        }

        private static string ConfigPath(string rootDir) => Path.Combine(rootDir, ".vs", ".branch.json");

        private static string? ReadString(string filename)
        {
            try
            {
                using var reader = new StreamReader(filename);
                return reader.ReadToEnd();
            }
            catch
            {
                return null;
            }
        }

        private static void WriteString(string filename, string value)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filename));
            }
            catch (IOException)
            {
                return;
            }
            using var writer = new StreamWriter(filename);
            writer.Write(value);
        }
    }
}