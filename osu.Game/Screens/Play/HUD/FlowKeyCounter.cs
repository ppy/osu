// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Timing;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public partial class FlowKeyCounter : KeyCounter
    {
        /// <summary>
        /// Whether to display the trigger name.
        /// </summary>
        public BindableBool ShowTriggerName { get; } = new BindableBool(true);

        /// <summary>
        /// How long it takes an input to travel through the whole flow.
        /// </summary>
        public BindableDouble FlowDuration { get; } = new BindableDouble(500);

        public BindableColour4 AccentColour { get; } = new BindableColour4(Colour4.White);

        private const float fade_height = 0.5f;

        private readonly DrawablePool<FlowBar> barPool = new DrawablePool<FlowBar>(5);

        private Container flow = null!;
        private OsuSpriteText triggerName = null!;
        private FlowBar? activeBar;

        [Resolved]
        private GameplayClockContainer? gameplayClockContainer { get; set; }

        [Resolved]
        private IFrameStableClock? frameStableClock { get; set; }

        private readonly ManualClock flowClockSource = new ManualClock
        {
            Rate = 1,
        };

        public FlowKeyCounter(InputTrigger trigger)
            : base(trigger)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Height = 200f;
            Width = 50f;

            Children = new Drawable[]
            {
                barPool,
                new BufferedContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Bottom = 14 },
                    Children = new Drawable[]
                    {
                        flow = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Clock = new FramedClock(flowClockSource)
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Height = fade_height,
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Blending = new BlendingParameters
                            {
                                RGBEquation = BlendingEquation.Add,
                                Source = BlendingType.Zero,
                                Destination = BlendingType.One,
                                AlphaEquation = BlendingEquation.Add,
                                SourceAlpha = BlendingType.Zero,
                                DestinationAlpha = BlendingType.OneMinusSrcAlpha
                            },
                            Colour = ColourInfo.GradientVertical(
                                Color4.White.Opacity(1f),
                                Color4.White.Opacity(0f)
                            ),
                        },
                    },
                },
                triggerName = new OsuSpriteText
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Text = Trigger.Name,
                    Font = OsuFont.Torus.With(size: 14, weight: FontWeight.Bold),
                }
            };
        }

        protected override void Update()
        {
            base.Update();

            // Pause flow when gameplay pause.
            bool running = gameplayClockContainer?.IsPaused.Value != true;

            flowClockSource.IsRunning = running;

            if (running)
                flowClockSource.CurrentTime += Time.Elapsed;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ShowTriggerName.BindValueChanged(s =>
            {
                if (s.NewValue)
                    triggerName.FadeIn(100);
                else
                    triggerName.FadeOut(100);
            }, true);

            IsActive.BindValueChanged(active =>
            {
                if (frameStableClock?.IsCatchingUp.Value == true)
                {
                    endFlow();
                    return;
                }

                if (active.NewValue)
                    beginFlow();
                else
                    endFlow();
            }, true);

            FlowDuration.BindValueChanged(_ => clearFlow());

            if (gameplayClockContainer != null)
                gameplayClockContainer.OnSeek += clearFlow;
        }

        private void beginFlow()
        {
            activeBar = barPool.Get(bar =>
            {
                bar.Reset();
                bar.Colour = AccentColour.Value;
            });

            flow.Add(activeBar);
            activeBar.Begin(FlowDuration.Value);
        }

        private void endFlow()
        {
            var bar = activeBar;
            activeBar = null;

            bar?.Release(FlowDuration.Value);
        }

        private void clearFlow()
        {
            foreach (var drawable in flow.Children)
            {
                drawable.ClearTransforms();
                drawable.Expire();
            }

            activeBar = null;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (gameplayClockContainer != null)
                gameplayClockContainer.OnSeek -= clearFlow;
        }

        private partial class FlowBar : PoolableDrawable
        {
            public FlowBar()
            {
                RelativeSizeAxes = Axes.Both;
                RelativePositionAxes = Axes.Y;

                Anchor = Anchor.BottomCentre;
                Origin = Anchor.BottomCentre;

                CornerRadius = 5;
                Masking = true;

                InternalChild = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                };
            }

            public void Reset()
            {
                ClearTransforms(true);

                Position = Vector2.Zero;
                Height = 0;
                Alpha = 1;
            }

            public void Begin(double duration)
            {
                this.ResizeHeightTo(1, duration);
            }

            public void Release(double duration)
            {
                ClearTransforms(targetMember: nameof(Height));

                this.MoveToY(-1, duration)
                    .Expire();
            }
        }
    }
}
