// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Matchmaking;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays;
using osu.Game.Overlays.Volume;
using osu.Game.Rulesets;
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

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private QueueController controller { get; set; } = null!;

        [Resolved]
        private UserLookupCache userLookupCache { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private MusicController musicController { get; set; } = null!;

        private readonly IBindable<MatchmakingScreenState> currentState = new Bindable<MatchmakingScreenState>();

        private readonly Bindable<MatchmakingPool[]> availablePools = new Bindable<MatchmakingPool[]>([]);
        private readonly Bindable<MatchmakingPool?> selectedPool = new Bindable<MatchmakingPool?>();

        private readonly MatchmakingPoolType poolType;

        private CancellationTokenSource userLookupCancellation = new CancellationTokenSource();

        private Sample? enqueueSample;
        private Sample? waitingLoopSample;
        private Sample? matchFoundSample;

        private SampleChannel? waitingLoopChannel;
        private ScheduledDelegate? startLoopPlaybackDelegate;

        public ScreenQueue(MatchmakingPoolType poolType)
        {
            this.poolType = poolType;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            InternalChild = new InverseScalingDrawSizePreservingFillContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new GlobalScrollAdjustsVolume(),
                    cloud = new CloudVisualisation
                    {
                        Y = -100,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.6f)
                    },
                    new MatchmakingAvatar(api.LocalUser.Value, true)
                    {
                        Y = -100,
                        Scale = new Vector2(3),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    new Container
                    {
                        RelativePositionAxes = Axes.Y,
                        Y = 0.25f,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        CornerRadius = 10f,
                        Masking = true,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Colour = colourProvider.Background3,
                                RelativeSizeAxes = Axes.Both,
                            },
                            mainContent = new Container
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Alpha = 0,
                                AutoSizeAxes = Axes.Both,
                                AutoSizeDuration = 300,
                                AutoSizeEasing = Easing.OutQuint,
                                Padding = new MarginPadding(20),
                            },
                        }
                    },
                }
            };

            currentState.BindTo(controller.CurrentState);
            currentState.BindValueChanged(s => SetState(s.NewValue));

            client.MatchmakingLobbyStatusChanged += onMatchmakingLobbyStatusChanged;

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

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            enqueueSample = audio.Samples.Get(@"Multiplayer/Matchmaking/enqueue");
            waitingLoopSample = audio.Samples.Get(@"Multiplayer/Matchmaking/waiting-loop");
            matchFoundSample = audio.Samples.Get(@"Multiplayer/Matchmaking/match-found");
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
        });

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            controller.SearchInForeground();

            client.MatchmakingJoinLobby().FireAndForget();

            using (BeginDelayedSequence(800))
                Schedule(() => SetState(currentState.Value));
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);

            client.MatchmakingJoinLobby().FireAndForget();
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
                    controller.SearchInBackground();
                    return false;

                case MatchmakingScreenState.PendingAccept:
                case MatchmakingScreenState.AcceptedWaitingForRoom:
                    controller.LeaveQueue();
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
                                    controller.JoinQueue(selectedPool.Value);
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
                        Spacing = new Vector2(20),
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
                                Action = () => controller.LeaveQueue()
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
                    musicController.DuckMomentarily(1250);
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
                        Schedule(() =>
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

            waitingLoopChannel = waitingLoopSample?.GetChannel();
            if (waitingLoopChannel == null)
                return;

            waitingLoopChannel.Looping = true;
            waitingLoopChannel?.Play();
        }

        private void stopWaitingLoopPlayback()
        {
            waitingLoopChannel?.Stop();
            waitingLoopChannel?.Dispose();
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
