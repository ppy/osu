using osu.Framework.IO.Network;
using osu.Framework.Extensions.IEnumerableExtensions;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace osu.Game.Extensions
{
    public class Cursor
    {
        [JsonExtensionData]
        public IDictionary<string, JToken> Properties;
    }

    public static class WebRequestExtensions
    {
        public static void AddCursor(this WebRequest webRequest, Cursor cursor)
        {
            cursor?.Properties.ForEach(x =>
            {
                webRequest.AddParameter("cursor[" + x.Key + "]", x.Value.ToString());
            });
        }
    }
}
