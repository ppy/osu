// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
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
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Resources;
using osuTK.Graphics;
using BeatmapCarousel = osu.Game.Screens.SelectV2.BeatmapCarousel;

namespace osu.Game.Tests.Visual.SongSelect
{
    public abstract partial class BeatmapCarouselV2TestScene : OsuManualInputManagerTestScene
    {
        protected readonly BindableList<BeatmapSetInfo> BeatmapSets = new BindableList<BeatmapSetInfo>();

        protected BeatmapCarousel Carousel = null!;

        protected OsuScrollContainer<Drawable> Scroll => Carousel.ChildrenOfType<OsuScrollContainer<Drawable>>().Single();

        [Cached(typeof(BeatmapStore))]
        private BeatmapStore store;

        private OsuTextFlowContainer stats = null!;

        private int beatmapCount;

        protected BeatmapCarouselV2TestScene()
        {
            store = new TestBeatmapStore
            {
                BeatmapSets = { BindTarget = BeatmapSets }
            };

            BeatmapSets.BindCollectionChanged((_, _) => beatmapCount = BeatmapSets.Sum(s => s.Beatmaps.Count));

            Scheduler.AddDelayed(updateStats, 100, true);
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            RemoveAllBeatmaps();

            CreateCarousel();

            SortBy(new FilterCriteria { Sort = SortMode.Title });
        }

        protected void CreateCarousel()
        {
            AddStep("create components", () =>
            {
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
                                Carousel = new BeatmapCarousel
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
                    stats = new OsuTextFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Padding = new MarginPadding(10),
                        TextAnchor = Anchor.CentreLeft,
                    },
                };
            });
        }

        protected void SortBy(FilterCriteria criteria) => AddStep($"sort by {criteria.Sort}", () => Carousel.Filter(criteria));

        protected void WaitForDrawablePanels() => AddUntilStep("drawable panels loaded", () => Carousel.ChildrenOfType<BeatmapCarouselPanel>().Count(), () => Is.GreaterThan(0));
        protected void WaitForSorting() => AddUntilStep("sorting finished", () => Carousel.IsFiltering, () => Is.False);
        protected void WaitForScrolling() => AddUntilStep("scroll finished", () => Scroll.Current, () => Is.EqualTo(Scroll.Target));

        /// <summary>
        /// Add requested beatmap sets count to list.
        /// </summary>
        /// <param name="count">The count of beatmap sets to add.</param>
        /// <param name="fixedDifficultiesPerSet">If not null, the number of difficulties per set. If null, randomised difficulty count will be used.</param>
        protected void AddBeatmaps(int count, int? fixedDifficultiesPerSet = null) => AddStep($"add {count} beatmaps", () =>
        {
            for (int i = 0; i < count; i++)
                BeatmapSets.Add(TestResources.CreateTestBeatmapSetInfo(fixedDifficultiesPerSet ?? RNG.Next(1, 4)));
        });

        protected void RemoveAllBeatmaps() => AddStep("clear all beatmaps", () => BeatmapSets.Clear());

        protected void RemoveFirstBeatmap() =>
            AddStep("remove first beatmap", () =>
            {
                if (BeatmapSets.Count == 0) return;

                BeatmapSets.Remove(BeatmapSets.First());
            });

        private void updateStats()
        {
            if (Carousel.IsNull())
                return;

            stats.Clear();
            createHeader("beatmap store");
            stats.AddParagraph($"""
                                sets: {BeatmapSets.Count}
                                beatmaps: {beatmapCount}
                                """);
            createHeader("carousel");
            stats.AddParagraph($"""
                                sorting: {Carousel.IsFiltering}
                                tracked: {Carousel.ItemsTracked}
                                displayable: {Carousel.DisplayableItems}
                                displayed: {Carousel.VisibleItems}
                                selected: {Carousel.CurrentSelection}
                                """);

            void createHeader(string text)
            {
                stats.AddParagraph(string.Empty);
                stats.AddParagraph(text, cp =>
                {
                    cp.Font = cp.Font.With(size: 18, weight: FontWeight.Bold);
                });
            }
        }
    }
}
