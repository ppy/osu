// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.DailyChallenge;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.DailyChallenge
{
    public partial class TestSceneDailyChallengeEventFeed : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        [Test]
        public void TestBasicAppearance()
        {
            DailyChallengeEventFeed feed = null!;

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
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            });
            AddSliderStep("adjust width", 0.1f, 1, 1, width =>
            {
                if (feed.IsNotNull())
                    feed.Width = width;
            });
            AddSliderStep("adjust height", 0.1f, 1, 1, height =>
            {
                if (feed.IsNotNull())
                    feed.Height = height;
            });

            AddStep("add normal score", () =>
            {
                var testScore = TestResources.CreateTestScoreInfo();
                testScore.TotalScore = RNG.Next(1_000_000);

                feed.AddNewScore(new DailyChallengeEventFeed.NewScoreEvent(testScore, null));
            });

            AddStep("add new user best", () =>
            {
                var testScore = TestResources.CreateTestScoreInfo();
                testScore.TotalScore = RNG.Next(1_000_000);

                feed.AddNewScore(new DailyChallengeEventFeed.NewScoreEvent(testScore, RNG.Next(1, 1000)));
            });

            AddStep("add top 10 score", () =>
            {
                var testScore = TestResources.CreateTestScoreInfo();
                testScore.TotalScore = RNG.Next(1_000_000);

                feed.AddNewScore(new DailyChallengeEventFeed.NewScoreEvent(testScore, RNG.Next(1, 10)));
            });
        }
    }
}
