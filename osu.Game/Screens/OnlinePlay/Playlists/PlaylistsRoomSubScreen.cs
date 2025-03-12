// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Cursor;
using osu.Game.Input;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Menu;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Match;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Users;
using osu.Game.Utils;
using osuTK;
using Container = osu.Framework.Graphics.Containers.Container;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public partial class PlaylistsRoomSubScreen : OnlinePlaySubScreen, IPreviewTrackOwner
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

        public override string ShortTitle => "playlist";

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
        private IdleTracker? idleTracker { get; set; }

        [Resolved]
        private OnlinePlayScreen? parentScreen { get; set; }

        [Resolved]
        private IOverlayManager? overlayManager { get; set; }

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        [Cached]
        private readonly OnlinePlayBeatmapAvailabilityTracker beatmapAvailabilityTracker = new OnlinePlayBeatmapAvailabilityTracker();

        protected readonly Bindable<PlaylistItem?> SelectedItem = new Bindable<PlaylistItem?>();
        protected readonly Bindable<BeatmapInfo?> UserBeatmap = new Bindable<BeatmapInfo?>();
        protected readonly Bindable<RulesetInfo?> UserRuleset = new Bindable<RulesetInfo?>();
        protected readonly Bindable<IReadOnlyList<Mod>> UserMods = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        private readonly IBindable<bool> isIdle = new BindableBool();
        private readonly Room room;

        private Drawable roomContent = null!;
        private PlaylistsRoomUpdater roomUpdater = null!;
        private PlaylistsRoomSettingsOverlay settingsOverlay = null!;

        private MatchLeaderboard leaderboard = null!;
        private FillFlowContainer progressSection = null!;
        private DrawableRoomPlaylist drawablePlaylist = null!;

        private FillFlowContainer userModsSection = null!;
        private RoomModSelectOverlay userModsSelectOverlay = null!;

        private FillFlowContainer userStyleSection = null!;
        private Container<DrawableRoomPlaylistItem> userStyleDisplayContainer = null!;

        private Sample? sampleStart;
        private IDisposable? userModsSelectOverlayRegistration;

        public PlaylistsRoomSubScreen(Room room)
        {
            this.room = room;

            Title = room.RoomID == null ? "New playlist" : room.Name;
            Activity.Value = new UserActivity.InLobby(room);

            Padding = new MarginPadding { Top = Header.HEIGHT };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (idleTracker != null)
                isIdle.BindTo(idleTracker.IsIdle);

            sampleStart = audio.Samples.Get(@"SongSelect/confirm-selection");

            InternalChild = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    roomUpdater = new PlaylistsRoomUpdater(room),
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
                                        new DrawableMatchRoom(room, false)
                                        {
                                            OnEdit = () => settingsOverlay.Show(),
                                            SelectedItem = SelectedItem
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
                                                                    new Dimension(GridSizeMode.AutoSize),
                                                                    new Dimension(),
                                                                    new Dimension(GridSizeMode.AutoSize),
                                                                },
                                                                Content = new[]
                                                                {
                                                                    new Drawable[]
                                                                    {
                                                                        new OverlinedPlaylistHeader(room),
                                                                    },
                                                                    new Drawable[]
                                                                    {
                                                                        drawablePlaylist = new DrawableRoomPlaylist
                                                                        {
                                                                            RelativeSizeAxes = Axes.Both,
                                                                            SelectedItem = { BindTarget = SelectedItem },
                                                                            AllowSelection = true,
                                                                            AllowShowingResults = true,
                                                                            RequestResults = item =>
                                                                            {
                                                                                Debug.Assert(room.RoomID != null);
                                                                                parentScreen?.Push(new PlaylistItemUserBestResultsScreen(room.RoomID.Value, item,
                                                                                    api.LocalUser.Value.Id));
                                                                            }
                                                                        }
                                                                    },
                                                                    new Drawable[]
                                                                    {
                                                                        new AddPlaylistToCollectionButton(room)
                                                                        {
                                                                            Margin = new MarginPadding { Top = 5 },
                                                                            RelativeSizeAxes = Axes.X,
                                                                            Size = new Vector2(1, 40)
                                                                        }
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
                                                                    new Dimension(GridSizeMode.AutoSize),
                                                                    new Dimension(GridSizeMode.AutoSize),
                                                                },
                                                                Content = new[]
                                                                {
                                                                    new Drawable[]
                                                                    {
                                                                        userModsSection = new FillFlowContainer
                                                                        {
                                                                            RelativeSizeAxes = Axes.X,
                                                                            AutoSizeAxes = Axes.Y,
                                                                            Margin = new MarginPadding { Bottom = row_padding },
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
                                                                                        new ModDisplay
                                                                                        {
                                                                                            Anchor = Anchor.CentreLeft,
                                                                                            Origin = Anchor.CentreLeft,
                                                                                            Current = UserMods,
                                                                                            Scale = new Vector2(0.8f),
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    },
                                                                    new Drawable[]
                                                                    {
                                                                        userStyleSection = new FillFlowContainer
                                                                        {
                                                                            RelativeSizeAxes = Axes.X,
                                                                            AutoSizeAxes = Axes.Y,
                                                                            Margin = new MarginPadding { Bottom = row_padding },
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
                                                                        }
                                                                    },
                                                                    new Drawable[]
                                                                    {
                                                                        progressSection = new FillFlowContainer
                                                                        {
                                                                            RelativeSizeAxes = Axes.X,
                                                                            AutoSizeAxes = Axes.Y,
                                                                            Margin = new MarginPadding { Bottom = row_padding },
                                                                            Alpha = 0,
                                                                            Direction = FillDirection.Vertical,
                                                                            Children = new Drawable[]
                                                                            {
                                                                                new OverlinedHeader("Progress"),
                                                                                new RoomLocalUserInfo(room),
                                                                            }
                                                                        }
                                                                    },
                                                                    new Drawable[]
                                                                    {
                                                                        new OverlinedHeader("Leaderboard")
                                                                    },
                                                                    new Drawable[]
                                                                    {
                                                                        leaderboard = new MatchLeaderboard(room)
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
                            settingsOverlay = new PlaylistsRoomSettingsOverlay(room)
                            {
                                EditPlaylist = () =>
                                {
                                    if (this.IsCurrentScreen())
                                        this.Push(new PlaylistsSongSelect(room));
                                }
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
                                Child = new PlaylistsRoomFooter(room)
                                {
                                    OnStart = startPlay,
                                    OnClose = closePlaylist
                                }
                            }
                        }
                    }
                }
            };

            LoadComponent(userModsSelectOverlay = new RoomModSelectOverlay
            {
                SelectedItem = { BindTarget = SelectedItem },
                SelectedMods = { BindTarget = UserMods },
                IsValidMod = _ => false
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            userModsSelectOverlayRegistration = overlayManager?.RegisterBlockingOverlay(userModsSelectOverlay);

            room.PropertyChanged += onRoomPropertyChanged;

            isIdle.BindValueChanged(_ => updatePollingRate(), true);

            SelectedItem.BindValueChanged(onSelectedItemChanged);

            beatmapAvailabilityTracker.SelectedItem.BindTo(SelectedItem);
            beatmapAvailabilityTracker.Availability.BindValueChanged(_ => updateGameplayState());

            UserBeatmap.BindValueChanged(_ => updateGameplayState());
            UserMods.BindValueChanged(_ => updateGameplayState());
            UserRuleset.BindValueChanged(_ =>
            {
                // The user mod selection overlay is separate from the beatmap/ruleset style selection screen,
                // and so the validity of mods has to be confirmed separately after the ruleset is changed.
                validateUserMods();
                updateGameplayState();
            });

            updateSetupState();
            updateGameplayState();
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
        /// Responds to changes in <see cref="Room.RoomID"/> to adjust the visibility of the settings and main content.
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

                // Scheduled because room properties are updated in arbitrary order.
                Schedule(() =>
                {
                    progressSection.Alpha = room.MaxAttempts != null ? 1 : 0;
                    drawablePlaylist.Items.ReplaceRange(0, drawablePlaylist.Items.Count, room.Playlist);

                    // Select an initial item for the user to help them get into a playable state quicker.
                    SelectedItem.Value = room.Playlist.FirstOrDefault();
                });
            }
        }

        /// <summary>
        /// Adjusts the rate at which the <see cref="Room"/> is updated.
        /// </summary>
        private void updatePollingRate()
        {
            roomUpdater.TimeBetweenPolls.Value = isIdle.Value ? 30000 : 5000;
            Logger.Log($"Polling adjusted (selection: {roomUpdater.TimeBetweenPolls.Value})");
        }

        /// <summary>
        /// Responds to changes in <see cref="SelectedItem"/> to validate the user style and update the global gameplay state.
        /// </summary>
        private void onSelectedItemChanged(ValueChangedEvent<PlaylistItem?> item)
        {
            if (item.NewValue == null)
                return;

            // Always resetting the user beatmap style when a new item is selected is most intuitive.
            UserBeatmap.Value = null;

            if (item.NewValue.Freestyle)
            {
                // If freestyle is active, attempt to preserve the user ruleset style but only if the online item is from the osu! ruleset
                // (i.e. the beatmap is generally always convertible to the current ruleset, excluding custom rulesets).
                if (item.NewValue.RulesetID > 0)
                    UserRuleset.Value = null;
            }
            else
                UserRuleset.Value = null;

            validateUserMods();
            updateGameplayState();
        }

        /// <summary>
        /// Lists the <see cref="Mod"/>s that are valid to be selected for the user mod style.
        /// </summary>
        private Mod[] listAllowedMods()
        {
            if (SelectedItem.Value == null)
                return [];

            PlaylistItem item = SelectedItem.Value;

            RulesetInfo gameplayRuleset = UserRuleset.Value ?? rulesets.GetRuleset(item.RulesetID)!;
            Ruleset rulesetInstance = gameplayRuleset.CreateInstance();

            if (item.Freestyle)
                return rulesetInstance.AllMods.OfType<Mod>().Where(m => ModUtils.IsValidFreeModForMatchType(m, room.Type)).ToArray();

            return item.AllowedMods.Select(m => m.ToMod(rulesetInstance)).ToArray();
        }

        /// <summary>
        /// Validates the user mod style against the selected item and ruleset style.
        /// </summary>
        private void validateUserMods()
        {
            Mod[] allowedMods = listAllowedMods();
            UserMods.Value = UserMods.Value.Where(m => allowedMods.Any(a => m.GetType() == a.GetType())).ToArray();
        }

        /// <summary>
        /// Updates the global states in preparation for a new gameplay session.
        /// </summary>
        private void updateGameplayState()
        {
            if (!this.IsCurrentScreen() || SelectedItem.Value == null)
                return;

            PlaylistItem item = SelectedItem.Value;

            IBeatmapInfo gameplayBeatmap = UserBeatmap.Value ?? item.Beatmap;
            RulesetInfo gameplayRuleset = UserRuleset.Value ?? rulesets.GetRuleset(item.RulesetID)!;
            Ruleset rulesetInstance = gameplayRuleset.CreateInstance();
            Mod[] allowedMods = listAllowedMods();

            // Update global gameplay state to correspond to the new selection.
            // Retrieve the corresponding local beatmap, since we can't directly use the playlist's beatmap info
            int beatmapId = gameplayBeatmap.OnlineID;
            var localBeatmap = beatmapManager.QueryBeatmap(b => b.OnlineID == beatmapId);
            Beatmap.Value = beatmapManager.GetWorkingBeatmap(localBeatmap);
            Ruleset.Value = gameplayRuleset;
            Mods.Value = UserMods.Value.Concat(item.RequiredMods.Select(m => m.ToMod(rulesetInstance))).ToArray();

            // Update UI elements to reflect the new selection.
            bool freemods = allowedMods.Length > 0;
            bool freestyle = item.Freestyle;

            if (freemods)
            {
                userModsSection.Show();
                userModsSelectOverlay.IsValidMod = m => allowedMods.Any(a => a.GetType() == m.GetType());
            }
            else
            {
                userModsSection.Hide();
                userModsSelectOverlay.Hide();
                userModsSelectOverlay.IsValidMod = _ => false;
            }

            if (freestyle)
            {
                userStyleSection.Show();

                PlaylistItem gameplayItem = item.With(ruleset: gameplayRuleset.OnlineID, beatmap: new Optional<IBeatmapInfo>(gameplayBeatmap));
                PlaylistItem? currentItem = userStyleDisplayContainer.SingleOrDefault()?.Item;

                if (!gameplayItem.Equals(currentItem))
                {
                    userStyleDisplayContainer.Child = new DrawableRoomPlaylistItem(gameplayItem, true)
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
        /// Pushes a <see cref="Player"/> to start gameplay with the current selection.
        /// </summary>
        private void startPlay()
        {
            if (!this.IsCurrentScreen() || SelectedItem.Value == null)
                return;

            PlaylistItem item = SelectedItem.Value;

            // Required for validation inside the player.
            RulesetInfo gameplayRuleset = UserRuleset.Value ?? rulesets.GetRuleset(item.RulesetID)!;
            IBeatmapInfo gameplayBeatmap = UserBeatmap.Value ?? item.Beatmap;
            PlaylistItem gameplayItem = item.With(ruleset: gameplayRuleset.OnlineID, beatmap: new Optional<IBeatmapInfo>(gameplayBeatmap));

            sampleStart?.Play();

            // fallback is to allow this class to operate when there is no parent OnlineScreen (testing purposes).
            var targetScreen = (Screen?)parentScreen ?? this;
            targetScreen.Push(new PlayerLoader(() => new PlaylistsPlayer(room, gameplayItem)
            {
                Exited = () => leaderboard.RefetchScores()
            }));
        }

        /// <summary>
        /// Shows the user mod selection.
        /// </summary>
        private void showUserModSelect()
        {
            if (!this.IsCurrentScreen() || SelectedItem.Value == null)
                return;

            userModsSelectOverlay.Show();
        }

        /// <summary>
        /// Shows the user style selection.
        /// </summary>
        private void showUserStyleSelect()
        {
            if (!this.IsCurrentScreen() || SelectedItem.Value == null)
                return;

            this.Push(new PlaylistsRoomFreestyleSelect(room, SelectedItem.Value)
            {
                Beatmap = { BindTarget = UserBeatmap },
                Ruleset = { BindTarget = UserRuleset }
            });
        }

        /// <summary>
        /// May be invoked by the owner of the room to permanently close the room ahead of its intended end date.
        /// </summary>
        private void closePlaylist()
        {
            dialogOverlay?.Push(new ClosePlaylistDialog(room, () =>
            {
                var request = new ClosePlaylistRequest(room.RoomID!.Value);
                request.Success += () => room.EndDate = DateTimeOffset.UtcNow;
                api.Queue(request);
            }));
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

            if (room.RoomID != null)
                api.Queue(new PartRoomRequest(room));

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

            if (api.State.Value != APIState.Online)
                return true;

            bool hasUnsavedChanges = room.RoomID == null && room.Playlist.Count > 0;

            if (dialogOverlay == null || !hasUnsavedChanges)
                return true;

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

        // Block all input to this screen during gameplay/etc when the parent screen is no longer current.
        // Normally this would be handled by ScreenStack, but we are in a child ScreenStack.
        public override bool PropagatePositionalInputSubTree => base.PropagatePositionalInputSubTree && (parentScreen?.IsCurrentScreen() ?? this.IsCurrentScreen());

        // Block all input to this screen during gameplay/etc when the parent screen is no longer current.
        // Normally this would be handled by ScreenStack, but we are in a child ScreenStack.
        public override bool PropagateNonPositionalInputSubTree => base.PropagateNonPositionalInputSubTree && (parentScreen?.IsCurrentScreen() ?? this.IsCurrentScreen());

        protected override BackgroundScreen CreateBackground() => new RoomBackgroundScreen(room.Playlist.FirstOrDefault())
        {
            SelectedItem = { BindTarget = SelectedItem }
        };

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            userModsSelectOverlayRegistration?.Dispose();
            room.PropertyChanged -= onRoomPropertyChanged;
        }
    }
}
