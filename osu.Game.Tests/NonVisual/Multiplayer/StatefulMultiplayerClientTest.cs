// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Humanizer;
using NUnit.Framework;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Testing;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.NonVisual.Multiplayer
{
    [HeadlessTest]
    public class StatefulMultiplayerClientTest : MultiplayerTestScene
    {
        [Test]
        public void TestUserAddedOnJoin()
        {
            var user = new APIUser { Id = 33 };

            AddRepeatStep("add user multiple times", () => Client.AddUser(user), 3);
            AddAssert("room has 2 users", () => Client.Room?.Users.Count == 2);
        }

        [Test]
        public void TestUserRemovedOnLeave()
        {
            var user = new APIUser { Id = 44 };

            AddStep("add user", () => Client.AddUser(user));
            AddAssert("room has 2 users", () => Client.Room?.Users.Count == 2);

            AddRepeatStep("remove user multiple times", () => Client.RemoveUser(user), 3);
            AddAssert("room has 1 user", () => Client.Room?.Users.Count == 1);
        }

        [Test]
        public void TestPlayingUserTracking()
        {
            int id = 2000;

            AddRepeatStep("add some users", () => Client.AddUser(new APIUser { Id = id++ }), 5);
            checkPlayingUserCount(0);

            changeState(3, MultiplayerUserState.WaitingForLoad);
            checkPlayingUserCount(3);

            changeState(3, MultiplayerUserState.Playing);
            checkPlayingUserCount(3);

            changeState(3, MultiplayerUserState.Results);
            checkPlayingUserCount(0);

            changeState(6, MultiplayerUserState.WaitingForLoad);
            checkPlayingUserCount(6);

            AddStep("another user left", () => Client.RemoveUser((Client.Room?.Users.Last().User).AsNonNull()));
            checkPlayingUserCount(5);

            AddStep("leave room", () => Client.LeaveRoom());
            checkPlayingUserCount(0);
        }

        [Test]
        public void TestPlayingUsersUpdatedOnJoin()
        {
            AddStep("leave room", () => Client.LeaveRoom());
            AddUntilStep("wait for room part", () => !RoomJoined);

            AddStep("create room initially in gameplay", () =>
            {
                var newRoom = new Room();
                newRoom.CopyFrom(SelectedRoom.Value);

                newRoom.RoomID.Value = null;
                Client.RoomSetupAction = room =>
                {
                    room.State = MultiplayerRoomState.Playing;
                    room.Users.Add(new MultiplayerRoomUser(PLAYER_1_ID)
                    {
                        User = new APIUser { Id = PLAYER_1_ID },
                        State = MultiplayerUserState.Playing
                    });
                };

                RoomManager.CreateRoom(newRoom);
            });

            AddUntilStep("wait for room join", () => RoomJoined);
            checkPlayingUserCount(1);
        }

        private void checkPlayingUserCount(int expectedCount)
            => AddAssert($"{"user".ToQuantity(expectedCount)} playing", () => Client.CurrentMatchPlayingUserIds.Count == expectedCount);

        private void changeState(int userCount, MultiplayerUserState state)
            => AddStep($"{"user".ToQuantity(userCount)} in {state}", () =>
            {
                for (int i = 0; i < userCount; ++i)
                {
                    int userId = Client.Room?.Users[i].UserID ?? throw new AssertionException("Room cannot be null!");
                    Client.ChangeUserState(userId, state);
                }
            });
    }
}
