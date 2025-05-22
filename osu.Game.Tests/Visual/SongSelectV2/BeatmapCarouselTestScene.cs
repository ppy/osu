// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Carousel;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Resources;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;
using BeatmapCarousel = osu.Game.Screens.SelectV2.BeatmapCarousel;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public abstract partial class BeatmapCarouselTestScene : OsuManualInputManagerTestScene
    {
        protected readonly BindableList<BeatmapSetInfo> BeatmapSets = new BindableList<BeatmapSetInfo>();

        protected TestBeatmapCarousel Carousel = null!;

        protected OsuScrollContainer<Drawable> Scroll => Carousel.ChildrenOfType<OsuScrollContainer<Drawable>>().Single();

        [Cached(typeof(BeatmapStore))]
        private BeatmapStore store;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        public Func<IEnumerable<BeatmapInfo>, BeatmapInfo>? BeatmapRecommendationFunction { get; set; }

        private OsuTextFlowContainer stats = null!;

        private int beatmapCount;

        protected int NewItemsPresentedInvocationCount;

        protected BeatmapCarouselTestScene()
        {
            store = new TestBeatmapStore
            {
                BeatmapSets = { BindTarget = BeatmapSets }
            };

            BeatmapSets.BindCollectionChanged((_, _) => beatmapCount = BeatmapSets.Sum(s => s.Beatmaps.Count));

            Scheduler.AddDelayed(updateStats, 100, true);
        }

        protected void CreateCarousel()
        {
            AddStep("create components", () =>
            {
                BeatmapRecommendationFunction = null;
                NewItemsPresentedInvocationCount = 0;

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
                                Carousel = new TestBeatmapCarousel
                                {
                                    NewItemsPresented = () => NewItemsPresentedInvocationCount++,
                                    ChooseRecommendedBeatmap = beatmaps => BeatmapRecommendationFunction?.Invoke(beatmaps) ?? beatmaps.First(),
                                    BleedTop = 50,
                                    BleedBottom = 50,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Width = 800,
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

            // Prefer title sorting so that order of carousel panels match order of BeatmapSets bindable.
            SortBy(SortMode.Title);
        }

        protected void SortBy(SortMode mode) => ApplyToFilter($"sort by {mode.GetDescription().ToLowerInvariant()}", c => c.Sort = mode);
        protected void GroupBy(GroupMode mode) => ApplyToFilter($"group by {mode.GetDescription().ToLowerInvariant()}", c => c.Group = mode);

        protected void SortAndGroupBy(SortMode sort, GroupMode group)
        {
            ApplyToFilter($"sort by {sort.GetDescription().ToLowerInvariant()} & group by {group.GetDescription().ToLowerInvariant()}", c =>
            {
                c.Sort = sort;
                c.Group = group;
            });
        }

        protected void ApplyToFilter(string description, Action<FilterCriteria>? apply)
        {
            AddStep(description, () =>
            {
                var criteria = Carousel.Criteria;
                apply?.Invoke(criteria);
                Carousel.Filter(criteria);
            });
        }

        protected void WaitForDrawablePanels() => AddUntilStep("drawable panels loaded", () => Carousel.ChildrenOfType<ICarouselPanel>().Count(), () => Is.GreaterThan(0));
        protected void WaitForFiltering() => AddUntilStep("filtering finished", () => Carousel.IsFiltering, () => Is.False);
        protected void WaitForScrolling() => AddUntilStep("scroll finished", () => Scroll.Current, () => Is.EqualTo(Scroll.Target));

        protected void SelectNextPanel() => AddStep("select next panel", () => InputManager.Key(Key.Down));
        protected void SelectPrevPanel() => AddStep("select prev panel", () => InputManager.Key(Key.Up));
        protected void SelectNextGroup() => AddStep("select next group", () => InputManager.Key(Key.Right));
        protected void SelectPrevGroup() => AddStep("select prev group", () => InputManager.Key(Key.Left));

        protected void Select() => AddStep("select", () => InputManager.Key(Key.Enter));

        protected void CheckNoSelection() => AddAssert("has no selection", () => Carousel.CurrentSelection, () => Is.Null);
        protected void CheckHasSelection() => AddAssert("has selection", () => Carousel.CurrentSelection, () => Is.Not.Null);

        protected void CheckDisplayedBeatmapsCount(int expected)
        {
            AddAssert($"{expected} diffs displayed", () => Carousel.MatchedBeatmapsCount, () => Is.EqualTo(expected));
        }

        protected void CheckDisplayedBeatmapSetsCount(int expected)
        {
            AddAssert($"{expected} sets displayed", () =>
            {
                var groupingFilter = Carousel.Filters.OfType<BeatmapCarouselFilterGrouping>().Single();

                // Using groupingFilter.SetItems.Count alone doesn't work.
                // When sorting by difficulty, there can be more than one set panel for the same set displayed.
                return groupingFilter.SetItems.Sum(s => s.Value.Count(i => i.Model is BeatmapSetInfo));
            }, () => Is.EqualTo(expected));
        }

        protected void CheckDisplayedGroupsCount(int expected)
        {
            AddAssert($"{expected} groups displayed", () =>
            {
                var groupingFilter = Carousel.Filters.OfType<BeatmapCarouselFilterGrouping>().Single();
                return groupingFilter.GroupItems.Count;
            }, () => Is.EqualTo(expected));
        }

        protected ICarouselPanel? GetSelectedPanel() => Carousel.ChildrenOfType<ICarouselPanel>().SingleOrDefault(p => p.Selected.Value);
        protected ICarouselPanel? GetKeyboardSelectedPanel() => Carousel.ChildrenOfType<ICarouselPanel>().SingleOrDefault(p => p.KeyboardSelected.Value);

        protected void WaitForGroupSelection(int group, int panel)
        {
            AddUntilStep($"selected is group{group} panel{panel}", () =>
            {
                var groupingFilter = Carousel.Filters.OfType<BeatmapCarouselFilterGrouping>().Single();

                GroupDefinition g = groupingFilter.GroupItems.Keys.ElementAt(group);
                // offset by one because the group itself is included in the items list.
                CarouselItem item = groupingFilter.GroupItems[g].ElementAt(panel + 1);

                return ReferenceEquals(Carousel.CurrentSelection, item.Model);
            });
        }

        protected void WaitForSelection(int set, int? diff = null)
        {
            AddUntilStep($"selected is set{set}{(diff.HasValue ? $" diff{diff.Value}" : "")}", () =>
            {
                if (diff != null)
                    return ReferenceEquals(Carousel.CurrentSelection, BeatmapSets[set].Beatmaps[diff.Value]);

                return BeatmapSets[set].Beatmaps.Contains(Carousel.CurrentSelection);
            });
        }

        protected IEnumerable<T> GetVisiblePanels<T>()
            where T : Drawable
        {
            return Carousel.ChildrenOfType<UserTrackingScrollContainer>().Single()
                           .ChildrenOfType<T>()
                           .Where(p => ((ICarouselPanel)p).Item?.IsVisible == true)
                           .OrderBy(p => p.Y);
        }

        protected void ClickVisiblePanel<T>(int index)
            where T : Drawable
        {
            AddStep($"click panel at index {index}", () =>
            {
                Carousel.ChildrenOfType<UserTrackingScrollContainer>().Single()
                        .ChildrenOfType<T>()
                        .Where(p => ((ICarouselPanel)p).Item?.IsVisible == true)
                        .OrderBy(p => p.Y)
                        .ElementAt(index)
                        .ChildrenOfType<Panel>().Single()
                        .TriggerClick();
            });
        }

        protected void ClickVisiblePanelWithOffset<T>(int index, Vector2 positionOffsetFromCentre)
            where T : Drawable
        {
            AddStep($"move mouse to panel {index} with offset {positionOffsetFromCentre}", () =>
            {
                var panel = Carousel.ChildrenOfType<UserTrackingScrollContainer>().Single()
                                    .ChildrenOfType<T>()
                                    .Where(p => ((ICarouselPanel)p).Item?.IsVisible == true)
                                    .OrderBy(p => p.Y)
                                    .ElementAt(index);

                InputManager.MoveMouseTo(panel.ScreenSpaceDrawQuad.Centre + panel.ToScreenSpace(positionOffsetFromCentre) - panel.ToScreenSpace(Vector2.Zero));
            });

            AddStep("click", () => InputManager.Click(MouseButton.Left));
        }

        /// <summary>
        /// Add requested beatmap sets count to list.
        /// </summary>
        /// <param name="count">The count of beatmap sets to add.</param>
        /// <param name="fixedDifficultiesPerSet">If not null, the number of difficulties per set. If null, randomised difficulty count will be used.</param>
        /// <param name="randomMetadata">Whether to randomise the metadata to make groupings more uniform.</param>
        protected void AddBeatmaps(int count, int? fixedDifficultiesPerSet = null, bool randomMetadata = false) => AddStep($"add {count} beatmaps{(randomMetadata ? " with random data" : "")}", () =>
        {
            for (int i = 0; i < count; i++)
                BeatmapSets.Add(CreateTestBeatmapSetInfo(fixedDifficultiesPerSet, randomMetadata));
        });

        protected static BeatmapSetInfo CreateTestBeatmapSetInfo(int? fixedDifficultiesPerSet, bool randomMetadata)
        {
            var beatmapSetInfo = TestResources.CreateTestBeatmapSetInfo(fixedDifficultiesPerSet ?? RNG.Next(1, 4));

            if (randomMetadata)
            {
                char randomCharacter = getRandomCharacter();

                var metadata = new BeatmapMetadata
                {
                    // Create random metadata, then we can check if sorting works based on these
                    Artist = $"{randomCharacter}ome Artist " + RNG.Next(0, 9),
                    Title = $"{randomCharacter}ome Song (set id {beatmapSetInfo.OnlineID:000}) {Guid.NewGuid()}",
                    Author = { Username = $"{randomCharacter}ome Guy " + RNG.Next(0, 9) },
                };

                foreach (var beatmap in beatmapSetInfo.Beatmaps)
                    beatmap.Metadata = metadata.DeepClone();
            }

            return beatmapSetInfo;
        }

        private static long randomCharPointer;

        private static char getRandomCharacter()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz*";
            return chars[(int)((randomCharPointer++ / 2) % chars.Length)];
        }

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

        public partial class TestBeatmapCarousel : BeatmapCarousel
        {
            public IEnumerable<BeatmapInfo> PostFilterBeatmaps = null!;

            protected override Task<IEnumerable<CarouselItem>> FilterAsync()
            {
                var filterAsync = base.FilterAsync();
                filterAsync.ContinueWith(result =>
                {
                    if (result.IsCompletedSuccessfully)
                        PostFilterBeatmaps = result.GetResultSafely().Select(i => i.Model).OfType<BeatmapInfo>();
                });
                return filterAsync;
            }
        }
    }
}
