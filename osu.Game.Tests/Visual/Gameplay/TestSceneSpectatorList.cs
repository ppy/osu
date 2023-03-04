// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public partial class TestSceneSpectatorList : OsuTestScene
    {
        private TestGameplaySpectatorList spectatorList;

        public TestSceneSpectatorList()
        {
            Child = spectatorList = new TestGameplaySpectatorList { };
        }

        [Test]
        public void TestManySpectators()
        {
            createSpectatorList();

            AddStep("Add peppy", () =>
            {
                createSpectator(new APIUser { Username = "peppy", Id = 3, AvatarUrl = "https://a.ppy.sh/1199528?1654635999.jpeg" });
            });

            AddStep("Add spectator", () =>
            {
                createRandomSpectator();
            });

            AddStep("Remove peppy", () =>
            {
                removeSpectator(new APIUser { Username = "peppy" });
            });
        }

        private void createSpectatorList()
        {
            AddStep("create spectator list", () =>
            {
                Child = spectatorList = new TestGameplaySpectatorList { };
            });
        }

        private void createSpectator(APIUser user) => spectatorList.Add(user);
        private void createRandomSpectator()
        {
            APIUser user = new APIUser
            {
                Username = RNG.NextDouble(1_000_000.0, 100_000_000_000.0).ToString(),
            };

            spectatorList.Add(user);
        }
        private void removeSpectator(APIUser user) => spectatorList.Remove(user);

        private partial class TestGameplaySpectatorList : GameplaySpectatorList
        {
            public float Spacing => Flow.Spacing.Y;
        }
    }
}
