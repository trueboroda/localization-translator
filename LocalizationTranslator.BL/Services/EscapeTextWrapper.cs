using System.Text.RegularExpressions;

namespace LocalizationTranslator.Core.Services
{
    public class EscapeTextWrapper : ITextPreprocessor, ITextPostprocessor
    {
        public string PostprocessText(string value)
        {
            string pattern = @"\<span class=""notranslate""\>(?<escaped>[^\<\>]*)\</span\>";
            string replacement = " ${escaped} ";
            string result = Regex.Replace(value, pattern, replacement);
            return result;
        }

        public string PreprocessText(string value)
        {

            string pattern = @"(?<param>\{\{[^\{\}]*\}\})";
            string replacement = "<span class=\"notranslate\">${param}</span>";
            string result = Regex.Replace(value, pattern, replacement);
            return result;

        }




    }
}
