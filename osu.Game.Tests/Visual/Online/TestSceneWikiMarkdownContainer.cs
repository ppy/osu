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
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics.Containers.Markdown;
using osu.Game.Graphics.Containers.Markdown.Footnotes;
using osu.Game.Overlays;
using osu.Game.Overlays.Wiki.Markdown;
using osu.Game.Users.Drawables;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneWikiMarkdownContainer : OsuManualInputManagerTestScene
    {
        private OverlayScrollContainer scrollContainer;
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
                scrollContainer = new OverlayScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(20),
                }
            };

            scrollContainer.Child = new DependencyProvidingContainer
            {
                CachedDependencies = new (Type, object)[]
                {
                    (typeof(OverlayScrollContainer), scrollContainer)
                },
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Child = markdownContainer = new TestMarkdownContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
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
                markdownContainer.Text = "![intro](/wiki/images/Client/Interface/img/intro-screen.jpg)";
            });
        }

        [Test]
        public void TestRelativeImage()
        {
            AddStep("Add relative image", () =>
            {
                markdownContainer.CurrentPath = "https://dev.ppy.sh/wiki/Interface/";
                markdownContainer.Text = "![intro](../images/Client/Interface/img/intro-screen.jpg)";
            });
        }

        [Test]
        public void TestBlockImage()
        {
            AddStep("Add paragraph with block image", () =>
            {
                markdownContainer.CurrentPath = "https://dev.ppy.sh/wiki/Interface/";
                markdownContainer.Text = @"Line before image

![play menu](../images/Client/Interface/img/play-menu.jpg ""Main Menu in osu!"")

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
| ![](/wiki/images/shared/judgement/osu!/hit300.png ""300"") | 300 | A possible score when tapping a hit circle precisely on time, completing a Slider and keeping the cursor over every tick, or completing a Spinner with the Spinner Metre full. A score of 300 appears in an blue score by default. Scoring nothing except 300s in a beatmap will award the player with the SS or SSH grade. |
| ![](/wiki/images/shared/judgement/osu!/hit300g.png ""Geki"") | (激) Geki | A term from Ouendan, called Elite Beat! in EBA. Appears when playing the last element in a combo in which the player has scored only 300s. Getting a Geki will give a sizable boost to the Life Bar. By default, it is blue. |
| ![](/wiki/images/shared/judgement/osu!/hit100.png ""100"") | 100 | A possible score one can get when tapping a Hit Object slightly late or early, completing a Slider and missing a number of ticks, or completing a Spinner with the Spinner Meter almost full. A score of 100 appears in a green score by default. When very skilled players test a beatmap and they get a lot of 100s, this may mean that the beatmap does not have correct timing. |
| ![](/wiki/images/shared/judgement/osu!/hit300k.png ""300 Katu"") ![](/wiki/Skinning/Interface/img/hit100k.png ""100 Katu"") | (喝) Katu or Katsu | A term from Ouendan, called Beat! in EBA. Appears when playing the last element in a combo in which the player has scored at least one 100, but no 50s or misses. Getting a Katu will give a small boost to the Life Bar. By default, it is coloured green or blue depending on whether the Katu itself is a 100 or a 300. |
| ![](/wiki/images/shared/judgement/osu!/hit50.png ""50"") | 50 | A possible score one can get when tapping a hit circle rather early or late but not early or late enough to cause a miss, completing a Slider and missing a lot of ticks, or completing a Spinner with the Spinner Metre close to full. A score of 50 appears in a orange score by default. Scoring a 50 in a combo will prevent the appearance of a Katu or a Geki at the combo's end. |
| ![](/wiki/images/shared/judgement/osu!/hit0.png ""Miss"") | Miss | A possible score one can get when not tapping a hit circle or too early (based on OD and AR, it may *shake* instead), not tapping or holding the Slider at least once, or completing a Spinner with low Spinner Metre fill. Scoring a Miss will reset the current combo to 0 and will prevent the appearance of a Katu or a Geki at the combo's end. |
";
            });
        }

        [Test]
        public void TestWideImageNotExceedContainer()
        {
            AddStep("Add image", () =>
            {
                markdownContainer.CurrentPath = "https://dev.ppy.sh/wiki/osu!_Program_Files/";
                markdownContainer.Text = "![](../images/Client/Program_files/img/file_structure.jpg \"The file structure of osu!'s installation folder, on Windows and macOS\")";
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

        [Test]
        public void TestFlag()
        {
            AddStep("Add flag", () =>
            {
                markdownContainer.CurrentPath = @"https://dev.ppy.sh";
                markdownContainer.Text = "::{flag=\"AU\"}:: ::{flag=\"ZZ\"}::";
            });
            AddAssert("Two flags visible", () => markdownContainer.ChildrenOfType<DrawableFlag>().Count(), () => Is.EqualTo(2));
        }

        [Test]
        public void TestHeadingWithIdAttribute()
        {
            AddStep("Add heading with ID", () =>
            {
                markdownContainer.Text = "# This is a heading with an ID {#this-is-the-id}";
            });
            AddAssert("ID not visible", () => markdownContainer.ChildrenOfType<SpriteText>().All(spriteText => spriteText.Text != "{#this-is-the-id}"));
        }

        [Test]
        public void TestFootnotes()
        {
            AddStep("set content", () => markdownContainer.Text = @"This text has a footnote[^test].

Here's some more text[^test2] with another footnote!

# Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam efficitur laoreet posuere. Ut accumsan tortor in ipsum tincidunt ultrices. Suspendisse a malesuada tellus. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Fusce a sagittis nibh. In et velit sit amet mauris aliquet consectetur quis vehicula lorem. Etiam sit amet tellus ac velit ornare maximus. Donec quis metus eget libero ullamcorper imperdiet id vitae arcu. Vivamus iaculis rhoncus purus malesuada mollis. Vestibulum dictum at nisi sed tincidunt. Suspendisse finibus, ipsum ut dapibus commodo, leo eros porttitor sapien, non scelerisque nisi ligula sed ex. Pellentesque magna orci, hendrerit eu iaculis sit amet, ullamcorper in urna. Vivamus dictum mauris orci, nec facilisis dolor fringilla eu. Sed at porttitor nisi, at venenatis urna. Ut at orci vitae libero semper ullamcorper eu ut risus. Mauris hendrerit varius enim, ut varius nisi feugiat mattis.

## In at eros urna. Sed ipsum lorem, tempor sit amet purus in, vehicula pellentesque leo. Fusce volutpat pellentesque velit sit amet porttitor. Nulla eget erat ex. Praesent eu lacinia est, quis vehicula lacus. Donec consequat ultrices neque, at finibus quam efficitur vel. Vestibulum molestie nisl sit amet metus semper, at vestibulum massa rhoncus. Quisque imperdiet suscipit augue, et dignissim odio eleifend ut.

Aliquam sed vestibulum mauris, ut lobortis elit. Sed quis lacinia erat. Nam ultricies, risus non pellentesque sollicitudin, mauris dolor tincidunt neque, ac porta ipsum dui quis libero. Integer eget velit neque. Vestibulum venenatis mauris vitae rutrum vestibulum. Maecenas suscipit eu purus eu tempus. Nam dui nisl, bibendum condimentum mollis et, gravida vel dui. Sed et eros rutrum, facilisis sapien eu, mattis ligula. Fusce finibus pulvinar dolor quis consequat.

Donec ipsum felis, feugiat vel fermentum at, commodo eu sapien. Suspendisse nec enim vitae felis laoreet laoreet. Phasellus purus quam, fermentum a pharetra vel, tempor et urna. Integer vitae quam diam. Aliquam tincidunt tortor a iaculis convallis. Suspendisse potenti. Cras quis risus quam. Nullam tincidunt in lorem posuere sagittis.

Phasellus eu nunc nec ligula semper fringilla. Aliquam magna neque, placerat sed urna tristique, laoreet pharetra nulla. Vivamus maximus turpis purus, eu viverra dolor sodales porttitor. Praesent bibendum sapien purus, sed ultricies dolor iaculis sed. Fusce congue hendrerit malesuada. Nulla nulla est, auctor ac fringilla sed, ornare a lorem. Donec quis velit imperdiet, imperdiet sem non, pellentesque sapien. Maecenas in orci id ipsum placerat facilisis non sed nisi. Duis dictum lorem sodales odio dictum eleifend. Vestibulum bibendum euismod quam, eget pharetra orci facilisis sed. Vivamus at diam non ipsum consequat tristique. Pellentesque gravida dignissim pellentesque. Donec ullamcorper lacinia orci, id consequat purus faucibus quis. Phasellus metus nunc, iaculis a interdum vel, congue sed erat. Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Etiam eros libero, hendrerit luctus nulla vitae, luctus maximus nunc.

[^test]: This is a **footnote**.
[^test2]: This is another footnote [with a link](https://google.com/)!");
            AddStep("shrink scroll height", () => scrollContainer.Height = 0.5f);

            AddStep("press second footnote link", () =>
            {
                InputManager.MoveMouseTo(markdownContainer.ChildrenOfType<OsuMarkdownFootnoteLink>().ElementAt(1));
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("second footnote scrolled into view", () =>
            {
                var footnote = markdownContainer.ChildrenOfType<OsuMarkdownFootnote>().ElementAt(1);
                return scrollContainer.ScreenSpaceDrawQuad.Contains(footnote.ScreenSpaceDrawQuad.TopLeft)
                       && scrollContainer.ScreenSpaceDrawQuad.Contains(footnote.ScreenSpaceDrawQuad.BottomRight);
            });

            AddStep("press first footnote backlink", () =>
            {
                InputManager.MoveMouseTo(markdownContainer.ChildrenOfType<OsuMarkdownFootnoteBacklink>().First());
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("first footnote link scrolled into view", () =>
            {
                var footnote = markdownContainer.ChildrenOfType<OsuMarkdownFootnoteLink>().First();
                return scrollContainer.ScreenSpaceDrawQuad.Contains(footnote.ScreenSpaceDrawQuad.TopLeft)
                       && scrollContainer.ScreenSpaceDrawQuad.Contains(footnote.ScreenSpaceDrawQuad.BottomRight);
            });
        }

        [Test]
        public void TestCodeSyntax()
        {
            AddStep("set content", () =>
            {
                markdownContainer.Text = @"
This is a paragraph containing `inline code` syntax.
Oh wow I do love the `WikiMarkdownContainer`, it is very cool!

This is a line before the fenced code block:
```csharp
public class WikiMarkdownContainer : MarkdownContainer
{
    public WikiMarkdownContainer()
    {
        this.foo = bar;
    }
}
```
This is a line after the fenced code block!
";
            });
        }

        private partial class TestMarkdownContainer : WikiMarkdownContainer
        {
            public LinkInline Link;

            public override OsuMarkdownTextFlowContainer CreateTextFlow() => new TestMarkdownTextFlowContainer
            {
                UrlAdded = link => Link = link,
            };

            private partial class TestMarkdownTextFlowContainer : OsuMarkdownTextFlowContainer
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
