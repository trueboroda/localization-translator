using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalizationTranslator.Core.Services

{

    /// <summary>
    /// translator
    /// </summary>
    public interface ITranslator
    {

        Dictionary<string, string> AvailableLanguages { get; }

        Task<string> DetectAsync(string input);

        Task<bool> IsTranslationServiceReadyAsync();
        bool IsTranslationServiceReady();


        string LanguageNameToLanguageCode(string languagename);

        string LanguageCodeToLanguageName(string languagecode);

        string TranslateString(string text, string from, string to, string contentType = "text/plain");

        string[] TranslateArray(string[] texts, string from, string to, string contentType = "text/plain");

        //string Translate(TranslationObject translationObject);



    }
}
