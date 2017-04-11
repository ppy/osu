using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.UserInterface;
using OpenTK;

namespace osu.Game.Screens.Ranking
{
    public class ResultModeTabControl : TabControl<ResultMode>
    {
        public ResultModeTabControl()
        {
            TabContainer.Anchor = Anchor.BottomCentre;
            TabContainer.Origin = Anchor.BottomCentre;
            TabContainer.Spacing = new Vector2(15);

            TabContainer.Masking = false;
            TabContainer.Padding = new MarginPadding(5);
        }

        protected override Dropdown<ResultMode> CreateDropdown() => null;

        protected override TabItem<ResultMode> CreateTabItem(ResultMode value) => new ResultModeButton(value)
        {
            Anchor = TabContainer.Anchor,
            Origin = TabContainer.Origin
        };
    }
}