// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Threading;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Queue
{
    public partial class ScreenQueue
    {
        /// <summary>
        /// Animation meant to convey the state of actively being in queue to the user.
        /// Doubles as a backdrop for the user avatar and metadata.
        /// </summary>
        public partial class InQueueAnimation : VisibilityContainer
        {
            public const int WIDTH = 250;

            private const int fade_duration = 500;
            private const int fade_stagger = 250;

            private const int outer_size = 200;
            private const int inner_size = 120;

            private const int sample_length = 2881;

            // values eyeballed by looking at the sample waveform in audacity
            private const int ping_duration = 270;
            private const int ping_stagger = 468;

            private static readonly IEasingFunction ring_pulse = new CubicBezierEasingFunction(0, 0.2, 0.15, 0.3);

            protected override bool StartHidden => true;

            private readonly Container background;
            private readonly CircularContainer outerRing;
            private readonly CircularContainer innerRing;

            private readonly List<CircularContainer> ringsInMotion = new List<CircularContainer>();

            private ScheduledDelegate? queueAnimationDelegate;

            private readonly Bindable<MatchmakingScreenState> currentState = new Bindable<MatchmakingScreenState>();

            [Resolved]
            private QueueController queue { get; set; } = null!;

            public InQueueAnimation()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                Children = new Drawable[]
                {
                    background = new Container
                    {
                        Size = new Vector2(WIDTH),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Alpha = 0,
                        Masking = true,
                        CornerRadius = WIDTH / 2f,
                        Children = new Drawable[]
                        {
                            new BufferedContainer
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Size = new Vector2(WIDTH),
                                Child = new Container
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(0),
                                    Masking = true,
                                    EdgeEffect = new EdgeEffectParameters
                                    {
                                        Type = EdgeEffectType.Shadow,
                                        Colour = Color4.Black.Opacity(0.75f),
                                        Radius = 180,
                                    },
                                },
                            },
                        },
                    },
                    outerRing = createOuterRing(),
                    innerRing = createInnerRing(),
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                currentState.BindTo(queue.CurrentState);
                currentState.BindValueChanged(onStateChanged, true);
            }

            private void onStateChanged(ValueChangedEvent<MatchmakingScreenState> e)
            {
                if (e.NewValue != MatchmakingScreenState.Queueing)
                {
                    queueAnimationDelegate?.Cancel();
                    queueAnimationDelegate = null;

                    foreach (var ring in ringsInMotion)
                        ring.FadeOut(200).Expire();
                }
                else
                {
                    queueAnimationDelegate = new ScheduledDelegate(() =>
                    {
                        var ring1 = createAnimationRing(260)
                            .With(r => r.OnLoadComplete += _ => r.FadeIn(ping_duration)
                                                                 .ResizeTo(r.Size + new Vector2(500), 5000 + ping_duration, ring_pulse)
                                                                 .Delay(ping_duration)
                                                                 .FadeOut(5270)
                                                                 .Expire());

                        var ring2 = createAnimationRing(180)
                            .With(r => r.OnLoadComplete += _ => r.Delay(ping_stagger)
                                                                 .FadeIn(ping_duration)
                                                                 .ResizeTo(r.Size + new Vector2(500), 5000 + ping_duration, ring_pulse)
                                                                 .Delay(ping_duration)
                                                                 .FadeOut(5270)
                                                                 .Expire());

                        AddRange([ring1, ring2]);
                        ringsInMotion.AddRange([ring1, ring2]);
                    }, Clock.CurrentTime + WAITING_LOOP_DELAY, sample_length);

                    Scheduler.Add(queueAnimationDelegate);
                }
            }

            private CircularContainer createOuterRing() => new CircularContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(outer_size),
                Alpha = 0,
                Masking = true,
                BorderColour = Color4.White.Opacity(0.05f),
                BorderThickness = 2,
                Child = new Box { RelativeSizeAxes = Axes.Both, Colour = Color4.Transparent },
            };

            private CircularContainer createInnerRing() => new CircularContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(inner_size),
                Alpha = 0,
                Masking = true,
                BorderColour = Color4.White.Opacity(0.2f),
                BorderThickness = 2,
                Child = new Box { RelativeSizeAxes = Axes.Both, Colour = Color4.Transparent },
            };

            private CircularContainer createAnimationRing(float size) => new CircularContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(size),
                Alpha = 0,
                Masking = true,
                BorderColour = Color4.White.Opacity(0.3f),
                BorderThickness = 2,
                Child = new Box { RelativeSizeAxes = Axes.Both, Colour = Color4.Transparent },
            };

            protected override void PopIn()
            {
                background.FadeIn(2000);

                using (BeginDelayedSequence(500))
                {
                    innerRing.FadeIn(fade_duration);
                    outerRing.Delay(fade_stagger).FadeIn(fade_duration);
                }
            }

            protected override void PopOut()
            {
            }
        }
    }
}
