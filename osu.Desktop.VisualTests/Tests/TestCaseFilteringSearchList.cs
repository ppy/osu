using System;
using System.IO;
using System.Linq;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using OpenTK;
using OpenTK.Graphics;

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

            var items = new DirectoryInfo(Environment.CurrentDirectory)
                .GetFiles()
                .Select(
                    f =>
                        new SpriteText
                        {
                            Text = f.Name
                        }).ToList();
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
                        filteringSearchList = new FilteringSearchList<SpriteText>(items, si => si.Colour = Color4.Orange, nsi => nsi.Colour = Color4.White)
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