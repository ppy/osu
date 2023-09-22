// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API;

namespace osu.Game.Online.Rooms
{
    public class ShowPlaylistUserScoreRequest : APIRequest<MultiplayerScore>
    {
        private readonly long roomId;
        private readonly long playlistItemId;
        private readonly long userId;

        public ShowPlaylistUserScoreRequest(long roomId, long playlistItemId, long userId)
        {
            this.roomId = roomId;
            this.playlistItemId = playlistItemId;
            this.userId = userId;
        }

        protected override string Target => $"rooms/{roomId}/playlist/{playlistItemId}/scores/users/{userId}";
    }
}
