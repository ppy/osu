// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
