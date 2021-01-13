// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Extensions;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Match;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;
using osu.Game.Screens.OnlinePlay.Multiplayer.Participants;
using osu.Game.Users;
using ParticipantsList = osu.Game.Screens.OnlinePlay.Multiplayer.Participants.ParticipantsList;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    [Cached]
    public class MultiplayerMatchSubScreen : RoomSubScreen
    {
        public override string Title { get; }

        public override string ShortTitle => "room";

        [Resolved]
        private StatefulMultiplayerClient client { get; set; }

        [Resolved]
        private OngoingOperationTracker ongoingOperationTracker { get; set; }

        private MultiplayerMatchSettingsOverlay settingsOverlay;

        private IBindable<bool> isConnected;

        [CanBeNull]
        private IDisposable readyClickOperation;

        public MultiplayerMatchSubScreen(Room room)
        {
            Title = room.RoomID.Value == null ? "New room" : room.Name.Value;
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
                                            new MultiplayerMatchHeader
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
                                                                        new ParticipantsList
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
                            new MultiplayerMatchFooter
                            {
                                SelectedItem = { BindTarget = SelectedItem },
                                OnReadyClick = onReadyClick
                            }
                        }
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.AutoSize),
                    }
                },
                settingsOverlay = new MultiplayerMatchSettingsOverlay
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

            isConnected = client.IsConnected.GetBoundCopy();
            isConnected.BindValueChanged(connected =>
            {
                if (!connected.NewValue)
                    Schedule(this.Exit);
            }, true);
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

        private void onReadyClick()
        {
            Debug.Assert(readyClickOperation == null);
            readyClickOperation = ongoingOperationTracker.BeginOperation();

            if (client.IsHost && client.LocalUser?.State == MultiplayerUserState.Ready)
            {
                client.StartMatch()
                      .ContinueWith(t =>
                      {
                          // accessing Exception here silences any potential errors from the antecedent task
                          if (t.Exception != null)
                          {
                              t.CatchUnobservedExceptions(true); // will run immediately.
                              // gameplay was not started due to an exception; unblock button.
                              endOperation();
                          }

                          // gameplay is starting, the button will be unblocked on load requested.
                      });
                return;
            }

            client.ToggleReady()
                  .ContinueWith(t =>
                  {
                      t.CatchUnobservedExceptions(true); // will run immediately.
                      endOperation();
                  });

            void endOperation()
            {
                readyClickOperation?.Dispose();
                readyClickOperation = null;
            }
        }

        private void onLoadRequested()
        {
            Debug.Assert(client.Room != null);

            int[] userIds = client.CurrentMatchPlayingUserIds.ToArray();

            StartPlay(() => new MultiplayerPlayer(SelectedItem.Value, userIds));

            readyClickOperation?.Dispose();
            readyClickOperation = null;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client != null)
                client.LoadRequested -= onLoadRequested;
        }
    }
}
