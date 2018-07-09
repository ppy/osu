// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Screens.Select.Leaderboards;
using osu.Framework.IO.Network;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetScoresRequest : APIRequest<APIScores>
    {
        private readonly BeatmapInfo beatmap;
        private readonly LeaderboardScope scope;
        private readonly RulesetInfo ruleset;

        public GetScoresRequest(BeatmapInfo beatmap, RulesetInfo ruleset, LeaderboardScope scope = LeaderboardScope.Global)
        {
            if (!beatmap.OnlineBeatmapID.HasValue)
                throw new InvalidOperationException($"Cannot lookup a beatmap's scores without having a populated {nameof(BeatmapInfo.OnlineBeatmapID)}.");

            if (scope == LeaderboardScope.Local)
                throw new InvalidOperationException("Should not attempt to request online scores for a local scoped leaderboard");

            this.beatmap = beatmap;
            this.scope = scope;
            this.ruleset = ruleset ?? throw new ArgumentNullException(nameof(ruleset));

            Success += onSuccess;
        }

        private void onSuccess(APIScores r)
        {
            foreach (APIScore score in r.Scores)
                score.ApplyBeatmap(beatmap);
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
