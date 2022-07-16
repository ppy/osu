// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.IO.Network;
using osu.Game.Rulesets;
using osu.Game.Users;

namespace osu.Game.Online.API.Requests
{
    public class GetUserRankingsRequest : GetRankingsRequest<GetTopUsersResponse>
    {
        public readonly UserRankingsType Type;

        private readonly Country country;

        public GetUserRankingsRequest(RulesetInfo ruleset, UserRankingsType type = UserRankingsType.Performance, int page = 1, Country country = default)
            : base(ruleset, page)
        {
            Type = type;
            this.country = country;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            if (country != default)
                req.AddParameter("country", country.ToString());

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
