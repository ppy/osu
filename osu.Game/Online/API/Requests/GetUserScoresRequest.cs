// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using osu.Framework.IO.Network;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;

namespace osu.Game.Online.API.Requests
{
    public class GetUserScoresRequest : PaginatedAPIRequest<List<SoloScoreInfo>>
    {
        public readonly long UserId;
        public readonly ScoreType Type;
        public readonly RulesetInfo Ruleset;

        public GetUserScoresRequest(long userId, ScoreType type, PaginationParameters pagination, RulesetInfo ruleset = null)
            : base(pagination)
        {
            UserId = userId;
            Type = type;
            Ruleset = ruleset;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            if (Ruleset != null)
                req.AddParameter("mode", Ruleset.ShortName);

            return req;
        }

        protected override string Target => $@"users/{UserId}/scores/{Type.ToString().ToLowerInvariant()}";
    }

    public enum ScoreType
    {
        Best,
        Firsts,
        Recent,
        Pinned
    }
}
