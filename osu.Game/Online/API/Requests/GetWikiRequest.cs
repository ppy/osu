// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Extensions;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetWikiRequest : APIRequest<APIWikiPage>
    {
        public readonly string Path;

        private readonly Language language;

        public GetWikiRequest(string path, Language language = Language.en)
        {
            Path = path;
            this.language = language;
        }

        protected override string Target => $"wiki/{language.ToCultureCode()}/{Path}";
    }
}
