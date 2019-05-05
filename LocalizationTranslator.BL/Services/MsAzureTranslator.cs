using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LocalizationTranslator.Core.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LocalizationTranslator.BL.Services
{
    public class MsAzureTranslator
        : ITranslator
    {

        private const int MillisecondsTimeout = 1000;
        private const string ApiHost = "https://api.cognitive.microsofttranslator.com";
        private const string TranslateRoute = "/translate?api-version=3.0";
        private const string DetectRoute = "/detect?api-version=3.0";
        private const string LngsRoute = "/languages?api-version=3.0&scope=translation";
        //private readonly Uri AuthServiceUrlPublic = new Uri("https://api.cognitive.microsoft.com/sts/v1.0/issueToken");


        //public int maxrequestsize = 5000;
        //public int maxelements = 100;


        private readonly string _subscriptionKey;


        //private enum AuthMode { Azure, AppId };
        //private readonly AuthMode authMode = AuthMode.Azure;

        private static List<string> autoDetectStrings = new List<string>() { "auto-detect", "detect" };

        private Dictionary<string, string> availableLanguages = new Dictionary<string, string>();

        public Dictionary<string, string> AvailableLanguages
        {
            get
            {

                if (availableLanguages.Count == 0)
                {
                    GetLanguages();
                }

                return availableLanguages;
            }
            private set => availableLanguages = value;
        }

        private string _categoryId;
        public string CategoryId { get => _categoryId; set => _categoryId = value; }






        // private readonly HttpClient _httpClient;

        public MsAzureTranslator(string subscriptionKey)
        {
            _subscriptionKey = subscriptionKey;
            // _httpClient = new HttpClient();
        }


        /// <summary>
        /// Detect the languages of the input
        /// </summary>
        /// <param name="input">Input string to detect the language of</param>
        /// <returns></returns>
        public async Task<string> DetectAsync(string input)
        {
            string uri = ApiHost + DetectRoute;
            string result = String.Empty;
            object[] body = new object[] { new { Text = input } };
            using (HttpClient client = new HttpClient())
            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                client.Timeout = System.TimeSpan.FromMilliseconds(1000);
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);
                string requestBody = JsonConvert.SerializeObject(body);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
                HttpResponseMessage response = await client.SendAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    result = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    result = null;
                }

                return result;
            }
        }


        /// <summary>
        /// Check if the Translation service is ready to use, with a valid Azure key
        /// </summary>
        /// <returns>true if ready, false if not</returns>
        public async Task<bool> IsTranslationServiceReadyAsync()
        {
            try
            {
                string detectedlanguage = await DetectAsync("Test");
                if (detectedlanguage == null) return false;
                return true;
            }
            catch
            {
                return false;
            }
        }



        /// <summary>
        /// Test whether the translation servoce credentials are valid by performing a simple function and checking whether it succeeds.
        /// Synchronous version of IsTranslationServceReadyAsync().
        /// </summary>
        /// <returns>Whether the translation servoce is ready to translate</returns>
        public bool IsTranslationServiceReady()
        {
            Task<bool> task = Task.Run<bool>(async () => await IsTranslationServiceReadyAsync());
            return task.Result;
        }



        private void GetLanguages()
        {
            availableLanguages.Clear();
            string uri = ApiHost + LngsRoute;
            using (HttpClient client = new HttpClient())
            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(uri);
                HttpResponseMessage response = client.SendAsync(request).Result;
                string jsonResponse = response.Content.ReadAsStringAsync().Result;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(jsonResponse);
                    var languages = result["translation"];

                    //string[] languagecodes = languages.Keys.ToArray();
                    foreach (var kv in languages)
                    {
                        availableLanguages.Add(kv.Key, kv.Value["name"]);
                    }
                }
            }
        }




        /// <summary>
        /// Test if a given category value is a valid category in the system.
        /// Works across V2 and V3 of the API.
        /// </summary>
        /// <param name="category">Category ID</param>
        /// <returns>True if the category is valid</returns>
        public bool IsCategoryValid(string category)
        {
            if (category == "") return true;
            if (category == string.Empty) return true;
            if (category.ToLower() == "general") return true;
            if (category.ToLower() == "generalnn") return true;
            if (category.ToLower() == "tech") return true;

            Task<bool> testV3 = IsCategoryValidV3Async(category);
            return testV3.Result;
        }

        private async Task<bool> IsCategoryValidV3Async(string category)
        {
            bool returnvalue = true;

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    string[] teststring = { "Test" };
                    Task<string[]> translateTask = TranslateV3Async(teststring, "en", "he", category, "text/plain");
                    await translateTask.ConfigureAwait(false);
                    returnvalue = translateTask.Result != null;
                    break;
                }
                catch (Exception e)
                {
                    returnvalue = false;
                    Thread.Sleep(1000);
                    continue;
                }
            }
            return returnvalue;
        }

        /// <summary>
        /// Takes a single language name and returns the matching language code. OK to pass a language code.
        /// </summary>
        /// <param name="languagename"></param>
        /// <returns></returns>
        public string LanguageNameToLanguageCode(string languagename)
        {
            if (AvailableLanguages.ContainsKey(languagename))
            {
                return languagename;
            }
            else if (AvailableLanguages.ContainsValue(languagename))
            {
                return AvailableLanguages.First(t => t.Value == languagename).Key;
            }
            else
            {
                throw new ArgumentException(String.Format("LanguageNameToLanguageCode: Language name {0} not found.", languagename));
            }
        }

        public string LanguageCodeToLanguageName(string languagecode)
        {
            if (AvailableLanguages.ContainsValue(languagecode))
            {
                return languagecode;
            }
            else if (AvailableLanguages.ContainsKey(languagecode))
            {
                return AvailableLanguages.First(t => t.Key == languagecode).Value;
            }
            else
            {
                throw new ArgumentException(String.Format("LanguageCodeToLanguageName: Language code {0} not found.", languagecode));
            }
        }


        /// <summary>
        /// Translates a string
        /// </summary>
        /// <param name="text">String to translate</param>
        /// <param name="from">From language</param>
        /// <param name="to">To language</param>
        /// <param name="contentType">Content Type</param>
        /// <returns></returns>
        public string TranslateString(string text, string from, string to, string contentType = "plain")
        {
            string[] texts = new string[1];
            texts[0] = text;
            string[] results = TranslateArray(texts, from, to, contentType);
            return results[0];
        }


        /// <summary>
        /// Translates an array of strings from the from langauge code to the to language code.
        /// From langauge code can stay empty, in that case the source language is auto-detected, across all elements of the array together.
        /// </summary>
        /// <param name="texts">Array of strings to translate</param>
        /// <param name="from">From language code. May be empty</param>
        /// <param name="to">To language code. Must be a valid language</param>
        /// <param name="contentType">text/plan or text/html depending on the type of string</param>
        /// <returns></returns>
        public string[] TranslateArray(string[] texts, string from, string to, string contentType = "plain")
        {
            string fromCode = string.Empty;
            string toCode = string.Empty;

            if (autoDetectStrings.Contains(from.ToLower(CultureInfo.InvariantCulture)) || from == string.Empty)
            {
                fromCode = string.Empty;
            }
            else
            {
                try { fromCode = AvailableLanguages.First(t => t.Value == from).Key; }
                catch { fromCode = from; }
            }

            toCode = LanguageNameToLanguageCode(to);

            string[] result = TranslateV3Async(texts, fromCode, toCode, _categoryId, contentType).Result;
            return result;

        }


        private async Task<string[]> TranslateV3Async(string[] texts, string from, string to, string category, string contentType, int retrycount = 3)
        {

            string @params = "&from=" + from + "&to=" + to;
            string thiscategory = category;
            if (String.IsNullOrEmpty(category))
            {
                thiscategory = null;
            }
            else
            {
                if (thiscategory == "generalnn") thiscategory = null;
                if (thiscategory == "general") thiscategory = null;
            }
            if (thiscategory != null) @params += "&category=" + System.Web.HttpUtility.UrlEncode(category);
            if (!string.IsNullOrEmpty(contentType))
            {
                @params += "&textType=" + contentType;
            }
            string uri = ApiHost + TranslateRoute + @params;



            ArrayList requestAL = new ArrayList();
            foreach (string text in texts)
            {
                requestAL.Add(new { Text = text });
            }
            string requestJson = JsonConvert.SerializeObject(requestAL);

            IList<string> resultList = new List<string>();
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                client.Timeout = TimeSpan.FromSeconds(10);
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);
                request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
                var response = await client.SendAsync(request).ConfigureAwait(false);
                var responseBody = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    int status = (int)response.StatusCode;
                    switch (status)
                    {
                        case 429:
                        case 500:
                        case 503:
                            if (texts.Length > 1)
                            {
                                for (int i = 0; i < texts.Length; i++)
                                {
                                    try
                                    {
                                        string[] totranslate = new string[1];
                                        totranslate[0] = texts[i];
                                        string[] result = new string[1];
                                        result = await TranslateV3Async(totranslate, from, to, category, contentType, 2);
                                        resultList.Add(result[0]);
                                    }
                                    catch
                                    {
                                        System.Diagnostics.Debug.WriteLine("Failed to translate: {0}\n", texts[i]);
                                        resultList.Add(texts[i]);
                                    }
                                }
                                return resultList.ToArray();
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("Retry #" + retrycount + " Response: " + (int)response.StatusCode);
                                Thread.Sleep(MillisecondsTimeout);
                                if (retrycount-- <= 0) break;
                                else await TranslateV3Async(texts, from, to, category, null, retrycount);
                                break;
                            }
                        default:
                            var errorstring = "ERROR " + response.StatusCode + "\n" + JsonConvert.SerializeObject(JsonConvert.DeserializeObject(responseBody), Formatting.Indented);
                            Exception ex = new Exception(errorstring);
                            throw ex;
                    }
                }
                JArray jaresult;
                try
                {
                    jaresult = JArray.Parse(responseBody);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(responseBody);
                    throw ex;
                }
                foreach (JObject result in jaresult)
                {
                    string txt = (string)result.SelectToken("translations[0].text");
                    resultList.Add(txt);
                }
            }
            return resultList.ToArray();
        }



        //public string Translate(TranslationObject translationObject)
        //{


        //    System.Object[] body = new System.Object[] { new { Text = translationObject.Text } };
        //    var requestBody = JsonConvert.SerializeObject(body);

        //    using (var request = new HttpRequestMessage())
        //    {
        //        // Set the method to POST
        //        request.Method = HttpMethod.Post;
        //        // Construct the full URI
        //        request.RequestUri = new Uri(host + route);
        //        // Add the serialized JSON object to your request
        //        request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        //        // Add the authorization header
        //        request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
        //        // Send request, get response
        //        var response = _httpClient.SendAsync(request).Result;
        //        var jsonResponse = response.Content.ReadAsStringAsync().Result;
        //        // Print the response
        //        Console.WriteLine(jsonResponse);
        //        Console.WriteLine("Press any key to continue.");
        //    }
        //}




        //private bool _isDisposed = false;


        //public void Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}


        //private void Dispose(bool disposing)
        //{
        //    if (disposing && !_isDisposed)
        //    {
        //        _httpClient.Dispose();
        //        _isDisposed = true;
        //    }
        //}



    }
}
