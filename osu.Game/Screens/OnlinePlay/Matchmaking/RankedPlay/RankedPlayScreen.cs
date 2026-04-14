// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
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
using osu.Game.Database;
using osu.Game.Graphics.Cursor;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Overlays.Volume;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match.Gameplay;
using osu.Game.Screens.OnlinePlay.Matchmaking.Queue;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Card;
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

        public RankedPlaySubScreen? ActiveSubScreen { get; private set; }

        private RankedPlayBackgroundScreen rankedPlayBackground = null!;

        protected override BackgroundScreen CreateBackground() => rankedPlayBackground = new RankedPlayBackgroundScreen
        {
            ShowBeatmapBackground = { BindTarget = showBeatmapBackground }
        };

        public override float BackgroundParallaxAmount => 0;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private UserLookupCache users { get; set; } = null!;

        [Resolved]
        private IDialogOverlay dialogOverlay { get; set; } = null!;

        [Resolved]
        private IOverlayManager overlayManager { get; set; } = null!;

        [Resolved]
        private AudioManager audio { get; set; } = null!;

        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; } = null!;

        [Resolved]
        private QueueController? controller { get; set; }

        private readonly MultiplayerRoom room;

        private APIUser localUser = null!;
        private APIUser opponentUser = null!;

        private readonly Container stageOverlayContainer;
        private readonly Container<RankedPlaySubScreen> screenContainer;
        private readonly RankedPlayChatDisplay chat;

        private RankedPlayBottomOrnament ornament = null!;
        private IDisposable? ornamentOverlayRegistration;

        private IBindable<RankedPlayStage> stage = null!;

        private Sample? sampleStart;

        private readonly Bindable<Visibility> cornerPieceVisibility = new Bindable<Visibility>();
        private readonly Bindable<bool> showBeatmapBackground = new Bindable<bool>();

        [Cached]
        private readonly RankedPlayMatchInfo matchInfo;

        [Cached]
        private readonly CardDetailsOverlayContainer overlayContainer;

        [Cached]
        private readonly SongPreviewParticleContainer particleContainer;

        [Cached]
        private BackgroundMusicManager backgroundMusic;

        public RankedPlayScreen(MultiplayerRoom room)
        {
            this.room = room;

            InternalChildren = new Drawable[]
            {
                matchInfo = new RankedPlayMatchInfo(),
                new RankedPlayBeatmapAvailabilityTracker(),
                new GlobalScrollAdjustsVolume(),
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
                            chat = new RankedPlayChatDisplay(room)
                            {
                                Anchor = Anchor.BottomRight,
                                Origin = Anchor.BottomRight,
                                Margin = new MarginPadding
                                {
                                    Bottom = 10,
                                    Right = 10
                                },
                                State = { Value = Visibility.Hidden }
                            },
                            new HamburgerMenu
                            {
                                Size = new Vector2(56),
                            }
                        }
                    }
                },
                stageOverlayContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
                overlayContainer = new CardDetailsOverlayContainer(),
                particleContainer = new SongPreviewParticleContainer(),
                backgroundMusic = new BackgroundMusicManager()
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            stage = matchInfo.Stage.GetBoundCopy();
            sampleStart = audio.Samples.Get(@"SongSelect/confirm-selection");

            LoadComponent(ornament = new RankedPlayBottomOrnament
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.RoomUpdated += onRoomUpdated;
            client.UserStateChanged += onUserStateChanged;
            client.LoadRequested += onLoadRequested;

            Scheduler.AddDelayed(() => ornament.Show(), VsSequence.INTRO_LENGTH);

            int localUserId = api.LocalUser.Value.OnlineID;
            int opponentUserId = ((RankedPlayRoomState)client.Room!.MatchState!).Users.Keys.Single(it => it != localUserId);

            localUser = users.GetUserAsync(localUserId).GetResultSafely()!;
            opponentUser = users.GetUserAsync(opponentUserId).GetResultSafely()!;

            AddRangeInternal([
                new RankedPlayCornerPiece(RankedPlayColourScheme.BLUE, Anchor.BottomLeft)
                {
                    State = { BindTarget = cornerPieceVisibility },
                    Child = new RankedPlayUserDisplay(localUser, Anchor.BottomLeft, RankedPlayColourScheme.BLUE)
                    {
                        RelativeSizeAxes = Axes.Both,
                        Health = { BindTarget = matchInfo.PlayerHealth }
                    }
                },
                new RankedPlayCornerPiece(RankedPlayColourScheme.RED, Anchor.TopRight)
                {
                    State = { BindTarget = cornerPieceVisibility },
                    Child = new RankedPlayUserDisplay(opponentUser, Anchor.TopRight, RankedPlayColourScheme.RED)
                    {
                        RelativeSizeAxes = Axes.Both,
                        Health = { BindTarget = matchInfo.OpponentHealth }
                    }
                },
            ]);

            ornamentOverlayRegistration = overlayManager.RegisterBlockingOverlay(ornament);

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
                showBeatmapBackground.Value = screen.ShowBeatmapBackground;

                if (screen.ShowStageOverlay)
                {
                    APIUser? pickingUser = null;
                    double? multiplier = matchInfo.Stage.Value < RankedPlayStage.CardPlay ? null : matchInfo.RoomState.DamageMultiplier;
                    RankedPlayColourScheme colourScheme = RankedPlayColourScheme.BLUE;

                    if (matchInfo.Stage.Value == RankedPlayStage.CardPlay && matchInfo.RoomState.ActiveUser != null)
                    {
                        pickingUser = matchInfo.IsOwnTurn ? localUser : opponentUser;
                        colourScheme = matchInfo.IsOwnTurn ? RankedPlayColourScheme.BLUE : RankedPlayColourScheme.RED;
                    }

                    stageOverlayContainer.Add(new RankedPlayStageOverlay(screen.StageHeading, colourScheme)
                    {
                        PickingUser = pickingUser,
                        Multiplier = multiplier,
                    });

                    rankedPlayBackground.ColourScheme = colourScheme;
                }
                else
                {
                    rankedPlayBackground.ColourScheme = null;
                }
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

        private void onLoadRequested() => Scheduler.Add(() =>
        {
            sampleStart?.Play();
            this.Push(new MultiplayerPlayerLoader(() => new ScreenGameplay(new Room(room), new PlaylistItem(client.Room!.CurrentPlaylistItem), room.Users.ToArray())));
        });

        private void onStageChanged(RankedPlayStage stage)
        {
            if (stage is RankedPlayStage.GameplayWarmup or RankedPlayStage.Gameplay)
                backgroundMusic.Stop();
            else
                backgroundMusic.Play();

            if (stage is RankedPlayStage.RoundWarmup && matchInfo.CurrentRound == 1)
                chat.State.Value = Visibility.Hidden;
            else
                chat.State.Value = Visibility.Visible;

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
                            retryRequested = retry;
                            exitConfirmed = true;

                            if (this.IsCurrentScreen())
                                this.Exit();
                        }
                    });
                    break;
            }
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            backgroundMusic.Stop();
            previewTrackManager.StopAnyPlaying(this);

            base.OnSuspending(e);
        }

        private bool exitConfirmed;
        private bool retryRequested;

        public override bool OnExiting(ScreenExitEvent e)
        {
            if (exitConfirmed || ActiveSubScreen is EndedScreen)
            {
                if (base.OnExiting(e))
                {
                    exitConfirmed = false;
                    return true;
                }

                backgroundMusic.Stop();
                previewTrackManager.StopAnyPlaying(this);

                client.LeaveRoom().FireAndForget();

                ornamentOverlayRegistration?.Dispose();
                ornamentOverlayRegistration = null;

                if (retryRequested)
                    controller?.RejoinQueue();

                return false;
            }

            if (dialogOverlay.CurrentDialog is ConfirmDialog confirmDialog)
                confirmDialog.PerformOkAction();
            else
            {
                dialogOverlay.Push(new ConfirmExitMultiplayerMatchDialog(() =>
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

            if (e.Last is not MultiplayerPlayerLoader playerLoader)
                return;

            if (!playerLoader.GameplayPassed)
            {
                client.AbortGameplay().FireAndForget();
                return;
            }

            client.ChangeState(MultiplayerUserState.Idle).FireAndForget();
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
            client.LoadRequested -= onLoadRequested;

            ornamentOverlayRegistration?.Dispose();
            ornamentOverlayRegistration = null;

            base.Dispose(isDisposing);
        }
    }
}
