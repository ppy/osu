// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.IO.Stores;

namespace osu.Game.Online
{
    /// <summary>
    /// An <see cref="OnlineStore"/> which proxies external media lookups through osu-web.
    /// </summary>
    public class OsuOnlineStore : OnlineStore
    {
        private readonly string apiEndpointUrl;

        public OsuOnlineStore(string apiEndpointUrl)
        {
            this.apiEndpointUrl = apiEndpointUrl;
        }

        protected override string GetLookupUrl(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) && uri.Host.EndsWith(@".ppy.sh", StringComparison.OrdinalIgnoreCase))
                return url;

            return $@"{apiEndpointUrl}/beatmapsets/discussions/media-url?url={url}";
        }
    }
}
