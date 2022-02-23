// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osuTK;
using osuTK.Input;

#nullable enable

namespace osu.Game.Overlays.Mods
{
    public class ModPanel : OsuClickableContainer
    {
        public Mod Mod { get; }
        public BindableBool Active { get; } = new BindableBool();

        protected readonly Box Background;
        protected readonly Container SwitchContainer;
        protected readonly Container MainContentContainer;
        protected readonly Box TextBackground;
        protected readonly FillFlowContainer TextFlow;

        [Resolved]
        protected OverlayColourProvider ColourProvider { get; private set; } = null!;

        protected const double TRANSITION_DURATION = 150;
        protected const float SHEAR_X = 0.2f;

        protected const float HEIGHT = 42;
        protected const float CORNER_RADIUS = 7;
        protected const float IDLE_SWITCH_WIDTH = 54;
        protected const float EXPANDED_SWITCH_WIDTH = 70;

        private Colour4 activeColour;
        private Colour4 activeHoverColour;

        private Sample? sampleOff;
        private Sample? sampleOn;

        public ModPanel(Mod mod)
        {
            Mod = mod;

            RelativeSizeAxes = Axes.X;
            Height = 42;

            // all below properties are applied to `Content` rather than the `ModPanel` in its entirety
            // to allow external components to set these properties on the panel without affecting
            // its "internal" appearance.
            Content.Masking = true;
            Content.CornerRadius = CORNER_RADIUS;
            Content.BorderThickness = 2;
            Content.Shear = new Vector2(SHEAR_X, 0);

            Children = new Drawable[]
            {
                Background = new Box
                {
                    RelativeSizeAxes = Axes.Both
                },
                SwitchContainer = new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    Child = new ModSwitchSmall(mod)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Active = { BindTarget = Active },
                        Shear = new Vector2(-SHEAR_X, 0),
                        Scale = new Vector2(HEIGHT / ModSwitchSmall.DEFAULT_SIZE)
                    }
                },
                MainContentContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = CORNER_RADIUS,
                        Children = new Drawable[]
                        {
                            TextBackground = new Box
                            {
                                RelativeSizeAxes = Axes.Both
                            },
                            TextFlow = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding
                                {
                                    Horizontal = 17.5f,
                                    Vertical = 4
                                },
                                Direction = FillDirection.Vertical,
                                Children = new[]
                                {
                                    new OsuSpriteText
                                    {
                                        Text = mod.Name,
                                        Font = OsuFont.TorusAlternate.With(size: 18, weight: FontWeight.SemiBold),
                                        Shear = new Vector2(-SHEAR_X, 0),
                                        Margin = new MarginPadding
                                        {
                                            Left = -18 * SHEAR_X
                                        }
                                    },
                                    new OsuSpriteText
                                    {
                                        Text = mod.Description,
                                        Font = OsuFont.Default.With(size: 12),
                                        RelativeSizeAxes = Axes.X,
                                        Truncate = true,
                                        Shear = new Vector2(-SHEAR_X, 0)
                                    }
                                }
                            }
                        }
                    }
                }
            };

            Action = Active.Toggle;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuColour colours)
        {
            sampleOn = audio.Samples.Get(@"UI/check-on");
            sampleOff = audio.Samples.Get(@"UI/check-off");

            activeColour = colours.ForModType(Mod.Type);
            activeHoverColour = activeColour.Lighten(0.3f);
        }

        protected override HoverSounds CreateHoverSounds(HoverSampleSet sampleSet) => new HoverSounds(sampleSet);

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Active.BindValueChanged(_ =>
            {
                playStateChangeSamples();
                UpdateState();
            });

            UpdateState();
            FinishTransforms(true);
        }

        private void playStateChangeSamples()
        {
            if (Active.Value)
                sampleOn?.Play();
            else
                sampleOff?.Play();
        }

        protected override bool OnHover(HoverEvent e)
        {
            UpdateState();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            UpdateState();
            base.OnHoverLost(e);
        }

        private double? mouseDownTime;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button == MouseButton.Left)
                mouseDownTime = Time.Current;
            return true;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            mouseDownTime = null;
            base.OnMouseUp(e);
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            mouseDownTime = null;
            return true;
        }

        protected override void Update()
        {
            base.Update();

            if (mouseDownTime != null)
            {
                double startTime = mouseDownTime.Value;
                double endTime = startTime + 600;

                float startValue = Active.Value ? EXPANDED_SWITCH_WIDTH : IDLE_SWITCH_WIDTH;
                float endValue = IDLE_SWITCH_WIDTH + (EXPANDED_SWITCH_WIDTH - IDLE_SWITCH_WIDTH) * (Active.Value ? 0.2f : 0.8f);

                float targetWidth = Interpolation.ValueAt<float>(Math.Clamp(Time.Current, startTime, endTime), startValue, endValue, startTime, endTime, Easing.OutQuint);

                SwitchContainer.Width = targetWidth;
                MainContentContainer.Padding = new MarginPadding
                {
                    Left = targetWidth,
                    Right = CORNER_RADIUS
                };
            }
        }

        protected virtual void UpdateState()
        {
            if (Active.Value)
            {
                Colour4 backgroundTextColour = IsHovered ? activeHoverColour : activeColour;
                Colour4 backgroundColour = Interpolation.ValueAt<Colour4>(0.7f, Colour4.Black, backgroundTextColour, 0, 1);

                Content.TransformTo(nameof(BorderColour), (ColourInfo)backgroundColour, TRANSITION_DURATION, Easing.OutQuint);
                Background.FadeColour(backgroundColour, TRANSITION_DURATION, Easing.OutQuint);
                SwitchContainer.ResizeWidthTo(EXPANDED_SWITCH_WIDTH, TRANSITION_DURATION, Easing.OutQuint);
                MainContentContainer.TransformTo(nameof(Padding), new MarginPadding
                {
                    Left = EXPANDED_SWITCH_WIDTH,
                    Right = CORNER_RADIUS
                }, TRANSITION_DURATION, Easing.OutQuint);
                TextBackground.FadeColour(backgroundTextColour, TRANSITION_DURATION, Easing.OutQuint);
                TextFlow.FadeColour(ColourProvider.Background6, TRANSITION_DURATION, Easing.OutQuint);
            }
            else
            {
                Colour4 backgroundColour = ColourProvider.Background3;
                if (IsHovered)
                    backgroundColour = Interpolation.ValueAt<Colour4>(0.25f, backgroundColour, activeColour, 0, 1);

                Colour4 textBackgroundColour = ColourProvider.Background2;
                if (IsHovered)
                    textBackgroundColour = Interpolation.ValueAt<Colour4>(0.25f, textBackgroundColour, activeColour, 0, 1);

                Content.TransformTo(nameof(BorderColour), ColourInfo.GradientVertical(backgroundColour, textBackgroundColour), TRANSITION_DURATION, Easing.OutQuint);
                Background.FadeColour(backgroundColour, TRANSITION_DURATION, Easing.OutQuint);
                SwitchContainer.ResizeWidthTo(IDLE_SWITCH_WIDTH, TRANSITION_DURATION, Easing.OutQuint);
                MainContentContainer.TransformTo(nameof(Padding), new MarginPadding
                {
                    Left = IDLE_SWITCH_WIDTH,
                    Right = CORNER_RADIUS
                }, TRANSITION_DURATION, Easing.OutQuint);
                TextBackground.FadeColour(textBackgroundColour, TRANSITION_DURATION, Easing.OutQuint);
                TextFlow.FadeColour(Colour4.White, TRANSITION_DURATION, Easing.OutQuint);
            }
        }
    }
}
