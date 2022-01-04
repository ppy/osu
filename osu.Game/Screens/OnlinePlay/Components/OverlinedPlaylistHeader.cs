// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public class OverlinedPlaylistHeader : OverlinedHeader
    {
        public OverlinedPlaylistHeader()
            : base("Playlist")
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Playlist.BindCollectionChanged((_, __) => Details.Value = Playlist.GetTotalDuration(), true);
        }
    }
}
