// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Network;
using osu.Game.Rulesets;

namespace osu.Game.Online.API.Requests
{
    public class GetSpotlightRankingsRequest : GetRankingsRequest<GetSpotlightRankingsResponse>
    {
        private readonly int spotlight;

        public GetSpotlightRankingsRequest(RulesetInfo ruleset, int spotlight)
            : base(ruleset, 1)
        {
            this.spotlight = spotlight;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.AddParameter("spotlight", spotlight.ToString());

            return req;
        }

        protected override string TargetPostfix() => "charts";
    }
}
