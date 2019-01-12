// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Ranking
{
    public class ResultModeTabControl : TabControl<IResultPageInfo>
    {
        public ResultModeTabControl()
        {
            TabContainer.Anchor = Anchor.BottomCentre;
            TabContainer.Origin = Anchor.BottomCentre;
            TabContainer.Spacing = new Vector2(15);

            TabContainer.Masking = false;
            TabContainer.Padding = new MarginPadding(5);
        }

        protected override Dropdown<IResultPageInfo> CreateDropdown() => null;

        protected override TabItem<IResultPageInfo> CreateTabItem(IResultPageInfo value) => new ResultModeButton(value)
        {
            Anchor = TabContainer.Anchor,
            Origin = TabContainer.Origin
        };
    }
}
