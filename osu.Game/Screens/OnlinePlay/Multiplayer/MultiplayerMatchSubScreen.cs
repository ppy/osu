// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Online;
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
using osu.Game.Screens.OnlinePlay.Multiplayer.Match.Playlist;
using osu.Game.Screens.OnlinePlay.Multiplayer.Participants;
using osu.Game.Screens.OnlinePlay.Multiplayer.Spectate;
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
        private MultiplayerClient client { get; set; }

        [Resolved]
        private OngoingOperationTracker ongoingOperationTracker { get; set; }

        private readonly IBindable<bool> isConnected = new Bindable<bool>();

        [CanBeNull]
        private IDisposable readyClickOperation;

        private AddItemButton addItemButton;

        public MultiplayerMatchSubScreen(Room room)
            : base(room)
        {
            Title = room.RoomID.Value == null ? "New room" : room.Name.Value;
            Activity.Value = new UserActivity.InLobby(room);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            BeatmapAvailability.BindValueChanged(updateBeatmapAvailability, true);
            UserMods.BindValueChanged(onUserModsChanged);

            client.LoadRequested += onLoadRequested;
            client.RoomUpdated += onRoomUpdated;

            isConnected.BindTo(client.IsConnected);
            isConnected.BindValueChanged(connected =>
            {
                if (!connected.NewValue)
                    handleRoomLost();
            }, true);
        }

        protected override Drawable CreateMainContent() => new Container
        {
            RelativeSizeAxes = Axes.Both,
            Padding = new MarginPadding { Horizontal = 5, Vertical = 10 },
            Child = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(),
                    new Dimension(GridSizeMode.Absolute, 10),
                    new Dimension(),
                    new Dimension(GridSizeMode.Absolute, 10),
                    new Dimension(),
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        // Participants column
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
                        // Beatmap column
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Content = new[]
                            {
                                new Drawable[] { new OverlinedHeader("Beatmap") },
                                new Drawable[]
                                {
                                    addItemButton = new AddItemButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = 40,
                                        Text = "Add item",
                                        Action = () => OpenSongSelection()
                                    },
                                },
                                null,
                                new Drawable[]
                                {
                                    new MultiplayerPlaylist
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        RequestEdit = item => OpenSongSelection(item.ID)
                                    }
                                },
                                new[]
                                {
                                    UserModsSection = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Margin = new MarginPadding { Top = 10 },
                                        Alpha = 0,
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
                                                    new UserModSelectButton
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
                                                        Current = UserMods,
                                                        Scale = new Vector2(0.8f),
                                                    },
                                                }
                                            },
                                        }
                                    },
                                },
                            },
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(GridSizeMode.Absolute, 5),
                                new Dimension(),
                                new Dimension(GridSizeMode.AutoSize),
                            }
                        },
                        // Spacer
                        null,
                        // Main right column
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Content = new[]
                            {
                                new Drawable[] { new OverlinedHeader("Chat") },
                                new Drawable[] { new MatchChatDisplay(Room) { RelativeSizeAxes = Axes.Both } }
                            },
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(),
                            }
                        },
                    }
                }
            }
        };

        /// <summary>
        /// Opens the song selection screen to add or edit an item.
        /// </summary>
        /// <param name="itemToEdit">An optional playlist item to edit. If null, a new item will be added instead.</param>
        internal void OpenSongSelection(long? itemToEdit = null)
        {
            if (!this.IsCurrentScreen())
                return;

            this.Push(new MultiplayerMatchSongSelect(Room, itemToEdit));
        }

        protected override Drawable CreateFooter() => new MultiplayerMatchFooter
        {
            OnReadyClick = onReadyClick,
            OnSpectateClick = onSpectateClick
        };

        protected override RoomSettingsOverlay CreateRoomSettingsOverlay(Room room) => new MultiplayerMatchSettingsOverlay(room);

        protected override void UpdateMods()
        {
            if (SelectedItem.Value == null || client.LocalUser == null || !this.IsCurrentScreen())
                return;

            // update local mods based on room's reported status for the local user (omitting the base call implementation).
            // this makes the server authoritative, and avoids the local user potentially setting mods that the server is not aware of (ie. if the match was started during the selection being changed).
            var ruleset = Ruleset.Value.CreateInstance();
            Mods.Value = client.LocalUser.Mods.Select(m => m.ToMod(ruleset)).Concat(SelectedItem.Value.RequiredMods).ToList();
        }

        [Resolved(canBeNull: true)]
        private DialogOverlay dialogOverlay { get; set; }

        private bool exitConfirmed;

        public override bool OnExiting(IScreen next)
        {
            // the room may not be left immediately after a disconnection due to async flow,
            // so checking the IsConnected status is also required.
            if (client.Room == null || !client.IsConnected.Value)
            {
                // room has not been created yet; exit immediately.
                return base.OnExiting(next);
            }

            if (!exitConfirmed && dialogOverlay != null)
            {
                if (dialogOverlay.CurrentDialog is ConfirmDialog confirmDialog)
                    confirmDialog.PerformOkAction();
                else
                {
                    dialogOverlay.Push(new ConfirmDialog("Are you sure you want to leave this multiplayer match?", () =>
                    {
                        exitConfirmed = true;
                        this.Exit();
                    }));
                }

                return true;
            }

            return base.OnExiting(next);
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

            if (availability.NewValue.State != DownloadState.LocallyAvailable)
            {
                // while this flow is handled server-side, this covers the edge case of the local user being in a ready state and then deleting the current beatmap.
                if (client.LocalUser?.State == MultiplayerUserState.Ready)
                    client.ChangeState(MultiplayerUserState.Idle);
            }
            else if (client.LocalUser?.State == MultiplayerUserState.Spectating
                     && (client.Room?.State == MultiplayerRoomState.WaitingForLoad || client.Room?.State == MultiplayerRoomState.Playing))
            {
                onLoadRequested();
            }
        }

        private void onReadyClick()
        {
            Debug.Assert(readyClickOperation == null);
            readyClickOperation = ongoingOperationTracker.BeginOperation();

            if (client.IsHost && (client.LocalUser?.State == MultiplayerUserState.Ready || client.LocalUser?.State == MultiplayerUserState.Spectating))
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

        private void onSpectateClick()
        {
            Debug.Assert(readyClickOperation == null);
            readyClickOperation = ongoingOperationTracker.BeginOperation();

            client.ToggleSpectate().ContinueWith(t => endOperation());

            void endOperation()
            {
                readyClickOperation?.Dispose();
                readyClickOperation = null;
            }
        }

        private void onRoomUpdated()
        {
            // may happen if the client is kicked or otherwise removed from the room.
            if (client.Room == null)
            {
                handleRoomLost();
                return;
            }

            updateCurrentItem();

            addItemButton.Alpha = client.IsHost || Room.QueueMode.Value != QueueMode.HostOnly ? 1 : 0;

            Scheduler.AddOnce(UpdateMods);
        }

        private void updateCurrentItem()
        {
            Debug.Assert(client.Room != null);

            var expectedSelectedItem = Room.Playlist.SingleOrDefault(i => i.ID == client.Room.Settings.PlaylistItemId);

            if (expectedSelectedItem == null)
                return;

            // There's no reason to renew the selected item if its content hasn't changed.
            if (SelectedItem.Value?.Equals(expectedSelectedItem) == true && expectedSelectedItem.Beatmap.Value != null)
                return;

            // Clear the selected item while the lookup is performed, so components like the ready button can enter their disabled states.
            SelectedItem.Value = null;

            if (expectedSelectedItem.Beatmap.Value == null)
            {
                Task.Run(async () =>
                {
                    var beatmap = await client.GetAPIBeatmap(expectedSelectedItem.BeatmapID).ConfigureAwait(false);

                    Schedule(() =>
                    {
                        expectedSelectedItem.Beatmap.Value = beatmap;

                        if (Room.Playlist.SingleOrDefault(i => i.ID == client.Room?.Settings.PlaylistItemId)?.Equals(expectedSelectedItem) == true)
                            applyCurrentItem();
                    });
                });
            }
            else
                applyCurrentItem();

            void applyCurrentItem() => SelectedItem.Value = expectedSelectedItem;
        }

        private void handleRoomLost() => Schedule(() =>
        {
            Logger.Log($"{this} exiting due to loss of room or connection");

            if (this.IsCurrentScreen())
                this.Exit();
            else
                ValidForResume = false;
        });

        private void onLoadRequested()
        {
            if (BeatmapAvailability.Value.State != DownloadState.LocallyAvailable)
                return;

            // In the case of spectating, IMultiplayerClient.LoadRequested can be fired while the game is still spectating a previous session.
            // For now, we want to game to switch to the new game so need to request exiting from the play screen.
            if (!ParentScreen.IsCurrentScreen())
            {
                ParentScreen.MakeCurrent();

                Schedule(onLoadRequested);
                return;
            }

            StartPlay();

            readyClickOperation?.Dispose();
            readyClickOperation = null;
        }

        protected override Screen CreateGameplayScreen()
        {
            Debug.Assert(client.LocalUser != null);
            Debug.Assert(client.Room != null);

            int[] userIds = client.CurrentMatchPlayingUserIds.ToArray();
            MultiplayerRoomUser[] users = userIds.Select(id => client.Room.Users.First(u => u.UserID == id)).ToArray();

            switch (client.LocalUser.State)
            {
                case MultiplayerUserState.Spectating:
                    return new MultiSpectatorScreen(users.Take(PlayerGrid.MAX_PLAYERS).ToArray());

                default:
                    return new MultiplayerPlayerLoader(() => new MultiplayerPlayer(Room, SelectedItem.Value, users));
            }
        }

        public void PresentBeatmap(WorkingBeatmap beatmap, RulesetInfo ruleset)
        {
            if (!this.IsCurrentScreen())
                return;

            if (client.Room == null)
                return;

            if (!client.IsHost)
            {
                // todo: should handle this when the request queue is implemented.
                // if we decide that the presentation should exit the user from the multiplayer game, the PresentBeatmap
                // flow may need to change to support an "unable to present" return value.
                return;
            }

            this.Push(new MultiplayerMatchSongSelect(Room, client.Room.Settings.PlaylistItemId, beatmap, ruleset));
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

        public class AddItemButton : PurpleTriangleButton
        {
        }
    }
}
