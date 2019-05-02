using LocalizationTranslator.BL.Services;
using LocalizationTranslator.Core.Model;
using LocalizationTranslator.Core.Services;
using Xunit;

namespace LocalizationTranslator.Test
{
    public class MsAzureTranslationTests
    {
        [Fact]
        public void Translate_HellowWorld()
        {
            ITranslator translator = new MsAzureTranslator();

            var sourceText = "Hellow world";


            var traslationObjext = new TranslationObject()
            {
                SourceLngCode = "en",
                TargetLngCode = "ru",
                Text = sourceText

            };

            var result = translator.Translate(traslationObjext);

            Assert.Equal("Привет мир".ToLower(), result.ToLower());

        }
    }
}
