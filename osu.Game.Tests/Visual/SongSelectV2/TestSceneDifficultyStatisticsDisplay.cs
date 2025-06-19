// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Screens.SelectV2;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneDifficultyStatisticsDisplay : OsuTestScene
    {
        private Container displayContainer = null!;
        private BeatmapTitleWedge.DifficultyStatisticsDisplay display = null!;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("setup", () =>
            {
                Child = displayContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 300,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                        },
                        display = new BeatmapTitleWedge.DifficultyStatisticsDisplay
                        {
                            RelativeSizeAxes = Axes.X,
                            Statistics = new[]
                            {
                                new BeatmapTitleWedge.StatisticDifficulty.Data("Statistic 1", 0.5f, 0.5f, 1f),
                                new BeatmapTitleWedge.StatisticDifficulty.Data("Statistic 2", 0.8f, 0.8f, 1f),
                                new BeatmapTitleWedge.StatisticDifficulty.Data("Statistic 3", 0.7f, 0.7f, 1f),
                            }
                        }
                    }
                };
            });
            AddSliderStep("display width", 0, 300, 300, v =>
            {
                if (displayContainer.IsNotNull())
                    displayContainer.Width = v;
            });
        }

        [Test]
        public void TestEmpty()
        {
            AddStep("set empty", () => display.Statistics = Array.Empty<BeatmapTitleWedge.StatisticDifficulty.Data>());
            AddAssert("no statistics", () => !display.ChildrenOfType<BeatmapTitleWedge.StatisticDifficulty>().Any());
            AddAssert("no tiny statistics", () => !display.ChildrenOfType<GridContainer>().Single().Content.Any());
        }

        [Test]
        public void TestDisplay()
        {
            AddStep("change data with same labels", () => display.Statistics = new[]
            {
                new BeatmapTitleWedge.StatisticDifficulty.Data("Statistic 1", 0.2f, 0.2f, 1f),
                new BeatmapTitleWedge.StatisticDifficulty.Data("Statistic 2", 0.7f, 0.7f, 1f),
                new BeatmapTitleWedge.StatisticDifficulty.Data("Statistic 3", 0.4f, 0.8f, 1f),
            });

            AddStep("change data with different labels", () => display.Statistics = new[]
            {
                new BeatmapTitleWedge.StatisticDifficulty.Data("Statistic 4", 0.3f, 0.3f, 1f),
                new BeatmapTitleWedge.StatisticDifficulty.Data("Statistic 5", 0.8f, 0.8f, 1f),
                new BeatmapTitleWedge.StatisticDifficulty.Data("Statistic 6", 0.5f, 0.5f, 1f),
            });

            AddAssert("statistics visible", () => display.ChildrenOfType<BeatmapTitleWedge.StatisticDifficulty>().First().Parent!.Alpha == 1);
            AddAssert("tiny statistics hidden", () => display.ChildrenOfType<GridContainer>().Last().Alpha == 0);

            AddStep("shrink width", () => displayContainer.Width = 100);
            AddAssert("statistics hidden", () => display.ChildrenOfType<BeatmapTitleWedge.StatisticDifficulty>().First().Parent!.Alpha == 0);
            AddUntilStep("tiny statistics displayed", () => display.ChildrenOfType<GridContainer>().Last().Alpha == 1);
        }

        [Test]
        public void TestContraction()
        {
            AddAssert("statistics visible", () => display.ChildrenOfType<BeatmapTitleWedge.StatisticDifficulty>().First().Parent!.Alpha == 1);
            AddAssert("tiny statistics hidden", () => display.ChildrenOfType<GridContainer>().Last().Alpha == 0);

            AddStep("set too many statistics", () => display.Statistics = new[]
            {
                new BeatmapTitleWedge.StatisticDifficulty.Data("Statistic 1", 0.2f, 0.2f, 1f),
                new BeatmapTitleWedge.StatisticDifficulty.Data("Statistic 2", 0.7f, 0.7f, 1f),
                new BeatmapTitleWedge.StatisticDifficulty.Data("Statistic 3", 0.4f, 0.8f, 1f),
                new BeatmapTitleWedge.StatisticDifficulty.Data("Statistic 4", 0.3f, 0.3f, 1f),
                new BeatmapTitleWedge.StatisticDifficulty.Data("Statistic 5", 0.8f, 0.8f, 1f),
                new BeatmapTitleWedge.StatisticDifficulty.Data("Statistic 6", 0.5f, 0.5f, 1f),
            });

            AddAssert("statistics hidden", () => display.ChildrenOfType<BeatmapTitleWedge.StatisticDifficulty>().First().Parent!.Alpha == 0);
            AddUntilStep("tiny statistics displayed", () => display.ChildrenOfType<GridContainer>().Last().Alpha == 1);

            AddStep("set less statistics", () => display.Statistics = new[]
            {
                new BeatmapTitleWedge.StatisticDifficulty.Data("Statistic 1", 0.2f, 0.2f, 1f),
            });

            AddAssert("tiny statistics hidden", () => display.ChildrenOfType<GridContainer>().Last().Alpha == 0);
            AddUntilStep("statistics visible", () => display.ChildrenOfType<BeatmapTitleWedge.StatisticDifficulty>().First().Parent!.Alpha == 1);
        }

        [Test]
        public void TestAutoSize()
        {
            AddStep("setup auto size", () => Child = display = new BeatmapTitleWedge.DifficultyStatisticsDisplay(true)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Statistics = new[]
                {
                    new BeatmapTitleWedge.StatisticDifficulty.Data("Statistic 1", 0.5f, 0.5f, 1f),
                    new BeatmapTitleWedge.StatisticDifficulty.Data("Statistic 2", 0.8f, 0.8f, 1f),
                    new BeatmapTitleWedge.StatisticDifficulty.Data("Statistic 3", 0.7f, 0.7f, 1f),
                }
            });

            AddAssert("statistics visible", () => display.ChildrenOfType<BeatmapTitleWedge.StatisticDifficulty>().First().Parent!.Alpha == 1);
            AddAssert("tiny statistics hidden", () => display.ChildrenOfType<GridContainer>().Last().Alpha == 0);

            AddStep("set too many statistics", () => display.Statistics = new[]
            {
                new BeatmapTitleWedge.StatisticDifficulty.Data("Statistic 1", 0.2f, 0.2f, 1f),
                new BeatmapTitleWedge.StatisticDifficulty.Data("Statistic 2", 0.7f, 0.7f, 1f),
                new BeatmapTitleWedge.StatisticDifficulty.Data("Statistic 3", 0.4f, 0.8f, 1f),
                new BeatmapTitleWedge.StatisticDifficulty.Data("Statistic 4", 0.3f, 0.3f, 1f),
                new BeatmapTitleWedge.StatisticDifficulty.Data("Statistic 5", 0.8f, 0.8f, 1f),
                new BeatmapTitleWedge.StatisticDifficulty.Data("Statistic 6", 0.5f, 0.5f, 1f),
            });

            AddAssert("statistics still visible", () => display.ChildrenOfType<BeatmapTitleWedge.StatisticDifficulty>().First().Parent!.Alpha == 1);
            AddAssert("tiny statistics still hidden", () => display.ChildrenOfType<GridContainer>().Last().Alpha == 0);

            AddStep("set less statistics", () => display.Statistics = new[]
            {
                new BeatmapTitleWedge.StatisticDifficulty.Data("Statistic 1", 0.2f, 0.2f, 1f),
            });

            AddAssert("statistics still visible", () => display.ChildrenOfType<BeatmapTitleWedge.StatisticDifficulty>().First().Parent!.Alpha == 1);
            AddAssert("tiny statistics still hidden", () => display.ChildrenOfType<GridContainer>().Last().Alpha == 0);
        }
    }
}
