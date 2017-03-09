// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;

namespace osu.Game.Screens.Select.Tab
{
    public class FilterTabDropDownHeader : DropDownHeader
    {
        protected override string Label { get; set; }

        private TextAwesome ellipses;

        public FilterTabDropDownHeader() {
            Background.Hide(); // don't need a background
            RelativeSizeAxes = Axes.None;
            AutoSizeAxes = Axes.Both;
            Foreground.RelativeSizeAxes = Axes.None;
            Foreground.AutoSizeAxes = Axes.Both;
            Foreground.Children = new Drawable[]
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
    }
}
