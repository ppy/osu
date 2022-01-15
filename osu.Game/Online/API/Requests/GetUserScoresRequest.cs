// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.IO.Network;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;

namespace osu.Game.Online.API.Requests
{
    public class GetUserScoresRequest : PaginatedAPIRequest<List<APIScore>>
    {
        private readonly long userId;
        private readonly ScoreType type;
        private readonly RulesetInfo ruleset;

        public GetUserScoresRequest(long userId, ScoreType type, int page = 0, int itemsPerPage = 5, RulesetInfo ruleset = null)
            : base(page, itemsPerPage)
        {
            this.userId = userId;
            this.type = type;
            this.ruleset = ruleset;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            if (ruleset != null)
                req.AddParameter("mode", ruleset.ShortName);

            return req;
        }

        protected override string Target => $@"users/{userId}/scores/{type.ToString().ToLowerInvariant()}";
    }

    public enum ScoreType
    {
        Best,
        Firsts,
        Recent
    }
}
