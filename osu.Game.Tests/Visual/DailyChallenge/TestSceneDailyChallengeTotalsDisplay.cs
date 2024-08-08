// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.DailyChallenge;
using osu.Game.Screens.OnlinePlay.DailyChallenge.Events;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.DailyChallenge
{
    public partial class TestSceneDailyChallengeTotalsDisplay : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        [Test]
        public void TestBasicAppearance()
        {
            DailyChallengeTotalsDisplay totals = null!;

            AddStep("create content", () => Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4,
                },
                totals = new DailyChallengeTotalsDisplay
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            });
            AddSliderStep("adjust width", 0.1f, 1, 1, width =>
            {
                if (totals.IsNotNull())
                    totals.Width = width;
            });
            AddSliderStep("adjust height", 0.1f, 1, 1, height =>
            {
                if (totals.IsNotNull())
                    totals.Height = height;
            });
            AddToggleStep("toggle visible", v => totals.Alpha = v ? 1 : 0);

            AddStep("set counts", () => totals.SetInitialCounts(totalPassCount: 9650, cumulativeTotalScore: 10_000_000_000));

            AddStep("add normal score", () =>
            {
                var ev = new NewScoreEvent(1, new APIUser
                {
                    Id = 2,
                    Username = "peppy",
                    CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                }, RNG.Next(1_000_000), null);

                totals.AddNewScore(ev);
            });

            AddStep("spam scores", () =>
            {
                for (int i = 0; i < 1000; ++i)
                {
                    var ev = new NewScoreEvent(1, new APIUser
                    {
                        Id = 2,
                        Username = "peppy",
                        CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                    }, RNG.Next(1_000_000), RNG.Next(11, 1000));

                    var testScore = TestResources.CreateTestScoreInfo();
                    testScore.TotalScore = RNG.Next(1_000_000);

                    totals.AddNewScore(ev);
                }
            });
        }
    }
}
