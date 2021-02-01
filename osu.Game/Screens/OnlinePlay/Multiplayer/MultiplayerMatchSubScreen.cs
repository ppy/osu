// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Match;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;
using osu.Game.Screens.OnlinePlay.Multiplayer.Participants;
using osu.Game.Screens.Play.HUD;
using osu.Game.Users;
using osuTK;
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

        private ModSelectOverlay userModsSelectOverlay;
        private MultiplayerMatchSettingsOverlay settingsOverlay;
        private Drawable userModsSection;

        private IBindable<bool> isConnected;

        [CanBeNull]
        private IDisposable readyClickOperation;

        private GridContainer mainContent;

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
                mainContent = new GridContainer
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
                                                            Spacing = new Vector2(0, 10),
                                                            Children = new[]
                                                            {
                                                                new FillFlowContainer
                                                                {
                                                                    RelativeSizeAxes = Axes.X,
                                                                    AutoSizeAxes = Axes.Y,
                                                                    Children = new Drawable[]
                                                                    {
                                                                        new OverlinedHeader("Beatmap"),
                                                                        new BeatmapSelectionControl { RelativeSizeAxes = Axes.X }
                                                                    }
                                                                },
                                                                userModsSection = new FillFlowContainer
                                                                {
                                                                    RelativeSizeAxes = Axes.X,
                                                                    AutoSizeAxes = Axes.Y,
                                                                    Children = new Drawable[]
                                                                    {
                                                                        new OverlinedHeader("Extra mods"),
                                                                        new ModDisplay
                                                                        {
                                                                            DisplayUnrankedText = false,
                                                                            Current = UserMods
                                                                        },
                                                                        new PurpleTriangleButton
                                                                        {
                                                                            RelativeSizeAxes = Axes.X,
                                                                            Text = "Select",
                                                                            Action = () => userModsSelectOverlay.Show()
                                                                        }
                                                                    }
                                                                }
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
                new Container
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.5f,
                    Child = userModsSelectOverlay = new UserModSelectOverlay
                    {
                        SelectedMods = { BindTarget = UserMods },
                        IsValidMod = _ => false
                    }
                },
                settingsOverlay = new MultiplayerMatchSettingsOverlay
                {
                    RelativeSizeAxes = Axes.Both,
                    State = { Value = client.Room == null ? Visibility.Visible : Visibility.Hidden }
                }
            };

            if (client.Room == null)
            {
                // A new room is being created.
                // The main content should be hidden until the settings overlay is hidden, signaling the room is ready to be displayed.
                mainContent.Hide();

                settingsOverlay.State.BindValueChanged(visibility =>
                {
                    if (visibility.NewValue == Visibility.Hidden)
                        mainContent.Show();
                }, true);
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Playlist.BindCollectionChanged(onPlaylistChanged, true);
            UserMods.BindValueChanged(onUserModsChanged);

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

            if (userModsSelectOverlay.State.Value == Visibility.Visible)
            {
                userModsSelectOverlay.Hide();
                return true;
            }

            return base.OnBackButton();
        }

        private void onPlaylistChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SelectedItem.Value = Playlist.FirstOrDefault();

            if (SelectedItem.Value?.AllowedMods.Any() != true)
            {
                userModsSection.Hide();
                userModsSelectOverlay.Hide();
                userModsSelectOverlay.IsValidMod = _ => false;
            }
            else
            {
                userModsSection.Show();
                userModsSelectOverlay.IsValidMod = m => SelectedItem.Value.AllowedMods.Any(a => a.GetType() == m.GetType());
            }
        }

        private void onUserModsChanged(ValueChangedEvent<IReadOnlyList<Mod>> mods)
        {
            if (client.Room == null)
                return;

            client.ChangeUserMods(mods.NewValue).CatchUnobservedExceptions();
        }

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
                              // gameplay was not started due to an exception; unblock button.
                              endOperation();
                          }

                          // gameplay is starting, the button will be unblocked on load requested.
                      });
                return;
            }

            client.ToggleReady()
                  .ContinueWith(t => endOperation());

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

        private class UserModSelectOverlay : ModSelectOverlay
        {
            protected override bool AllowCustomisation => false;
        }
    }
}
