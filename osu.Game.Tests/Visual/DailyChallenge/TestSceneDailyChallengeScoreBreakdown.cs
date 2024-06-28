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
    public partial class TestSceneDailyChallengeScoreBreakdown : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        [Test]
        public void TestBasicAppearance()
        {
            DailyChallengeScoreBreakdown breakdown = null!;

            AddStep("create content", () => Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4,
                },
                breakdown = new DailyChallengeScoreBreakdown
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            });
            AddSliderStep("adjust width", 0.1f, 1, 1, width =>
            {
                if (breakdown.IsNotNull())
                    breakdown.Width = width;
            });
            AddSliderStep("adjust height", 0.1f, 1, 1, height =>
            {
                if (breakdown.IsNotNull())
                    breakdown.Height = height;
            });

            AddStep("set initial data", () => breakdown.SetInitialCounts([1, 4, 9, 16, 25, 36, 49, 36, 25, 16, 9, 4, 1]));
            AddStep("add new score", () =>
            {
                var testScore = TestResources.CreateTestScoreInfo();
                testScore.TotalScore = RNG.Next(1_000_000);

                breakdown.AddNewScore(testScore);
            });
        }
    }
}
