// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Scoring;

namespace osu.Game.Online.API.Requests
{
    public class DownloadReplayRequest : APIDownloadRequest
    {
        private readonly ScoreInfo score;

        public DownloadReplayRequest(ScoreInfo score)
        {
            this.score = score;
        }

        protected override string Target => $@"scores/{score.Ruleset.ShortName}/{score.OnlineScoreID}/download";
    }
}
