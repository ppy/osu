// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Scoring;

namespace osu.Game.Online.Rooms
{
    public class SubmitRoomScoreRequest : SubmitScoreRequest
    {
        private readonly long roomId;
        private readonly long playlistItemId;

        public SubmitRoomScoreRequest(ScoreInfo scoreInfo, long scoreId, long roomId, long playlistItemId)
            : base(scoreInfo, scoreId)
        {
            this.roomId = roomId;
            this.playlistItemId = playlistItemId;
        }

        protected override string Target => $@"rooms/{roomId}/playlist/{playlistItemId}/scores/{ScoreId}";
    }
}
