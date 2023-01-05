// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Mvis.Plugin.CloudMusicSupport.Misc;
using osu.Game.Online.API;

namespace Mvis.Plugin.CloudMusicSupport.Helper
{
    public class APISearchRequest : OsuJsonWebRequest<APISearchResponseRoot>
    {
        public APISearchRequest(string target)
        {
            Url = $"https://music.163.com/api/search/get/web?hlpretag=&hlposttag=&s={target}&type=1&total=true&limit=1";
        }
    }
}
