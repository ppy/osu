using System;
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

        private TextBox tb;
        private FilteringSearchList<SpriteText> filteringSearchList;

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
            Children = new Drawable[]
            {
                new FlowContainer
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Direction = FlowDirection.VerticalOnly,
                    Children = new Drawable[]
                    {
                        tb = new TextBox
                        {
                            Size = new Vector2(300, 30)
                        },
                        filteringSearchList = new FilteringSearchList<SpriteText>(items)
                        {
                            Size = new Vector2(300, 400)
                        }
                    }
                }
            };
            tb.OnChange += tb_OnChange;
        }

        private void tb_OnChange(TextBox sender, bool newText)
        {
            if (newText)
                filteringSearchList.Filter(
                    item => item.Text.IndexOf(tb.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}