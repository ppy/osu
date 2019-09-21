// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public class SupporterIcon : CompositeDrawable, IHasTooltip
    {
        private readonly Box background;
        private readonly FillFlowContainer iconContainer;
        private readonly CircularContainer content;

        public string TooltipText => "osu!supporter";

        public int SupportLevel
        {
            set
            {
                int count = MathHelper.Clamp(value, 0, 3);

                if (count == 0)
                {
                    content.Hide();
                }
                else
                {
                    content.Show();
                    iconContainer.Clear();

                    for (int i = 0; i < count; i++)
                    {
                        iconContainer.Add(new SpriteIcon
                        {
                            Width = 12,
                            RelativeSizeAxes = Axes.Y,
                            Icon = FontAwesome.Solid.Heart,
                        });
                    }

                    iconContainer.Padding = new MarginPadding { Horizontal = DrawHeight / 2 };
                }
            }
        }

        public SupporterIcon()
        {
            AutoSizeAxes = Axes.X;

            InternalChild = content = new CircularContainer
            {
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                Masking = true,
                Alpha = 0,
                Children = new Drawable[]
                {
                    background = new Box { RelativeSizeAxes = Axes.Both },
                    iconContainer = new FillFlowContainer
                    {
                        Direction = FillDirection.Horizontal,
                        RelativeSizeAxes = Axes.Y,
                        AutoSizeAxes = Axes.X,
                        Height = 0.6f,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = colours.Pink;
        }
    }
}
