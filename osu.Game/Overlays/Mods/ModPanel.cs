// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

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
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osuTK;
using osuTK.Input;

namespace osu.Game.Overlays.Mods
{
    public class ModPanel : OsuClickableContainer
    {
        public Mod Mod { get; }
        public BindableBool Active { get; } = new BindableBool();
        public BindableBool Filtered { get; } = new BindableBool();

        protected readonly Box Background;
        protected readonly Container SwitchContainer;
        protected readonly Container MainContentContainer;
        protected readonly Box TextBackground;
        protected readonly FillFlowContainer TextFlow;

        [Resolved]
        protected OverlayColourProvider ColourProvider { get; private set; } = null!;

        protected const double TRANSITION_DURATION = 150;

        public const float CORNER_RADIUS = 7;

        protected const float HEIGHT = 42;
        protected const float IDLE_SWITCH_WIDTH = 54;
        protected const float EXPANDED_SWITCH_WIDTH = 70;

        private Colour4 activeColour;

        private readonly Bindable<bool> samplePlaybackDisabled = new BindableBool();
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
            Content.Shear = new Vector2(ShearedOverlayContainer.SHEAR, 0);

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
                        Shear = new Vector2(-ShearedOverlayContainer.SHEAR, 0),
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
                                        Shear = new Vector2(-ShearedOverlayContainer.SHEAR, 0),
                                        Margin = new MarginPadding
                                        {
                                            Left = -18 * ShearedOverlayContainer.SHEAR
                                        }
                                    },
                                    new OsuSpriteText
                                    {
                                        Text = mod.Description,
                                        Font = OsuFont.Default.With(size: 12),
                                        RelativeSizeAxes = Axes.X,
                                        Truncate = true,
                                        Shear = new Vector2(-ShearedOverlayContainer.SHEAR, 0)
                                    }
                                }
                            }
                        }
                    }
                }
            };

            Action = Active.Toggle;
        }

        [BackgroundDependencyLoader(true)]
        private void load(AudioManager audio, OsuColour colours, ISamplePlaybackDisabler? samplePlaybackDisabler)
        {
            sampleOn = audio.Samples.Get(@"UI/check-on");
            sampleOff = audio.Samples.Get(@"UI/check-off");

            activeColour = colours.ForModType(Mod.Type);

            if (samplePlaybackDisabler != null)
                ((IBindable<bool>)samplePlaybackDisabled).BindTo(samplePlaybackDisabler.SamplePlaybackDisabled);
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
            Filtered.BindValueChanged(_ => updateFilterState(), true);

            UpdateState();
            FinishTransforms(true);
        }

        private void playStateChangeSamples()
        {
            if (samplePlaybackDisabled.Value)
                return;

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

        private bool mouseDown;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button == MouseButton.Left)
                mouseDown = true;

            UpdateState();
            return false;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            mouseDown = false;

            UpdateState();
            base.OnMouseUp(e);
        }

        protected virtual Colour4 BackgroundColour => Active.Value ? activeColour.Darken(0.3f) : (Colour4)ColourProvider.Background3;
        protected virtual Colour4 ForegroundColour => Active.Value ? activeColour : (Colour4)ColourProvider.Background2;
        protected virtual Colour4 TextColour => Active.Value ? (Colour4)ColourProvider.Background6 : Colour4.White;

        protected virtual void UpdateState()
        {
            float targetWidth = Active.Value ? EXPANDED_SWITCH_WIDTH : IDLE_SWITCH_WIDTH;
            double transitionDuration = TRANSITION_DURATION;

            Colour4 backgroundColour = BackgroundColour;
            Colour4 foregroundColour = ForegroundColour;
            Colour4 textColour = TextColour;

            // Hover affects colour of button background
            if (IsHovered)
            {
                backgroundColour = backgroundColour.Lighten(0.1f);
                foregroundColour = foregroundColour.Lighten(0.1f);
            }

            // Mouse down adds a halfway tween of the movement
            if (mouseDown)
            {
                targetWidth = (float)Interpolation.Lerp(IDLE_SWITCH_WIDTH, EXPANDED_SWITCH_WIDTH, 0.5f);
                transitionDuration *= 4;
            }

            Content.TransformTo(nameof(BorderColour), ColourInfo.GradientVertical(backgroundColour, foregroundColour), transitionDuration, Easing.OutQuint);
            Background.FadeColour(backgroundColour, transitionDuration, Easing.OutQuint);
            SwitchContainer.ResizeWidthTo(targetWidth, transitionDuration, Easing.OutQuint);
            MainContentContainer.TransformTo(nameof(Padding), new MarginPadding
            {
                Left = targetWidth,
                Right = CORNER_RADIUS
            }, transitionDuration, Easing.OutQuint);
            TextBackground.FadeColour(foregroundColour, transitionDuration, Easing.OutQuint);
            TextFlow.FadeColour(textColour, transitionDuration, Easing.OutQuint);
        }

        #region Filtering support

        public void ApplyFilter(Func<Mod, bool>? filter)
        {
            Filtered.Value = filter != null && !filter.Invoke(Mod);
        }

        private void updateFilterState()
        {
            this.FadeTo(Filtered.Value ? 0 : 1);
        }

        #endregion
    }
}
