// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Mods;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

namespace osu.Game.Online.API.Requests
{
    public class GetScoresRequest : APIRequest<APILegacyScores>
    {
        private readonly BeatmapInfo beatmapInfo;
        private readonly BeatmapLeaderboardScope scope;
        private readonly RulesetInfo ruleset;
        private readonly IEnumerable<IMod> mods;

        public GetScoresRequest(BeatmapInfo beatmapInfo, RulesetInfo ruleset, BeatmapLeaderboardScope scope = BeatmapLeaderboardScope.Global, IEnumerable<IMod> mods = null)
        {
            if (!beatmapInfo.OnlineBeatmapID.HasValue)
                throw new InvalidOperationException($"Cannot lookup a beatmap's scores without having a populated {nameof(BeatmapInfo.OnlineBeatmapID)}.");

            if (scope == BeatmapLeaderboardScope.Local)
                throw new InvalidOperationException("Should not attempt to request online scores for a local scoped leaderboard");

            this.beatmapInfo = beatmapInfo;
            this.scope = scope;
            this.ruleset = ruleset ?? throw new ArgumentNullException(nameof(ruleset));
            this.mods = mods ?? Array.Empty<IMod>();

            Success += onSuccess;
        }

        private void onSuccess(APILegacyScores r)
        {
            Debug.Assert(ruleset.ID != null, "ruleset.ID != null");

            foreach (APILegacyScoreInfo score in r.Scores)
            {
                score.BeatmapInfo = beatmapInfo;
                score.OnlineRulesetID = ruleset.ID.Value;
            }

            var userScore = r.UserScore;

            if (userScore != null)
            {
                userScore.Score.BeatmapInfo = beatmapInfo;
                userScore.Score.OnlineRulesetID = ruleset.ID.Value;
            }
        }

        protected override string Target => $@"beatmaps/{beatmapInfo.OnlineBeatmapID}/scores{createQueryParameters()}";

        private string createQueryParameters()
        {
            StringBuilder query = new StringBuilder(@"?");

            query.Append($@"type={scope.ToString().ToLowerInvariant()}");
            query.Append($@"&mode={ruleset.ShortName}");

            foreach (var mod in mods)
                query.Append($@"&mods[]={mod.Acronym}");

            return query.ToString();
        }
    }
}
