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
            Assert.IsTrue(MessageNotifier.MatchUsername("This is a test message", "Test").Success);
        }

        [Test]
        public void TestContainsUsernameStartOfLinePositive()
        {
            Assert.IsTrue(MessageNotifier.MatchUsername("Test message", "Test").Success);
        }

        [Test]
        public void TestContainsUsernameEndOfLinePositive()
        {
            Assert.IsTrue(MessageNotifier.MatchUsername("This is a test", "Test").Success);
        }

        [Test]
        public void TestContainsUsernameMidlineNegative()
        {
            Assert.IsFalse(MessageNotifier.MatchUsername("This is a testmessage for notifications", "Test").Success);
        }

        [Test]
        public void TestContainsUsernameStartOfLineNegative()
        {
            Assert.IsFalse(MessageNotifier.MatchUsername("Testmessage", "Test").Success);
        }

        [Test]
        public void TestContainsUsernameEndOfLineNegative()
        {
            Assert.IsFalse(MessageNotifier.MatchUsername("This is a notificationtest", "Test").Success);
        }

        [Test]
        public void TestContainsUsernameBetweenPunctuation()
        {
            Assert.IsTrue(MessageNotifier.MatchUsername("Hello 'test'-message", "Test").Success);
        }

        [Test]
        public void TestContainsUsernameUnicode()
        {
            Assert.IsTrue(MessageNotifier.MatchUsername("Test \u0460\u0460 message", "\u0460\u0460").Success);
        }

        [Test]
        public void TestContainsUsernameUnicodeNegative()
        {
            Assert.IsFalse(MessageNotifier.MatchUsername("Test ha\u0460\u0460o message", "\u0460\u0460").Success);
        }

        [Test]
        public void TestContainsUsernameSpecialCharactersPositive()
        {
            Assert.IsTrue(MessageNotifier.MatchUsername("Test [#^-^#] message", "[#^-^#]").Success);
        }

        [Test]
        public void TestContainsUsernameSpecialCharactersNegative()
        {
            Assert.IsFalse(MessageNotifier.MatchUsername("Test pad[#^-^#]oru message", "[#^-^#]").Success);
        }

        [Test]
        public void TestContainsUsernameAtSign()
        {
            Assert.IsTrue(MessageNotifier.MatchUsername("@username hi", "username").Success);
        }

        [Test]
        public void TestContainsUsernameColon()
        {
            Assert.IsTrue(MessageNotifier.MatchUsername("username: hi", "username").Success);
        }
    }
}
