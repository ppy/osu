// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
