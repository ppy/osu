// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Resources;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.SongSelect
{
    [TestFixture]
    public partial class TestSceneBeatmapCarouselV2 : OsuManualInputManagerTestScene
    {
        private readonly BindableList<BeatmapSetInfo> beatmapSets = new BindableList<BeatmapSetInfo>();

        [Cached(typeof(BeatmapStore))]
        private BeatmapStore store;

        private OsuTextFlowContainer stats = null!;
        private BeatmapCarouselV2 carousel = null!;

        private int beatmapCount;

        public TestSceneBeatmapCarouselV2()
        {
            store = new TestBeatmapStore
            {
                BeatmapSets = { BindTarget = beatmapSets }
            };

            beatmapSets.BindCollectionChanged((_, _) =>
            {
                beatmapCount = beatmapSets.Sum(s => s.Beatmaps.Count);
            });

            Scheduler.AddDelayed(updateStats, 100, true);
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create components", () =>
            {
                beatmapSets.Clear();

                Box topBox;
                Children = new Drawable[]
                {
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        ColumnDimensions = new[]
                        {
                            new Dimension(GridSizeMode.Relative, 1),
                        },
                        RowDimensions = new[]
                        {
                            new Dimension(GridSizeMode.Absolute, 200),
                            new Dimension(),
                            new Dimension(GridSizeMode.Absolute, 200),
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                topBox = new Box
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Colour = Color4.Cyan,
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0.4f,
                                },
                            },
                            new Drawable[]
                            {
                                carousel = new BeatmapCarouselV2
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Width = 500,
                                    RelativeSizeAxes = Axes.Y,
                                },
                            },
                            new[]
                            {
                                new Box
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Colour = Color4.Cyan,
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0.4f,
                                },
                                topBox.CreateProxy(),
                            }
                        }
                    },
                    stats = new OsuTextFlowContainer(cp => cp.Font = FrameworkFont.Regular.With())
                    {
                        Padding = new MarginPadding(10),
                        TextAnchor = Anchor.CentreLeft,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                    },
                };
            });
        }

        [Test]
        public void TestBasic()
        {
            AddStep("add 10 beatmaps", () =>
            {
                for (int i = 0; i < 10; i++)
                    beatmapSets.Add(TestResources.CreateTestBeatmapSetInfo(RNG.Next(1, 4)));
            });

            AddStep("add 1 beatmap", () => beatmapSets.Add(TestResources.CreateTestBeatmapSetInfo(RNG.Next(1, 4))));

            AddStep("remove all beatmaps", () => beatmapSets.Clear());
        }

        [Test]
        public void TestAddRemoveOneByOne()
        {
            AddRepeatStep("add beatmaps", () => beatmapSets.Add(TestResources.CreateTestBeatmapSetInfo(RNG.Next(1, 4))), 20);

            AddRepeatStep("remove beatmaps", () => beatmapSets.RemoveAt(RNG.Next(0, beatmapSets.Count)), 20);
        }

        [Test]
        [Explicit]
        public void TestInsane()
        {
            const int count = 200000;

            List<BeatmapSetInfo> generated = new List<BeatmapSetInfo>();

            AddStep($"populate {count} test beatmaps", () =>
            {
                generated.Clear();
                Task.Run(() =>
                {
                    for (int j = 0; j < count; j++)
                        generated.Add(TestResources.CreateTestBeatmapSetInfo(RNG.Next(1, 4)));
                }).ConfigureAwait(true);
            });

            AddUntilStep("wait for beatmaps populated", () => generated.Count, () => Is.GreaterThan(count / 3));
            AddUntilStep("this takes a while", () => generated.Count, () => Is.GreaterThan(count / 3 * 2));
            AddUntilStep("maybe they are done now", () => generated.Count, () => Is.EqualTo(count));

            AddStep("add all beatmaps", () => beatmapSets.AddRange(generated));
        }

        private void updateStats()
        {
            if (carousel.IsNull())
                return;

            stats.Text = $"""
                                        store
                                          sets: {beatmapSets.Count}
                                          beatmaps: {beatmapCount}
                                        carousel:
                                          sorting: {carousel.IsFiltering}
                                          tracked: {carousel.ItemsTracked}
                                          displayable: {carousel.DisplayableItems}
                                          displayed: {carousel.VisibleItems}
                          """;
        }
    }
}
