// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
        private readonly MultiplayerScoresSort? sort;

        public IndexPlaylistScoresRequest(int roomId, int playlistItemId, Cursor cursor = null, MultiplayerScoresSort? sort = null)
        {
            this.roomId = roomId;
            this.playlistItemId = playlistItemId;
            this.cursor = cursor;
            this.sort = sort;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.AddCursor(cursor);

            switch (sort)
            {
                case MultiplayerScoresSort.Ascending:
                    req.AddParameter("sort", "scores_asc");
                    break;

                case MultiplayerScoresSort.Descending:
                    req.AddParameter("sort", "scores_desc");
                    break;
            }

            return req;
        }

        protected override string Target => $@"rooms/{roomId}/playlist/{playlistItemId}/scores";
    }
}
