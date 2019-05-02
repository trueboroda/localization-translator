using LocalizationTranslator.Core.Model;

namespace LocalizationTranslator.Core.Services

{

    /// <summary>
    /// translator
    /// </summary>
    public interface ITranslator
    {

        string Translate(TranslationObject translationObject);

    }
}
