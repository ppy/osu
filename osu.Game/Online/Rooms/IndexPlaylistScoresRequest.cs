// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using JetBrains.Annotations;
using osu.Framework.IO.Network;
using osu.Game.Extensions;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;

namespace osu.Game.Online.Rooms
{
    /// <summary>
    /// Returns a list of scores for the specified playlist item.
    /// </summary>
    public class IndexPlaylistScoresRequest : APIRequest<IndexedMultiplayerScores>
    {
        public readonly long RoomId;
        public readonly long PlaylistItemId;

        [CanBeNull]
        public readonly Cursor Cursor;

        [CanBeNull]
        public readonly IndexScoresParams IndexParams;

        public IndexPlaylistScoresRequest(long roomId, long playlistItemId)
        {
            RoomId = roomId;
            PlaylistItemId = playlistItemId;
        }

        public IndexPlaylistScoresRequest(long roomId, long playlistItemId, [NotNull] Cursor cursor, [NotNull] IndexScoresParams indexParams)
            : this(roomId, playlistItemId)
        {
            Cursor = cursor;
            IndexParams = indexParams;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            if (Cursor != null)
            {
                Debug.Assert(IndexParams != null);

                req.AddCursor(Cursor);

                foreach ((string key, var value) in IndexParams.Properties)
                    req.AddParameter(key, value.ToString());
            }

            return req;
        }

        protected override string Target => $@"rooms/{RoomId}/playlist/{PlaylistItemId}/scores";
    }
}
