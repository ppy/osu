// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.OnlinePlay.Matchmaking.Queue;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneMatchmakingCloud : OsuTestScene
    {
        private CloudVisualisation cloud = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Child = cloud = new CloudVisualisation
            {
                RelativeSizeAxes = Axes.Both,
            };
        }

        [Test]
        public void TestBasic()
        {
            AddStep("refresh users", () =>
            {
                var testUsers = Enumerable.Range(0, 50).Select(_ => new APIUser
                {
                    Username = "peppy",
                    Statistics = new UserStatistics { GlobalRank = 1234 },
                    Id = RNG.Next(2, 30000000),
                }).ToArray();

                cloud.Users = testUsers;
            });
        }
    }
}
