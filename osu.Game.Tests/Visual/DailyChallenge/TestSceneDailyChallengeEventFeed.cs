// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.DailyChallenge;
using osu.Game.Screens.OnlinePlay.DailyChallenge.Events;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.DailyChallenge
{
    public partial class TestSceneDailyChallengeEventFeed : OsuTestScene
    {
        private DailyChallengeEventFeed feed = null!;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create content", () => Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4,
                },
                feed = new DailyChallengeEventFeed
                {
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.3f,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            });

            AddSliderStep("adjust width", 0.1f, 1, 1, width =>
            {
                if (feed.IsNotNull())
                    feed.Width = width;
            });
            AddSliderStep("adjust height", 0.1f, 1, 0.3f, height =>
            {
                if (feed.IsNotNull())
                    feed.Height = height;
            });
        }

        [Test]
        public void TestBasicAppearance()
        {
            AddRepeatStep("add normal score", () =>
            {
                var ev = new NewScoreEvent(1, new APIUser
                {
                    Id = 2,
                    Username = "peppy",
                    CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                }, RNG.Next(1_000_000), null);

                feed.AddNewScore(ev);
            }, 50);

            AddRepeatStep("add new user best", () =>
            {
                var ev = new NewScoreEvent(1, new APIUser
                {
                    Id = 2,
                    Username = "peppy",
                    CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                }, RNG.Next(1_000_000), RNG.Next(11, 1000));

                var testScore = TestResources.CreateTestScoreInfo();
                testScore.TotalScore = RNG.Next(1_000_000);

                feed.AddNewScore(ev);
            }, 50);

            AddRepeatStep("add top 10 score", () =>
            {
                var ev = new NewScoreEvent(1, new APIUser
                {
                    Id = 2,
                    Username = "peppy",
                    CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                }, RNG.Next(1_000_000), RNG.Next(1, 10));

                feed.AddNewScore(ev);
            }, 50);
        }

        [Test]
        public void TestMassAdd()
        {
            AddStep("add 1000 scores at once", () =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    var ev = new NewScoreEvent(1, new APIUser
                    {
                        Id = 2,
                        Username = "peppy",
                        CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                    }, RNG.Next(1_000_000), null);

                    feed.AddNewScore(ev);
                }
            });
        }
    }
}
