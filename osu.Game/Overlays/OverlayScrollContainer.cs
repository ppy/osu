// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Resources.Localisation.Web;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    /// <summary>
    /// <see cref="UserTrackingScrollContainer"/> which provides <see cref="ScrollBackButton"/>. Mostly used in <see cref="FullscreenOverlay{T}"/>.
    /// </summary>
    public partial class OverlayScrollContainer : UserTrackingScrollContainer
    {
        /// <summary>
        /// Scroll position at which the <see cref="ScrollBackButton"/> will be shown.
        /// </summary>
        private const int button_scroll_position = 200;

        public ScrollBackButton Button { get; private set; }

        private readonly Bindable<double?> lastScrollTarget = new Bindable<double?>();
        private readonly Bindable<double> progress = new Bindable<double>();

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(Button = new ScrollBackButton
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                Margin = new MarginPadding(20),
                Action = scrollBack,
                LastScrollTarget = { BindTarget = lastScrollTarget },
                Progress = { BindTarget = progress },
            });
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            // Map current position to standardized progress
            float height = AvailableContent - DrawHeight;
            progress.Value = height == 0 ? 1 : Math.Round(Math.Clamp(Current / height, 0, 1), 3);

            if (ScrollContent.DrawHeight + button_scroll_position < DrawHeight)
            {
                Button.State = Visibility.Hidden;
                return;
            }

            Button.State = Target > button_scroll_position || lastScrollTarget.Value != null ? Visibility.Visible : Visibility.Hidden;
        }

        protected override void OnUserScroll(double value, bool animated = true, double? distanceDecay = default)
        {
            base.OnUserScroll(value, animated, distanceDecay);

            lastScrollTarget.Value = null;
        }

        private void scrollBack()
        {
            if (lastScrollTarget.Value == null)
            {
                lastScrollTarget.Value = Target;
                ScrollToStart();
            }
            else
            {
                ScrollTo(lastScrollTarget.Value.Value);
                lastScrollTarget.Value = null;
            }
        }

        public partial class ScrollBackButton : OsuHoverContainer
        {
            private const int fade_duration = 500;

            private Visibility state;

            public Visibility State
            {
                get => state;
                set
                {
                    if (value == state)
                        return;

                    state = value;
                    Enabled.Value = state == Visibility.Visible;
                    this.FadeTo(state == Visibility.Visible ? 1 : 0, fade_duration, Easing.OutQuint);
                }
            }

            protected override IEnumerable<Drawable> EffectTargets => new[] { background };

            private Color4 flashColour;

            private readonly Container content;
            private readonly Box background;
            private readonly CircularProgress currentCircularProgress;
            private readonly SpriteIcon spriteIcon;

            public Bindable<double?> LastScrollTarget = new Bindable<double?>();
            public Bindable<double> Progress = new Bindable<double>();

            protected override HoverSounds CreateHoverSounds(HoverSampleSet sampleSet) => new HoverSounds();

            private Sample scrollToTopSample;
            private Sample scrollToPreviousSample;

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => content.ReceivePositionalInputAt(screenSpacePos);

            public ScrollBackButton()
            {
                Size = new Vector2(50);
                Alpha = 0;

                Add(content = new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Masking = true,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Offset = new Vector2(0f, 1f),
                        Radius = 3f,
                        Colour = Color4.Black.Opacity(0.25f),
                    },
                    Children = new Drawable[]
                    {
                        background = new Box
                        {
                            RelativeSizeAxes = Axes.Both
                        },
                        currentCircularProgress = new CircularProgress
                        {
                            RelativeSizeAxes = Axes.Both,
                            InnerRadius = 0.1f,
                        },
                        spriteIcon = new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(15),
                            Icon = FontAwesome.Solid.ChevronUp
                        }
                    }
                });

                TooltipText = CommonStrings.ButtonsBackToTop;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider, AudioManager audio)
            {
                IdleColour = colourProvider.Background6;
                HoverColour = colourProvider.Background5;
                flashColour = colourProvider.Light1;
                currentCircularProgress.Colour = colourProvider.Highlight1;

                scrollToTopSample = audio.Samples.Get(@"UI/scroll-to-top");
                scrollToPreviousSample = audio.Samples.Get(@"UI/scroll-to-previous");
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Progress.BindValueChanged(p => currentCircularProgress.Progress = p.NewValue, true);

                LastScrollTarget.BindValueChanged(target =>
                {
                    spriteIcon.ScaleTo(target.NewValue != null ? new Vector2(1f, -1f) : Vector2.One, fade_duration, Easing.OutQuint);
                    TooltipText = target.NewValue != null ? CommonStrings.ButtonsBackToPrevious : CommonStrings.ButtonsBackToTop;
                }, true);
            }

            protected override bool OnClick(ClickEvent e)
            {
                background.FlashColour(flashColour, 800, Easing.OutQuint);

                if (LastScrollTarget.Value == null)
                    scrollToTopSample?.Play();
                else
                    scrollToPreviousSample?.Play();

                return base.OnClick(e);
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                content.ScaleTo(0.75f, 2000, Easing.OutQuint);
                return true;
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                content.ScaleTo(1, 1000, Easing.OutElastic);
                base.OnMouseUp(e);
            }

            protected override bool OnHover(HoverEvent e)
            {
                base.OnHover(e);
                return true;
            }
        }
    }
}
