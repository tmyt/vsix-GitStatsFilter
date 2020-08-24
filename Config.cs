using System.IO;
using Newtonsoft.Json;

namespace BranchFilter
{
    static class Config
    {
        public static string TargetBranch { get; set; }

        public static void Load(string rootDir)
        {
            var text = ReadString(ConfigPath(rootDir));
            var context = text == null ? new Context() : JsonConvert.DeserializeObject<Context>(text);
            TargetBranch = context.TargetBranch;
        }

        public static void Save(string rootDir)
        {
            var context = new Context
            {
                TargetBranch = TargetBranch,
            };
            var text = JsonConvert.SerializeObject(context);
            using var writer = new StreamWriter(ConfigPath(rootDir));
            writer.Write(text);
        }

        private static string ConfigPath(string rootDir) => Path.Combine(rootDir, ".branch.json");

        private static string ReadString(string filename)
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

        class Context
        {
            public string TargetBranch { get; set; }
        }
    }
}