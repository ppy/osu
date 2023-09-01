// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.Rooms;
using osu.Game.Scoring;

namespace osu.Game.Online.Solo
{
    public class SubmitSoloScoreRequest : SubmitScoreRequest
    {
        private readonly int beatmapId;

        public SubmitSoloScoreRequest(ScoreInfo scoreInfo, ScoreToken scoreToken, int beatmapId)
            : base(scoreInfo, scoreToken.ID)
        {
            this.beatmapId = beatmapId;
        }

        protected override string Target => $@"beatmaps/{beatmapId}/solo/scores/{ScoreTokenId}";
    }
}
