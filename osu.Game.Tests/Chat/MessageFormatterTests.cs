﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Online.Chat;

namespace osu.Game.Tests.Chat
{
    [TestFixture]
    public class MessageFormatterTests
    {
        [Test]
        public void TestBareLink()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is a http://www.basic-link.com/?test=test." });

            Assert.AreEqual(result.Content, result.DisplayContent);
            Assert.AreEqual(1, result.Links.Count);
            Assert.AreEqual("http://www.basic-link.com/?test=test", result.Links[0].Url);
            Assert.AreEqual(10, result.Links[0].Index);
            Assert.AreEqual(36, result.Links[0].Length);
        }

        [Test]
        public void TestMultipleComplexLinks()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is a http://test.io/link#fragment. (see https://twitter.com). Also, This string should not be altered. http://example.com/" });

            Assert.AreEqual(result.Content, result.DisplayContent);
            Assert.AreEqual(3, result.Links.Count);

            Assert.AreEqual("http://test.io/link#fragment", result.Links[0].Url);
            Assert.AreEqual(10, result.Links[0].Index);
            Assert.AreEqual(28, result.Links[0].Length);

            Assert.AreEqual("https://twitter.com", result.Links[1].Url);
            Assert.AreEqual(45, result.Links[1].Index);
            Assert.AreEqual(19, result.Links[1].Length);

