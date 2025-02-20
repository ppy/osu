// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Cursor;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Rulesets;
using osu.Game.Screens.Menu;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Match;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match.Playlist;
using osu.Game.Screens.OnlinePlay.Multiplayer.Participants;
using osu.Game.Screens.OnlinePlay.Multiplayer.Spectate;
using osu.Game.Users;
using osu.Game.Utils;
using osuTK;
using Container = osu.Framework.Graphics.Containers.Container;
using ParticipantsList = osu.Game.Screens.OnlinePlay.Multiplayer.Participants.ParticipantsList;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    [Cached(typeof(IMultiplayerMatchScreen))]
    public class MultiplayerMatchSubScreen2 : OnlinePlaySubScreen, IPreviewTrackOwner, IMultiplayerMatchScreen, IHandlePresentBeatmap
    {
        /// <summary>
        /// Footer height.
        /// </summary>
        private const float footer_height = 50;

        /// <summary>
        /// Padding between content and footer.
        /// </summary>
        private const float footer_padding = 30;

        /// <summary>
        /// Internal padding of the content.
        /// </summary>
        private const float content_padding = 20;

        /// <summary>
        /// Padding between columns of the content.
        /// </summary>
        private const float column_padding = 10;

        /// <summary>
        /// Padding between rows of the content.
        /// </summary>
        private const float row_padding = 10;

        public override string Title { get; }

        public override string ShortTitle => "room";

        public override bool? ApplyModTrackAdjustments => true;

        public override bool DisallowExternalBeatmapRulesetChanges => true;

        /// <summary>
        /// Whether the user has confirmed they want to exit this screen in the presence of unsaved changes.
        /// </summary>
        protected bool ExitConfirmed { get; private set; }

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private AudioManager audio { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; } = null!;

        [Resolved]
        private MusicController music { get; set; } = null!;

        [Resolved]
        private OnlinePlayScreen? parentScreen { get; set; }

        [Resolved]
        private IOverlayManager? overlayManager { get; set; }

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private OsuGame? game { get; set; }

        [Cached]
        private readonly OnlinePlayBeatmapAvailabilityTracker beatmapAvailabilityTracker = new OnlinePlayBeatmapAvailabilityTracker();

        /// <summary>
        /// Describes the current playlist item to be used for the next gameplay session.
        /// </summary>
        private readonly Bindable<PlaylistItem?> currentItem = new Bindable<PlaylistItem?>();

        private readonly Room room;

        private Drawable roomContent = null!;
        private MultiplayerMatchSettingsOverlay settingsOverlay = null!;

        private FillFlowContainer userModsSection = null!;
        private MultiplayerUserModSelectOverlay userModsSelectOverlay = null!;

        private FillFlowContainer userStyleSection = null!;
        private Container<DrawableRoomPlaylistItem> userStyleDisplayContainer = null!;

        private Sample? sampleStart;
        private IDisposable? userModsSelectOverlayRegistration;

        public MultiplayerMatchSubScreen2(Room room)
        {
            this.room = room;

            Title = room.RoomID == null ? "New playlist" : room.Name;
            Activity.Value = new UserActivity.InLobby(room);

            Padding = new MarginPadding { Top = Header.HEIGHT };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            sampleStart = audio.Samples.Get(@"SongSelect/confirm-selection");

            InternalChild = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    beatmapAvailabilityTracker,
                    new MultiplayerRoomSounds(),
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding
                        {
                            Horizontal = WaveOverlayContainer.WIDTH_PADDING,
                            Bottom = footer_height + footer_padding
                        },
                        Children = new[]
                        {
                            roomContent = new GridContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                RowDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.AutoSize),
                                    new Dimension(GridSizeMode.Absolute, row_padding),
                                },
                                Content = new[]
                                {
                                    new Drawable[]
                                    {
                                        new DrawableMatchRoom(room, true)
                                        {
                                            OnEdit = () => settingsOverlay.Show(),
                                            SelectedItem = currentItem
                                        }
                                    },
                                    null,
                                    new Drawable[]
                                    {
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Masking = true,
                                            CornerRadius = 10,
                                            Children = new Drawable[]
                                            {
                                                new Box
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Colour = Color4Extensions.FromHex(@"3e3a44") // Temporary.
                                                },
                                                new GridContainer
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Padding = new MarginPadding(content_padding),
                                                    ColumnDimensions = new[]
                                                    {
                                                        new Dimension(),
                                                        new Dimension(GridSizeMode.Absolute, column_padding),
                                                        new Dimension(),
                                                        new Dimension(GridSizeMode.Absolute, column_padding),
                                                        new Dimension(),
                                                    },
                                                    Content = new[]
                                                    {
                                                        new Drawable?[]
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
                                                                    new Drawable[]
                                                                    {
                                                                        new ParticipantsListHeader()
                                                                    },
                                                                    new Drawable[]
                                                                    {
                                                                        new ParticipantsList
                                                                        {
                                                                            RelativeSizeAxes = Axes.Both
                                                                        },
                                                                    }
                                                                }
                                                            },
                                                            null,
                                                            new GridContainer
                                                            {
                                                                RelativeSizeAxes = Axes.Both,
                                                                RowDimensions = new[]
                                                                {
                                                                    new Dimension(GridSizeMode.AutoSize),
                                                                    new Dimension(GridSizeMode.AutoSize),
                                                                    new Dimension(GridSizeMode.Absolute, 5),
                                                                    new Dimension(),
                                                                    new Dimension(GridSizeMode.AutoSize),
                                                                    new Dimension(GridSizeMode.AutoSize),
                                                                },
                                                                Content = new[]
                                                                {
                                                                    new Drawable[]
                                                                    {
                                                                        new OverlinedHeader("Beatmap queue")
                                                                    },
                                                                    new Drawable[]
                                                                    {
                                                                        new AddItemButton
                                                                        {
                                                                            RelativeSizeAxes = Axes.X,
                                                                            Height = 40,
                                                                            Text = "Add item",
                                                                            Action = () => showSongSelect()
                                                                        },
                                                                    },
                                                                    null,
                                                                    new Drawable[]
                                                                    {
                                                                        new MultiplayerPlaylist(room)
                                                                        {
                                                                            RelativeSizeAxes = Axes.Both,
                                                                            RequestEdit = showSongSelect,
                                                                            SelectedItem = currentItem
                                                                        }
                                                                    },
                                                                    new Drawable[]
                                                                    {
                                                                        userModsSection = new FillFlowContainer
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
                                                                                            Height = 30,
                                                                                            Text = "Select",
                                                                                            Action = showUserModSelect,
                                                                                        },
                                                                                        new MultiplayerUserModDisplay
                                                                                        {
                                                                                            Anchor = Anchor.CentreLeft,
                                                                                            Origin = Anchor.CentreLeft,
                                                                                            Scale = new Vector2(0.8f),
                                                                                        },
                                                                                    }
                                                                                },
                                                                            }
                                                                        }
                                                                    },
                                                                    new Drawable[]
                                                                    {
                                                                        userStyleSection = new FillFlowContainer
                                                                        {
                                                                            RelativeSizeAxes = Axes.X,
                                                                            AutoSizeAxes = Axes.Y,
                                                                            Margin = new MarginPadding { Top = 10 },
                                                                            Alpha = 0,
                                                                            Children = new Drawable[]
                                                                            {
                                                                                new OverlinedHeader("Difficulty"),
                                                                                userStyleDisplayContainer = new Container<DrawableRoomPlaylistItem>
                                                                                {
                                                                                    RelativeSizeAxes = Axes.X,
                                                                                    AutoSizeAxes = Axes.Y
                                                                                }
                                                                            }
                                                                        },
                                                                    },
                                                                },
                                                            },
                                                            null,
                                                            new GridContainer
                                                            {
                                                                RelativeSizeAxes = Axes.Both,
                                                                RowDimensions = new[]
                                                                {
                                                                    new Dimension(GridSizeMode.AutoSize)
                                                                },
                                                                Content = new[]
                                                                {
                                                                    new Drawable[]
                                                                    {
                                                                        new OverlinedHeader("Chat")
                                                                    },
                                                                    new Drawable[]
                                                                    {
                                                                        new MatchChatDisplay(room)
                                                                        {
                                                                            RelativeSizeAxes = Axes.Both
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
                                }
                            },
                            settingsOverlay = new MultiplayerMatchSettingsOverlay(room)
                            {
                                SelectedItem = currentItem
                            }
                        }
                    },
                    new Container
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        RelativeSizeAxes = Axes.X,
                        Height = footer_height,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4Extensions.FromHex(@"28242d") // Temporary.
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding(5),
                                Child = new MultiplayerMatchFooter
                                {
                                    SelectedItem = currentItem
                                }
                            }
                        }
                    }
                }
            };

            LoadComponent(userModsSelectOverlay = new MultiplayerUserModSelectOverlay());
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            userModsSelectOverlayRegistration = overlayManager?.RegisterBlockingOverlay(userModsSelectOverlay);

            room.PropertyChanged += onRoomPropertyChanged;
            client.RoomUpdated += onRoomUpdated;
            client.LoadRequested += onLoadRequested;

            beatmapAvailabilityTracker.SelectedItem.BindTo(currentItem);
            beatmapAvailabilityTracker.Availability.BindValueChanged(onBeatmapAvailabilityChanged);

            currentItem.BindValueChanged(onCurrentItemChanged);

            updateSetupState();
            onRoomUpdated();
        }

        /// <summary>
        /// Responds to changes of the <see cref="Room"/>'s properties.
        /// </summary>
        /// <param name="sender">The <see cref="Room"/> that changed.</param>
        /// <param name="e">Describes the property that changed.</param>
        private void onRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Room.RoomID):
                    updateSetupState();
                    break;
            }
        }

        /// <summary>
        /// Responds to changes of the <see cref="MultiplayerRoom"/>'s properties.
        /// </summary>
        private void onRoomUpdated()
        {
            if (client.Room == null || client.LocalUser == null)
            {
                if (room.RoomID != null)
                {
                    Logger.Log($"{this} exiting due to loss of room or connection");

                    if (this.IsCurrentScreen())
                        this.Exit();
                    else
                        ValidForResume = false;
                }

                return;
            }

            Activity.Value = new UserActivity.InLobby(room);

            PlaylistItem? roomItem = room.Playlist.SingleOrDefault(i => i.ID == client.Room.Settings.PlaylistItemId);
            currentItem.Value = roomItem?.With(
                beatmap: new Optional<IBeatmapInfo>(new APIBeatmap { OnlineID = client.LocalUser.BeatmapId ?? roomItem.Beatmap.OnlineID }),
                ruleset: client.LocalUser.RulesetId ?? roomItem.RulesetID);
        }

        /// <summary>
        /// Responds to notifications from the server that a gameplay session has started and the local user should proceed to a gameplay screen.
        /// </summary>
        private void onLoadRequested()
        {
            Debug.Assert(client.LocalUser != null);

            // In the case of spectating, IMultiplayerClient.LoadRequested can be fired while the game is still spectating a previous session.
            // For now, we want to game to switch to the new game so need to request exiting from the play screen.
            if (!parentScreen.IsCurrentScreen())
            {
                parentScreen.MakeCurrent();
                Schedule(onLoadRequested);
                return;
            }

            // The beatmap is queried asynchronously when the selected item changes.
            // This is an issue with MultiSpectatorScreen which is effectively in an always "ready" state and receives LoadRequested() callbacks
            // even when it is not truly ready (i.e. the beatmap hasn't been selected by the client yet). For the time being, a simple fix to this is to ignore the callback.
            // Note that spectator will be entered automatically when the client is capable of doing so via beatmap availability callbacks (see: updateBeatmapAvailability()).
            if (client.LocalUser.State == MultiplayerUserState.Spectating && (currentItem.Value == null || Beatmap.IsDefault))
                return;

            if (beatmapAvailabilityTracker.Availability.Value.State != DownloadState.LocallyAvailable)
                return;

            startPlay();
        }

        /// <summary>
        /// Responds to changes in the playlist item in preparation for a new gameplay session.
        /// </summary>
        private void onCurrentItemChanged(ValueChangedEvent<PlaylistItem?> e)
        {
            if (client.Room == null || client.LocalUser == null)
                return;

            if (e.NewValue is not PlaylistItem item)
                return;

            updateGameplayState();

            bool freemods = item.Freestyle || item.AllowedMods.Length > 0;
            bool freestyle = item.Freestyle;

            if (freemods)
                userModsSection.Show();
            else
            {
                userModsSection.Hide();
                userModsSelectOverlay.Hide();
            }

            if (freestyle)
            {
                userStyleSection.Show();

                if (!item.Equals(userStyleDisplayContainer.SingleOrDefault()?.Item))
                {
                    userStyleDisplayContainer.Child = new DrawableRoomPlaylistItem(item, true)
                    {
                        AllowReordering = false,
                        AllowEditing = true,
                        RequestEdit = _ => showUserStyleSelect()
                    };
                }
            }
            else
                userStyleSection.Hide();
        }

        /// <summary>
        /// Responds to changes in the local user's beatmap availability to notify the server and prepare the gameplay session.
        /// </summary>
        private void onBeatmapAvailabilityChanged(ValueChangedEvent<BeatmapAvailability> e)
        {
            if (client.Room == null || client.LocalUser == null)
                return;

            client.ChangeBeatmapAvailability(e.NewValue).FireAndForget();

            switch (e.NewValue.State)
            {
                case DownloadState.LocallyAvailable:
                    updateGameplayState();

                    // Optimistically enter spectator if the match is in progress while spectating.
                    if (client.LocalUser.State == MultiplayerUserState.Spectating && (client.Room.State == MultiplayerRoomState.WaitingForLoad || client.Room.State == MultiplayerRoomState.Playing))
                        onLoadRequested();
                    break;

                case DownloadState.NotDownloaded:
                    updateGameplayState();

                    if (client.LocalUser.State == MultiplayerUserState.Ready)
                        client.ChangeState(MultiplayerUserState.Idle);
                    break;
            }
        }

        /// <summary>
        /// Invoked on changes to <see cref="Room.RoomID"/> to adjust the visibility of the settings and main content.
        /// Only the settings overlay is visible while the room isn't created, and only the main content is visible after creation.
        /// </summary>
        private void updateSetupState()
        {
            if (room.RoomID == null)
            {
                // A new room is being created.
                // The main content should be hidden until the settings overlay is hidden, signaling the room is ready to be displayed.
                roomContent.Hide();
                settingsOverlay.Show();
            }
            else
            {
                roomContent.Show();
                settingsOverlay.Hide();
            }
        }

        /// <summary>
        /// Updates the global beatmap/ruleset/mods in preparation for a new gameplay session.
        /// </summary>
        private void updateGameplayState()
        {
            if (client.Room == null || client.LocalUser == null)
                return;

            if (currentItem.Value is not PlaylistItem item)
                return;

            RulesetInfo ruleset = rulesets.GetRuleset(item.RulesetID)!;
            Ruleset rulesetInstance = ruleset.CreateInstance();

            // Update global gameplay state to correspond to the new selection.
            // Retrieve the corresponding local beatmap, since we can't directly use the playlist's beatmap info
            int beatmapId = item.Beatmap.OnlineID;
            var localBeatmap = beatmapManager.QueryBeatmap(b => b.OnlineID == beatmapId);
            Beatmap.Value = beatmapManager.GetWorkingBeatmap(localBeatmap);
            Ruleset.Value = ruleset;
            Mods.Value = client.LocalUser.Mods.Concat(item.RequiredMods).Select(m => m.ToMod(rulesetInstance)).ToArray();
        }

        /// <summary>
        /// Pushes a gameplay or spectate screen to start gameplay for the current selection.
        /// </summary>
        private void startPlay()
        {
            Debug.Assert(client.Room != null);
            Debug.Assert(client.LocalUser != null);

            if (!this.IsCurrentScreen() || currentItem.Value is not PlaylistItem item)
                return;

            sampleStart?.Play();

            int[] userIds = client.CurrentMatchPlayingUserIds.ToArray();
            MultiplayerRoomUser[] users = userIds.Select(id => client.Room.Users.First(u => u.UserID == id)).ToArray();

            // fallback is to allow this class to operate when there is no parent OnlineScreen (testing purposes).
            var targetScreen = (Screen?)parentScreen ?? this;

            switch (client.LocalUser.State)
            {
                case MultiplayerUserState.Spectating:
                    targetScreen.Push(new MultiSpectatorScreen(room, users.Take(PlayerGrid.MAX_PLAYERS).ToArray()));
                    break;

                default:
                    targetScreen.Push(new MultiplayerPlayerLoader(() => new MultiplayerPlayer(room, item, users)));
                    break;
            }
        }

        /// <summary>
        /// Shows the song selection screen to add or edit an item.
        /// </summary>
        /// <param name="itemToEdit">An optional playlist item to edit. If null, a new item will be added instead.</param>
        private void showSongSelect(PlaylistItem? itemToEdit = null)
        {
            if (!this.IsCurrentScreen())
                return;

            this.Push(new MultiplayerMatchSongSelect(room, itemToEdit));
        }

        /// <summary>
        /// Shows the user mod selection.
        /// </summary>
        private void showUserModSelect()
        {
            if (!this.IsCurrentScreen() || currentItem.Value is not PlaylistItem)
                return;

            userModsSelectOverlay.Show();
        }

        /// <summary>
        /// Shows the user style selection.
        /// </summary>
        private void showUserStyleSelect()
        {
            if (!this.IsCurrentScreen() || currentItem.Value is not PlaylistItem item)
                return;

            this.Push(new MultiplayerMatchFreestyleSelect(room, item));
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);
            beginHandlingTrack();
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            endHandlingTrack();
            base.OnSuspending(e);
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);
            beginHandlingTrack();

            // Required to update beatmap/ruleset when resuming from style selection.
            currentItem.TriggerChange();
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            if (!ensureExitConfirmed())
                return true;

            RoomManager?.PartRoom();

            endHandlingTrack();
            return base.OnExiting(e);
        }

        public override bool OnBackButton()
        {
            if (room.RoomID == null)
            {
                if (!ensureExitConfirmed())
                    return true;

                settingsOverlay.Hide();
                return base.OnBackButton();
            }

            if (userModsSelectOverlay.State.Value == Visibility.Visible)
            {
                userModsSelectOverlay.Hide();
                return true;
            }

            if (settingsOverlay.State.Value == Visibility.Visible)
            {
                settingsOverlay.Hide();
                return true;
            }

            return base.OnBackButton();
        }

        /// <summary>
        /// Handles changes in the track to keep it looping while active.
        /// </summary>
        private void beginHandlingTrack()
        {
            Beatmap.BindValueChanged(applyLoopingToTrack, true);
        }

        /// <summary>
        /// Stops looping the current track and stops handling further changes to the track.
        /// </summary>
        private void endHandlingTrack()
        {
            Beatmap.ValueChanged -= applyLoopingToTrack;
            Beatmap.Value.Track.Looping = false;

            previewTrackManager.StopAnyPlaying(this);
        }

        /// <summary>
        /// Invoked on changes to the beatmap to loop the track. See: <see cref="beginHandlingTrack"/>.
        /// </summary>
        /// <param name="beatmap">The beatmap change event.</param>
        private void applyLoopingToTrack(ValueChangedEvent<WorkingBeatmap> beatmap)
        {
            if (!this.IsCurrentScreen())
                return;

            beatmap.NewValue.PrepareTrackForPreview(true);
            music.EnsurePlayingSomething();
        }

        /// <summary>
        /// Prompts the user to discard unsaved changes to the room before exiting.
        /// </summary>
        /// <returns><c>true</c> if the user has confirmed they want to exit.</returns>
        private bool ensureExitConfirmed()
        {
            if (ExitConfirmed)
                return true;

            if (api.State.Value != APIState.Online || !client.IsConnected.Value)
                return true;

            if (dialogOverlay == null)
                return true;

            bool hasUnsavedChanges = room.RoomID == null && room.Playlist.Count > 0;

            if (hasUnsavedChanges)
            {
                // if the dialog is already displayed, block exiting until the user explicitly makes a decision.
                if (dialogOverlay.CurrentDialog is ConfirmDiscardChangesDialog discardChangesDialog)
                {
                    discardChangesDialog.Flash();
                    return false;
                }

                dialogOverlay.Push(new ConfirmDiscardChangesDialog(() =>
                {
                    ExitConfirmed = true;
                    settingsOverlay.Hide();
                    this.Exit();
                }));
            }

            if (client.Room != null)
            {
                if (dialogOverlay.CurrentDialog is ConfirmDialog confirmDialog)
                    confirmDialog.PerformOkAction();
                else
                {
                    dialogOverlay.Push(new ConfirmDialog("Are you sure you want to leave this multiplayer match?", () =>
                    {
                        ExitConfirmed = true;
                        this.Exit();
                    }));
                }
            }

            return false;
        }

        public void PresentBeatmap(WorkingBeatmap beatmap, RulesetInfo ruleset)
        {
            if (!this.IsCurrentScreen())
                return;

            if (client.Room == null || client.LocalUser == null)
                return;

            if (client.LocalUser.CanAddPlaylistItems(client.Room) != true)
                return;

            // If there's only one playlist item and we are the host, assume we want to change it. Else add a new one.
            PlaylistItem? itemToEdit = client.IsHost && room.Playlist.Count == 1 ? room.Playlist.Single() : null;

            showSongSelect(itemToEdit);

            // Re-run PresentBeatmap now that we've pushed a song select that can handle it.
            game?.PresentBeatmap(beatmap.BeatmapSetInfo, b => b.ID == beatmap.BeatmapInfo.ID);
        }

        protected override BackgroundScreen CreateBackground() => new RoomBackgroundScreen(room.Playlist.FirstOrDefault())
        {
            SelectedItem = { BindTarget = currentItem }
        };

        Room IMultiplayerMatchScreen.Room => room;

        bool IMultiplayerMatchScreen.IsCurrentScreen() => this.IsCurrentScreen();

        void IMultiplayerMatchScreen.Push(IScreen screen) => this.Push(screen);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            userModsSelectOverlayRegistration?.Dispose();

            room.PropertyChanged -= onRoomPropertyChanged;

            if (client.IsNotNull())
            {
                client.RoomUpdated -= onRoomUpdated;
                client.LoadRequested -= onLoadRequested;
            }
        }

        public partial class AddItemButton : PurpleRoundedButton
        {
            [Resolved]
            private MultiplayerClient client { get; set; } = null!;

            protected override void LoadComplete()
            {
                base.LoadComplete();

                client.RoomUpdated += onRoomUpdated;
                onRoomUpdated();
            }

            private void onRoomUpdated()
            {
                if (client.Room is not MultiplayerRoom room
                    || client.LocalUser is not MultiplayerRoomUser user)
                    return;

                Alpha = user.CanAddPlaylistItems(room) ? 1 : 0;
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                if (client.IsNotNull())
                    client.RoomUpdated -= onRoomUpdated;
            }
        }
    }
}
