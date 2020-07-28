// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.IO.Network;
using osu.Game.Extensions;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// Returns a list of scores for the specified playlist item.
    /// </summary>
    public class IndexPlaylistScoresRequest : APIRequest<MultiplayerScores>
    {
        private readonly int roomId;
        private readonly int playlistItemId;
        private readonly Cursor cursor;
        private readonly IndexScoresParams indexParams;

        public IndexPlaylistScoresRequest(int roomId, int playlistItemId)
        {
            this.roomId = roomId;
            this.playlistItemId = playlistItemId;
        }

        public IndexPlaylistScoresRequest(int roomId, int playlistItemId, [NotNull] Cursor cursor, [NotNull] IndexScoresParams indexParams)
            : this(roomId, playlistItemId)
        {
            this.cursor = cursor;
            this.indexParams = indexParams;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            if (cursor != null)
            {
                req.AddCursor(cursor);

                foreach (var (key, value) in indexParams.Properties)
                    req.AddParameter(key, value.ToString());
            }

            return req;
        }

        protected override string Target => $@"rooms/{roomId}/playlist/{playlistItemId}/scores";
    }
}
