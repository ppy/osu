// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.RealtimeMultiplayer;
using osu.Game.Screens.Multi.Components;
using osu.Game.Screens.Multi.Match;
using osu.Game.Screens.Multi.Match.Components;
using osu.Game.Screens.Multi.RealtimeMultiplayer.Match;
using osu.Game.Screens.Multi.RealtimeMultiplayer.Participants;
using osu.Game.Screens.Play;
using osu.Game.Users;

namespace osu.Game.Screens.Multi.RealtimeMultiplayer
{
    [Cached]
    public class RealtimeMatchSubScreen : RoomSubScreen
    {
        public override string Title { get; }

        public override string ShortTitle => "match";

        [Resolved(canBeNull: true)]
        private Multiplayer multiplayer { get; set; }

        [Resolved]
        private StatefulMultiplayerClient client { get; set; }

        private RealtimeMatchSettingsOverlay settingsOverlay;

        public RealtimeMatchSubScreen(Room room)
        {
            Title = room.RoomID.Value == null ? "New match" : room.Name.Value;
            Activity.Value = new UserActivity.InLobby(room);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding
                                {
                                    Horizontal = 105,
                                    Vertical = 20
                                },
                                Child = new GridContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    RowDimensions = new[]
                                    {
                                        new Dimension(GridSizeMode.AutoSize),
                                        new Dimension(),
                                    },
                                    Content = new[]
                                    {
                                        new Drawable[]
                                        {
                                            new RealtimeMatchHeader
                                            {
                                                OpenSettings = () => settingsOverlay.Show()
                                            }
                                        },
                                        new Drawable[]
                                        {
                                            new GridContainer
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Content = new[]
                                                {
                                                    new Drawable[]
                                                    {
                                                        new Container
                                                        {
                                                            RelativeSizeAxes = Axes.Both,
                                                            Padding = new MarginPadding { Horizontal = 5, Vertical = 10 },
                                                            Child = new GridContainer
                                                            {
                                                                RelativeSizeAxes = Axes.Both,
                                                                RowDimensions = new[]
                                                                {
                                                                    new Dimension(GridSizeMode.AutoSize)
                                                                },
                                                                Content = new[]
                                                                {
                                                                    new Drawable[] { new ParticipantsListHeader() },
                                                                    new Drawable[]
                                                                    {
                                                                        new Participants.ParticipantsList
                                                                        {
                                                                            RelativeSizeAxes = Axes.Both
                                                                        },
                                                                    }
                                                                }
                                                            }
                                                        },
                                                        new FillFlowContainer
                                                        {
                                                            Anchor = Anchor.Centre,
                                                            Origin = Anchor.Centre,
                                                            RelativeSizeAxes = Axes.X,
                                                            AutoSizeAxes = Axes.Y,
                                                            Padding = new MarginPadding { Horizontal = 5 },
                                                            Children = new Drawable[]
                                                            {
                                                                new OverlinedHeader("Beatmap"),
                                                                new BeatmapSelectionControl { RelativeSizeAxes = Axes.X }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        },
                                        new Drawable[]
                                        {
                                            new GridContainer
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                RowDimensions = new[]
                                                {
                                                    new Dimension(GridSizeMode.AutoSize)
                                                },
                                                Content = new[]
                                                {
                                                    new Drawable[] { new OverlinedHeader("Chat") },
                                                    new Drawable[] { new MatchChatDisplay { RelativeSizeAxes = Axes.Both } }
                                                }
                                            }
                                        }
                                    },
                                }
                            }
                        },
                        new Drawable[]
                        {
                            new RealtimeMatchFooter { SelectedItem = { BindTarget = SelectedItem } }
                        }
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.AutoSize),
                    }
                },
                settingsOverlay = new RealtimeMatchSettingsOverlay
                {
                    RelativeSizeAxes = Axes.Both,
                    State = { Value = client.Room == null ? Visibility.Visible : Visibility.Hidden }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Playlist.BindCollectionChanged(onPlaylistChanged, true);

            client.LoadRequested += onLoadRequested;
        }

        public override bool OnBackButton()
        {
            if (client.Room != null && settingsOverlay.State.Value == Visibility.Visible)
            {
                settingsOverlay.Hide();
                return true;
            }

            return base.OnBackButton();
        }

        private void onPlaylistChanged(object sender, NotifyCollectionChangedEventArgs e) => SelectedItem.Value = Playlist.FirstOrDefault();

        private void onLoadRequested() => multiplayer?.Push(new PlayerLoader(() => new RealtimePlayer(SelectedItem.Value)));

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client != null)
                client.LoadRequested -= onLoadRequested;
        }
    }
}
