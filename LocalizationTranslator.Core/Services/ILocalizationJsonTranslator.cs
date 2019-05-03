using Newtonsoft.Json.Linq;

namespace LocalizationTranslator.Core.Services
{
    public interface ILocalizationJsonTranslator
    {
        JObject Translate(JObject source, string from, string to);
    }
}
