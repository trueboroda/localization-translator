using System.IO;
using LocalizationTranslator.Core.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LocalizationTranslator.BL.Services
{
    public class JsonFileService : IJsonFileService
    {
        public JObject OpenFile(string path)
        {
            using (StreamReader file = File.OpenText(path))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JObject o2 = (JObject)JToken.ReadFrom(reader);

                return o2;
            }
        }

        public void SaveFile(JObject obj, string path)
        {
            using (StreamWriter file = File.CreateText(path))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                writer.Formatting = Formatting.Indented;
                obj.WriteTo(writer);
            }
        }
    }
}
