// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Markdig.Syntax.Inlines;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers.Markdown;
using osu.Game.Overlays;
using osu.Game.Overlays.Wiki.Markdown;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneWikiMarkdownContainer : OsuTestScene
    {
        private TestMarkdownContainer markdownContainer;

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
                    Child = markdownContainer = new TestMarkdownContainer
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
            AddStep("set current path", () => markdownContainer.CurrentPath = $"{API.WebsiteRootUrl}/wiki/Article_styling_criteria/");

            AddStep("set '/wiki/Main_Page''", () => markdownContainer.Text = "[wiki main page](/wiki/Main_Page)");
            AddAssert("check url", () => markdownContainer.Link.Url == $"{API.WebsiteRootUrl}/wiki/Main_Page");

            AddStep("set '../FAQ''", () => markdownContainer.Text = "[FAQ](../FAQ)");
            AddAssert("check url", () => markdownContainer.Link.Url == $"{API.WebsiteRootUrl}/wiki/FAQ");

            AddStep("set './Writing''", () => markdownContainer.Text = "[wiki writing guidline](./Writing)");
            AddAssert("check url", () => markdownContainer.Link.Url == $"{API.WebsiteRootUrl}/wiki/Article_styling_criteria/Writing");

            AddStep("set 'Formatting''", () => markdownContainer.Text = "[wiki formatting guidline](Formatting)");
            AddAssert("check url", () => markdownContainer.Link.Url == $"{API.WebsiteRootUrl}/wiki/Article_styling_criteria/Formatting");
        }

        [Test]
        public void TestOutdatedNoticeBox()
        {
            AddStep("Add outdated yaml header", () =>
            {
                markdownContainer.Text = @"---
outdated: true
---";
            });
        }

        [Test]
        public void TestNeedsCleanupNoticeBox()
        {
            AddStep("Add needs cleanup yaml header", () =>
            {
                markdownContainer.Text = @"---
needs_cleanup: true
---";
            });
        }

        [Test]
        public void TestOnlyShowOutdatedNoticeBox()
        {
            AddStep("Add outdated and needs cleanup yaml", () =>
            {
                markdownContainer.Text = @"---
outdated: true
needs_cleanup: true
---";
            });
        }

        [Test]
        public void TestAbsoluteImage()
        {
            AddStep("Add absolute image", () =>
            {
                markdownContainer.DocumentUrl = "https://dev.ppy.sh";
                markdownContainer.Text = "![intro](/wiki/Interface/img/intro-screen.jpg)";
            });
        }

        [Test]
        public void TestRelativeImage()
        {
            AddStep("Add relative image", () =>
            {
                markdownContainer.DocumentUrl = "https://dev.ppy.sh";
                markdownContainer.CurrentPath = $"{API.WebsiteRootUrl}/wiki/Interface/";
                markdownContainer.Text = "![intro](img/intro-screen.jpg)";
            });
        }

        [Test]
        public void TestBlockImage()
        {
            AddStep("Add paragraph with block image", () =>
            {
                markdownContainer.DocumentUrl = "https://dev.ppy.sh";
                markdownContainer.CurrentPath = $"{API.WebsiteRootUrl}/wiki/Interface/";
                markdownContainer.Text = @"Line before image

![play menu](img/play-menu.jpg ""Main Menu in osu!"")

Line after image";
            });
        }

        [Test]
        public void TestInlineImage()
        {
            AddStep("Add inline image", () =>
            {
                markdownContainer.DocumentUrl = "https://dev.ppy.sh";
                markdownContainer.Text = "![osu! mode icon](/wiki/shared/mode/osu.png) osu!";
            });
        }

        private class TestMarkdownContainer : WikiMarkdownContainer
        {
            public LinkInline Link;

            public new string DocumentUrl
            {
                set => base.DocumentUrl = value;
            }

            public override MarkdownTextFlowContainer CreateTextFlow() => new TestMarkdownTextFlowContainer
            {
                UrlAdded = link => Link = link,
            };

            private class TestMarkdownTextFlowContainer : OsuMarkdownTextFlowContainer
            {
                public Action<LinkInline> UrlAdded;

                protected override void AddLinkText(string text, LinkInline linkInline)
                {
                    base.AddLinkText(text, linkInline);

                    UrlAdded?.Invoke(linkInline);
                }

                protected override void AddImage(LinkInline linkInline) => AddDrawable(new WikiMarkdownImage(linkInline));
            }
        }
    }
}
