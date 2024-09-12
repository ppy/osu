// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Ranking.Statistics;

namespace osu.Game.Tests.Visual.Ranking
{
    public partial class TestSceneProgressiveStatistic : OsuTestScene
    {
        private Container container;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = new Container
            {
                AutoSizeAxes = Axes.Y,
                Width = 700,
                Children = new Drawable[]
                {
                    container = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                    }
                }
            };
        });

        [Test]
        public void TestDisplayAccuracy()
        {
            AddStep("50%",
                () => container.Add(new ProgressiveStatistic(0.5000f, 1)));
            AddStep("90%",
                () => container.Add(new ProgressiveStatistic(0.9000f, 1)));
            AddStep("95%",
                () => container.Add(new ProgressiveStatistic(0.9500f, 1)));
            AddStep("99%",
                () => container.Add(new ProgressiveStatistic(0.9900f, 1)));
            AddStep("100%",
                () => container.Add(new ProgressiveStatistic(1, 1)));
        }

        [Test]
        public void TestDisplayCombo()
        {
            // testing with combo 1000, but can be used for pp
            AddStep("500/1000",
                () => container.Add(new ProgressiveStatistic(500, 1000)));
            AddStep("750/1000",
                () => container.Add(new ProgressiveStatistic(750, 1000)));
            AddStep("950/1000",
                () => container.Add(new ProgressiveStatistic(950, 1000)));
            AddStep("999/1000",
                () => container.Add(new ProgressiveStatistic(999, 1000)));
            AddStep("1000/1000",
                () => container.Add(new ProgressiveStatistic(1000, 1000)));
        }
    }
}
