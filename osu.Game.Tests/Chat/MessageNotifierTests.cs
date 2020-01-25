using NUnit.Framework;
using osu.Game.Online.Chat;

namespace osu.Game.Tests.Chat
{
    [TestFixture]
    public class MessageNotifierTests
    {
        private readonly MessageNotifier messageNotifier = new MessageNotifier();

        [Test]
        public void TestMentions()
        {
            // Message (with mention, different casing)
            Assert.IsTrue(messageNotifier.IsMentioning("Hey, Somebody Playing OSU!", "Somebody playing osu!"));

            // Message (with mention, underscores)
            Assert.IsTrue(messageNotifier.IsMentioning("Hey, Somebody_playing_osu!", "Somebody playing osu!"));

            // Message (with mention, different casing, underscores)
            Assert.IsTrue(messageNotifier.IsMentioning("Hey, Somebody_Playing_OSU!", "Somebody playing osu!"));

            // Message (without mention)
            Assert.IsTrue(!messageNotifier.IsMentioning("peppy, can you please fix this?", "Cookiezi"));
        }
    }
}
