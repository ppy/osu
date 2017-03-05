// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;

namespace osu.Game.Screens.Select.Tab
{
    public class FilterTabDropDownHeader : BasicDropDownHeader
    {
        protected override string Label { get; set; }

        private TextAwesome ellipses;

        public FilterTabDropDownHeader()
        {
            AutoSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                ellipses = new TextAwesome
                {
                    Icon = FontAwesome.fa_ellipsis_h,
                    TextSize = 14,
                    Margin = new MarginPadding{ Top = 6, Bottom = 4 },
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            ellipses.Colour = colours.Blue;
        }
    }
}
