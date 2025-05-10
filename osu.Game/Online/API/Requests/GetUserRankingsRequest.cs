// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Network;
using osu.Game.Rulesets;
using osu.Game.Users;

namespace osu.Game.Online.API.Requests
{
    public class GetUserRankingsRequest : GetRankingsRequest<GetTopUsersResponse>
    {
        public readonly UserRankingsType Type;

        private readonly CountryCode countryCode;

        public GetUserRankingsRequest(RulesetInfo ruleset, UserRankingsType type = UserRankingsType.Performance, int page = 1, CountryCode countryCode = CountryCode.Unknown)
            : base(ruleset, page)
        {
            Type = type;
            this.countryCode = countryCode;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            if (countryCode != CountryCode.Unknown)
                req.AddParameter("country", countryCode.ToString());

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
