// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using Newtonsoft.Json.Linq;
using osu.Framework.IO.Network;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Online.API.Requests;

namespace osu.Game.Extensions
{
    public static class WebRequestExtensions
    {
        /// <summary>
        /// Add a pagination cursor to the web request in the format required by osu-web.
        /// </summary>
        public static void AddCursor(this WebRequest webRequest, Cursor cursor)
        {
            cursor?.Properties.ForEach(x =>
            {
                webRequest.AddParameter("cursor[" + x.Key + "]", (x.Value as JValue)?.ToString(CultureInfo.InvariantCulture) ?? x.Value.ToString());
            });
        }
    }
}
