using LocalizationTranslator.Core.Services;
using Newtonsoft.Json.Linq;

namespace LocalizationTranslator.BL.Services
{
    public class LocalizationJsonTranslator : ILocalizationJsonTranslator
    {

        private readonly ITranslator _translator;

        public LocalizationJsonTranslator(ITranslator translator)
        {
            _translator = translator;
        }

        public JObject Translate(JObject source, string from, string to)
        {
            var result = (JObject)source.DeepClone();

            //foreach (var item in result.Children<JProperty>())
            //{
            //    TranslateNode(item, from, to);
            //}

            TranslateNode(result, from, to);

            return result;
        }


        private void TranslateNode(JToken node, string from, string to)
        {

            if (node.Type == JTokenType.Object || node.Type == JTokenType.Property)
            {
                foreach (var item in node.Children<JToken>())
                {
                    TranslateNode(item, from, to);
                }
            }
            else if (node.Type == JTokenType.String)
            {
                var sourceValue = node.Value<string>();
                var resultValue = _translator.TranslateString(sourceValue, from, to);

                var property = (JProperty)node.Parent;

                property.Value = resultValue;




            }

        }

    }
}
