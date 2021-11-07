// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Resources.Localisation.Web;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    /// <summary>
    /// <see cref="UserTrackingScrollContainer"/> which provides <see cref="ScrollToTopButton"/>. Mostly used in <see cref="FullscreenOverlay{T}"/>.
    /// </summary>
    public class OverlayScrollContainer : UserTrackingScrollContainer
    {
        /// <summary>
        /// Scroll position at which the <see cref="ScrollToTopButton"/> will be shown.
        /// </summary>
        private const int button_scroll_position = 200;

        protected readonly ScrollToTopButton Button;

        public OverlayScrollContainer()
        {
            AddInternal(Button = new ScrollToTopButton
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                Margin = new MarginPadding(20),
                Action = scrollToTop
            });
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (ScrollContent.DrawHeight + button_scroll_position < DrawHeight)
            {
                Button.State = Visibility.Hidden;
                return;
            }

            Button.State = Target > button_scroll_position ? Visibility.Visible : Visibility.Hidden;
        }

        private void scrollToTop()
        {
            ScrollToStart();
            Button.State = Visibility.Hidden;
        }

        public class ScrollToTopButton : OsuHoverContainer
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

            public ScrollToTopButton()
                : base(HoverSampleSet.ScrollToTop)
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
                        new SpriteIcon
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
            private void load(OverlayColourProvider colourProvider)
            {
                IdleColour = colourProvider.Background6;
                HoverColour = colourProvider.Background5;
                flashColour = colourProvider.Light1;
            }

            protected override bool OnClick(ClickEvent e)
            {
                background.FlashColour(flashColour, 800, Easing.OutQuint);
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
        }
    }
}
