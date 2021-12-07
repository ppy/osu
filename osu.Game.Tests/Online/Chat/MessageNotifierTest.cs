// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Online.Chat;

namespace osu.Game.Tests.Online.Chat
{
    [TestFixture]
    public class MessageNotifierTest
    {
        [Test]
        public void TestContainsUsernameMidlinePositive()
        {
            Assert.IsTrue(MessageNotifier.checkContainsUsername("This is a test message", "Test"));
        }

        [Test]
        public void TestContainsUsernameStartOfLinePositive()
        {
            Assert.IsTrue(MessageNotifier.checkContainsUsername("Test message", "Test"));
        }

        [Test]
        public void TestContainsUsernameEndOfLinePositive()
        {
            Assert.IsTrue(MessageNotifier.checkContainsUsername("This is a test", "Test"));
        }

        [Test]
        public void TestContainsUsernameMidlineNegative()
        {
            Assert.IsFalse(MessageNotifier.checkContainsUsername("This is a testmessage for notifications", "Test"));
        }

        [Test]
        public void TestContainsUsernameStartOfLineNegative()
        {
            Assert.IsFalse(MessageNotifier.checkContainsUsername("Testmessage", "Test"));
        }

        [Test]
        public void TestContainsUsernameEndOfLineNegative()
        {
            Assert.IsFalse(MessageNotifier.checkContainsUsername("This is a notificationtest", "Test"));
        }

        [Test]
        public void TestContainsUsernameBetweenInterpunction()
        {
            Assert.IsTrue(MessageNotifier.checkContainsUsername("Hello 'test'-message", "Test"));
        }

        [Test]
        public void TestContainsUsernameUnicode()
        {
            Assert.IsTrue(MessageNotifier.checkContainsUsername("Test \u0460\u0460 message", "\u0460\u0460"));
        }

        [Test]
        public void TestContainsUsernameUnicodeNegative()
        {
            Assert.IsFalse(MessageNotifier.checkContainsUsername("Test ha\u0460\u0460o message", "\u0460\u0460"));
        }

        [Test]
        public void TestContainsUsernameSpecialCharactersPositive()
        {
            Assert.IsTrue(MessageNotifier.checkContainsUsername("Test [#^-^#] message", "[#^-^#]"));
        }

        [Test]
        public void TestContainsUsernameSpecialCharactersNegative()
        {
            Assert.IsFalse(MessageNotifier.checkContainsUsername("Test pad[#^-^#]oru message", "[#^-^#]"));
        }
    }
}
