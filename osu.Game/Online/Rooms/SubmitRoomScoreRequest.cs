// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Scoring;

namespace osu.Game.Online.Rooms
{
    public class SubmitRoomScoreRequest : SubmitScoreRequest
    {
        private readonly long roomId;
        private readonly long playlistItemId;

        public SubmitRoomScoreRequest(ScoreInfo scoreInfo, ScoreToken scoreToken, long roomId, long playlistItemId)
            : base(scoreInfo, scoreToken.ID)
        {
            if (scoreToken.Type != ScoreTokenType.Multiplayer)
                throw new ArgumentException($@"Invalid score token type supplied (expected {nameof(ScoreTokenType.Multiplayer)}, got {scoreToken.Type})", nameof(scoreToken));

            this.roomId = roomId;
            this.playlistItemId = playlistItemId;
        }

        protected override string Target => $@"rooms/{roomId}/playlist/{playlistItemId}/scores/{ScoreTokenId}";
    }
}
