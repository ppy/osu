// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Humanizer;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Online.Multiplayer;
using osu.Game.Tests.Visual.Multiplayer;
using osu.Game.Users;

namespace osu.Game.Tests.NonVisual.Multiplayer
{
    [HeadlessTest]
    public class StatefulMultiplayerClientTest : MultiplayerTestScene
    {
        [Test]
        public void TestPlayingUserTracking()
        {
            int id = 2000;

            AddRepeatStep("add some users", () => Client.AddUser(new User { Id = id++ }), 5);
            checkPlayingUserCount(0);

            changeState(3, MultiplayerUserState.WaitingForLoad);
            checkPlayingUserCount(3);

            changeState(3, MultiplayerUserState.Playing);
            checkPlayingUserCount(3);

            changeState(3, MultiplayerUserState.Results);
            checkPlayingUserCount(0);

            changeState(6, MultiplayerUserState.WaitingForLoad);
            checkPlayingUserCount(6);

            AddStep("another user left", () => Client.RemoveUser(Client.Room?.Users.Last().User));
            checkPlayingUserCount(5);

            AddStep("leave room", () => Client.LeaveRoom());
            checkPlayingUserCount(0);
        }

        private void checkPlayingUserCount(int expectedCount)
            => AddAssert($"{"user".ToQuantity(expectedCount)} playing", () => Client.CurrentMatchPlayingUserIds.Count == expectedCount);

        private void changeState(int userCount, MultiplayerUserState state)
            => AddStep($"{"user".ToQuantity(userCount)} in {state}", () =>
            {
                for (int i = 0; i < userCount; ++i)
                {
                    var userId = Client.Room?.Users[i].UserID ?? throw new AssertionException("Room cannot be null!");
                    Client.ChangeUserState(userId, state);
                }
            });
    }
}
