// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.Edit.Timing
{
    internal partial class TapButton : CircularContainer, IKeyBindingHandler<GlobalAction>
    {
        public const float SIZE = 140;

        public readonly BindableBool IsHandlingTapping = new BindableBool();

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private Bindable<ControlPointGroup>? selectedGroup { get; set; }

        [Resolved]
        private IBeatSyncProvider? beatSyncSource { get; set; }

        private Circle hoverLayer = null!;

        private CircularContainer innerCircle = null!;
        private Box innerCircleHighlight = null!;

        private int currentLight;

        private Container scaleContainer = null!;
        private Container lights = null!;
        private Container lightsGlow = null!;
        private OsuSpriteText bpmText = null!;
        private Container textContainer = null!;

        private bool grabbedMouseDown;

        private ScheduledDelegate? resetDelegate;

        private const int light_count = 8;

        private const int initial_taps_to_ignore = 4;

        private const int max_taps_to_consider = 128;

        private const double transition_length = 500;

        private const float angular_light_gap = 0.007f;

        private readonly List<double> tapTimings = new List<double>();

        [BackgroundDependencyLoader]
        private void load()
        {
            Size = new Vector2(SIZE);

            const float ring_width = 22;
            const float light_padding = 3;

            InternalChild = scaleContainer = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Circle
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background4
                    },
                    lights = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    new CircularContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Name = @"outer masking",
                        Masking = true,
                        BorderThickness = light_padding,
                        BorderColour = colourProvider.Background4,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Colour = Color4.Black,
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                                AlwaysPresent = true,
                            },
                        }
                    },
                    new Circle
                    {
                        Name = @"inner masking",
                        Size = new Vector2(SIZE - ring_width * 2 + light_padding * 2),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = colourProvider.Background4,
                    },
                    lightsGlow = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    innerCircle = new CircularContainer
                    {
                        Size = new Vector2(SIZE - ring_width * 2),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Masking = true,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Colour = colourProvider.Background2,
                                RelativeSizeAxes = Axes.Both,
                            },
                            innerCircleHighlight = new Box
                            {
                                Colour = colourProvider.Colour3,
                                Blending = BlendingParameters.Additive,
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                            },
                            textContainer = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colourProvider.Background1,
                                Children = new Drawable[]
                                {
                                    new OsuSpriteText
                                    {
                                        Font = OsuFont.Torus.With(size: 34, weight: FontWeight.SemiBold),
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.BottomCentre,
                                        Y = 5,
                                        Text = "Tap",
                                    },
                                    bpmText = new OsuSpriteText
                                    {
                                        Font = OsuFont.Torus.With(size: 23, weight: FontWeight.Regular),
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.TopCentre,
                                        Y = -1,
                                    },
                                }
                            },
                            hoverLayer = new Circle
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colourProvider.Background1.Opacity(0.3f),
                                Blending = BlendingParameters.Additive,
                                Alpha = 0,
                            },
                        }
                    },
                }
            };

            for (int i = 0; i < light_count; i++)
            {
                var light = new Light
                {
                    Rotation = (i + 1) * (360f / light_count) + 360 * angular_light_gap / 2,
                };

                lights.Add(light);
                lightsGlow.Add(light.Glow.CreateProxy());
            }

            reset();
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
            hoverLayer.ReceivePositionalInputAt(screenSpacePos);

        private ColourInfo textColour
        {
            get
            {
                if (grabbedMouseDown)
                    return colourProvider.Background4;

                if (IsHovered)
                    return colourProvider.Content2;

                return colourProvider.Background1;
            }
        }

        protected override bool OnHover(HoverEvent e)
        {
            hoverLayer.FadeIn(transition_length, Easing.OutQuint);
            textContainer.FadeColour(textColour, transition_length, Easing.OutQuint);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hoverLayer.FadeOut(transition_length, Easing.OutQuint);
            textContainer.FadeColour(textColour, transition_length, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            const double in_duration = 100;

            grabbedMouseDown = true;
            IsHandlingTapping.Value = true;

            resetDelegate?.Cancel();

            handleTap();

            textContainer.FadeColour(textColour, in_duration, Easing.OutQuint);

            scaleContainer.ScaleTo(0.99f, in_duration, Easing.OutQuint);
            innerCircle.ScaleTo(0.96f, in_duration, Easing.OutQuint);

            innerCircleHighlight
                .FadeIn(50, Easing.OutQuint)
                .FlashColour(Color4.White, 1000, Easing.OutQuint);

            lights[currentLight % light_count].Hide();
            lights[(currentLight + light_count / 2) % light_count].Hide();

            currentLight++;

            lights[currentLight % light_count].Show();
            lights[(currentLight + light_count / 2) % light_count].Show();

            return true;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            const double out_duration = 800;

            grabbedMouseDown = false;

            textContainer.FadeColour(textColour, out_duration, Easing.OutQuint);

            scaleContainer.ScaleTo(1, out_duration, Easing.OutQuint);
            innerCircle.ScaleTo(1, out_duration, Easing.OutQuint);

            innerCircleHighlight.FadeOut(out_duration, Easing.OutQuint);

            resetDelegate = Scheduler.AddDelayed(reset, 1000);

            base.OnMouseUp(e);
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Action == GlobalAction.EditorTapForBPM && !e.Repeat)
            {
                // Direct through mouse handling to achieve animation
                OnMouseDown(new MouseDownEvent(e.CurrentState, MouseButton.Left));
                return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
            if (e.Action == GlobalAction.EditorTapForBPM)
                OnMouseUp(new MouseUpEvent(e.CurrentState, MouseButton.Left));
        }

        private void handleTap()
        {
            if (selectedGroup?.Value == null)
                return;

            tapTimings.Add(Clock.CurrentTime);

            if (tapTimings.Count > initial_taps_to_ignore + max_taps_to_consider)
                tapTimings.RemoveAt(0);

            if (tapTimings.Count < initial_taps_to_ignore * 2)
            {
                bpmText.Text = new string('.', tapTimings.Count);
                return;
            }

            double averageBeatLength = (tapTimings.Last() - tapTimings.Skip(initial_taps_to_ignore).First()) / (tapTimings.Count - initial_taps_to_ignore - 1);
            double clockRate = beatSyncSource?.Clock.Rate ?? 1;

            double bpm = Math.Round(60000 / averageBeatLength / clockRate);

            bpmText.Text = $"{bpm} BPM";

            var timingPoint = selectedGroup?.Value.ControlPoints.OfType<TimingControlPoint>().FirstOrDefault();

            if (timingPoint != null)
            {
                // Intentionally use the rounded BPM here.
                timingPoint.BeatLength = 60000 / bpm;
            }
        }

        private void reset()
        {
            bpmText.FadeOut(transition_length, Easing.OutQuint);

            using (BeginDelayedSequence(tapTimings.Count > 0 ? transition_length : 0))
            {
                Schedule(() => bpmText.Text = "the beat!");
                bpmText.FadeIn(800, Easing.OutQuint);
            }

            foreach (var light in lights)
                light.Hide();

            tapTimings.Clear();
            currentLight = 0;
            IsHandlingTapping.Value = false;
        }

        private partial class Light : CompositeDrawable
        {
            public Drawable Glow { get; private set; } = null!;

            private Container fillContent = null!;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                RelativeSizeAxes = Axes.Both;
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                Size = new Vector2(0.98f); // Avoid bleed into masking edge.

                InternalChildren = new Drawable[]
                {
                    new CircularProgress
                    {
                        RelativeSizeAxes = Axes.Both,
                        Progress = 1f / light_count - angular_light_gap,
                        Colour = colourProvider.Background2,
                    },
                    fillContent = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        Colour = colourProvider.Colour1,
                        Children = new[]
                        {
                            new CircularProgress
                            {
                                RelativeSizeAxes = Axes.Both,
                                Progress = 1f / light_count - angular_light_gap,
                                Blending = BlendingParameters.Additive
                            },
                            // Please do not try and make sense of this.
                            // Getting the visual effect I was going for relies on what I can only imagine is broken implementation
                            // of `PadExtent`. If that's ever fixed in the future this will likely need to be adjusted.
                            Glow = new CircularProgress
                            {
                                RelativeSizeAxes = Axes.Both,
                                Progress = 1f / light_count - 0.01f,
                                Blending = BlendingParameters.Additive
                            }.WithEffect(new GlowEffect
                            {
                                Colour = colourProvider.Colour1.Opacity(0.4f),
                                BlurSigma = new Vector2(9f),
                                Strength = 10,
                                PadExtent = true
                            }),
                        }
                    },
                };
            }

            public override void Show()
            {
                fillContent
                    .FadeIn(50, Easing.OutQuint)
                    .FlashColour(Color4.White, 1000, Easing.OutQuint);
            }

            public override void Hide()
            {
                fillContent
                    .FadeOut(300, Easing.OutQuint);
            }
        }
    }
}
