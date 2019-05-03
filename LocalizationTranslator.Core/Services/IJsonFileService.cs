using Newtonsoft.Json.Linq;

namespace LocalizationTranslator.Core.Services
{
    public interface IJsonFileService
    {
        JObject OpenFile(string path);

        void SaveFile(JObject obj, string path);
    }
}
