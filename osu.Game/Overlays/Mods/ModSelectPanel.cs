// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Overlays.Mods
{
    public abstract partial class ModSelectPanel : OsuClickableContainer, IHasAccentColour, IFilterable
    {
        public abstract BindableBool Active { get; }

        public Color4 AccentColour { get; set; }

        public LocalisableString Title
        {
            get => titleText.Text;
            set => titleText.Text = value;
        }

        public LocalisableString Description
        {
            get => descriptionText.Text;
            set => descriptionText.Text = value;
        }

        public const float CORNER_RADIUS = 7;
        public const float HEIGHT = 42;

        public const double SAMPLE_PLAYBACK_DELAY = 30;

        protected virtual float IdleSwitchWidth => 14;
        protected virtual float ExpandedSwitchWidth => 30;
        protected virtual Colour4 BackgroundColour => Active.Value ? AccentColour.Darken(0.3f) : ColourProvider.Background3;
        protected virtual Colour4 ForegroundColour => Active.Value ? AccentColour : ColourProvider.Background2;
        protected virtual Colour4 TextColour => Active.Value ? ColourProvider.Background6 : Colour4.White;

        protected const double TRANSITION_DURATION = 150;

        protected readonly Box Background;
        protected readonly Container SwitchContainer;
        protected readonly Container MainContentContainer;
        protected readonly Box TextBackground;
        protected readonly FillFlowContainer TextFlow;

        [Resolved]
        protected OverlayColourProvider ColourProvider { get; private set; } = null!;

        private readonly OsuSpriteText titleText;
        private readonly OsuSpriteText descriptionText;

        private readonly Bindable<bool> samplePlaybackDisabled = new BindableBool();
        private Sample? sampleOff;
        private Sample? sampleOn;

        private Bindable<double?> lastPlaybackTime = null!;

        protected ModSelectPanel()
        {
            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;

            // all below properties are applied to `Content` rather than the `ModPanel` in its entirety
            // to allow external components to set these properties on the panel without affecting
            // its "internal" appearance.
            Content.Masking = true;
            Content.CornerRadius = CORNER_RADIUS;
            Content.BorderThickness = 2;

            Shear = new Vector2(ShearedOverlayContainer.SHEAR, 0);

            Children = new Drawable[]
            {
                Background = new Box
                {
                    RelativeSizeAxes = Axes.Both
                },
                SwitchContainer = new Container
                {
                    RelativeSizeAxes = Axes.Y,
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
                                    titleText = new TruncatingSpriteText
                                    {
                                        Font = OsuFont.TorusAlternate.With(size: 18, weight: FontWeight.SemiBold),
                                        RelativeSizeAxes = Axes.X,
                                        Shear = new Vector2(-ShearedOverlayContainer.SHEAR, 0),
                                        Margin = new MarginPadding
                                        {
                                            Left = -18 * ShearedOverlayContainer.SHEAR
                                        },
                                        ShowTooltip = false, // Tooltip is handled by `IncompatibilityDisplayingModPanel`.
                                    },
                                    descriptionText = new TruncatingSpriteText
                                    {
                                        Font = OsuFont.Default.With(size: 12),
                                        RelativeSizeAxes = Axes.X,
                                        Shear = new Vector2(-ShearedOverlayContainer.SHEAR, 0),
                                        ShowTooltip = false, // Tooltip is handled by `IncompatibilityDisplayingModPanel`.
                                    }
                                }
                            }
                        }
                    }
                }
            };

            Action = () =>
            {
                if (!Active.Value)
                    Select();
                else
                    Deselect();
            };
        }

        /// <summary>
        /// Performs all actions necessary to select this <see cref="ModSelectPanel"/>.
        /// </summary>
        protected abstract void Select();

        /// <summary>
        /// Performs all actions necessary to deselect this <see cref="ModSelectPanel"/>.
        /// </summary>
        protected abstract void Deselect();

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, SessionStatics statics, ISamplePlaybackDisabler? samplePlaybackDisabler)
        {
            sampleOn = audio.Samples.Get(@"UI/check-on");
            sampleOff = audio.Samples.Get(@"UI/check-off");

            if (samplePlaybackDisabler != null)
                ((IBindable<bool>)samplePlaybackDisabled).BindTo(samplePlaybackDisabler.SamplePlaybackDisabled);

            lastPlaybackTime = statics.GetBindable<double?>(Static.LastHoverSoundPlaybackTime);
        }

        protected sealed override HoverSounds CreateHoverSounds(HoverSampleSet sampleSet) => new HoverSounds(sampleSet);

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
            if (samplePlaybackDisabled.Value)
                return;

            if (!IsPresent)
                return;

            bool enoughTimePassedSinceLastPlayback = !lastPlaybackTime.Value.HasValue || Time.Current - lastPlaybackTime.Value >= SAMPLE_PLAYBACK_DELAY;

            if (enoughTimePassedSinceLastPlayback)
            {
                if (Active.Value)
                    sampleOn?.Play();
                else
                    sampleOff?.Play();

                lastPlaybackTime.Value = Time.Current;
            }
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

        protected virtual void UpdateState()
        {
            float targetWidth = Active.Value ? ExpandedSwitchWidth : IdleSwitchWidth;
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
                targetWidth = (float)Interpolation.Lerp(IdleSwitchWidth, ExpandedSwitchWidth, 0.5f);
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

        #region IFilterable

        public abstract IEnumerable<LocalisableString> FilterTerms { get; }

        private bool matchingFilter = true;

        public virtual bool MatchingFilter
        {
            get => matchingFilter;
            set
            {
                if (matchingFilter == value)
                    return;

                matchingFilter = value;
                this.FadeTo(value ? 1 : 0);
            }
        }

        public bool FilteringActive { set { } }

        #endregion
    }
}
