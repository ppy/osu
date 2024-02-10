// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Network;
using osu.Game.Overlays.Rankings;
using osu.Game.Rulesets;

namespace osu.Game.Online.API.Requests
{
    public class GetSpotlightRankingsRequest : GetRankingsRequest<GetSpotlightRankingsResponse>
    {
        private readonly int spotlight;
        private readonly RankingsSortCriteria sort;

        public GetSpotlightRankingsRequest(RulesetInfo ruleset, int spotlight, RankingsSortCriteria sort)
            : base(ruleset)
        {
            this.spotlight = spotlight;
            this.sort = sort;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.AddParameter("spotlight", spotlight.ToString());
            req.AddParameter("filter", sort.ToString().ToLowerInvariant());

            return req;
        }

        protected override string TargetPostfix() => "charts";
    }
}
