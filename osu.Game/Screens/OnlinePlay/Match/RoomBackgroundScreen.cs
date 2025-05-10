// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;

namespace osu.Game.Screens.OnlinePlay.Match
{
    public partial class RoomBackgroundScreen : OnlinePlayBackgroundScreen
    {
        public readonly Bindable<PlaylistItem?> SelectedItem = new Bindable<PlaylistItem?>();

        public RoomBackgroundScreen(PlaylistItem? initialPlaylistItem)
        {
            PlaylistItem = initialPlaylistItem;
            SelectedItem.BindValueChanged(item => PlaylistItem = item.NewValue);
        }
    }
}
