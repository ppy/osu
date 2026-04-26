// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Matchmaking;
using osu.Game.Online.Matchmaking.Requests;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Overlays;
using osu.Game.Overlays.Volume;
using osu.Game.Rulesets;
using osu.Game.Screens.Footer;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Queue
{
    /// <summary>
    /// The initial screen that users arrive at when preparing for a quick play session.
    /// </summary>
    public partial class ScreenQueue : OsuScreen
    {
        public override bool ShowFooter => true;

        public override bool? ApplyModTrackAdjustments => false;

        private Container mainContent = null!;
        private CloudVisualisation cloud = null!;
        private RatingDistributionGraph ratingGraph = null!;
        private FillFlowContainer resultPanelContainer = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private QueueController queue { get; set; } = null!;

        [Resolved]
        private UserLookupCache userLookupCache { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private MusicController music { get; set; } = null!;

        private readonly IBindable<MatchmakingScreenState> currentState = new Bindable<MatchmakingScreenState>();

        private readonly Bindable<MatchmakingPool[]> availablePools = new Bindable<MatchmakingPool[]>([]);
        private readonly Bindable<MatchmakingPool?> selectedPool = new Bindable<MatchmakingPool?>();

        private readonly MatchmakingPoolType poolType;

        private CancellationTokenSource userLookupCancellation = new CancellationTokenSource();

        private Sample? enqueueSample;
        private Sample? matchFoundSample;

        private SampleChannel? waitingLoopChannel;
        private ScheduledDelegate? startLoopPlaybackDelegate;
        private DrawableSample waitingLoop = null!;
        private ScheduledDelegate? pushScreenDelegate;

        private int? userRating;

        private GridContainer mainGrid = null!;

        public ScreenQueue(MatchmakingPoolType poolType)
        {
            this.poolType = poolType;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, IAPIProvider api)
        {
            enqueueSample = audio.Samples.Get(@"Multiplayer/Matchmaking/enqueue");
            matchFoundSample = audio.Samples.Get(@"Multiplayer/Matchmaking/match-found");

            InternalChild = new InverseScalingDrawSizePreservingFillContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    waitingLoop = new DrawableSample(audio.Samples.Get(@"Multiplayer/Matchmaking/waiting-loop")),
                    new GlobalScrollAdjustsVolume(),
                    mainGrid = new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding
                        {
                            Horizontal = 20,
                            Top = 20,
                            Bottom = ScreenFooter.HEIGHT + 20
                        },
                        RowDimensions =
                        [
                            new Dimension(),
                            new Dimension(GridSizeMode.Relative, RuntimeInfo.IsMobile ? 0.55f : 0.35f)
                        ],
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding(5),
                                    Child = new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        CornerRadius = 10f,
                                        Masking = true,
                                        Children = new Drawable[]
                                        {
                                            new PanelBackground(),
                                            new GridContainer
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Padding = new MarginPadding(10),
                                                RowDimensions =
                                                [
                                                    new Dimension(GridSizeMode.AutoSize)
                                                ],
                                                Content = new[]
                                                {
                                                    new Drawable[] { new QueueSectionHeader("Queued players") },
                                                    new Drawable[]
                                                    {
                                                        new Container
                                                        {
                                                            RelativeSizeAxes = Axes.Both,
                                                            Children = new Drawable[]
                                                            {
                                                                cloud = new CloudVisualisation
                                                                {
                                                                    Anchor = Anchor.Centre,
                                                                    Origin = Anchor.Centre,
                                                                    RelativeSizeAxes = Axes.Both,
                                                                    Size = new Vector2(0.6f)
                                                                },
                                                                new MatchmakingAvatar(api.LocalUser.Value, true)
                                                                {
                                                                    Anchor = Anchor.Centre,
                                                                    Origin = Anchor.Centre,
                                                                    Scale = new Vector2(3),
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding(5),
                                    Child = new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        CornerRadius = 10f,
                                        Masking = true,
                                        Children = new Drawable[]
                                        {
                                            new PanelBackground(),
                                            new GridContainer
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Padding = new MarginPadding(10) { Bottom = 0 },
                                                RowDimensions =
                                                [
                                                    new Dimension(GridSizeMode.AutoSize)
                                                ],
                                                Content = new[]
                                                {
                                                    new Drawable[] { new QueueSectionHeader("Recent Matches") },
                                                    new Drawable[]
                                                    {
                                                        new OsuScrollContainer(Direction.Vertical)
                                                        {
                                                            RelativeSizeAxes = Axes.Both,
                                                            ScrollbarOverlapsContent = false,
                                                            Child = resultPanelContainer = new FillFlowContainer
                                                            {
                                                                RelativeSizeAxes = Axes.X,
                                                                AutoSizeAxes = Axes.Y,
                                                                Spacing = new Vector2(10),
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
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding(5),
                                    Child = new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        CornerRadius = 10f,
                                        Masking = true,
                                        Children = new Drawable[]
                                        {
                                            new PanelBackground(),
                                            new GridContainer
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Padding = new MarginPadding(10),
                                                RowDimensions =
                                                [
                                                    new Dimension(GridSizeMode.AutoSize)
                                                ],
                                                Content = new[]
                                                {
                                                    new Drawable[] { new QueueSectionHeader("Queues") },
                                                    new Drawable[]
                                                    {
                                                        mainContent = new Container
                                                        {
                                                            RelativeSizeAxes = Axes.Both,
                                                            Padding = new MarginPadding(20),
                                                            Alpha = 0,
                                                        },
                                                    }
                                                }
                                            }
                                        }
                                    }
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding(5),
                                    Child = new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        CornerRadius = 10f,
                                        Masking = true,
                                        Children = new Drawable[]
                                        {
                                            new PanelBackground(),
                                            new GridContainer
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Padding = new MarginPadding(10),
                                                RowDimensions =
                                                [
                                                    new Dimension(GridSizeMode.AutoSize)
                                                ],
                                                Content = new[]
                                                {
                                                    new Drawable[] { new QueueSectionHeader("Ratings") },
                                                    new Drawable[]
                                                    {
                                                        new Container
                                                        {
                                                            RelativeSizeAxes = Axes.Both,
                                                            Padding = new MarginPadding { Top = -10 },
                                                            Child = ratingGraph = new RatingDistributionGraph
                                                            {
                                                                RelativeSizeAxes = Axes.Both,
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
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            int delay = 0;

            foreach (var a in mainGrid.Content)
            {
                foreach (var d in a)
                {
                    d.FadeOut()
                     .Delay(delay)
                     .FadeInFromZero(500, Easing.OutQuint);

                    delay += 100;
                }
            }

            currentState.BindTo(queue.CurrentState);
            currentState.BindValueChanged(s => SetState(s.NewValue));
            client.MatchmakingLobbyStatusChanged += onMatchmakingLobbyStatusChanged;
            selectedPool.BindValueChanged(onSelectedPoolChanged, true);
            populateAvailablePools().FireAndForget();
        }

        private async Task populateAvailablePools()
        {
            MatchmakingPool[] pools = await client.GetMatchmakingPoolsOfType(poolType).ConfigureAwait(false);

            Schedule(() =>
            {
                availablePools.Value = pools;

                // Default to the user's ruleset for the initial pool selection.
                selectedPool.Value = pools.FirstOrDefault(p => p.RulesetId == ruleset.Value.OnlineID) ?? pools.FirstOrDefault();
            });
        }

        private void onMatchmakingLobbyStatusChanged(MatchmakingLobbyStatus status) => Scheduler.Add(() =>
        {
            userLookupCancellation.Cancel();
            var cancellation = userLookupCancellation = new CancellationTokenSource();

            userLookupCache.GetUsersAsync(status.UsersInQueue, cancellation.Token)
                           .ContinueWith(result => Schedule(() =>
                           {
                               APIUser?[] users = result.GetResultSafely();
                               if (!cancellation.IsCancellationRequested)
                                   Users = users.OfType<APIUser>().ToArray();
                           }), cancellation.Token);

            // Global (incremental) updates will not contain the user rating, so keep the one we already received from initial status data.
            if (status.UserRating != null)
                userRating = status.UserRating;

            ratingGraph.SetData(status.RatingDistribution, userRating);

            foreach (var state in status.RecentMatches.OfType<RankedPlayRoomState>())
            {
                resultPanelContainer.Insert(-resultPanelContainer.Count, new DelayedLoadWrapper(new RankedPlayMatchPanel(state)
                {
                    RelativeSizeAxes = Axes.X,
                    Width = 1
                }, 0)
                {
                    RelativeSizeAxes = Axes.X,
                    Width = 0.48f
                });
            }
        });

        private void onSelectedPoolChanged(ValueChangedEvent<MatchmakingPool?> e)
        {
            userRating = null;
            ratingGraph.SetData([], null);
            resultPanelContainer.Clear();

            if (e.NewValue == null)
            {
                client.MatchmakingLeaveLobby().FireAndForget();
                return;
            }

            client.MatchmakingJoinLobbyWithParams(new MatchmakingJoinLobbyRequest
            {
                PoolId = e.NewValue.Id
            }).FireAndForget();
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            queue.SearchInForeground();

            using (BeginDelayedSequence(800))
                Schedule(() => SetState(currentState.Value));
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);

            // Rejoin the lobby.
            selectedPool.TriggerChange();
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            base.OnSuspending(e);

            client.MatchmakingLeaveLobby().FireAndForget();
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            if (base.OnExiting(e))
                return true;

            client.MatchmakingLeaveLobby().FireAndForget();

            switch (currentState.Value)
            {
                default:
                    return false;

                case MatchmakingScreenState.Queueing:
                    queue.SearchInBackground();
                    return false;

                case MatchmakingScreenState.PendingAccept:
                case MatchmakingScreenState.AcceptedWaitingForRoom:
                    queue.LeaveQueue();
                    return true;

                case MatchmakingScreenState.InRoom:
                    // Block exit until it's initiated from inside the matchmaking screen.
                    return true;
            }
        }

        public APIUser[] Users
        {
            set => cloud.Users = value;
        }

        public void SetState(MatchmakingScreenState newState)
        {
            mainContent.FadeInFromZero(500, Easing.OutQuint);
            mainContent.Clear();

            startLoopPlaybackDelegate?.Cancel();
            stopWaitingLoopPlayback();

            pushScreenDelegate?.Cancel();
            pushScreenDelegate = null;

            switch (newState)
            {
                case MatchmakingScreenState.Idle:
                    mainContent.Child = new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(10),
                        Children = new Drawable[]
                        {
                            new PoolSelector
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                AvailablePools = { BindTarget = availablePools },
                                SelectedPool = { BindTarget = selectedPool }
                            },
                            new BeginQueueingButton
                            {
                                DarkerColour = colours.Blue2,
                                LighterColour = colours.Blue1,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Width = 200,
                                SelectedPool = { BindTarget = selectedPool },
                                Action = () =>
                                {
                                    Debug.Assert(selectedPool.Value != null);
                                    queue.JoinQueue(selectedPool.Value);
                                },
                                Text = "Begin queueing",
                            }
                        }
                    };
                    break;

                case MatchmakingScreenState.Queueing:
                    mainContent.Child = new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(15),
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "Waiting for a game...",
                                Font = OsuFont.GetFont(size: 32, weight: FontWeight.Light, typeface: Typeface.TorusAlternate),
                            },
                            new LoadingSpinner
                            {
                                State = { Value = Visibility.Visible },
                            },
                            new ShearedButton
                            {
                                DarkerColour = colours.Red3,
                                LighterColour = colours.Red4,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Width = 200,
                                Text = "Stop queueing",
                                Action = () => queue.LeaveQueue()
                            }
                        }
                    };

                    enqueueSample?.Play();
                    startLoopPlaybackDelegate = Scheduler.AddDelayed(startWaitingLoopPlayback, 2000);
                    break;

                case MatchmakingScreenState.PendingAccept:
                    client.MatchmakingAcceptInvitation().FireAndForget();
                    SetState(MatchmakingScreenState.AcceptedWaitingForRoom);

                    matchFoundSample?.Play();
                    music.DuckMomentarily(1250);
                    break;

                case MatchmakingScreenState.AcceptedWaitingForRoom:
                    mainContent.Child = new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(20),
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "Waiting for opponents...",
                                Font = OsuFont.GetFont(size: 32, weight: FontWeight.Light, typeface: Typeface.TorusAlternate),
                            },
                            new LoadingSpinner
                            {
                                State = { Value = Visibility.Visible },
                            },
                        }
                    };

                    startWaitingLoopPlayback();
                    break;

                case MatchmakingScreenState.InRoom:
                    // room received, show users and transition to next screen.
                    mainContent.Child = new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(20),
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "Good luck!",
                                Font = OsuFont.GetFont(size: 32, weight: FontWeight.Light, typeface: Typeface.TorusAlternate),
                            },
                        }
                    };

                    using (BeginDelayedSequence(2000))
                    {
                        pushScreenDelegate = Schedule(() =>
                        {
                            switch (poolType)
                            {
                                case MatchmakingPoolType.QuickPlay:
                                    this.Push(new ScreenMatchmaking(client.Room!));
                                    break;

                                case MatchmakingPoolType.RankedPlay:
                                    this.Push(new RankedPlayScreen(client.Room!));
                                    break;
                            }
                        });
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            stopWaitingLoopPlayback();

            if (client.IsNotNull())
                client.MatchmakingLobbyStatusChanged -= onMatchmakingLobbyStatusChanged;
        }

        public enum MatchmakingScreenState
        {
            Idle,
            Queueing,
            PendingAccept,
            AcceptedWaitingForRoom,
            InRoom
        }

        private void startWaitingLoopPlayback()
        {
            stopWaitingLoopPlayback();

            waitingLoopChannel = waitingLoop.GetChannel();
            if (waitingLoopChannel == null)
                return;

            waitingLoopChannel.Looping = true;
            waitingLoopChannel?.Play();

            waitingLoop.VolumeTo(1)
                       .Delay(2000)
                       .VolumeTo(0, 12000);
        }

        private void stopWaitingLoopPlayback()
        {
            waitingLoopChannel?.Stop();
            waitingLoopChannel?.Dispose();
        }

        public partial class PanelBackground : CompositeDrawable
        {
            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                RelativeSizeAxes = Axes.Both;

                InternalChild = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background3,
                    Blending = BlendingParameters.Additive,
                    Alpha = 0.3f,
                };
            }
        }

        public partial class QueueSectionHeader : SectionHeader
        {
            public QueueSectionHeader(string header)
                : base(header)
            {
                // Reduce base class padding.
                Margin = new MarginPadding { Top = 5, Bottom = 10, Horizontal = 5 };
            }
        }

        private partial class BeginQueueingButton : SelectionButton
        {
            public readonly IBindable<MatchmakingPool?> SelectedPool = new Bindable<MatchmakingPool?>();

            protected override void LoadComplete()
            {
                base.LoadComplete();

                SelectedPool.BindValueChanged(p => Enabled.Value = p.NewValue != null, true);
            }
        }

        private partial class SelectionButton : ShearedButton, IKeyBindingHandler<GlobalAction>
        {
            public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
            {
                if (e.Action == GlobalAction.Select && !e.Repeat)
                {
                    TriggerClickWithSound();
                    return true;
                }

                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
            {
            }
        }
    }
}
