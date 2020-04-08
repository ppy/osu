// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Screens.Multi.Components
{
    public class OverlinedPlaylist : OverlinedDisplay
    {
        public readonly Bindable<PlaylistItem> SelectedItem = new Bindable<PlaylistItem>();

        private readonly DrawableRoomPlaylist playlist;

        public OverlinedPlaylist(bool allowSelection)
            : base("Playlist")
        {
            Content.Add(playlist = new DrawableRoomPlaylist(false, allowSelection)
            {
                RelativeSizeAxes = Axes.Both,
                SelectedItem = { BindTarget = SelectedItem }
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            playlist.Items.BindTo(Playlist);
        }
    }
}
