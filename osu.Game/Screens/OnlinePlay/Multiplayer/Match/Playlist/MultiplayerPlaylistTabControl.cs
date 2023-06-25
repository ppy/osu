// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match.Playlist
{
    public partial class MultiplayerPlaylistTabControl : OsuTabControl<MultiplayerPlaylistDisplayMode>
    {
        public readonly IBindableList<PlaylistItem> QueueItems = new BindableList<PlaylistItem>();

        protected override TabItem<MultiplayerPlaylistDisplayMode> CreateTabItem(MultiplayerPlaylistDisplayMode value)
        {
            if (value == MultiplayerPlaylistDisplayMode.Queue)
                return new QueueTabItem { QueueItems = { BindTarget = QueueItems } };

            return base.CreateTabItem(value);
        }

        private partial class QueueTabItem : OsuTabItem
        {
            public readonly IBindableList<PlaylistItem> QueueItems = new BindableList<PlaylistItem>();

            public QueueTabItem()
                : base(MultiplayerPlaylistDisplayMode.Queue)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                QueueItems.BindCollectionChanged((_, _) => Text.Text = QueueItems.Count > 0 ? $"Queue ({QueueItems.Count})" : "Queue", true);
            }
        }
    }
}
