// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Parsers;
using Markdig.Syntax;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Game.Overlays;
using osu.Game.Overlays.Wiki;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneWikiSidebar : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Orange);

        [Cached]
        private readonly OverlayScrollContainer scrollContainer = new OverlayScrollContainer();

        private WikiSidebar sidebar;

        private readonly MarkdownHeading dummyHeading = new MarkdownHeading(new HeadingBlock(new HeadingBlockParser()));

        [SetUp]
        public void SetUp() => Schedule(() => Child = sidebar = new WikiSidebar());

        [Test]
        public void TestNoContent()
        {
            AddStep("No Content", () => { });
        }

        [Test]
        public void TestOnlyMainTitle()
        {
            AddStep("Add TOC", () =>
            {
                for (var i = 0; i < 10; i++)
                {
                    sidebar.AddToc($"This is a very long title {i + 1}", dummyHeading, 2);
                }
            });
        }

        [Test]
        public void TestWithSubtitle()
        {
            AddStep("Add TOC", () =>
            {
                for (var i = 0; i < 20; i++)
                {
                    sidebar.AddToc($"This is a very long title {i + 1}", dummyHeading, i % 4 == 0 ? 2 : 3);
                }
            });
        }
    }
}
