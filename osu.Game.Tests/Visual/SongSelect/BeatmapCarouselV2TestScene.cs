// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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
using osu.Game.Overlays;
using osu.Game.Screens.Select;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Resources;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;
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

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

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
        }

        protected void SortBy(FilterCriteria criteria) => AddStep($"sort:{criteria.Sort} group:{criteria.Group}", () => Carousel.Filter(criteria));

        protected void WaitForDrawablePanels() => AddUntilStep("drawable panels loaded", () => Carousel.ChildrenOfType<ICarouselPanel>().Count(), () => Is.GreaterThan(0));
        protected void WaitForSorting() => AddUntilStep("sorting finished", () => Carousel.IsFiltering, () => Is.False);
        protected void WaitForScrolling() => AddUntilStep("scroll finished", () => Scroll.Current, () => Is.EqualTo(Scroll.Target));

        protected void SelectNextPanel() => AddStep("select next panel", () => InputManager.Key(Key.Down));
        protected void SelectPrevPanel() => AddStep("select prev panel", () => InputManager.Key(Key.Up));
        protected void SelectNextGroup() => AddStep("select next group", () => InputManager.Key(Key.Right));
        protected void SelectPrevGroup() => AddStep("select prev group", () => InputManager.Key(Key.Left));

        protected void Select() => AddStep("select", () => InputManager.Key(Key.Enter));

        protected void CheckNoSelection() => AddAssert("has no selection", () => Carousel.CurrentSelection, () => Is.Null);
        protected void CheckHasSelection() => AddAssert("has selection", () => Carousel.CurrentSelection, () => Is.Not.Null);

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
                        .ChildrenOfType<PanelBase>().Single()
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
    }
}
