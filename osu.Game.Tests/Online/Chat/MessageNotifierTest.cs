// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using NUnit.Framework.Legacy;
using osu.Game.Online.Chat;

namespace osu.Game.Tests.Online.Chat
{
    [TestFixture]
    public class MessageNotifierTest
    {
        [Test]
        public void TestContainsUsernameMidlinePositive()
        {
            ClassicAssert.True(MessageNotifier.MatchUsername("This is a test message", "Test").Success);
        }

        [Test]
        public void TestContainsUsernameStartOfLinePositive()
        {
            ClassicAssert.True(MessageNotifier.MatchUsername("Test message", "Test").Success);
        }

        [Test]
        public void TestContainsUsernameEndOfLinePositive()
        {
            ClassicAssert.True(MessageNotifier.MatchUsername("This is a test", "Test").Success);
        }

        [Test]
        public void TestContainsUsernameMidlineNegative()
        {
            ClassicAssert.False(MessageNotifier.MatchUsername("This is a testmessage for notifications", "Test").Success);
        }

        [Test]
        public void TestContainsUsernameStartOfLineNegative()
        {
            ClassicAssert.False(MessageNotifier.MatchUsername("Testmessage", "Test").Success);
        }

        [Test]
        public void TestContainsUsernameEndOfLineNegative()
        {
            ClassicAssert.False(MessageNotifier.MatchUsername("This is a notificationtest", "Test").Success);
        }

        [Test]
        public void TestContainsUsernameBetweenPunctuation()
        {
            ClassicAssert.True(MessageNotifier.MatchUsername("Hello 'test'-message", "Test").Success);
        }

        [Test]
        public void TestContainsUsernameUnicode()
        {
            ClassicAssert.True(MessageNotifier.MatchUsername("Test \u0460\u0460 message", "\u0460\u0460").Success);
        }

        [Test]
        public void TestContainsUsernameUnicodeNegative()
        {
            ClassicAssert.False(MessageNotifier.MatchUsername("Test ha\u0460\u0460o message", "\u0460\u0460").Success);
        }

        [Test]
        public void TestContainsUsernameSpecialCharactersPositive()
        {
            ClassicAssert.True(MessageNotifier.MatchUsername("Test [#^-^#] message", "[#^-^#]").Success);
        }

        [Test]
        public void TestContainsUsernameSpecialCharactersNegative()
        {
            ClassicAssert.False(MessageNotifier.MatchUsername("Test pad[#^-^#]oru message", "[#^-^#]").Success);
        }

        [Test]
        public void TestContainsUsernameAtSign()
        {
            ClassicAssert.True(MessageNotifier.MatchUsername("@username hi", "username").Success);
        }

        [Test]
        public void TestContainsUsernameColon()
        {
            ClassicAssert.True(MessageNotifier.MatchUsername("username: hi", "username").Success);
        }
    }
}
