// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.Play.HUD;
using osuTK;

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
                createSpectator(new APIUser { Username = "peppy" });
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
                Child = spectatorList = new TestGameplaySpectatorList
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Margin = new MarginPadding { Top = 50 },
                    Scale = new Vector2(1.5f),
                };
            });
        }

        private void createSpectator(APIUser user) => spectatorList.Add(user);
        private void createRandomSpectator()
        {
            APIUser user = new APIUser
            {
                Username = RNG.NextDouble(1_000.0, 100_000_000_000.0).ToString(),
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
