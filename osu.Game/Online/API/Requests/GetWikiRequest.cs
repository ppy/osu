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

        public readonly Language Language;

        public GetWikiRequest(string path, Language language = Language.en)
        {
            Path = path;
            Language = language;
        }

        protected override string Target => $"wiki/{Language.ToCultureCode()}/{Path}";
    }
}
