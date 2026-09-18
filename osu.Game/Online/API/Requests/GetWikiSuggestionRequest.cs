// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Network;
using osu.Game.Extensions;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetWikiSuggestionRequest : APIRequest<APIWikiSuggestion[]>
    {
        private readonly string query;
        private readonly Language language;

        public GetWikiSuggestionRequest(string query, Language language = Language.en)
        {
            this.query = query;
            this.language = language;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.AddParameter(@"query", query);
            req.AddParameter(@"locale", language.ToCultureCode());

            return req;
        }

        protected override string Target => @"suggestions/wiki";
    }
}
