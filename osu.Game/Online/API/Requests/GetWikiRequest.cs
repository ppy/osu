// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetWikiRequest : APIRequest<APIWikiPage>
    {
        private readonly string path;
        private readonly string locale;

        public GetWikiRequest(string path, string locale = "en")
        {
            this.path = path;
            this.locale = locale;
        }

        protected override string Target => $"wiki/{locale}/{path}";
    }
}
