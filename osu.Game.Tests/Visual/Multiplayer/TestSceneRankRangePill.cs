// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.OnlinePlay.Lounge.Components;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneRankRangePill : MultiplayerTestScene
    {
        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            Child = new RankRangePill
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            };
        });

        [Test]
        public void TestSingleUser()
        {
            AddStep("add user", () =>
            {
                MultiplayerClient.AddUser(new APIUser
                {
                    Id = 2,
                    Statistics = { GlobalRank = 1234 }
                });

                // Remove the local user so only the one above is displayed.
                MultiplayerClient.RemoveUser(API.LocalUser.Value);
            });
        }

        [Test]
        public void TestMultipleUsers()
        {
            AddStep("add users", () =>
            {
                MultiplayerClient.AddUser(new APIUser
                {
                    Id = 2,
                    Statistics = { GlobalRank = 1234 }
                });

                MultiplayerClient.AddUser(new APIUser
                {
                    Id = 3,
                    Statistics = { GlobalRank = 3333 }
                });

                MultiplayerClient.AddUser(new APIUser
                {
                    Id = 4,
                    Statistics = { GlobalRank = 4321 }
                });

                // Remove the local user so only the ones above are displayed.
                MultiplayerClient.RemoveUser(API.LocalUser.Value);
            });
        }

        [TestCase(1, 10)]
        [TestCase(10, 100)]
        [TestCase(100, 1000)]
        [TestCase(1000, 10000)]
        [TestCase(10000, 100000)]
        [TestCase(100000, 1000000)]
        [TestCase(1000000, 10000000)]
        public void TestRange(int min, int max)
        {
            AddStep("add users", () =>
            {
                MultiplayerClient.AddUser(new APIUser
                {
                    Id = 2,
                    Statistics = { GlobalRank = min }
                });

                MultiplayerClient.AddUser(new APIUser
                {
                    Id = 3,
                    Statistics = { GlobalRank = max }
                });

                // Remove the local user so only the ones above are displayed.
                MultiplayerClient.RemoveUser(API.LocalUser.Value);
            });
        }
    }
}
