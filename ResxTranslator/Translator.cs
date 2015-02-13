using System;
using System.Diagnostics;
using System.Net;
using System.Net.Mime;
using System.Text;

namespace ResxTranslator
{
    /// <summary>
    /// Translator class responsible for translating the text.
    /// </summary>
    public static class Translator
    {
        /// <summary>
        /// Translates the given text in the given language.
        /// </summary>
        /// <param name="targetLanguage">Target language.</param>
        /// <param name="text">Text to be translated.</param>
        /// <returns>Translated value of the given text.</returns>
        public static string Translate(Language.Language targetLanguage, string text)
        {
            string translatedValue;
            try
            {
                var languagePair = string.Format("en|{0}", targetLanguage.Value);
                // Get the translated value.
                translatedValue = TranslateText(text, languagePair);
                Trace.WriteLine(string.Format("Given Text is {0} and Target Language is {1}. Result - {2}.",
                    text, targetLanguage.Name, translatedValue));
            }
            catch (Exception ex)
            {
                var errorString = "Exception while translating, please check the connectivity." + ex.Message;
                Trace.WriteLine(errorString);
                throw new WebException(errorString);
            }
            return translatedValue;
        }

        #region Get Translated Response from Google Translate Website
        /// <summary>
        /// Translate Text using Google Translate
        /// </summary>
        /// <param name="input">The string you want translated</param>
        /// <param name="languagePair">2 letter Language Pair, delimited by "|". 
        /// e.g. "en|da" language pair means to translate from English to Danish</param>
        /// <returns>Translated to String</returns>
        private static string TranslateText(string input, string languagePair)
        {
            var url = String.Format("http://www.google.com/translate_t?hl=en&ie=UTF8&text={0}&langpair={1}", input, languagePair);
            var webClient = new WebClient
            {
                Encoding = Encoding.UTF8
            };
            var result = webClient.DownloadStringUsingResponseEncoding(url);
            result = result.Substring(result.IndexOf("<span title=\"") + "<span title=\"".Length);
            result = result.Substring(result.IndexOf(">") + 1);
            result = result.Substring(0, result.IndexOf("</span>"));
            return result.Trim();
        }

        /// <summary>
        /// DownloadStringUsingResponseEncoding
        /// </summary>
        /// <param name="client"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        private static string DownloadStringUsingResponseEncoding(this WebClient client, string address)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }
            return DownloadStringUsingResponseEncoding(client, client.DownloadData(address));
        }

        /// <summary>
        /// DownloadStringUsingResponseEncoding
        /// </summary>
        /// <param name="client"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private static string DownloadStringUsingResponseEncoding(WebClient client, byte[] data)
        {
            Debug.Assert(client != null);
            Debug.Assert(data != null);

            var contentType = client.GetResponseContentType();

            var encoding = contentType == null || string.IsNullOrEmpty(contentType.CharSet)
                   ? client.Encoding
                   : Encoding.GetEncoding(contentType.CharSet);

            return encoding.GetString(data);
        }

        /// <summary>
        /// GetResponseContentType
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private static ContentType GetResponseContentType(this WebClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            var headers = client.ResponseHeaders;
            if (headers == null)
            {
                throw new InvalidOperationException("Response headers not available.");
            }

            var header = headers["Content-Type"];

            return !string.IsNullOrEmpty(header)
               ? new ContentType(header)
               : null;
        }
        #endregion
    }
}