// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match.Playlist
{
    public class MultiplayerPlaylist : MultiplayerRoomComposite
    {
        public readonly Bindable<MultiplayerPlaylistDisplayMode> DisplayMode = new Bindable<MultiplayerPlaylistDisplayMode>();

        private MultiplayerQueueList queueList;
        private MultiplayerHistoryList historyList;
        private bool firstPopulation = true;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new OsuTabControl<MultiplayerPlaylistDisplayMode>
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 25,
                    Current = { BindTarget = DisplayMode }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = 27 },
                    Masking = true,
                    Children = new Drawable[]
                    {
                        queueList = new MultiplayerQueueList
                        {
                            RelativeSizeAxes = Axes.Both,
                            SelectedItem = { BindTarget = SelectedItem }
                        },
                        historyList = new MultiplayerHistoryList
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            SelectedItem = { BindTarget = SelectedItem }
                        }
                    }
                }
            };
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

        protected override void OnRoomUpdated()
        {
            base.OnRoomUpdated();

            if (Room == null)
            {
                historyList.Items.Clear();
                queueList.Items.Clear();
                firstPopulation = true;
                return;
            }

            if (firstPopulation)
            {
                foreach (var item in Room.Playlist)
                    PlaylistItemAdded(item);

                firstPopulation = false;
            }
        }

        protected override void PlaylistItemAdded(MultiplayerPlaylistItem item)
        {
            base.PlaylistItemAdded(item);

            var apiItem = Playlist.Single(i => i.ID == item.ID);

            if (item.Expired)
                historyList.Items.Add(apiItem);
            else
                queueList.Items.Add(apiItem);
        }

        protected override void PlaylistItemRemoved(long item)
        {
            base.PlaylistItemRemoved(item);

            queueList.Items.RemoveAll(i => i.ID == item);
            historyList.Items.RemoveAll(i => i.ID == item);
        }

        protected override void PlaylistItemChanged(MultiplayerPlaylistItem item)
        {
            base.PlaylistItemChanged(item);

            PlaylistItemRemoved(item.ID);
            PlaylistItemAdded(item);
        }
    }
}
