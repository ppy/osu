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
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public class OverlayScrollContainer : OsuScrollContainer
    {
        public ScrollToTopButton Button { get; }

        public OverlayScrollContainer()
        {
            AddInternal(Button = new ScrollToTopButton
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                Margin = new MarginPadding(20),
                Alpha = 0,
                Action = () => ScrollToStart()
            });
        }

        private bool buttonIsVisible;

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            var buttonShouldBeVisible = Current > 200;

            if (buttonIsVisible == buttonShouldBeVisible)
                return;

            Button.FadeTo(buttonShouldBeVisible ? 1 : 0, 500, Easing.OutQuint);
            buttonIsVisible = !buttonIsVisible;
        }

        public class ScrollToTopButton : OsuHoverContainer
        {
            protected override IEnumerable<Drawable> EffectTargets => new[] { background };

            private Color4 flashColour;

            private readonly Container content;
            private readonly Box background;

            public ScrollToTopButton()
            {
                Size = new Vector2(50);
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

                TooltipText = @"Scroll to top";
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
                return base.OnMouseDown(e);
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                content.ScaleTo(1, 1000, Easing.OutElastic);
                base.OnMouseUp(e);
            }
        }
    }
}
