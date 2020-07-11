// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Overlays.Comments.Buttons
{
    public abstract class CommentRepliesButton : CompositeDrawable
    {
        public Action Action { get; set; }

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

        protected SpriteIcon Icon;
        private Box background;

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;
            InternalChildren = new Drawable[]
            {
                new CircularContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        background = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background2
                        },
                        new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            Margin = new MarginPadding
                            {
                                Vertical = 5,
                                Horizontal = 10,
                            },
                            Child = new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(15, 0),
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Children = new Drawable[]
                                {
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold),
                                        Text = GetText()
                                    },
                                    Icon = new SpriteIcon
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Size = new Vector2(7.5f),
                                        Icon = FontAwesome.Solid.ChevronDown,
                                        Colour = colourProvider.Foreground1
                                    }
                                }
                            }
                        }
                    }
                },
                new HoverClickSounds(),
            };
        }

        protected abstract string GetText();

        protected override bool OnHover(HoverEvent e)
        {
            base.OnHover(e);
            background.FadeColour(colourProvider.Background1, 200, Easing.OutQuint);
            Icon.FadeColour(colourProvider.Light1, 200, Easing.OutQuint);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            background.FadeColour(colourProvider.Background2, 200, Easing.OutQuint);
            Icon.FadeColour(colourProvider.Foreground1, 200, Easing.OutQuint);
        }

        protected override bool OnClick(ClickEvent e)
        {
            Action?.Invoke();
            return base.OnClick(e);
        }
    }
}
