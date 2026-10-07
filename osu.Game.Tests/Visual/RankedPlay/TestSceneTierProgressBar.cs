// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.RankedPlay;
using osu.Game.Overlays;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Matchmaking.Queue;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneTierProgressBar : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        private readonly Container content;
        protected override Container<Drawable> Content => content;

        private TierProgressBar progressBar = null!;

        private int customRating;
        private float width = 1.0f;

        public TestSceneTierProgressBar()
        {
            base.Content.Child = content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(10),
                Child = createProgressBar(),
            };
            AddSliderStep("width", 0.1f, 1.0f, 1.0f, value =>
            {
                width = value;

                if (progressBar.IsLoaded)
                    progressBar.Width = value;
            });
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create", () => Child = createProgressBar());
        }

        [Test]
        public void TestRatingChange()
        {
            AddStep("set to 650", () => progressBar.SetRating(650));
            AddStep("set to 935", () => progressBar.SetRating(935));
            AddStep("set to 1861", () => progressBar.SetRating(1861));
            AddStep("set to 1975", () => progressBar.SetRating(1975));
            AddStep("set to 2210", () => progressBar.SetRating(2210));
            AddStep("set to 2750", () => progressBar.SetRating(2750));
            AddSliderStep("custom rating", 600, 3000, 1000, value => customRating = value);
            AddStep("set custom rating", () => progressBar.SetRating(customRating));
        }

        [Test]
        public void TestEntering()
        {
            AddStep("set to 1250", () => progressBar.SetRating(1250, true));
        }

        [Test]
        public void TestUpdateDivisions()
        {
            float originalLength = 0;

            AddStep("set to 2500", () => progressBar.SetRating(2500));
            AddStep("skip animation", () =>
            {
                progressBar.FinishTransforms();
                progressBar.ChildrenOfType<FillFlowContainer<TierProgressBar.DivisionSegment>>().Single().FinishTransforms();
            });
            AddStep("save length", () => originalLength = getLastDivisionLength());
            AddStep("update divisions", () => progressBar.Divisions = new List<APIRankedPlayDivision>(DIVISIONS)
            {
                [^1] = new APIRankedPlayDivision { Tier = RankingTier.Lustrous, StartRating = 2400, EndRating = 2700 },
            });
            AddAssert("last division is shorter", () => getLastDivisionLength() / originalLength, () => Is.EqualTo(0.5).Within(0.001));

            float getLastDivisionLength() => this.ChildrenOfType<TierProgressBar.DivisionSegment>().Last().DrawWidth;
        }

        private TierProgressBar createProgressBar() => progressBar = new TierProgressBar
        {
            RelativeSizeAxes = Axes.X,
            Width = width,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Padding = new MarginPadding { Horizontal = 20 },
            Divisions = DIVISIONS,
        };

        public static readonly APIRankedPlayDivision[] DIVISIONS =
        [
            new APIRankedPlayDivision { Tier = RankingTier.Bronze, Division = RankedPlayDivision.I, StartRating = 600, EndRating = 699 },
            new APIRankedPlayDivision { Tier = RankingTier.Bronze, Division = RankedPlayDivision.II, StartRating = 700, EndRating = 799 },
            new APIRankedPlayDivision { Tier = RankingTier.Bronze, Division = RankedPlayDivision.III, StartRating = 800, EndRating = 899 },
            new APIRankedPlayDivision { Tier = RankingTier.Silver, Division = RankedPlayDivision.I, StartRating = 900, EndRating = 999 },
            new APIRankedPlayDivision { Tier = RankingTier.Silver, Division = RankedPlayDivision.II, StartRating = 1000, EndRating = 1099 },
            new APIRankedPlayDivision { Tier = RankingTier.Silver, Division = RankedPlayDivision.III, StartRating = 1100, EndRating = 1199 },
            new APIRankedPlayDivision { Tier = RankingTier.Gold, Division = RankedPlayDivision.I, StartRating = 1200, EndRating = 1299 },
            new APIRankedPlayDivision { Tier = RankingTier.Gold, Division = RankedPlayDivision.II, StartRating = 1300, EndRating = 1399 },
            new APIRankedPlayDivision { Tier = RankingTier.Gold, Division = RankedPlayDivision.III, StartRating = 1400, EndRating = 1499 },
            new APIRankedPlayDivision { Tier = RankingTier.Platinum, Division = RankedPlayDivision.I, StartRating = 1500, EndRating = 1599 },
            new APIRankedPlayDivision { Tier = RankingTier.Platinum, Division = RankedPlayDivision.II, StartRating = 1600, EndRating = 1699 },
            new APIRankedPlayDivision { Tier = RankingTier.Platinum, Division = RankedPlayDivision.III, StartRating = 1700, EndRating = 1799 },
            new APIRankedPlayDivision { Tier = RankingTier.Rhodium, Division = RankedPlayDivision.I, StartRating = 1800, EndRating = 1899 },
            new APIRankedPlayDivision { Tier = RankingTier.Rhodium, Division = RankedPlayDivision.II, StartRating = 1900, EndRating = 1999 },
            new APIRankedPlayDivision { Tier = RankingTier.Rhodium, Division = RankedPlayDivision.III, StartRating = 2000, EndRating = 2099 },
            new APIRankedPlayDivision { Tier = RankingTier.Radiant, Division = RankedPlayDivision.I, StartRating = 2100, EndRating = 2199 },
            new APIRankedPlayDivision { Tier = RankingTier.Radiant, Division = RankedPlayDivision.II, StartRating = 2200, EndRating = 2299 },
            new APIRankedPlayDivision { Tier = RankingTier.Radiant, Division = RankedPlayDivision.III, StartRating = 2300, EndRating = 2399 },
            new APIRankedPlayDivision { Tier = RankingTier.Lustrous, StartRating = 2400, EndRating = 3000 },
        ];
    }
}
