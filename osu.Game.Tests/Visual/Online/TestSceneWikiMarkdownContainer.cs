// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays;
using osu.Game.Overlays.Wiki.Markdown;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneWikiMarkdownContainer : OsuTestScene
    {
        private WikiMarkdownContainer markdownContainer;

        [Cached]
        private readonly OverlayColourProvider overlayColour = new OverlayColourProvider(OverlayColourScheme.Orange);

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = overlayColour.Background5,
                    RelativeSizeAxes = Axes.Both,
                },
                new BasicScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(20),
                    Child = markdownContainer = new WikiMarkdownContainer()
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                    }
                }
            };
        });

        [Test]
        public void TestLink()
        {
            AddStep("set current path", () => markdownContainer.CurrentPath = "Article_styling_criteria/");

            AddStep("set '/wiki/Main_Page''", () => markdownContainer.Text = "[wiki main page](/wiki/Main_Page)");

            AddStep("set '../FAQ''", () => markdownContainer.Text = "[FAQ](../FAQ)");

            AddStep("set './Writing''", () => markdownContainer.Text = "[wiki writing guidline](./Writing)");

            AddStep("set 'Formatting''", () => markdownContainer.Text = "[wiki formatting guidline](Formatting)");
        }
    }
}
