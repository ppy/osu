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
            Assert.IsTrue(MessageNotifier.CheckContainsUsername("This is a test message", "Test"));
        }

        [Test]
        public void TestContainsUsernameStartOfLinePositive()
        {
            Assert.IsTrue(MessageNotifier.CheckContainsUsername("Test message", "Test"));
        }

        [Test]
        public void TestContainsUsernameEndOfLinePositive()
        {
            Assert.IsTrue(MessageNotifier.CheckContainsUsername("This is a test", "Test"));
        }

        [Test]
        public void TestContainsUsernameMidlineNegative()
        {
            Assert.IsFalse(MessageNotifier.CheckContainsUsername("This is a testmessage for notifications", "Test"));
        }

        [Test]
        public void TestContainsUsernameStartOfLineNegative()
        {
            Assert.IsFalse(MessageNotifier.CheckContainsUsername("Testmessage", "Test"));
        }

        [Test]
        public void TestContainsUsernameEndOfLineNegative()
        {
            Assert.IsFalse(MessageNotifier.CheckContainsUsername("This is a notificationtest", "Test"));
        }

        [Test]
        public void TestContainsUsernameBetweenPunctuation()
        {
            Assert.IsTrue(MessageNotifier.CheckContainsUsername("Hello 'test'-message", "Test"));
        }

        [Test]
        public void TestContainsUsernameUnicode()
        {
            Assert.IsTrue(MessageNotifier.CheckContainsUsername("Test \u0460\u0460 message", "\u0460\u0460"));
        }

        [Test]
        public void TestContainsUsernameUnicodeNegative()
        {
            Assert.IsFalse(MessageNotifier.CheckContainsUsername("Test ha\u0460\u0460o message", "\u0460\u0460"));
        }

        [Test]
        public void TestContainsUsernameSpecialCharactersPositive()
        {
            Assert.IsTrue(MessageNotifier.CheckContainsUsername("Test [#^-^#] message", "[#^-^#]"));
        }

        [Test]
        public void TestContainsUsernameSpecialCharactersNegative()
        {
            Assert.IsFalse(MessageNotifier.CheckContainsUsername("Test pad[#^-^#]oru message", "[#^-^#]"));
        }

        [Test]
        public void TestContainsUsernameAtSign()
        {
            Assert.IsTrue(MessageNotifier.CheckContainsUsername("@username hi", "username"));
        }

        [Test]
        public void TestContainsUsernameColon()
        {
            Assert.IsTrue(MessageNotifier.CheckContainsUsername("username: hi", "username"));
        }
    }
}
