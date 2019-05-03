using LocalizationTranslator.BL.Services;
using LocalizationTranslator.Core.Services;
using Xunit;

namespace LocalizationTranslator.Test
{
    public class MsAzureTranslationTests
    {
        private const string SUBS_KEY = "396204b01c604a87b9a02d019044d4cf";

        [Fact]
        public void Translate_HellowWorld()
        {

            ITranslator translator = new MsAzureTranslator(SUBS_KEY);

            var sourceText = "Hello world";
            var sourceLngCode = "en";
            var targetLngCode = "ru";

            var result = translator.TranslateString(sourceText, sourceLngCode, targetLngCode);

            Assert.NotNull(result);

            Assert.Equal("Всем привет".ToLower(), result.ToLower());

        }
    }
}
