// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Visual.UserInterface;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneBeatmapTitleWedgeStatistic : ThemeComparisonTestScene
    {
        private BeatmapTitleWedge.StatisticPlayCount playCount = null!;
        private BeatmapTitleWedge.Statistic statistic2 = null!;
        private BeatmapTitleWedge.Statistic statistic3 = null!;
        private BeatmapTitleWedge.Statistic statistic4 = null!;

        public TestSceneBeatmapTitleWedgeStatistic()
            : base(false)
        {
        }

        [Test]
        public void TestLoading()
        {
            AddStep("setup", () => CreateThemedContent(OverlayColourScheme.Aquamarine));
            AddStep("set loading", () => this.ChildrenOfType<BeatmapTitleWedge.Statistic>().ForEach(s => s.Value = null));
            AddWaitStep("wait", 3);
            AddStep("set values", () =>
            {
                playCount.Value = new BeatmapTitleWedge.StatisticPlayCount.Data(1234, 12);
                statistic2.Value = "3,234";
                statistic3.Value = "12:34";
                statistic4.Value = "123";
            });
        }

        protected override Drawable CreateContent() => new FillFlowContainer
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Direction = FillDirection.Horizontal,
            AutoSizeAxes = Axes.Both,
            Children = new[]
            {
                playCount = new BeatmapTitleWedge.StatisticPlayCount(true, minSize: 50)
                {
                    Value = new BeatmapTitleWedge.StatisticPlayCount.Data(1234, 12),
                },
                statistic2 = new BeatmapTitleWedge.Statistic(OsuIcon.Clock, true, minSize: 30)
                {
                    Value = "3,234",
                    TooltipText = "Statistic 2",
                },
                statistic3 = new BeatmapTitleWedge.Statistic(OsuIcon.Metronome)
                {
                    Value = "12:34",
                    Margin = new MarginPadding { Right = 10f },
                    TooltipText = "Statistic 3",
                },
                statistic4 = new BeatmapTitleWedge.Statistic(OsuIcon.Graphics)
                {
                    Value = "123",
                    TooltipText = "Statistic 4",
                },
            },
        };
    }
}
