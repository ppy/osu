// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Screens.Select.Leaderboards;
using osu.Framework.IO.Network;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetScoresRequest : APIRequest<APILegacyScores>
    {
        private readonly BeatmapInfo beatmap;
        private readonly BeatmapLeaderboardScope scope;
        private readonly RulesetInfo ruleset;

        public GetScoresRequest(BeatmapInfo beatmap, RulesetInfo ruleset, BeatmapLeaderboardScope scope = BeatmapLeaderboardScope.Global)
        {
            if (!beatmap.OnlineBeatmapID.HasValue)
                throw new InvalidOperationException($"Cannot lookup a beatmap's scores without having a populated {nameof(BeatmapInfo.OnlineBeatmapID)}.");

            if (scope == BeatmapLeaderboardScope.Local)
                throw new InvalidOperationException("Should not attempt to request online scores for a local scoped leaderboard");

            this.beatmap = beatmap;
            this.scope = scope;
            this.ruleset = ruleset ?? throw new ArgumentNullException(nameof(ruleset));

            Success += onSuccess;
        }

        private void onSuccess(APILegacyScores r)
        {
            foreach (APILegacyScoreInfo score in r.Scores)
                score.Beatmap = beatmap;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.Timeout = 30000;
            req.AddParameter(@"type", scope.ToString().ToLowerInvariant());
            req.AddParameter(@"mode", ruleset.ShortName);

            return req;
        }

        protected override string Target => $@"beatmaps/{beatmap.OnlineBeatmapID}/scores";
    }
}
