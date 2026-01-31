// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics.Cursor;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match.Gameplay;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Cards;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Intro;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    [Cached]
    public partial class RankedPlayScreen : OsuScreen, IPreviewTrackOwner, IHandlePresentBeatmap
    {
        protected override bool InitialBackButtonVisibility => false;

        public override bool HideOverlaysOnEnter => true;

        public bool RetryRequested { get; private set; }

        public RankedPlaySubScreen? ActiveSubScreen { get; private set; }

        protected override BackgroundScreen CreateBackground() => new MatchmakingBackgroundScreen(new OverlayColourProvider(OverlayColourScheme.Pink));

        [Cached(typeof(OnlinePlayBeatmapAvailabilityTracker))]
        private readonly OnlinePlayBeatmapAvailabilityTracker beatmapAvailabilityTracker = new MultiplayerBeatmapAvailabilityTracker();

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

        [Resolved]
        private BeatmapModelDownloader beatmapDownloader { get; set; } = null!;

        [Resolved]
        private IDialogOverlay dialogOverlay { get; set; } = null!;

        [Resolved]
        private AudioManager audio { get; set; } = null!;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; } = null!;

        [Resolved]
        private MusicController music { get; set; } = null!;

        private readonly MultiplayerRoom room;
        private readonly Container<RankedPlaySubScreen> screenContainer;
        private readonly MatchmakingChatDisplay chat;

        private IBindable<RankedPlayStage> stage = null!;

        private Sample? sampleStart;
        private CancellationTokenSource? downloadCheckCancellation;
        private int? lastDownloadCheckedBeatmapId;

        private readonly Bindable<Visibility> cornerPieceVisibility = new Bindable<Visibility>();

        [Cached]
        private readonly RankedPlayMatchInfo matchInfo;

        [Cached]
        private readonly CardDetailsOverlayContainer overlayContainer;

        public RankedPlayScreen(MultiplayerRoom room)
        {
            this.room = room;

            InternalChildren = new Drawable[]
            {
                matchInfo = new RankedPlayMatchInfo(),
                beatmapAvailabilityTracker,
                new PopoverContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new OsuContextMenuContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            screenContainer = new Container<RankedPlaySubScreen>
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                            chat = new MatchmakingChatDisplay(new Room(room))
                            {
                                Anchor = Anchor.BottomRight,
                                Origin = Anchor.BottomRight,
                                Size = new Vector2(320, 160),
                                Margin = new MarginPadding
                                {
                                    Bottom = 10,
                                    Right = 10
                                },
                                Alpha = 0,
                            },
                            new HamburgerMenu
                            {
                                Size = new Vector2(56),
                            }
                        }
                    }
                },
                overlayContainer = new CardDetailsOverlayContainer()
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            stage = matchInfo.Stage.GetBoundCopy();
            sampleStart = audio.Samples.Get(@"SongSelect/confirm-selection");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.RoomUpdated += onRoomUpdated;
            client.UserStateChanged += onUserStateChanged;
            client.SettingsChanged += onSettingsChanged;
            client.LoadRequested += onLoadRequested;

            beatmapAvailabilityTracker.Availability.BindValueChanged(onBeatmapAvailabilityChanged, true);

            int localUserId = api.LocalUser.Value.OnlineID;
            int opponentUserId = ((RankedPlayRoomState)client.Room!.MatchState!).Users.Keys.Single(it => it != localUserId);

            AddRangeInternal([
                new RankedPlayCornerPiece(RankedPlayColourScheme.Blue, Anchor.BottomLeft)
                {
                    State = { BindTarget = cornerPieceVisibility },
                    Child = new RankedPlayUserDisplay(localUserId, Anchor.BottomLeft, RankedPlayColourScheme.Blue)
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                },
                new RankedPlayCornerPiece(RankedPlayColourScheme.Red, Anchor.TopRight)
                {
                    State = { BindTarget = cornerPieceVisibility },
                    Child = new RankedPlayUserDisplay(opponentUserId, Anchor.TopRight, RankedPlayColourScheme.Red)
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                },
            ]);

            cornerPieceVisibility.BindValueChanged(e =>
            {
                if (e.NewValue == Visibility.Visible)
                    chat.Appear();
                else
                    chat.Disappear();
            });

            stage.BindValueChanged(e => onStageChanged(e.NewValue));
        }

        public void ShowScreen(RankedPlaySubScreen screen)
        {
            if (screen == ActiveSubScreen)
                return;

            LoadComponent(screen);

            var previousScreen = ActiveSubScreen;

            screenContainer.Add(ActiveSubScreen = screen);
            screen.OnLoadComplete += _ =>
            {
                previousScreen?.OnExiting(screen);
                screen.OnEntering(previousScreen);
                previousScreen?.Expire();

                if (previousScreen != null)
                    cornerPieceVisibility.UnbindFrom(previousScreen.CornerPieceVisibility);

                cornerPieceVisibility.BindTo(screen.CornerPieceVisibility);
            };
        }

        private void onRoomUpdated()
        {
            if (this.IsCurrentScreen() && client.Room == null)
            {
                Logger.Log($"{this} exiting due to loss of room or connection");
                exitConfirmed = true;
                this.Exit();
            }
        }

        private void onUserStateChanged(MultiplayerRoomUser user, MultiplayerUserState state)
        {
            if (user.Equals(client.LocalUser) && state == MultiplayerUserState.Idle)
                this.MakeCurrent();
        }

        private void onSettingsChanged(MultiplayerRoomSettings _) => Scheduler.Add(() =>
        {
            checkForAutomaticDownload();
            updateGameplayState();
        });

        private void onLoadRequested() => Scheduler.Add(() =>
        {
            updateGameplayState();

            if (Beatmap.IsDefault)
            {
                Logger.Log("Aborting gameplay start - beatmap not downloaded.");
                return;
            }

            sampleStart?.Play();

            this.Push(new MultiplayerPlayerLoader(() => new ScreenGameplay(new Room(room), new PlaylistItem(client.Room!.CurrentPlaylistItem), room.Users.ToArray())));
        });

        private void onStageChanged(RankedPlayStage stage)
        {
            switch (stage)
            {
                case RankedPlayStage.RoundWarmup when matchInfo.CurrentRound == 1:
                    ShowScreen(new IntroScreen());
                    break;

                case RankedPlayStage.CardDiscard:
                    ShowScreen(new DiscardScreen());
                    break;

                case RankedPlayStage.FinishCardDiscard:
                    (ActiveSubScreen as DiscardScreen)?.PresentRemainingCards();
                    break;

                case RankedPlayStage.CardPlay:
                    ShowScreen(matchInfo.IsOwnTurn ? new PickScreen() : new OpponentPickScreen());
                    break;

                case RankedPlayStage.FinishCardPlay:
                    Debug.Assert(ActiveSubScreen is PickScreen || ActiveSubScreen is OpponentPickScreen);
                    break;

                case RankedPlayStage.GameplayWarmup:
                    ShowScreen(new GameplayWarmupScreen());
                    break;

                case RankedPlayStage.Gameplay:
                    ShowScreen(new GameplayScreen());
                    break;

                case RankedPlayStage.Results:
                    ShowScreen(new ResultsScreen());
                    break;

                case RankedPlayStage.Ended:
                    ShowScreen(new EndedScreen
                    {
                        ExitRequested = retry =>
                        {
                            RetryRequested = retry;
                            exitConfirmed = true;

                            if (this.IsCurrentScreen())
                                this.Exit();
                        }
                    });
                    break;

                default:
                    ShowScreen(new PlaceholderScreen(stage));
                    break;
            }
        }

        private void onBeatmapAvailabilityChanged(ValueChangedEvent<BeatmapAvailability> e) => Scheduler.Add(() =>
        {
            if (client.Room == null || client.LocalUser == null)
                return;

            client.ChangeBeatmapAvailability(e.NewValue).FireAndForget();

            switch (e.NewValue.State)
            {
                case DownloadState.NotDownloaded:
                case DownloadState.LocallyAvailable:
                    updateGameplayState();
                    break;
            }
        });

        private void updateGameplayState()
        {
            MultiplayerPlaylistItem item = client.Room!.CurrentPlaylistItem;

            if (item.Expired)
                return;

            RulesetInfo ruleset = rulesets.GetRuleset(item.RulesetID)!;
            Ruleset rulesetInstance = ruleset.CreateInstance();

            // Update global gameplay state to correspond to the new selection.
            // Retrieve the corresponding local beatmap, since we can't directly use the playlist's beatmap info
            var localBeatmap = beatmapManager.QueryBeatmap($@"{nameof(BeatmapInfo.OnlineID)} == $0 AND {nameof(BeatmapInfo.MD5Hash)} == {nameof(BeatmapInfo.OnlineMD5Hash)}", item.BeatmapID);

            if (localBeatmap != null)
            {
                Beatmap.Value = beatmapManager.GetWorkingBeatmap(localBeatmap);
                Ruleset.Value = ruleset;
                Mods.Value = item.RequiredMods.Select(m => m.ToMod(rulesetInstance)).ToArray();

                // Notify the server that the beatmap has been set and that we are ready to start gameplay.
                if (client.LocalUser!.State == MultiplayerUserState.Idle)
                    client.ChangeState(MultiplayerUserState.Ready).FireAndForget();
            }
            else
            {
                // Notify the server that we don't have the beatmap.
                if (client.LocalUser!.State == MultiplayerUserState.Ready)
                    client.ChangeState(MultiplayerUserState.Idle).FireAndForget();
            }

            client.ChangeBeatmapAvailability(beatmapAvailabilityTracker.Availability.Value).FireAndForget();
        }

        private void checkForAutomaticDownload()
        {
            if (client.Room == null)
                return;

            MultiplayerPlaylistItem item = client.Room.CurrentPlaylistItem;

            // This method is called every time anything changes in the room.
            // This could result in download requests firing far too often, when we only expect them to fire once per beatmap.
            //
            // Without this check, we would see especially egregious behaviour when a user has hit the download rate limit.
            if (lastDownloadCheckedBeatmapId == item.BeatmapID)
                return;

            lastDownloadCheckedBeatmapId = item.BeatmapID;

            downloadCheckCancellation?.Cancel();

            if (beatmapManager.IsAvailableLocally(new APIBeatmap { OnlineID = item.BeatmapID }))
                return;

            // In a perfect world we'd use BeatmapAvailability, but there's no event-driven flow for when a selection changes.
            // ie. if selection changes from "not downloaded" to another "not downloaded" we wouldn't get a value changed raised.
            beatmapLookupCache
                .GetBeatmapAsync(item.BeatmapID, (downloadCheckCancellation = new CancellationTokenSource()).Token)
                .ContinueWith(resolved => Schedule(() =>
                {
                    APIBeatmapSet? beatmapSet = resolved.GetResultSafely()?.BeatmapSet;

                    if (beatmapSet == null)
                        return;

                    beatmapDownloader.Download(beatmapSet, config.Get<bool>(OsuSetting.PreferNoVideo));
                }));
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

        private bool exitConfirmed;

        public override bool OnExiting(ScreenExitEvent e)
        {
            if (exitConfirmed || ActiveSubScreen is EndedScreen)
            {
                if (base.OnExiting(e))
                {
                    exitConfirmed = false;
                    return true;
                }

                endHandlingTrack();

                client.LeaveRoom().FireAndForget();
                return false;
            }

            if (dialogOverlay.CurrentDialog is ConfirmDialog confirmDialog)
                confirmDialog.PerformOkAction();
            else
            {
                dialogOverlay.Push(new ConfirmDialog("Are you sure you want to leave this multiplayer match?", () =>
                {
                    exitConfirmed = true;
                    if (this.IsCurrentScreen())
                        this.Exit();
                }));
            }

            return true;
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);

            beginHandlingTrack();

            if (e.Last is not MultiplayerPlayerLoader playerLoader)
                return;

            if (!playerLoader.GameplayPassed)
            {
                client.AbortGameplay().FireAndForget();
                return;
            }

            client.ChangeState(MultiplayerUserState.Idle).FireAndForget();
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

        public void PresentBeatmap(WorkingBeatmap beatmap, RulesetInfo ruleset)
        {
            // Do nothing to prevent the user from potentially being kicked out
            // of gameplay due to the screen performer's internal processes.
        }

        protected override void Dispose(bool isDisposing)
        {
            client.RoomUpdated -= onRoomUpdated;
            client.UserStateChanged -= onUserStateChanged;
            client.SettingsChanged -= onSettingsChanged;
            client.LoadRequested -= onLoadRequested;

            base.Dispose(isDisposing);
        }
    }
}
