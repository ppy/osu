// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using Markdig.Syntax.Inlines;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Framework.Utils;
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
                markdownContainer.CurrentPath = "https://dev.ppy.sh";
                markdownContainer.Text = "![intro](/wiki/Interface/img/intro-screen.jpg)";
            });
        }

        [Test]
        public void TestRelativeImage()
        {
            AddStep("Add relative image", () =>
            {
                markdownContainer.CurrentPath = "https://dev.ppy.sh/wiki/Interface/";
                markdownContainer.Text = "![intro](img/intro-screen.jpg)";
            });
        }

        [Test]
        public void TestBlockImage()
        {
            AddStep("Add paragraph with block image", () =>
            {
                markdownContainer.CurrentPath = "https://dev.ppy.sh/wiki/Interface/";
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
                markdownContainer.CurrentPath = "https://dev.ppy.sh";
                markdownContainer.Text = "![osu! mode icon](/wiki/shared/mode/osu.png) osu!";
            });
        }

        [Test]
        public void TestTableWithImageContent()
        {
            AddStep("Add Table", () =>
            {
                markdownContainer.CurrentPath = "https://dev.ppy.sh";
                markdownContainer.Text = @"
| Image | Name | Effect |
| :-: | :-: | :-- |
| ![](/wiki/Skinning/Interface/img/hit300.png ""300"") | 300 | A possible score when tapping a hit circle precisely on time, completing a Slider and keeping the cursor over every tick, or completing a Spinner with the Spinner Metre full. A score of 300 appears in an blue score by default. Scoring nothing except 300s in a beatmap will award the player with the SS or SSH grade. |
| ![](/wiki/Skinning/Interface/img/hit300g.png ""Geki"") | (激) Geki | A term from Ouendan, called Elite Beat! in EBA. Appears when playing the last element in a combo in which the player has scored only 300s. Getting a Geki will give a sizable boost to the Life Bar. By default, it is blue. |
| ![](/wiki/Skinning/Interface/img/hit100.png ""100"") | 100 | A possible score one can get when tapping a Hit Object slightly late or early, completing a Slider and missing a number of ticks, or completing a Spinner with the Spinner Meter almost full. A score of 100 appears in a green score by default. When very skilled players test a beatmap and they get a lot of 100s, this may mean that the beatmap does not have correct timing. |
| ![](/wiki/Skinning/Interface/img/hit300k.png ""300 Katu"") ![](/wiki/Skinning/Interface/img/hit100k.png ""100 Katu"") | (喝) Katu or Katsu | A term from Ouendan, called Beat! in EBA. Appears when playing the last element in a combo in which the player has scored at least one 100, but no 50s or misses. Getting a Katu will give a small boost to the Life Bar. By default, it is coloured green or blue depending on whether the Katu itself is a 100 or a 300. |
| ![](/wiki/Skinning/Interface/img/hit50.png ""50"") | 50 | A possible score one can get when tapping a hit circle rather early or late but not early or late enough to cause a miss, completing a Slider and missing a lot of ticks, or completing a Spinner with the Spinner Metre close to full. A score of 50 appears in a orange score by default. Scoring a 50 in a combo will prevent the appearance of a Katu or a Geki at the combo's end. |
| ![](/wiki/Skinning/Interface/img/hit0.png ""Miss"") | Miss | A possible score one can get when not tapping a hit circle or too early (based on OD and AR, it may *shake* instead), not tapping or holding the Slider at least once, or completing a Spinner with low Spinner Metre fill. Scoring a Miss will reset the current combo to 0 and will prevent the appearance of a Katu or a Geki at the combo's end. |
";
            });
        }

        [Test]
        public void TestWideImageNotExceedContainer()
        {
            AddStep("Add image", () =>
            {
                markdownContainer.CurrentPath = "https://dev.ppy.sh/wiki/osu!_Program_Files/";
                markdownContainer.Text = "![](img/file_structure.jpg \"The file structure of osu!'s installation folder, on Windows and macOS\")";
            });

            AddUntilStep("Wait image to load", () => markdownContainer.ChildrenOfType<DelayedLoadWrapper>().First().DelayedLoadCompleted);

            AddStep("Change container width", () =>
            {
                markdownContainer.Width = 0.5f;
            });

            AddAssert("Image not exceed container width", () =>
            {
                var spriteImage = markdownContainer.ChildrenOfType<Sprite>().First();
                return Precision.DefinitelyBigger(markdownContainer.DrawWidth, spriteImage.DrawWidth);
            });
        }

        private class TestMarkdownContainer : WikiMarkdownContainer
        {
            public LinkInline Link;

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
