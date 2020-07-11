// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Overlays.Comments.Buttons
{
    public abstract class CommentRepliesButton : OsuHoverContainer
    {
        protected override IEnumerable<Drawable> EffectTargets => new[] { background };

        protected ChevronIcon Icon;
        private Box background;

        public CommentRepliesButton()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Add(new CircularContainer
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
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold),
                                    Text = GetText()
                                },
                                Icon = new ChevronIcon
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                }
                            }
                        }
                    }
                }
            });

            IdleColour = colourProvider.Background2;
            HoverColour = colourProvider.Background1;
        }

        protected abstract string GetText();

        protected class ChevronIcon : SpriteIcon
        {
            public ChevronIcon()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                Size = new Vector2(7.5f);
                Icon = FontAwesome.Solid.ChevronDown;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Colour = colourProvider.Foreground1;
            }
        }
    }
}
