// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Markdig.Parsers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Game.Graphics.Containers.Markdown;
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
                    addTitle($"This is a very long title {i + 1}");
            });
        }

        [Test]
        public void TestWithSubtitle()
        {
            AddStep("Add TOC", () =>
            {
                for (var i = 0; i < 10; i++)
                    addTitle($"This is a very long title {i + 1}", i % 4 != 0);
            });
        }

        private void addTitle(string text, bool subtitle = false)
        {
            var headingBlock = createHeadingBlock(text, subtitle ? 3 : 2);
            sidebar.AddToc(headingBlock, createHeading(headingBlock));
        }

        private HeadingBlock createHeadingBlock(string text, int level = 2) => new HeadingBlock(new HeadingBlockParser())
        {
            Inline = new ContainerInline().AppendChild(new LiteralInline(text)),
            Level = level,
        };

        private MarkdownHeading createHeading(HeadingBlock headingBlock) => new OsuMarkdownHeading(headingBlock);
    }
}
