// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.OnlinePlay.DailyChallenge;
using osu.Game.Screens.OnlinePlay.DailyChallenge.Events;
using osu.Game.Tests.Resources;

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
                breakdown = new DailyChallengeScoreBreakdown(
                    new Room
                    {
                        Name = "Daily Challenge: Febuary 31, 2069",
                        Playlist =
                        [
                            new PlaylistItem(TestResources.CreateTestBeatmapSetInfo().Beatmaps.First())
                            {
                                RequiredMods = [new APIMod(new OsuModTraceable())],
                                AllowedMods = [
                                    new APIMod(new OsuModDoubleTime()),
                                    new APIMod(new OsuModFlashlight()),
                                    new APIMod(new OsuModNightcore()),
                                    new APIMod(new OsuModBlinds()),
                                    new APIMod(new OsuModHidden())
                                ]
                            }
                        ],
                        EndDate = DateTimeOffset.Now.AddHours(12),
                        Category = RoomCategory.DailyChallenge
                    })
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

            AddStep("set initial data", () => breakdown.SetInitialCounts());
        }

        [Test]
        public void TestScaledAppearance()
        {
            AddStep("add score", () =>
            {
                var ev = new NewScoreEvent(1, new APIUser
                {
                    Id = 5,
                    Username = "peppy",
                    CoverUrl = TestResources.COVER_IMAGE_3,
                }, RNG.Next(1_500_000), null);

                breakdown.AddNewScore(ev);
            });
            AddSliderStep("number of bars", 1, 100, 13, bars =>
            {
                if (breakdown.IsNotNull())
                    breakdown.RescaleBar(bars, null);
            });
            AddSliderStep("bar range", 1, 20, 2, range =>
            {
                if (breakdown.IsNotNull())
                    breakdown.RescaleBar(null, range * 50_000);
            });
            AddStep("reset to default", () =>
            {
                breakdown.RescaleBar(13, 1_00_000);
            });
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
                    CoverUrl = TestResources.COVER_IMAGE_3,
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
                        CoverUrl = TestResources.COVER_IMAGE_3,
                    }, RNG.Next(1_000_000), null);

                    breakdown.AddNewScore(ev);
                }
            });
        }
    }
}
