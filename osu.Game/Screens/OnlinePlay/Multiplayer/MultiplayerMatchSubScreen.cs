// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Rulesets;
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
    public class MultiplayerMatchSubScreen : RoomSubScreen, IHandlePresentBeatmap
    {
        public override string Title { get; }

        public override string ShortTitle => "room";

        [Resolved]
        private StatefulMultiplayerClient client { get; set; }

        [Resolved]
        private OngoingOperationTracker ongoingOperationTracker { get; set; }

        private MultiplayerMatchSettingsOverlay settingsOverlay;

        private readonly IBindable<bool> isConnected = new Bindable<bool>();

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
            AddRangeInternal(new Drawable[]
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
                                    Horizontal = HORIZONTAL_OVERFLOW_PADDING + 55,
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
                                            new Container
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Padding = new MarginPadding { Horizontal = 5, Vertical = 10 },
                                                Child = new GridContainer
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    ColumnDimensions = new[]
                                                    {
                                                        new Dimension(GridSizeMode.Relative, size: 0.5f, maxSize: 400),
                                                        new Dimension(),
                                                        new Dimension(GridSizeMode.Relative, size: 0.5f, maxSize: 600),
                                                    },
                                                    Content = new[]
                                                    {
                                                        new Drawable[]
                                                        {
                                                            // Main left column
                                                            new GridContainer
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
                                                            },
                                                            // Spacer
                                                            null,
                                                            // Main right column
                                                            new FillFlowContainer
                                                            {
                                                                RelativeSizeAxes = Axes.X,
                                                                AutoSizeAxes = Axes.Y,
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
                                                                    UserModsSection = new FillFlowContainer
                                                                    {
                                                                        RelativeSizeAxes = Axes.X,
                                                                        AutoSizeAxes = Axes.Y,
                                                                        Margin = new MarginPadding { Top = 10 },
                                                                        Children = new Drawable[]
                                                                        {
                                                                            new OverlinedHeader("Extra mods"),
                                                                            new FillFlowContainer
                                                                            {
                                                                                AutoSizeAxes = Axes.Both,
                                                                                Direction = FillDirection.Horizontal,
                                                                                Spacing = new Vector2(10, 0),
                                                                                Children = new Drawable[]
                                                                                {
                                                                                    new PurpleTriangleButton
                                                                                    {
                                                                                        Anchor = Anchor.CentreLeft,
                                                                                        Origin = Anchor.CentreLeft,
                                                                                        Width = 90,
                                                                                        Text = "Select",
                                                                                        Action = ShowUserModSelect,
                                                                                    },
                                                                                    new ModDisplay
                                                                                    {
                                                                                        Anchor = Anchor.CentreLeft,
                                                                                        Origin = Anchor.CentreLeft,
                                                                                        DisplayUnrankedText = false,
                                                                                        Current = UserMods,
                                                                                        Scale = new Vector2(0.8f),
                                                                                    },
                                                                                }
                                                                            }
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
            });

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

            SelectedItem.BindTo(client.CurrentMatchPlayingItem);

            BeatmapAvailability.BindValueChanged(updateBeatmapAvailability, true);
            UserMods.BindValueChanged(onUserModsChanged);

            client.LoadRequested += onLoadRequested;
            client.RoomUpdated += onRoomUpdated;

            isConnected.BindTo(client.IsConnected);
            isConnected.BindValueChanged(connected =>
            {
                if (!connected.NewValue)
                    Schedule(this.Exit);
            }, true);
        }

        protected override void UpdateMods()
        {
            if (SelectedItem.Value == null || client.LocalUser == null)
                return;

            // update local mods based on room's reported status for the local user (omitting the base call implementation).
            // this makes the server authoritative, and avoids the local user potentially setting mods that the server is not aware of (ie. if the match was started during the selection being changed).
            var ruleset = Ruleset.Value.CreateInstance();
            Mods.Value = client.LocalUser.Mods.Select(m => m.ToMod(ruleset)).Concat(SelectedItem.Value.RequiredMods).ToList();
        }

        [Resolved(canBeNull: true)]
        private DialogOverlay dialogOverlay { get; set; }

        private bool exitConfirmed;

        public override bool OnBackButton()
        {
            if (client.Room == null)
            {
                // room has not been created yet; exit immediately.
                return base.OnBackButton();
            }

            if (settingsOverlay.State.Value == Visibility.Visible)
            {
                settingsOverlay.Hide();
                return true;
            }

            if (!exitConfirmed && dialogOverlay != null)
            {
                dialogOverlay.Push(new ConfirmDialog("Are you sure you want to leave this multiplayer match?", () =>
                {
                    exitConfirmed = true;
                    this.Exit();
                }));

                return true;
            }

            return base.OnBackButton();
        }

        private ModSettingChangeTracker modSettingChangeTracker;
        private ScheduledDelegate debouncedModSettingsUpdate;

        private void onUserModsChanged(ValueChangedEvent<IReadOnlyList<Mod>> mods)
        {
            modSettingChangeTracker?.Dispose();

            if (client.Room == null)
                return;

            client.ChangeUserMods(mods.NewValue);

            modSettingChangeTracker = new ModSettingChangeTracker(mods.NewValue);
            modSettingChangeTracker.SettingChanged += onModSettingsChanged;
        }

        private void onModSettingsChanged(Mod mod)
        {
            // Debounce changes to mod settings so as to not thrash the network.
            debouncedModSettingsUpdate?.Cancel();
            debouncedModSettingsUpdate = Scheduler.AddDelayed(() =>
            {
                if (client.Room == null)
                    return;

                client.ChangeUserMods(UserMods.Value);
            }, 500);
        }

        private void updateBeatmapAvailability(ValueChangedEvent<BeatmapAvailability> availability)
        {
            if (client.Room == null)
                return;

            client.ChangeBeatmapAvailability(availability.NewValue);

            // while this flow is handled server-side, this covers the edge case of the local user being in a ready state and then deleting the current beatmap.
            if (availability.NewValue != Online.Rooms.BeatmapAvailability.LocallyAvailable()
                && client.LocalUser?.State == MultiplayerUserState.Ready)
                client.ChangeState(MultiplayerUserState.Idle);
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

        private void onRoomUpdated()
        {
            // user mods may have changed.
            Scheduler.AddOnce(UpdateMods);
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
            {
                client.RoomUpdated -= onRoomUpdated;
                client.LoadRequested -= onLoadRequested;
            }

            modSettingChangeTracker?.Dispose();
        }

        public void PresentBeatmap(WorkingBeatmap beatmap, RulesetInfo ruleset)
        {
            if (!this.IsCurrentScreen())
                return;

            if (!client.IsHost)
            {
                // todo: should handle this when the request queue is implemented.
                // if we decide that the presentation should exit the user from the multiplayer game, the PresentBeatmap
                // flow may need to change to support an "unable to present" return value.
                return;
            }

            this.Push(new MultiplayerMatchSongSelect(beatmap, ruleset));
        }
    }
}
