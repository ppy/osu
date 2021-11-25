// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.OnlinePlay
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
    }
}
