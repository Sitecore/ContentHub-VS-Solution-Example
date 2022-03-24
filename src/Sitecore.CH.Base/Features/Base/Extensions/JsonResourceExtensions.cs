using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sitecore.CH.Base.Features.Base.Extensions
{
    /// <summary>
    /// Collection of json related extensions, string and byte arrays handling
    /// </summary>
    public static class JsonResourceExtensions
    {
        /// <summary>
        /// Interprets <paramref name="bytes"/> as UTF-8 encoded
        /// string and returns <see cref="GetJson(string)"/>.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string GetJson(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes).GetJson();
        }

        /// <summary>
        /// Interprets <paramref name="bytes"/> as UTF-8 encoded
        /// string and returns <see cref="AsToken(string)"/>.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static JToken AsToken(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes).AsToken();
        }

        /// <summary>
        /// Gets instance of <see cref="JToken"/> from the json contained
        /// in the string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static JToken AsToken(this string value)
        {
            return JToken.Parse(value);
        }

        /// <summary>
        /// Gets the formatted (indented) string representation of the
        /// json contained in the string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetJson(this string value)
        {
            var json = AsToken(value);
            return json.ToString(Formatting.Indented);
        }
    }
}
