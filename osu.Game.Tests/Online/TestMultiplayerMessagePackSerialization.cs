// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using MessagePack;
using NUnit.Framework;
using osu.Game.Online;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.TeamVersus;

namespace osu.Game.Tests.Online
{
    [TestFixture]
    public class TestMultiplayerMessagePackSerialization
    {
        [Test]
        public void TestSerialiseRoom()
        {
            var room = new MultiplayerRoom(1)
            {
                MatchState = new TeamVersusRoomState()
            };

            byte[] serialized = MessagePackSerializer.Serialize(room);

            var deserialized = MessagePackSerializer.Deserialize<MultiplayerRoom>(serialized);

            Assert.IsTrue(deserialized.MatchState is TeamVersusRoomState);
        }

        [Test]
        public void TestSerialiseUserStateExpected()
        {
            var state = new TeamVersusUserState();

            byte[] serialized = MessagePackSerializer.Serialize(typeof(MatchUserState), state);
            var deserialized = MessagePackSerializer.Deserialize<MatchUserState>(serialized);

            Assert.IsTrue(deserialized is TeamVersusUserState);
        }

        [Test]
        public void TestSerialiseUnionFailsWithSignalR()
        {
            var state = new TeamVersusUserState();

            // SignalR serialises using the actual type, rather than a base specification.
            byte[] serialized = MessagePackSerializer.Serialize(typeof(TeamVersusUserState), state);

            // works with explicit type specified.
            MessagePackSerializer.Deserialize<TeamVersusUserState>(serialized);

            // fails with base (union) type.
            Assert.Throws<MessagePackSerializationException>(() => MessagePackSerializer.Deserialize<MatchUserState>(serialized));
        }

        [Test]
        public void TestSerialiseUnionSucceedsWithWorkaround()
        {
            var state = new TeamVersusUserState();

            // SignalR serialises using the actual type, rather than a base specification.
            byte[] serialized = MessagePackSerializer.Serialize(typeof(TeamVersusUserState), state, SignalRUnionWorkaroundResolver.OPTIONS);

            // works with explicit type specified.
            MessagePackSerializer.Deserialize<TeamVersusUserState>(serialized);

            // works with custom resolver.
            var deserialized = MessagePackSerializer.Deserialize<MatchUserState>(serialized, SignalRUnionWorkaroundResolver.OPTIONS);
            Assert.IsTrue(deserialized is TeamVersusUserState);
        }
    }
}
