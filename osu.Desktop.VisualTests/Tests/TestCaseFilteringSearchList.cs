using System.Collections.Generic;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using OpenTK;

namespace osu.Desktop.Tests
{
    public class TestCaseFilteringSearchList : TestCase
    {
        public override string Name => @"FilteringSearchList";

        public override string Description => @"Tests filtering search list";

        public TextBox Tb { get; set; }
        public FilteringSearchList<SpriteText> FilteringSearchList { get; set; }

        public override void Reset()
        {
            base.Reset();
            var items = new List<SpriteText>
            {
                new SpriteText
                {
                    Text = "A search textbox"
                },
                new SpriteText
                {
                    Text = "A filtering search list"
                },
                new SpriteText
                {
                    Text = "A dropdown menu"
                },
                new SpriteText
                {
                    Text = "Possibly more I've missed"
                }
            };
            var property = typeof(SpriteText).GetProperty("Text");
            Children = new Drawable[]
            {
                new FlowContainer
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Direction = FlowDirection.VerticalOnly,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        Tb = new TextBox
                        {
                            Size = new Vector2(300, 30)
                        },
                        FilteringSearchList = new FilteringSearchList<SpriteText>(items, property)
                    }
                }
            };
            Tb.OnChange += Tb_OnChange;
        }

        private void Tb_OnChange(TextBox sender, bool newText)
        {
            if (newText)
                FilteringSearchList.Filter(Tb.Text);
        }
    }
}