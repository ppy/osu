// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
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
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Users;
using osu.Game.Utils;
using osuTK;
using ParticipantsList = osu.Game.Screens.OnlinePlay.Multiplayer.Participants.ParticipantsList;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    [Cached]
    public partial class MultiplayerMatchSubScreen : OnlinePlaySubScreen, IPreviewTrackOwner, IHandlePresentBeatmap
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

        /// <summary>
        /// Used for testing - whether the local user style can be edited.
        /// False if the beatmap hasn't been downloaded yet, or if freestyle isn't enabled.
        /// </summary>
        internal bool UserStyleEditingEnabled
        {
            get
            {
                if (!userStyleDisplayContainer.IsPresent)
                    return false;

                return userStyleDisplayContainer.SingleOrDefault()?.AllowEditing == true;
            }
        }

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

        [Cached(typeof(OnlinePlayBeatmapAvailabilityTracker))]
        private readonly OnlinePlayBeatmapAvailabilityTracker beatmapAvailabilityTracker = new MultiplayerBeatmapAvailabilityTracker();

        private readonly Room room;

        private Drawable roomContent = null!;
        private MultiplayerMatchSettingsOverlay settingsOverlay = null!;

        private FillFlowContainer userModsSection = null!;
        private MultiplayerUserModSelectOverlay userModsSelectOverlay = null!;

        private FillFlowContainer userStyleSection = null!;
        private Container<DrawableRoomPlaylistItem> userStyleDisplayContainer = null!;

        private Sample? sampleStart;
        private IDisposable? userModsSelectOverlayRegistration;

        private long lastPlaylistItemId;
        private bool isRoomJoined;

        public MultiplayerMatchSubScreen(Room room)
        {
            this.room = room;

            Title = room.RoomID == null ? "New room" : room.Name;
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
                Child = new PopoverContainer
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
                                            new MultiplayerRoomPanel(room)
                                            {
                                                OnEdit = () => settingsOverlay.Show()
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
                                                                                Height = 30,
                                                                                Text = "Add item",
                                                                                Action = () => ShowSongSelect()
                                                                            },
                                                                        },
                                                                        null,
                                                                        new Drawable[]
                                                                        {
                                                                            new MultiplayerPlaylist
                                                                            {
                                                                                RelativeSizeAxes = Axes.Both,
                                                                                RequestEdit = ShowSongSelect,
                                                                                RequestResults = showResults
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
                                    Child = new MultiplayerMatchFooter()
                                }
                            }
                        }
                    }
                }
            };

            LoadComponent(userModsSelectOverlay = new MultiplayerUserModSelectOverlay
            {
                Beatmap = { BindTarget = Beatmap }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            userModsSelectOverlayRegistration = overlayManager?.RegisterBlockingOverlay(userModsSelectOverlay);

            client.RoomUpdated += onRoomUpdated;
            client.SettingsChanged += onSettingsChanged;
            client.ItemChanged += onItemChanged;
            client.UserStyleChanged += onUserStyleChanged;
            client.UserModsChanged += onUserModsChanged;
            client.LoadRequested += onLoadRequested;

            beatmapAvailabilityTracker.Availability.BindValueChanged(onBeatmapAvailabilityChanged, true);

            onRoomUpdated();
            updateGameplayState();
            updateUserActivity();
        }

        /// <summary>
        /// Responds to changes in the active room to adjust the visibility of the settings and main content.
        /// Only the settings overlay is visible while the room isn't created, and only the main content is visible after creation.
        /// </summary>
        private void onRoomUpdated() => Scheduler.AddOnce(() =>
        {
            bool wasRoomJoined = isRoomJoined;
            isRoomJoined = client.Room != null;

            // Creating a room.
            if (!wasRoomJoined && !isRoomJoined)
            {
                roomContent.Hide();
                settingsOverlay.Show();
            }

            // Joining a room.
            if (!wasRoomJoined && isRoomJoined)
            {
                roomContent.Show();
                settingsOverlay.Hide();
            }

            // Leaving a room.
            if (wasRoomJoined && !isRoomJoined)
            {
                Logger.Log($"{this} exiting due to loss of room or connection");

                if (this.IsCurrentScreen())
                    this.Exit();
                else
                    ValidForResume = false;
            }
        });

        /// <summary>
        /// Responds to changes in the room's settings to update the gameplay state and local user's activity.
        /// </summary>
        private void onSettingsChanged(MultiplayerRoomSettings settings)
        {
            if (settings.PlaylistItemId != lastPlaylistItemId)
            {
                onActivePlaylistItemChanged();
                lastPlaylistItemId = settings.PlaylistItemId;
            }

            updateUserActivity();
        }

        /// <summary>
        /// Responds to changes in the active playlist item to update the gameplay state.
        /// </summary>
        private void onItemChanged(MultiplayerPlaylistItem item)
        {
            if (item.ID == client.Room?.Settings.PlaylistItemId)
                onActivePlaylistItemChanged();
        }

        /// <summary>
        /// Responds to changes in the active playlist item resulting from the playlist item being edited or the room settings changing.
        /// </summary>
        private void onActivePlaylistItemChanged()
        {
            if (client.Room == null)
                return;

            // Check if we need to make this the current screen as a result of the beatmap set changing while the user's selecting a style.
            if (this.GetChildScreen() is MultiplayerMatchFreestyleSelect)
            {
                MultiplayerPlaylistItem item = client.Room.CurrentPlaylistItem;

                var newBeatmap = beatmapManager.QueryBeatmap($@"{nameof(BeatmapInfo.OnlineID)} == $0 AND {nameof(BeatmapInfo.MD5Hash)} == {nameof(BeatmapInfo.OnlineMD5Hash)}", item.BeatmapID);

                if (!Beatmap.Value.BeatmapSetInfo.Equals(newBeatmap?.BeatmapSet))
                    this.MakeCurrent();
            }

            Scheduler.AddOnce(updateGameplayState);
        }

        /// <summary>
        /// Responds to changes in the local user's style to update the gameplay state.
        /// </summary>
        private void onUserStyleChanged(MultiplayerRoomUser user)
        {
            if (user.Equals(client.LocalUser))
                Scheduler.AddOnce(updateGameplayState);
        }

        /// <summary>
        /// Responds to changes in the local user's mods style to update the gameplay state.
        /// </summary>
        private void onUserModsChanged(MultiplayerRoomUser user)
        {
            if (user.Equals(client.LocalUser))
                Scheduler.AddOnce(updateGameplayState);
        }

        /// <summary>
        /// Responds to notifications from the server that a gameplay session is ready to attempt to start the gameplay session.
        /// </summary>
        private void onLoadRequested()
        {
            if (client.Room == null || client.LocalUser == null)
                return;

            // In the case of spectating, IMultiplayerClient.LoadRequested can be fired while the game is still spectating a previous session.
            // For now, we want to game to switch to the new game so need to request exiting from the play screen.
            if (!parentScreen.IsCurrentScreen())
            {
                parentScreen.MakeCurrent();
                Schedule(onLoadRequested);
                return;
            }

            if (!this.IsCurrentScreen())
            {
                this.MakeCurrent();
                Schedule(onLoadRequested);
                return;
            }

            // Ensure all the gameplay states are up-to-date, forgoing any misordering/scheduling shenanigans.
            updateGameplayState();

            // ... And then check that the set gameplay state is valid.
            // When spectating, we'll receive LoadRequested() from the server even though we may not yet have the beatmap.
            // In that case, this method will be invoked again after availability changes in onBeatmapAvailabilityChanged().
            if (Beatmap.IsDefault)
            {
                Logger.Log("Aborting gameplay start - beatmap not downloaded.");
                return;
            }

            // Start the gameplay session.
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
                    targetScreen.Push(new MultiplayerPlayerLoader(() => new MultiplayerPlayer(room, new PlaylistItem(client.Room.CurrentPlaylistItem), users)));
                    break;
            }
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
        /// Updates the local user's activity to publish their presence in the room.
        /// </summary>
        private void updateUserActivity()
        {
            if (client.Room == null)
                return;

            if (Activity.Value is not UserActivity.InLobby existing || existing.RoomName != client.Room.Settings.Name)
                Activity.Value = new UserActivity.InLobby(client.Room);
        }

        /// <summary>
        /// Updates the global beatmap/ruleset/mods in preparation for a new gameplay session.
        /// </summary>
        private void updateGameplayState()
        {
            if (client.Room == null || client.LocalUser == null)
                return;

            MultiplayerPlaylistItem item = client.Room.CurrentPlaylistItem;
            int gameplayBeatmapId = client.LocalUser.BeatmapId ?? item.BeatmapID;
            int gameplayRulesetId = client.LocalUser.RulesetId ?? item.RulesetID;

            RulesetInfo ruleset = rulesets.GetRuleset(gameplayRulesetId)!;
            Ruleset rulesetInstance = ruleset.CreateInstance();

            // Update global gameplay state to correspond to the new selection.
            // Retrieve the corresponding local beatmap, since we can't directly use the playlist's beatmap info
            var localBeatmap = beatmapManager.QueryBeatmap($@"{nameof(BeatmapInfo.OnlineID)} == $0 AND {nameof(BeatmapInfo.MD5Hash)} == {nameof(BeatmapInfo.OnlineMD5Hash)}", gameplayBeatmapId);
            Beatmap.Value = beatmapManager.GetWorkingBeatmap(localBeatmap);
            Ruleset.Value = ruleset;
            Mods.Value = client.LocalUser.Mods.Concat(item.RequiredMods).Select(m => m.ToMod(rulesetInstance)).ToArray();

            bool freemods = item.Freestyle || item.AllowedMods.Any();
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

                PlaylistItem apiItem = new PlaylistItem(item).With(beatmap: new Optional<IBeatmapInfo>(new APIBeatmap { OnlineID = gameplayBeatmapId }), ruleset: gameplayRulesetId);
                DrawableRoomPlaylistItem? currentDisplay = userStyleDisplayContainer.SingleOrDefault();

                if (!apiItem.Equals(currentDisplay?.Item))
                {
                    userStyleDisplayContainer.Child = currentDisplay = new DrawableRoomPlaylistItem(apiItem, true)
                    {
                        AllowReordering = false,
                        RequestEdit = _ => ShowUserStyleSelect()
                    };
                }

                currentDisplay.AllowEditing = localBeatmap != null;
            }
            else
                userStyleSection.Hide();
        }

        /// <summary>
        /// Shows the song selection screen to add or edit an item.
        /// </summary>
        /// <param name="itemToEdit">An optional playlist item to edit. If null, a new item will be added instead.</param>
        public void ShowSongSelect(PlaylistItem? itemToEdit = null)
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
            if (!this.IsCurrentScreen())
                return;

            userModsSelectOverlay.Show();
        }

        /// <summary>
        /// Shows the user style selection.
        /// </summary>
        public void ShowUserStyleSelect()
        {
            if (!this.IsCurrentScreen() || client.Room == null || client.LocalUser == null)
                return;

            MultiplayerPlaylistItem item = client.Room.CurrentPlaylistItem;
            this.Push(new MultiplayerMatchFreestyleSelect(room, new PlaylistItem(item)));
        }

        /// <summary>
        /// Shows the results screen for a playlist item.
        /// </summary>
        private void showResults(PlaylistItem item)
        {
            if (!this.IsCurrentScreen() || client.Room == null || client.LocalUser == null)
                return;

            // fallback is to allow this class to operate when there is no parent OnlineScreen (testing purposes).
            var targetScreen = (Screen?)parentScreen ?? this;
            targetScreen.Push(new PlaylistItemUserBestResultsScreen(client.Room.RoomID, item, client.LocalUser.UserID));
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);
            beginHandlingTrack();
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            onLeaving();
            base.OnSuspending(e);
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);
            beginHandlingTrack();

            // Required to update beatmap/ruleset when resuming from style selection.
            updateGameplayState();
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            if (!ensureExitConfirmed())
                return true;

            client.LeaveRoom().FireAndForget();

            onLeaving();
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

        private void onLeaving()
        {
            // Must hide this overlay because it is added to a global container.
            userModsSelectOverlay.Hide();

            endHandlingTrack();
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

                return false;
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

                return false;
            }

            return true;
        }

        public void PresentBeatmap(WorkingBeatmap beatmap, RulesetInfo ruleset)
        {
            if (!this.IsCurrentScreen())
                return;

            if (client.Room == null || client.LocalUser == null)
                return;

            if (client.Room.CanAddPlaylistItems(client.LocalUser) != true)
                return;

            // If there's only one playlist item and we are the host, assume we want to change it. Else add a new one.
            PlaylistItem? itemToEdit = client.IsHost && room.Playlist.Count == 1 ? room.Playlist.Single() : null;

            ShowSongSelect(itemToEdit);

            // Re-run PresentBeatmap now that we've pushed a song select that can handle it.
            game?.PresentBeatmap(beatmap.BeatmapSetInfo, b => b.ID == beatmap.BeatmapInfo.ID);
        }

        // Block all input to this screen during gameplay/etc when the parent screen is no longer current.
        // Normally this would be handled by ScreenStack, but we are in a child ScreenStack.
        public override bool PropagatePositionalInputSubTree => base.PropagatePositionalInputSubTree && (parentScreen?.IsCurrentScreen() ?? this.IsCurrentScreen());

        // Block all input to this screen during gameplay/etc when the parent screen is no longer current.
        // Normally this would be handled by ScreenStack, but we are in a child ScreenStack.
        public override bool PropagateNonPositionalInputSubTree => base.PropagateNonPositionalInputSubTree && (parentScreen?.IsCurrentScreen() ?? this.IsCurrentScreen());

        protected override BackgroundScreen CreateBackground() => new MultiplayerRoomBackgroundScreen();

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            userModsSelectOverlayRegistration?.Dispose();

            if (client.IsNotNull())
            {
                client.RoomUpdated -= onRoomUpdated;
                client.SettingsChanged -= onSettingsChanged;
                client.ItemChanged -= onItemChanged;
                client.UserStyleChanged -= onUserStyleChanged;
                client.UserModsChanged -= onUserModsChanged;
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
                if (client.Room == null || client.LocalUser == null)
                    return;

                Alpha = client.Room.CanAddPlaylistItems(client.LocalUser) ? 1 : 0;
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
