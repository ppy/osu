// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match.Playlist
{
    /// <summary>
    /// The multiplayer playlist, containing lists to show the items from a <see cref="MultiplayerRoom"/> in both gameplay-order and historical-order.
    /// </summary>
    public partial class MultiplayerPlaylist : CompositeDrawable
    {
        public readonly Bindable<MultiplayerPlaylistDisplayMode> DisplayMode = new Bindable<MultiplayerPlaylistDisplayMode>();

        /// <summary>
        /// Invoked when an item requests to be edited.
        /// </summary>
        public Action<PlaylistItem>? RequestEdit;

        private MultiplayerPlaylistTabControl playlistTabControl = null!;
        private MultiplayerQueueList queueList = null!;
        private MultiplayerHistoryList historyList = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            const float tab_control_height = 25;

            InternalChildren = new Drawable[]
            {
                playlistTabControl = new MultiplayerPlaylistTabControl
                {
                    RelativeSizeAxes = Axes.X,
                    Height = tab_control_height,
                    Current = { BindTarget = DisplayMode }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = tab_control_height + 5 },
                    Masking = true,
                    Children = new Drawable[]
                    {
                        queueList = new MultiplayerQueueList
                        {
                            RelativeSizeAxes = Axes.Both,
                            RequestEdit = item => RequestEdit?.Invoke(item)
                        },
                        historyList = new MultiplayerHistoryList
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                        }
                    }
                }
            };

            playlistTabControl.QueueItems.BindTarget = queueList.Items;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            DisplayMode.BindValueChanged(onDisplayModeChanged, true);
        }

        private void onDisplayModeChanged(ValueChangedEvent<MultiplayerPlaylistDisplayMode> mode)
        {
            historyList.FadeTo(mode.NewValue == MultiplayerPlaylistDisplayMode.History ? 1 : 0, 100);
            queueList.FadeTo(mode.NewValue == MultiplayerPlaylistDisplayMode.Queue ? 1 : 0, 100);
        }
    }
}
