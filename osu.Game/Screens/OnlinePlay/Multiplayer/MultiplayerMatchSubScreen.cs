// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.Cursor;
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
    public partial class MultiplayerMatchSubScreen : RoomSubScreen, IHandlePresentBeatmap
    {
        public override string Title { get; }

        public override string ShortTitle => "room";

        [Resolved]
        private MultiplayerClient client { get; set; }

        [Resolved(canBeNull: true)]
        private OsuGame game { get; set; }

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

            if (!client.IsConnected.Value)
                handleRoomLost();
        }

        protected override bool IsConnected => base.IsConnected && client.IsConnected.Value;

        protected override Drawable CreateMainContent() => new Container
        {
            RelativeSizeAxes = Axes.Both,
            Padding = new MarginPadding { Horizontal = 5, Vertical = 10 },
            Child = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
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
                                            RequestEdit = OpenSongSelection
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
            }
        };

        /// <summary>
        /// Opens the song selection screen to add or edit an item.
        /// </summary>
        /// <param name="itemToEdit">An optional playlist item to edit. If null, a new item will be added instead.</param>
        internal void OpenSongSelection(PlaylistItem itemToEdit = null)
        {
            if (!this.IsCurrentScreen())
                return;

            this.Push(new MultiplayerMatchSongSelect(Room, itemToEdit));
        }

        protected override Drawable CreateFooter() => new MultiplayerMatchFooter();

        protected override RoomSettingsOverlay CreateRoomSettingsOverlay(Room room) => new MultiplayerMatchSettingsOverlay(room);

        protected override void UpdateMods()
        {
            if (SelectedItem.Value == null || client.LocalUser == null || !this.IsCurrentScreen())
                return;

            // update local mods based on room's reported status for the local user (omitting the base call implementation).
            // this makes the server authoritative, and avoids the local user potentially setting mods that the server is not aware of (ie. if the match was started during the selection being changed).
            var rulesetInstance = Rulesets.GetRuleset(SelectedItem.Value.RulesetID)?.CreateInstance();
            Debug.Assert(rulesetInstance != null);
            Mods.Value = client.LocalUser.Mods.Select(m => m.ToMod(rulesetInstance)).Concat(SelectedItem.Value.RequiredMods.Select(m => m.ToMod(rulesetInstance))).ToList();
        }

        [Resolved(canBeNull: true)]
        private IDialogOverlay dialogOverlay { get; set; }

        public override bool OnExiting(ScreenExitEvent e)
        {
            // room has not been created yet or we're offline; exit immediately.
            if (client.Room == null || !IsConnected)
                return base.OnExiting(e);

            if (dialogOverlay != null)
            {
                if (dialogOverlay.CurrentDialog is ConfirmDialog confirmDialog)
                    confirmDialog.PerformOkAction();
                else
                {
                    dialogOverlay.Push(new ConfirmDialog("Are you sure you want to leave this multiplayer match?", () =>
                    {
                        if (this.IsCurrentScreen())
                            this.Exit();
                    }));
                }

                return true;
            }

            return base.OnExiting(e);
        }

        private ModSettingChangeTracker modSettingChangeTracker;
        private ScheduledDelegate debouncedModSettingsUpdate;

        private void onUserModsChanged(ValueChangedEvent<IReadOnlyList<Mod>> mods)
        {
            modSettingChangeTracker?.Dispose();

            if (client.Room == null)
                return;

            client.ChangeUserMods(mods.NewValue).FireAndForget();

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

                client.ChangeUserMods(UserMods.Value).FireAndForget();
            }, 500);
        }

        private void updateBeatmapAvailability(ValueChangedEvent<BeatmapAvailability> availability)
        {
            if (client.Room == null)
                return;

            client.ChangeBeatmapAvailability(availability.NewValue).FireAndForget();

            switch (availability.NewValue.State)
            {
                case DownloadState.LocallyAvailable:
                    if (client.LocalUser?.State == MultiplayerUserState.Spectating
                        && (client.Room?.State == MultiplayerRoomState.WaitingForLoad || client.Room?.State == MultiplayerRoomState.Playing))
                    {
                        onLoadRequested();
                    }

                    break;

                case DownloadState.Unknown:
                    // Don't do anything rash in an unknown state.
                    break;

                default:
                    // while this flow is handled server-side, this covers the edge case of the local user being in a ready state and then deleting the current beatmap.
                    if (client.LocalUser?.State == MultiplayerUserState.Ready)
                        client.ChangeState(MultiplayerUserState.Idle);
                    break;
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

            addItemButton.Alpha = localUserCanAddItem ? 1 : 0;

            Scheduler.AddOnce(UpdateMods);

            Activity.Value = new UserActivity.InLobby(Room);
        }

        private bool localUserCanAddItem => client.IsHost || Room.QueueMode.Value != QueueMode.HostOnly;

        private void updateCurrentItem()
        {
            Debug.Assert(client.Room != null);
            SelectedItem.Value = Room.Playlist.SingleOrDefault(i => i.ID == client.Room.Settings.PlaylistItemId);
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
            // In the case of spectating, IMultiplayerClient.LoadRequested can be fired while the game is still spectating a previous session.
            // For now, we want to game to switch to the new game so need to request exiting from the play screen.
            if (!ParentScreen.IsCurrentScreen())
            {
                ParentScreen.MakeCurrent();

                Schedule(onLoadRequested);
                return;
            }

            // The beatmap is queried asynchronously when the selected item changes.
            // This is an issue with MultiSpectatorScreen which is effectively in an always "ready" state and receives LoadRequested() callbacks
            // even when it is not truly ready (i.e. the beatmap hasn't been selected by the client yet). For the time being, a simple fix to this is to ignore the callback.
            // Note that spectator will be entered automatically when the client is capable of doing so via beatmap availability callbacks (see: updateBeatmapAvailability()).
            if (client.LocalUser?.State == MultiplayerUserState.Spectating && (SelectedItem.Value == null || Beatmap.IsDefault))
                return;

            if (BeatmapAvailability.Value.State != DownloadState.LocallyAvailable)
                return;

            StartPlay();
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
                    return new MultiSpectatorScreen(Room, users.Take(PlayerGrid.MAX_PLAYERS).ToArray());

                default:
                    return new MultiplayerPlayerLoader(() => new MultiplayerPlayer(Room, SelectedItem.Value, users));
            }
        }

        public void PresentBeatmap(WorkingBeatmap beatmap, RulesetInfo ruleset)
        {
            if (!this.IsCurrentScreen())
                return;

            if (!localUserCanAddItem)
                return;

            // If there's only one playlist item and we are the host, assume we want to change it. Else add a new one.
            PlaylistItem itemToEdit = client.IsHost && Room.Playlist.Count == 1 ? Room.Playlist.Single() : null;

            OpenSongSelection(itemToEdit);

            // Re-run PresentBeatmap now that we've pushed a song select that can handle it.
            game?.PresentBeatmap(beatmap.BeatmapSetInfo, b => b.ID == beatmap.BeatmapInfo.ID);
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

        public partial class AddItemButton : PurpleRoundedButton
        {
        }
    }
}
