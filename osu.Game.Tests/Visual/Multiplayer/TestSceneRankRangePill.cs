// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Lounge.Components;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneRankRangePill : OsuTestScene
    {
        private readonly Mock<MultiplayerClient> multiplayerClient = new Mock<MultiplayerClient>();

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            // not used directly in component, but required due to it inheriting from OnlinePlayComposite.
            new CachedModelDependencyContainer<Room>(base.CreateChildDependencies(parent));

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.CacheAs(multiplayerClient.Object);

            Child = new RankRangePill
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            };
        }

        [Test]
        public void TestSingleUser()
        {
            setupRoomWithUsers(new APIUser
            {
                Id = 2,
                Statistics = { GlobalRank = 1234 }
            });
        }

        [Test]
        public void TestMultipleUsers()
        {
            setupRoomWithUsers(
                new APIUser
                {
                    Id = 2,
                    Statistics = { GlobalRank = 1234 }
                },
                new APIUser
                {
                    Id = 3,
                    Statistics = { GlobalRank = 3333 }
                },
                new APIUser
                {
                    Id = 4,
                    Statistics = { GlobalRank = 4321 }
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
            setupRoomWithUsers(
                new APIUser
                {
                    Id = 2,
                    Statistics = { GlobalRank = min }
                },
                new APIUser
                {
                    Id = 3,
                    Statistics = { GlobalRank = max }
                });
        }

        private void setupRoomWithUsers(params APIUser[] users)
        {
            AddStep("setup room", () =>
            {
                multiplayerClient.SetupGet(m => m.Room).Returns(new MultiplayerRoom(0)
                {
                    Users = new List<MultiplayerRoomUser>(users.Select(apiUser => new MultiplayerRoomUser(apiUser.Id) { User = apiUser }))
                });

                multiplayerClient.Raise(m => m.RoomUpdated -= null);
            });
        }
    }
}
