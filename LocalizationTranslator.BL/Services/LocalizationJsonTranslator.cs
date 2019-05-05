using LocalizationTranslator.Core.Services;
using Newtonsoft.Json.Linq;

namespace LocalizationTranslator.BL.Services
{
    public class LocalizationJsonTranslator : ILocalizationJsonTranslator
    {

        private readonly ITranslator _translator;
        private readonly ITextPreprocessor _textPreprocessor;
        private readonly ITextPostprocessor _textPostprocessor;

        public LocalizationJsonTranslator(ITranslator translator, ITextPreprocessor textPreprocessor, ITextPostprocessor textPostprocessor)
        {
            _translator = translator;
            _textPreprocessor = textPreprocessor;
            _textPostprocessor = textPostprocessor;
        }

        public JObject Translate(JObject source, string from, string to)
        {
            var result = (JObject)source.DeepClone();

            TranslateNode(result, from, to);

            return result;
        }


        /// <summary>
        /// recursive go down through json tree and traslating string leaf
        /// </summary>
        /// <param name="node"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
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
                var processedValue = _textPreprocessor.PreprocessText(sourceValue);
                var resultValue = _translator.TranslateString(processedValue, from, to);
                var processedResult = _textPostprocessor.PostprocessText(resultValue);

                var property = (JProperty)node.Parent;

                property.Value = processedResult;

            }

        }

    }
}
