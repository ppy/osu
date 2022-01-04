// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Network;
using osu.Game.Rulesets;

namespace osu.Game.Online.API.Requests
{
    public class GetUserRankingsRequest : GetRankingsRequest<GetTopUsersResponse>
    {
        public readonly UserRankingsType Type;

        private readonly string country;

        public GetUserRankingsRequest(RulesetInfo ruleset, UserRankingsType type = UserRankingsType.Performance, int page = 1, string country = null)
            : base(ruleset, page)
        {
            Type = type;
            this.country = country;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            if (country != null)
                req.AddParameter("country", country);

            return req;
        }

        protected override string TargetPostfix() => Type.ToString().ToLowerInvariant();
    }

    public enum UserRankingsType
    {
        Performance,
        Score
    }
}
