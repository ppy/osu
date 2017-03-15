// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.UserInterface.Tab;
using osu.Game.Graphics;

namespace osu.Game.Screens.Select.Tab
{
    public class FilterTabDropDownHeader : TabDropDownHeader
    {
        protected override string Label { get; set; }

        public FilterTabDropDownHeader() {
            Foreground.Children = new Drawable[]
            {
                new TextAwesome
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
