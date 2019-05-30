// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Scoring;
using System;

namespace osu.Game.Online.API.Requests
{
    class DownloadReplayRequest : APIDownloadRequest
    {
        private ScoreInfo score;

        public DownloadReplayRequest(ScoreInfo score)
        {
            this.score = score;
        }

        protected override String Target => $@"scores/{score.Ruleset.ShortName}/{score.OnlineScoreID}/download";
    }
}
