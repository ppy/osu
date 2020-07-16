﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
        protected string Text
        {
            get => text.Text;
            set => text.Text = value;
        }

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

        private readonly SpriteIcon icon;
        private readonly Box background;
        private readonly OsuSpriteText text;

        protected CommentRepliesButton()
        {
            AutoSizeAxes = Axes.Both;
            Margin = new MarginPadding
            {
                Vertical = 2
            };
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
                            RelativeSizeAxes = Axes.Both
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
                                    text = new OsuSpriteText
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        AlwaysPresent = true,
                                        Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold)
                                    },
                                    icon = new SpriteIcon
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Size = new Vector2(7.5f),
                                        Icon = FontAwesome.Solid.ChevronDown
                                    }
                                }
                            }
                        }
                    }
                },
                new HoverClickSounds(),
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            background.Colour = colourProvider.Background2;
            icon.Colour = colourProvider.Foreground1;
        }

        protected void SetIconDirection(bool upwards) => icon.ScaleTo(new Vector2(1, upwards ? -1 : 1));

        public void ToggleTextVisibility(bool visible) => text.FadeTo(visible ? 1 : 0, 200, Easing.OutQuint);

        protected override bool OnHover(HoverEvent e)
        {
            base.OnHover(e);
            background.FadeColour(colourProvider.Background1, 200, Easing.OutQuint);
            icon.FadeColour(colourProvider.Light1, 200, Easing.OutQuint);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            background.FadeColour(colourProvider.Background2, 200, Easing.OutQuint);
            icon.FadeColour(colourProvider.Foreground1, 200, Easing.OutQuint);
        }
    }
}
