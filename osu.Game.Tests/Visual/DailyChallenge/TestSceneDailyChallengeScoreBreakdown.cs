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
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.DailyChallenge;
using osu.Game.Screens.OnlinePlay.DailyChallenge.Events;

namespace osu.Game.Tests.Visual.DailyChallenge
{
    public partial class TestSceneDailyChallengeScoreBreakdown : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        private DailyChallengeScoreBreakdown breakdown = null!;

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

            AddToggleStep("toggle visible", v => breakdown.Alpha = v ? 1 : 0);

            AddStep("set initial data", () => breakdown.SetInitialCounts([1, 4, 9, 16, 25, 36, 49, 36, 25, 16, 9, 4, 1]));
        }

        [Test]
        public void TestBasicAppearance()
        {
            AddStep("add new score", () =>
            {
                var ev = new NewScoreEvent(1, new APIUser
                {
                    Id = 2,
                    Username = "peppy",
                    CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                }, RNG.Next(1_000_000), null);

                breakdown.AddNewScore(ev);
            });
            AddStep("set user score", () => breakdown.UserBestScore.Value = new MultiplayerScore { TotalScore = RNG.Next(1_000_000) });
            AddStep("unset user score", () => breakdown.UserBestScore.Value = null);
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

                    breakdown.AddNewScore(ev);
                }
            });
        }
    }
}
