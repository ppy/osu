using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using OpenTK;

namespace osu.Desktop.Tests
{
    class TestCaseSearchTextBox : TestCase
    {
        public override string Name => "SearchTextBox";
        public override string Description => "Tests SearchTextBox";

        public SearchTextBox Stb { get; set; }
        public ScrollContainer ScrollContainerEventCalls { get; set; }
        public FlowContainer FlowContainerEventCalls { get; set; }

        public override void Reset()
        {
            base.Reset();
            Stb = new SearchTextBox
            {
                Size = new Vector2(300, 30)
            };
            Stb.OnSearch += Stb_OnSearch;
            ScrollContainerEventCalls = new ScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    FlowContainerEventCalls = new FlowContainer
                    {
                        Direction = FlowDirection.VerticalOnly,
                        RelativeSizeAxes = Axes.Both
                    }
                }
            };
            Add(new FlowContainer
            {
                Direction = FlowDirection.VerticalOnly,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    Stb,
                    ScrollContainerEventCalls
                }
            });
        }

        private void Stb_OnSearch(object sender, SearchTextBox.OnSearchEventArgs e)
        {
            FlowContainerEventCalls.Add(new SpriteText
            {
                Text = $"OnSearch event fired: {e.RequestText}"
            });
        }
    }
}