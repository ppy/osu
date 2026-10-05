// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using NUnit.Framework.Legacy;
using osu.Game.Online.Chat;

namespace osu.Game.Tests.Chat
{
    [TestFixture]
    public class MessageFormatterTests
    {
        private string originalWebsiteRootUrl;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            originalWebsiteRootUrl = MessageFormatter.WebsiteRootUrl;
            MessageFormatter.WebsiteRootUrl = "dev.ppy.sh";
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            MessageFormatter.WebsiteRootUrl = originalWebsiteRootUrl;
        }

        [Test]
        public void TestUnsupportedProtocolLink()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is a gopher://really-old-protocol we don't support." });

            ClassicAssert.AreEqual(result.Content, result.DisplayContent);
            ClassicAssert.AreEqual(0, result.Links.Count);
        }

        [Test]
        public void TestFakeProtocolLink()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is a osunotarealprotocol://completely-made-up-protocol we don't support." });

            ClassicAssert.AreEqual(result.Content, result.DisplayContent);
            ClassicAssert.AreEqual(0, result.Links.Count);
        }

        [Test]
        public void TestSupportedProtocolLinkParsing()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "forgotspacehttps://dev.ppy.sh joinmyosump://12345 jointheosu://chan/#english" });

            ClassicAssert.AreEqual("https://dev.ppy.sh", result.Links[0].Url);
            ClassicAssert.AreEqual("osump://12345", result.Links[1].Url);
            ClassicAssert.AreEqual("osu://chan/#english", result.Links[2].Url);
        }

        [Test]
        public void TestBareLink()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is a http://www.basic-link.com/?test=test." });

            ClassicAssert.AreEqual(result.Content, result.DisplayContent);
            ClassicAssert.AreEqual(1, result.Links.Count);
            ClassicAssert.AreEqual("http://www.basic-link.com/?test=test", result.Links[0].Url);
            ClassicAssert.AreEqual(10, result.Links[0].Index);
            ClassicAssert.AreEqual(36, result.Links[0].Length);
        }

        [TestCase(LinkAction.OpenBeatmap, "456", "https://dev.ppy.sh/beatmapsets/123#osu/456")]
        [TestCase(LinkAction.OpenBeatmap, "456", "https://dev.ppy.sh/beatmapsets/123#osu/456?whatever")]
        [TestCase(LinkAction.OpenBeatmap, "456", "https://dev.ppy.sh/beatmapsets/123/456")]
        [TestCase(LinkAction.External, "https://dev.ppy.sh/beatmapsets/abc/def", "https://dev.ppy.sh/beatmapsets/abc/def")]
        [TestCase(LinkAction.OpenBeatmapSet, "123", "https://dev.ppy.sh/beatmapsets/123")]
        [TestCase(LinkAction.OpenBeatmapSet, "123", "https://dev.ppy.sh/beatmapsets/123/whatever")]
        [TestCase(LinkAction.External, "https://dev.ppy.sh/beatmapsets/abc", "https://dev.ppy.sh/beatmapsets/abc")]
        [TestCase(LinkAction.External, "https://dev.ppy.sh/beatmapsets/discussions", "https://dev.ppy.sh/beatmapsets/discussions")]
        [TestCase(LinkAction.External, "https://dev.ppy.sh/beatmapsets/discussions/123", "https://dev.ppy.sh/beatmapsets/discussions/123")]
        public void TestBeatmapLinks(LinkAction expectedAction, string expectedArg, string link)
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = link });

            ClassicAssert.AreEqual(result.Content, result.DisplayContent);
            ClassicAssert.AreEqual(1, result.Links.Count);
            ClassicAssert.AreEqual(expectedAction, result.Links[0].Action);
            ClassicAssert.AreEqual(expectedArg, result.Links[0].Argument);
            if (expectedAction == LinkAction.External)
                ClassicAssert.AreEqual(link, result.Links[0].Url);
        }

        [Test]
        public void TestMultipleComplexLinks()
        {
            Message result = MessageFormatter.FormatMessage(new Message
            {
                Content = "This is a http://test.io/link#fragment. (see https://twitter.com). Also, This string should not be altered. http://example.com/"
            });

            ClassicAssert.AreEqual(result.Content, result.DisplayContent);
            ClassicAssert.AreEqual(3, result.Links.Count);

            ClassicAssert.AreEqual("http://test.io/link#fragment", result.Links[0].Url);
            ClassicAssert.AreEqual(10, result.Links[0].Index);
            ClassicAssert.AreEqual(28, result.Links[0].Length);

            ClassicAssert.AreEqual("https://twitter.com", result.Links[1].Url);
            ClassicAssert.AreEqual(45, result.Links[1].Index);
            ClassicAssert.AreEqual(19, result.Links[1].Length);

            ClassicAssert.AreEqual("http://example.com/", result.Links[2].Url);
            ClassicAssert.AreEqual(108, result.Links[2].Index);
            ClassicAssert.AreEqual(19, result.Links[2].Length);
        }

        [Test]
        public void TestAjaxLinks()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "https://twitter.com/#!/hashbanglinks" });

            ClassicAssert.AreEqual(result.Content, result.DisplayContent);
            ClassicAssert.AreEqual(result.Content, result.Links[0].Url);
            ClassicAssert.AreEqual(0, result.Links[0].Index);
            ClassicAssert.AreEqual(36, result.Links[0].Length);
        }

        [Test]
        public void TestUnixHomeLinks()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "http://www.chiark.greenend.org.uk/~sgtatham/putty/" });

            ClassicAssert.AreEqual(result.Content, result.DisplayContent);
            ClassicAssert.AreEqual(result.Content, result.Links[0].Url);
            ClassicAssert.AreEqual(0, result.Links[0].Index);
            ClassicAssert.AreEqual(50, result.Links[0].Length);
        }

        [Test]
        public void TestInsensitiveLinks()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "look: http://puu.sh/7Ggh8xcC6/asf0asd9876.NEF" });

            ClassicAssert.AreEqual(result.Content, result.DisplayContent);
            ClassicAssert.AreEqual(6, result.Links[0].Index);
            ClassicAssert.AreEqual(39, result.Links[0].Length);
        }

        [Test]
        public void TestWikiLink()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is a [[Wiki Link]]." });

            ClassicAssert.AreEqual("This is a Wiki Link.", result.DisplayContent);
            ClassicAssert.AreEqual(1, result.Links.Count);
            ClassicAssert.AreEqual("https://dev.ppy.sh/wiki/Wiki Link", result.Links[0].Url);
            ClassicAssert.AreEqual(10, result.Links[0].Index);
            ClassicAssert.AreEqual(9, result.Links[0].Length);
        }

        [Test]
        public void TestMultiWikiLink()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is a [[Wiki Link]] [[Wiki:Link]][[Wiki.Link]]." });

            ClassicAssert.AreEqual("This is a Wiki Link Wiki:LinkWiki.Link.", result.DisplayContent);
            ClassicAssert.AreEqual(3, result.Links.Count);

            ClassicAssert.AreEqual("https://dev.ppy.sh/wiki/Wiki Link", result.Links[0].Url);
            ClassicAssert.AreEqual(10, result.Links[0].Index);
            ClassicAssert.AreEqual(9, result.Links[0].Length);

            ClassicAssert.AreEqual("https://dev.ppy.sh/wiki/Wiki:Link", result.Links[1].Url);
            ClassicAssert.AreEqual(20, result.Links[1].Index);
            ClassicAssert.AreEqual(9, result.Links[1].Length);

            ClassicAssert.AreEqual("https://dev.ppy.sh/wiki/Wiki.Link", result.Links[2].Url);
            ClassicAssert.AreEqual(29, result.Links[2].Index);
            ClassicAssert.AreEqual(9, result.Links[2].Length);
        }

        [Test]
        public void TestOldFormatLink()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is a (simple test)[https://osu.ppy.sh] of links." });

            ClassicAssert.AreEqual("This is a simple test of links.", result.DisplayContent);
            ClassicAssert.AreEqual(1, result.Links.Count);
            ClassicAssert.AreEqual("https://osu.ppy.sh", result.Links[0].Url);
            ClassicAssert.AreEqual(10, result.Links[0].Index);
            ClassicAssert.AreEqual(11, result.Links[0].Length);
        }

        [Test]
        public void TestOldFormatLinkWithBalancedBrackets()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is a (tricky (one))[https://osu.ppy.sh]!" });

            ClassicAssert.AreEqual("This is a tricky (one)!", result.DisplayContent);
            ClassicAssert.AreEqual(1, result.Links.Count);
            ClassicAssert.AreEqual("https://osu.ppy.sh", result.Links[0].Url);
            ClassicAssert.AreEqual(10, result.Links[0].Index);
            ClassicAssert.AreEqual(12, result.Links[0].Length);
        }

        [Test]
        public void TestOldFormatLinkWithEscapedBrackets()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is (another loose bracket \\))[https://osu.ppy.sh]." });

            ClassicAssert.AreEqual("This is another loose bracket ).", result.DisplayContent);
            ClassicAssert.AreEqual(1, result.Links.Count);
            ClassicAssert.AreEqual("https://osu.ppy.sh", result.Links[0].Url);
            ClassicAssert.AreEqual(8, result.Links[0].Index);
            ClassicAssert.AreEqual(23, result.Links[0].Length);
        }

        [Test]
        public void TestOldFormatWithBackslashes()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This link (should end with a backslash \\)[https://osu.ppy.sh]." });
            ClassicAssert.AreEqual("This link should end with a backslash \\.", result.DisplayContent);
            ClassicAssert.AreEqual(1, result.Links.Count);
            ClassicAssert.AreEqual("https://osu.ppy.sh", result.Links[0].Url);
            ClassicAssert.AreEqual(10, result.Links[0].Index);
            ClassicAssert.AreEqual(29, result.Links[0].Length);
        }

        [Test]
        public void TestOldFormatLinkWithEscapedAndBalancedBrackets()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is a (\\)super\\(\\( tricky (one))[https://osu.ppy.sh]!" });

            ClassicAssert.AreEqual("This is a )super(( tricky (one)!", result.DisplayContent);
            ClassicAssert.AreEqual(1, result.Links.Count);
            ClassicAssert.AreEqual("https://osu.ppy.sh", result.Links[0].Url);
            ClassicAssert.AreEqual(10, result.Links[0].Index);
            ClassicAssert.AreEqual(21, result.Links[0].Length);
        }

        [Test]
        public void TestNewFormatLink()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is a [https://osu.ppy.sh simple test]." });

            ClassicAssert.AreEqual("This is a simple test.", result.DisplayContent);
            ClassicAssert.AreEqual(1, result.Links.Count);
            ClassicAssert.AreEqual("https://osu.ppy.sh", result.Links[0].Url);
            ClassicAssert.AreEqual(10, result.Links[0].Index);
            ClassicAssert.AreEqual(11, result.Links[0].Length);
        }

        [Test]
        public void TestNewFormatLinkWithEscapedBrackets()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is a [https://osu.ppy.sh nasty link with escaped brackets: \\] and \\[]" });

            ClassicAssert.AreEqual("This is a nasty link with escaped brackets: ] and [", result.DisplayContent);
            ClassicAssert.AreEqual(1, result.Links.Count);
            ClassicAssert.AreEqual("https://osu.ppy.sh", result.Links[0].Url);
            ClassicAssert.AreEqual(10, result.Links[0].Index);
            ClassicAssert.AreEqual(41, result.Links[0].Length);
        }

        [Test]
        public void TestNewFormatLinkWithBackslashesInside()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is a [https://osu.ppy.sh link \\ with \\ backslashes \\]" });

            ClassicAssert.AreEqual("This is a link \\ with \\ backslashes \\", result.DisplayContent);
            ClassicAssert.AreEqual(1, result.Links.Count);
            ClassicAssert.AreEqual("https://osu.ppy.sh", result.Links[0].Url);
            ClassicAssert.AreEqual(10, result.Links[0].Index);
            ClassicAssert.AreEqual(27, result.Links[0].Length);
        }

        [Test]
        public void TestNewFormatLinkWithEscapedAndBalancedBrackets()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is a [https://osu.ppy.sh [link [with \\] too many brackets \\[ ]]]" });

            ClassicAssert.AreEqual("This is a [link [with ] too many brackets [ ]]", result.DisplayContent);
            ClassicAssert.AreEqual(1, result.Links.Count);
            ClassicAssert.AreEqual("https://osu.ppy.sh", result.Links[0].Url);
            ClassicAssert.AreEqual(10, result.Links[0].Index);
            ClassicAssert.AreEqual(36, result.Links[0].Length);
        }

        [Test]
        public void TestMarkdownFormatLink()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is a [simple test](https://osu.ppy.sh)." });

            ClassicAssert.AreEqual("This is a simple test.", result.DisplayContent);
            ClassicAssert.AreEqual(1, result.Links.Count);
            ClassicAssert.AreEqual("https://osu.ppy.sh", result.Links[0].Url);
            ClassicAssert.AreEqual(10, result.Links[0].Index);
            ClassicAssert.AreEqual(11, result.Links[0].Length);
        }

        [Test]
        public void TestMarkdownFormatLinkWithBalancedBrackets()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is a [tricky [one]](https://osu.ppy.sh)!" });

            ClassicAssert.AreEqual("This is a tricky [one]!", result.DisplayContent);
            ClassicAssert.AreEqual(1, result.Links.Count);
            ClassicAssert.AreEqual("https://osu.ppy.sh", result.Links[0].Url);
            ClassicAssert.AreEqual(10, result.Links[0].Index);
            ClassicAssert.AreEqual(12, result.Links[0].Length);
        }

        [Test]
        public void TestMarkdownFormatLinkWithEscapedBrackets()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is [another loose bracket \\]](https://osu.ppy.sh)." });

            ClassicAssert.AreEqual("This is another loose bracket ].", result.DisplayContent);
            ClassicAssert.AreEqual(1, result.Links.Count);
            ClassicAssert.AreEqual("https://osu.ppy.sh", result.Links[0].Url);
            ClassicAssert.AreEqual(8, result.Links[0].Index);
            ClassicAssert.AreEqual(23, result.Links[0].Length);
        }

        [Test]
        public void TestMarkdownFormatWithBackslashes()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This link [should end with a backslash \\](https://osu.ppy.sh)." });
            ClassicAssert.AreEqual("This link should end with a backslash \\.", result.DisplayContent);
            ClassicAssert.AreEqual(1, result.Links.Count);
            ClassicAssert.AreEqual("https://osu.ppy.sh", result.Links[0].Url);
            ClassicAssert.AreEqual(10, result.Links[0].Index);
            ClassicAssert.AreEqual(29, result.Links[0].Length);
        }

        [Test]
        public void TestMarkdownFormatLinkWithEscapedAndBalancedBrackets()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is a [\\]super\\[\\[ tricky [one]](https://osu.ppy.sh)!" });

            ClassicAssert.AreEqual("This is a ]super[[ tricky [one]!", result.DisplayContent);
            ClassicAssert.AreEqual(1, result.Links.Count);
            ClassicAssert.AreEqual("https://osu.ppy.sh", result.Links[0].Url);
            ClassicAssert.AreEqual(10, result.Links[0].Index);
            ClassicAssert.AreEqual(21, result.Links[0].Length);
        }

        [Test]
        public void TestMarkdownFormatLinkWithInlineTitle()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "I haven't seen [this link format](https://osu.ppy.sh \"osu!\") before..." });

            ClassicAssert.AreEqual("I haven't seen this link format before...", result.DisplayContent);
            ClassicAssert.AreEqual(1, result.Links.Count);
            ClassicAssert.AreEqual("https://osu.ppy.sh", result.Links[0].Url);
            ClassicAssert.AreEqual(15, result.Links[0].Index);
            ClassicAssert.AreEqual(16, result.Links[0].Length);
        }

        [Test]
        public void TestMarkdownFormatLinkWithInlineTitleAndEscapedQuotes()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "I haven't seen [this link format](https://osu.ppy.sh \"inner quote \\\" just to confuse \") before..." });

            ClassicAssert.AreEqual("I haven't seen this link format before...", result.DisplayContent);
            ClassicAssert.AreEqual(1, result.Links.Count);
            ClassicAssert.AreEqual("https://osu.ppy.sh", result.Links[0].Url);
            ClassicAssert.AreEqual(15, result.Links[0].Index);
            ClassicAssert.AreEqual(16, result.Links[0].Length);
        }

        [Test]
        public void TestMarkdownFormatLinkWithUrlInTextAndInlineTitle()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "I haven't seen [https://osu.ppy.sh](https://osu.ppy.sh \"https://osu.ppy.sh\") before..." });

            ClassicAssert.AreEqual("I haven't seen https://osu.ppy.sh before...", result.DisplayContent);
            ClassicAssert.AreEqual(1, result.Links.Count);
            ClassicAssert.AreEqual("https://osu.ppy.sh", result.Links[0].Url);
            ClassicAssert.AreEqual(15, result.Links[0].Index);
            ClassicAssert.AreEqual(18, result.Links[0].Length);
        }

        [Test]
        public void TestMarkdownFormatLinkWithUrlAndTextInTitle()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "I haven't seen [oh no, text here! https://osu.ppy.sh](https://osu.ppy.sh) before..." });

            ClassicAssert.AreEqual("I haven't seen oh no, text here! https://osu.ppy.sh before...", result.DisplayContent);
            ClassicAssert.AreEqual(1, result.Links.Count);
            ClassicAssert.AreEqual("https://osu.ppy.sh", result.Links[0].Url);
            ClassicAssert.AreEqual(15, result.Links[0].Index);
            ClassicAssert.AreEqual(36, result.Links[0].Length);
        }

        [Test]
        public void TestMarkdownFormatLinkWithMisleadingUrlInText()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "I haven't seen [https://google.com](https://osu.ppy.sh) before..." });

            ClassicAssert.AreEqual("I haven't seen https://google.com before...", result.DisplayContent);
            ClassicAssert.AreEqual(1, result.Links.Count);
            ClassicAssert.AreEqual("https://osu.ppy.sh", result.Links[0].Url);
            ClassicAssert.AreEqual(15, result.Links[0].Index);
            ClassicAssert.AreEqual(18, result.Links[0].Length);
        }

        [Test]
        public void TestMarkdownFormatLinkThatContractsIntoLargerLink()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "super broken https://[osu.ppy](https://reddit.com).sh/" });

            ClassicAssert.AreEqual("super broken https://osu.ppy.sh/", result.DisplayContent);
            ClassicAssert.AreEqual(1, result.Links.Count);
            ClassicAssert.AreEqual("https://reddit.com", result.Links[0].Url);
            ClassicAssert.AreEqual(21, result.Links[0].Index);
            ClassicAssert.AreEqual(7, result.Links[0].Length);
        }

        [Test]
        public void TestMarkdownFormatLinkDirectlyNextToRawLink()
        {
            // the raw link has a port at the end of it, so that the raw link regex terminates at the port and doesn't consume display text from the formatted one
            Message result = MessageFormatter.FormatMessage(new Message { Content = "https://localhost:8080[https://osu.ppy.sh](https://osu.ppy.sh) should be two links" });

            ClassicAssert.AreEqual("https://localhost:8080https://osu.ppy.sh should be two links", result.DisplayContent);
            ClassicAssert.AreEqual(2, result.Links.Count);

            ClassicAssert.AreEqual("https://localhost:8080", result.Links[0].Url);
            ClassicAssert.AreEqual(0, result.Links[0].Index);
            ClassicAssert.AreEqual(22, result.Links[0].Length);

            ClassicAssert.AreEqual("https://osu.ppy.sh", result.Links[1].Url);
            ClassicAssert.AreEqual(22, result.Links[1].Index);
            ClassicAssert.AreEqual(18, result.Links[1].Length);
        }

        [Test]
        public void TestChannelLink()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is an #english and #japanese." });

            ClassicAssert.AreEqual(result.Content, result.DisplayContent);
            ClassicAssert.AreEqual(2, result.Links.Count);
            ClassicAssert.AreEqual($"{OsuGameBase.OSU_PROTOCOL}chan/#english", result.Links[0].Url);
            ClassicAssert.AreEqual($"{OsuGameBase.OSU_PROTOCOL}chan/#japanese", result.Links[1].Url);
        }

        [Test]
        public void TestOsuProtocol()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = $"This is a custom protocol {OsuGameBase.OSU_PROTOCOL}chan/#english." });

            ClassicAssert.AreEqual(result.Content, result.DisplayContent);
            ClassicAssert.AreEqual(1, result.Links.Count);
            ClassicAssert.AreEqual($"{OsuGameBase.OSU_PROTOCOL}chan/#english", result.Links[0].Url);
            ClassicAssert.AreEqual(26, result.Links[0].Index);
            ClassicAssert.AreEqual(19, result.Links[0].Length);

            result = MessageFormatter.FormatMessage(new Message { Content = $"This is a [custom protocol]({OsuGameBase.OSU_PROTOCOL}chan/#english)." });

            ClassicAssert.AreEqual("This is a custom protocol.", result.DisplayContent);
            ClassicAssert.AreEqual(1, result.Links.Count);
            ClassicAssert.AreEqual($"{OsuGameBase.OSU_PROTOCOL}chan/#english", result.Links[0].Url);
            ClassicAssert.AreEqual("#english", result.Links[0].Argument);
            ClassicAssert.AreEqual(10, result.Links[0].Index);
            ClassicAssert.AreEqual(15, result.Links[0].Length);
        }

        [Test]
        public void TestOsuMpProtocol()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "Join my multiplayer game osump://12346." });

            ClassicAssert.AreEqual(result.Content, result.DisplayContent);
            ClassicAssert.AreEqual(1, result.Links.Count);
            ClassicAssert.AreEqual("osump://12346", result.Links[0].Url);
            ClassicAssert.AreEqual(25, result.Links[0].Index);
            ClassicAssert.AreEqual(13, result.Links[0].Length);
        }

        [Test]
        public void TestRecursiveBreaking()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is a [https://osu.ppy.sh [[simple test]]]." });

            ClassicAssert.AreEqual("This is a [[simple test]].", result.DisplayContent);
            ClassicAssert.AreEqual(1, result.Links.Count);
            ClassicAssert.AreEqual("https://osu.ppy.sh", result.Links[0].Url);
            ClassicAssert.AreEqual(10, result.Links[0].Index);
            ClassicAssert.AreEqual(15, result.Links[0].Length);
        }

        [Test]
        public void TestLinkComplex()
        {
            Message result = MessageFormatter.FormatMessage(new Message
            {
                Content = "This is a [http://www.simple-test.com simple test] with some [traps] and [[wiki links]]. Don't forget to visit https://osu.ppy.sh (now!)[http://google.com]\uD83D\uDE12"
            });

            ClassicAssert.AreEqual("This is a simple test with some [traps] and wiki links. Don't forget to visit https://osu.ppy.sh now![emoji]", result.DisplayContent);
            ClassicAssert.AreEqual(4, result.Links.Count);

            Link f = result.Links.Find(l => l.Url == "https://dev.ppy.sh/wiki/wiki links");
            Assert.That(f, Is.Not.Null);
            ClassicAssert.AreEqual(44, f.Index);
            ClassicAssert.AreEqual(10, f.Length);

            f = result.Links.Find(l => l.Url == "http://www.simple-test.com");
            Assert.That(f, Is.Not.Null);
            ClassicAssert.AreEqual(10, f.Index);
            ClassicAssert.AreEqual(11, f.Length);

            f = result.Links.Find(l => l.Url == "http://google.com");
            Assert.That(f, Is.Not.Null);
            ClassicAssert.AreEqual(97, f.Index);
            ClassicAssert.AreEqual(4, f.Length);

            f = result.Links.Find(l => l.Url == "https://osu.ppy.sh");
            Assert.That(f, Is.Not.Null);
            ClassicAssert.AreEqual(78, f.Index);
            ClassicAssert.AreEqual(18, f.Length);
        }

        [Test]
        public void TestEmoji()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "Hello world\uD83D\uDE12<--This is an emoji,There are more emojis among us:\uD83D\uDE10\uD83D\uDE00,\uD83D\uDE20" });
            ClassicAssert.AreEqual("Hello world[emoji]<--This is an emoji,There are more emojis among us:[emoji][emoji],[emoji]", result.DisplayContent);
            ClassicAssert.AreEqual(result.Links.Count, 0);
        }

        [Test]
        public void TestEmojiWithSuccessiveParens()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "\uD83D\uDE10(let's hope this doesn't accidentally turn into a link)" });
            ClassicAssert.AreEqual("[emoji](let's hope this doesn't accidentally turn into a link)", result.DisplayContent);
            ClassicAssert.AreEqual(result.Links.Count, 0);
        }

        [Test]
        public void TestAbsoluteExternalLinks()
        {
            LinkDetails result = MessageFormatter.GetLinkDetails("https://google.com");

            ClassicAssert.AreEqual(LinkAction.External, result.Action);
            ClassicAssert.AreEqual("https://google.com", result.Argument);
        }

        [Test]
        public void TestRelativeExternalLinks()
        {
            LinkDetails result = MessageFormatter.GetLinkDetails("/relative");

            ClassicAssert.AreEqual(LinkAction.External, result.Action);
            ClassicAssert.AreEqual("/relative", result.Argument);
        }

        [TestCase("https://dev.ppy.sh/home/changelog", "")]
        [TestCase("https://dev.ppy.sh/home/changelog/lazer/2021.1012", "lazer/2021.1012")]
        public void TestChangelogLinks(string link, string expectedArg)
        {
            LinkDetails result = MessageFormatter.GetLinkDetails(link);

            ClassicAssert.AreEqual(LinkAction.OpenChangelog, result.Action);
            ClassicAssert.AreEqual(expectedArg, result.Argument);
        }
    }
}