            Assert.AreEqual("http://example.com/", result.Links[2].Url);
            Assert.AreEqual(108, result.Links[2].Index);
            Assert.AreEqual(19, result.Links[2].Length);
        }

        [Test]
        public void TestAjaxLinks()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "https://twitter.com/#!/hashbanglinks" });

            Assert.AreEqual(result.Content, result.DisplayContent);
            Assert.AreEqual(result.Content, result.Links[0].Url);
            Assert.AreEqual(0, result.Links[0].Index);
            Assert.AreEqual(36, result.Links[0].Length);
        }

        [Test]
        public void TestUnixHomeLinks()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "http://www.chiark.greenend.org.uk/~sgtatham/putty/" });

            Assert.AreEqual(result.Content, result.DisplayContent);
            Assert.AreEqual(result.Content, result.Links[0].Url);
            Assert.AreEqual(0, result.Links[0].Index);
            Assert.AreEqual(50, result.Links[0].Length);
        }

        [Test]
        public void TestCaseInsensitiveLinks()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "look: http://puu.sh/7Ggh8xcC6/asf0asd9876.NEF" });

            Assert.AreEqual(result.Content, result.DisplayContent);
            Assert.AreEqual(6, result.Links[0].Index);
            Assert.AreEqual(39, result.Links[0].Length);
        }

        [Test]
        public void TestWikiLink()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is a [[Wiki Link]]." });

            Assert.AreEqual("This is a Wiki Link.", result.DisplayContent);
            Assert.AreEqual(1, result.Links.Count);
            Assert.AreEqual("https://osu.ppy.sh/wiki/Wiki Link", result.Links[0].Url);
            Assert.AreEqual(10, result.Links[0].Index);
            Assert.AreEqual(9, result.Links[0].Length);
        }

        [Test]
        public void TestMultiWikiLink()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is a [[Wiki Link]] [[Wiki:Link]][[Wiki.Link]]." });

            Assert.AreEqual("This is a Wiki Link Wiki:LinkWiki.Link.", result.DisplayContent);
            Assert.AreEqual(3, result.Links.Count);

            Assert.AreEqual("https://osu.ppy.sh/wiki/Wiki Link", result.Links[0].Url);
            Assert.AreEqual(10, result.Links[0].Index);
            Assert.AreEqual(9, result.Links[0].Length);

            Assert.AreEqual("https://osu.ppy.sh/wiki/Wiki:Link", result.Links[1].Url);
            Assert.AreEqual(20, result.Links[1].Index);
            Assert.AreEqual(9, result.Links[1].Length);

            Assert.AreEqual("https://osu.ppy.sh/wiki/Wiki.Link", result.Links[2].Url);
            Assert.AreEqual(29, result.Links[2].Index);
            Assert.AreEqual(9, result.Links[2].Length);
        }

        [Test]
        public void TestOldFormatLink()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is a (simple test)[https://osu.ppy.sh] of links." });

            Assert.AreEqual("This is a simple test of links.", result.DisplayContent);
            Assert.AreEqual(1, result.Links.Count);
            Assert.AreEqual("https://osu.ppy.sh", result.Links[0].Url);
            Assert.AreEqual(10, result.Links[0].Index);
            Assert.AreEqual(11, result.Links[0].Length);
        }

        [Test]
        public void TestNewFormatLink()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is a [https://osu.ppy.sh simple test]." });

            Assert.AreEqual("This is a simple test.", result.DisplayContent);
            Assert.AreEqual(1, result.Links.Count);
            Assert.AreEqual("https://osu.ppy.sh", result.Links[0].Url);
            Assert.AreEqual(10, result.Links[0].Index);
            Assert.AreEqual(11, result.Links[0].Length);
        }

        [Test]
        public void TestMarkdownFormatLink()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is a [simple test](https://osu.ppy.sh)." });

            Assert.AreEqual("This is a simple test.", result.DisplayContent);
            Assert.AreEqual(1, result.Links.Count);
            Assert.AreEqual("https://osu.ppy.sh", result.Links[0].Url);
            Assert.AreEqual(10, result.Links[0].Index);
            Assert.AreEqual(11, result.Links[0].Length);
        }

        [Test]
        public void TestChannelLink()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is an #english and #japanese." });

            Assert.AreEqual(result.Content, result.DisplayContent);
            Assert.AreEqual(2, result.Links.Count);
            Assert.AreEqual("osu://chan/#english", result.Links[0].Url);
            Assert.AreEqual("osu://chan/#japanese", result.Links[1].Url);
        }

        [Test]
        public void TestOsuProtocol()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is a custom protocol osu://chan/#english." });

            Assert.AreEqual(result.Content, result.DisplayContent);
            Assert.AreEqual(1, result.Links.Count);
            Assert.AreEqual("osu://chan/#english", result.Links[0].Url);
            Assert.AreEqual(26, result.Links[0].Index);
            Assert.AreEqual(19, result.Links[0].Length);

            result = MessageFormatter.FormatMessage(new Message { Content = "This is a [custom protocol](osu://chan/#english)." });

            Assert.AreEqual("This is a custom protocol.", result.DisplayContent);
            Assert.AreEqual(1, result.Links.Count);
            Assert.AreEqual("osu://chan/#english", result.Links[0].Url);
            Assert.AreEqual("#english", result.Links[0].Argument);
            Assert.AreEqual(10, result.Links[0].Index);
            Assert.AreEqual(15, result.Links[0].Length);
        }

        [Test]
        public void TestOsuMpProtocol()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "Join my multiplayer game osump://12346." });

            Assert.AreEqual(result.Content, result.DisplayContent);
            Assert.AreEqual(1, result.Links.Count);
            Assert.AreEqual("osump://12346", result.Links[0].Url);
            Assert.AreEqual(25, result.Links[0].Index);
            Assert.AreEqual(13, result.Links[0].Length);
        }

        [Test]
        public void TestRecursiveBreaking()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is a [https://osu.ppy.sh [[simple test]]]." });

            Assert.AreEqual("This is a [[simple test]].", result.DisplayContent);
            Assert.AreEqual(1, result.Links.Count);
            Assert.AreEqual("https://osu.ppy.sh", result.Links[0].Url);
            Assert.AreEqual(10, result.Links[0].Index);
            Assert.AreEqual(15, result.Links[0].Length);
        }

        [Test]
        public void TestLinkComplex()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "This is a [http://www.simple-test.com simple test] with some [traps] and [[wiki links]]. Don't forget to visit https://osu.ppy.sh (now!)[http://google.com]\uD83D\uDE12" });

            Assert.AreEqual("This is a simple test with some [traps] and wiki links. Don't forget to visit https://osu.ppy.sh now!\0\0\0", result.DisplayContent);
            Assert.AreEqual(5, result.Links.Count);

            Link f = result.Links.Find(l => l.Url == "https://osu.ppy.sh/wiki/wiki links");
            Assert.AreEqual(44, f.Index);
            Assert.AreEqual(10, f.Length);

            f = result.Links.Find(l => l.Url == "http://www.simple-test.com");
            Assert.AreEqual(10, f.Index);
            Assert.AreEqual(11, f.Length);

            f = result.Links.Find(l => l.Url == "http://google.com");
            Assert.AreEqual(97, f.Index);
            Assert.AreEqual(4, f.Length);

            f = result.Links.Find(l => l.Url == "https://osu.ppy.sh");
            Assert.AreEqual(78, f.Index);
            Assert.AreEqual(18, f.Length);

            f = result.Links.Find(l => l.Url == "\uD83D\uDE12");
            Assert.AreEqual(101, f.Index);
            Assert.AreEqual(3, f.Length);
        }

        [Test]
        public void TestEmoji()
        {
            Message result = MessageFormatter.FormatMessage(new Message { Content = "Hello world\uD83D\uDE12<--This is an emoji,There are more:\uD83D\uDE10\uD83D\uDE00,\uD83D\uDE20" });
            Assert.AreEqual("Hello world\0\0\0<--This is an emoji,There are more:\0\0\0\0\0\0,\0\0\0", result.DisplayContent);
            Assert.AreEqual(result.Links.Count, 4);
            Assert.AreEqual(result.Links[0].Index, 11);
            Assert.AreEqual(result.Links[1].Index, 49);
            Assert.AreEqual(result.Links[2].Index, 52);
            Assert.AreEqual(result.Links[3].Index, 56);
            Assert.AreEqual(result.Links[0].Url, "\uD83D\uDE12");
            Assert.AreEqual(result.Links[1].Url, "\uD83D\uDE10");
            Assert.AreEqual(result.Links[2].Url, "\uD83D\uDE00");
            Assert.AreEqual(result.Links[3].Url, "\uD83D\uDE20");
        }
    }
}
