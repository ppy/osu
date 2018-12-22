// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Profile.Header
{
    public class SupporterIcon : CircularContainer, IHasTooltip
    {
        private readonly Box background;
        private readonly FillFlowContainer iconContainer;

        public string TooltipText => "osu!supporter";

        public int SupporterLevel
        {
            set
            {
                if (value == 0)
                {
                    Hide();
                }
                else
                {
                    Show();
                    iconContainer.Clear();
                    for (int i = 0; i < value; i++)
                    {
                        iconContainer.Add(new SpriteIcon
                        {
                            Width = 12,
                            RelativeSizeAxes = Axes.Y,
                            Icon = FontAwesome.fa_heart,
                        });
                    }
                }
            }
        }

        public SupporterIcon()
        {
            Masking = true;
            AutoSizeAxes = Axes.X;
            Hide();

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
                    Origin = Anchor.Centre,
                }
            };
        }

        public override bool Invalidate(Invalidation invalidation = Invalidation.All, Drawable source = null, bool shallPropagate = true)
        {
            bool invalid = base.Invalidate(invalidation, source, shallPropagate);

            if ((invalidation & Invalidation.DrawSize) != 0)
            {
                iconContainer.Padding = new MarginPadding { Horizontal = DrawHeight / 2 };
            }

            return invalid;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = colours.Pink;
            iconContainer.Colour = colours.CommunityUserGrayGreenDark;
        }
    }
}
