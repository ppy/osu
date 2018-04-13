﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Configuration;
using osu.Framework.Input;
using OpenTK.Input;
using osu.Framework.MathUtils;
using System.Diagnostics;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Caching;
using osu.Framework.Threading;
using osu.Framework.Configuration;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Screens.Select.Carousel;

namespace osu.Game.Screens.Select
{
    public class BeatmapCarousel : OsuScrollContainer
    {
        /// <summary>
        /// Triggered when the <see cref="BeatmapSets"/> loaded change and are completely loaded.
        /// </summary>
        public Action BeatmapSetsChanged;

        /// <summary>
        /// The currently selected beatmap.
        /// </summary>
        public BeatmapInfo SelectedBeatmap => selectedBeatmap?.Beatmap;

        private CarouselBeatmap selectedBeatmap => selectedBeatmapSet?.Beatmaps.FirstOrDefault(s => s.State == CarouselItemState.Selected);

        /// <summary>
        /// The currently selected beatmap set.
        /// </summary>
        public BeatmapSetInfo SelectedBeatmapSet => selectedBeatmapSet?.BeatmapSet;

        private CarouselBeatmapSet selectedBeatmapSet;

        /// <summary>
        /// Raised when the <see cref="SelectedBeatmap"/> is changed.
        /// </summary>
        public Action<BeatmapInfo> SelectionChanged;

        public override bool HandleKeyboardInput => AllowSelection;
        public override bool HandleMouseInput => AllowSelection;

        /// <summary>
        /// Used to avoid firing null selections before the initial beatmaps have been loaded via <see cref="BeatmapSets"/>.
        /// </summary>
        private bool initialLoadComplete;

        private IEnumerable<CarouselBeatmapSet> beatmapSets => root.Children.OfType<CarouselBeatmapSet>();

        public IEnumerable<BeatmapSetInfo> BeatmapSets
        {
            get { return beatmapSets.Select(g => g.BeatmapSet); }
            set
            {
                CarouselGroup newRoot = new CarouselGroupEagerSelect();

                Task.Run(() =>
                {
                    value.Select(createCarouselSet).Where(g => g != null).ForEach(newRoot.AddChild);
                    newRoot.Filter(activeCriteria);

                    // preload drawables as the ctor overhead is quite high currently.
                    var _ = newRoot.Drawables;
                }).ContinueWith(_ => Schedule(() =>
                {
                    root = newRoot;
                    scrollableContent.Clear(false);
                    itemsCache.Invalidate();
                    scrollPositionCache.Invalidate();

                    Schedule(() =>
                    {
                        BeatmapSetsChanged?.Invoke();
                        initialLoadComplete = true;
                    });
                }));
            }
        }

        private readonly List<float> yPositions = new List<float>();
        private Cached itemsCache = new Cached();
        private Cached scrollPositionCache = new Cached();

        private readonly Container<DrawableCarouselItem> scrollableContent;

        public Bindable<RandomSelectAlgorithm> RandomAlgorithm = new Bindable<RandomSelectAlgorithm>();
        private readonly List<CarouselBeatmapSet> previouslyVisitedRandomSets = new List<CarouselBeatmapSet>();
        private readonly Stack<CarouselBeatmap> randomSelectedBeatmaps = new Stack<CarouselBeatmap>();

        protected List<DrawableCarouselItem> Items = new List<DrawableCarouselItem>();
        private CarouselGroup root = new CarouselGroupEagerSelect();

        public BeatmapCarousel()
        {
            Child = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Child = scrollableContent = new Container<DrawableCarouselItem>
                {
                    RelativeSizeAxes = Axes.X,
                }
            };
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.RandomSelectAlgorithm, RandomAlgorithm);
        }

        public void RemoveBeatmapSet(BeatmapSetInfo beatmapSet)
        {
            Schedule(() =>
            {
                var existingSet = beatmapSets.FirstOrDefault(b => b.BeatmapSet.ID == beatmapSet.ID);

                if (existingSet == null)
                    return;

                root.RemoveChild(existingSet);
                itemsCache.Invalidate();
            });
        }

        public void UpdateBeatmapSet(BeatmapSetInfo beatmapSet)
        {
            Schedule(() =>
            {
                CarouselBeatmapSet existingSet = beatmapSets.FirstOrDefault(b => b.BeatmapSet.ID == beatmapSet.ID);

                bool hadSelection = existingSet?.State?.Value == CarouselItemState.Selected;

                var newSet = createCarouselSet(beatmapSet);

                if (existingSet != null)
                    root.RemoveChild(existingSet);

                if (newSet == null)
                {
                    itemsCache.Invalidate();
                    return;
                }

                root.AddChild(newSet);

                applyActiveCriteria(false, false);

                //check if we can/need to maintain our current selection.
                if (hadSelection)
                    select((CarouselItem)newSet.Beatmaps.FirstOrDefault(b => b.Beatmap.ID == selectedBeatmap?.Beatmap.ID) ?? newSet);

                itemsCache.Invalidate();
                Schedule(() => BeatmapSetsChanged?.Invoke());
            });
        }

        /// <summary>
        /// Selects a given beatmap on the carousel.
        ///
        /// If bypassFilters is false, we will try to select another unfiltered beatmap in the same set. If the
        /// entire set is filtered, no selection is made.
        /// </summary>
        /// <param name="beatmap">The beatmap to select.</param>
        /// <param name="bypassFilters">Whether to select the beatmap even if it is filtered (i.e., not visible on carousel).</param>
        /// <returns>True if a selection was made, False if it wasn't.</returns>
        public bool SelectBeatmap(BeatmapInfo beatmap, bool bypassFilters = true)
        {
            if (beatmap?.Hidden != false)
                return false;

            foreach (CarouselBeatmapSet set in beatmapSets)
            {
                if (!bypassFilters && set.Filtered)
                    continue;

                var item = set.Beatmaps.FirstOrDefault(p => p.Beatmap.Equals(beatmap));

                if (item == null)
                    // The beatmap that needs to be selected doesn't exist in this set
                    continue;

                if (!bypassFilters && item.Filtered)
                    // The beatmap exists in this set but is filtered, so look for the first unfiltered map in the set
                    item = set.Beatmaps.FirstOrDefault(b => !b.Filtered);

                if (item != null)
                {
                    select(item);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Increment selection in the carousel in a chosen direction.
        /// </summary>
        /// <param name="direction">The direction to increment. Negative is backwards.</param>
        /// <param name="skipDifficulties">Whether to skip individual difficulties and only increment over full groups.</param>
        public void SelectNext(int direction = 1, bool skipDifficulties = true)
        {
            var visibleItems = Items.Where(s => !s.Item.Filtered).ToList();

            if (!visibleItems.Any())
                return;

            DrawableCarouselItem drawable = null;

            if (selectedBeatmap != null && (drawable = selectedBeatmap.Drawables.FirstOrDefault()) == null)
                // if the selected beatmap isn't present yet, we can't correctly change selection.
                // we can fix this by changing this method to not reference drawables / Items in the first place.
                return;

            int originalIndex = visibleItems.IndexOf(drawable);
            int currentIndex = originalIndex;

            // local function to increment the index in the required direction, wrapping over extremities.
            int incrementIndex() => currentIndex = (currentIndex + direction + visibleItems.Count) % visibleItems.Count;

            while (incrementIndex() != originalIndex)
            {
                var item = visibleItems[currentIndex].Item;

                if (item.Filtered || item.State == CarouselItemState.Selected) continue;

                switch (item)
                {
                    case CarouselBeatmap beatmap:
                        if (skipDifficulties) continue;
                        select(beatmap);
                        return;
                    case CarouselBeatmapSet set:
                        if (skipDifficulties)
                            select(set);
                        else
                            select(direction > 0 ? set.Beatmaps.First(b => !b.Filtered) : set.Beatmaps.Last(b => !b.Filtered));
                        return;
                }
            }
        }

        /// <summary>
        /// Select the next beatmap in the random sequence.
        /// </summary>
        /// <returns>True if a selection could be made, else False.</returns>
        public bool SelectNextRandom()
        {
            var visibleSets = beatmapSets.Where(s => !s.Filtered).ToList();
            if (!visibleSets.Any())
                return false;

            if (selectedBeatmap != null)
            {
                randomSelectedBeatmaps.Push(selectedBeatmap);

                // when performing a random, we want to add the current set to the previously visited list
                // else the user may be "randomised" to the existing selection.
                if (previouslyVisitedRandomSets.LastOrDefault() != selectedBeatmapSet)
                    previouslyVisitedRandomSets.Add(selectedBeatmapSet);
            }

            CarouselBeatmapSet set;

            if (RandomAlgorithm == RandomSelectAlgorithm.RandomPermutation)
            {
                var notYetVisitedSets = visibleSets.Except(previouslyVisitedRandomSets).ToList();
                if (!notYetVisitedSets.Any())
                {
                    previouslyVisitedRandomSets.RemoveAll(s => visibleSets.Contains(s));
                    notYetVisitedSets = visibleSets;
                }

                set = notYetVisitedSets.ElementAt(RNG.Next(notYetVisitedSets.Count));
                previouslyVisitedRandomSets.Add(set);
            }
            else
                set = visibleSets.ElementAt(RNG.Next(visibleSets.Count));

            var visibleBeatmaps = set.Beatmaps.Where(s => !s.Filtered).ToList();
            select(visibleBeatmaps[RNG.Next(visibleBeatmaps.Count)]);
            return true;
        }

        public void SelectPreviousRandom()
        {
            while (randomSelectedBeatmaps.Any())
            {
                var beatmap = randomSelectedBeatmaps.Pop();

                if (!beatmap.Filtered)
                {
                    if (RandomAlgorithm == RandomSelectAlgorithm.RandomPermutation)
                        previouslyVisitedRandomSets.Remove(selectedBeatmapSet);
                    select(beatmap);
                    break;
                }
            }
        }

        private void select(CarouselItem item)
        {
            if (item == null) return;
            item.State.Value = CarouselItemState.Selected;
        }

        private FilterCriteria activeCriteria = new FilterCriteria();

        protected ScheduledDelegate FilterTask;

        public bool AllowSelection = true;

        public void FlushPendingFilterOperations()
        {
            if (FilterTask?.Completed == false)
            {
                applyActiveCriteria(false, false);
                Update();
            }
        }

        public void Filter(FilterCriteria newCriteria, bool debounce = true)
        {
            if (newCriteria != null)
                activeCriteria = newCriteria;

            applyActiveCriteria(debounce, true);
        }

        private void applyActiveCriteria(bool debounce, bool scroll)
        {
            if (root.Children.Any() != true) return;

            void perform()
            {
                FilterTask = null;

                root.Filter(activeCriteria);
                itemsCache.Invalidate();
                if (scroll) scrollPositionCache.Invalidate();
            }

            FilterTask?.Cancel();
            FilterTask = null;

            if (debounce)
                FilterTask = Scheduler.AddDelayed(perform, 250);
            else
                perform();
        }

        private float? scrollTarget;

        public void ScrollToSelected() => scrollPositionCache.Invalidate();

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            int direction = 0;
            bool skipDifficulties = false;

            switch (args.Key)
            {
                case Key.Up:
                    direction = -1;
                    break;
                case Key.Down:
                    direction = 1;
                    break;
                case Key.Left:
                    direction = -1;
                    skipDifficulties = true;
                    break;
                case Key.Right:
                    direction = 1;
                    skipDifficulties = true;
                    break;
            }

            if (direction == 0)
                return base.OnKeyDown(state, args);

            SelectNext(direction, skipDifficulties);
            return true;
        }

        protected override void Update()
        {
            base.Update();

            if (!itemsCache.IsValid)
                updateItems();

            if (!scrollPositionCache.IsValid)
                updateScrollPosition();

            float drawHeight = DrawHeight;

            // Remove all items that should no longer be on-screen
            scrollableContent.RemoveAll(p => p.Y < Current - p.DrawHeight || p.Y > Current + drawHeight || !p.IsPresent);

            // Find index range of all items that should be on-screen
            Trace.Assert(Items.Count == yPositions.Count);

            int firstIndex = yPositions.BinarySearch(Current - DrawableCarouselItem.MAX_HEIGHT);
            if (firstIndex < 0) firstIndex = ~firstIndex;
            int lastIndex = yPositions.BinarySearch(Current + drawHeight);
            if (lastIndex < 0) lastIndex = ~lastIndex;

            int notVisibleCount = 0;

            // Add those items within the previously found index range that should be displayed.
            for (int i = firstIndex; i < lastIndex; ++i)
            {
                DrawableCarouselItem item = Items[i];

                if (!item.Item.Visible)
                {
                    if (!item.IsPresent)
                        notVisibleCount++;
                    continue;
                }

                float depth = i + (item is DrawableCarouselBeatmapSet ? -Items.Count : 0);

                // Only add if we're not already part of the content.
                if (!scrollableContent.Contains(item))
                {
                    // Makes sure headers are always _below_ items,
                    // and depth flows downward.
                    item.Depth = depth;

                    switch (item.LoadState)
                    {
                        case LoadState.NotLoaded:
                            LoadComponentAsync(item);
                            break;
                        case LoadState.Loading:
                            break;
                        default:
                            scrollableContent.Add(item);
                            break;
                    }
                }
                else
                {
                    scrollableContent.ChangeChildDepth(item, depth);
                }
            }

            // this is not actually useful right now, but once we have groups may well be.
            if (notVisibleCount > 50)
                itemsCache.Invalidate();

            // Update externally controlled state of currently visible items
            // (e.g. x-offset and opacity).
            float halfHeight = drawHeight / 2;
            foreach (DrawableCarouselItem p in scrollableContent.Children)
                updateItem(p, halfHeight);
        }

        private CarouselBeatmapSet createCarouselSet(BeatmapSetInfo beatmapSet)
        {
            if (beatmapSet.Beatmaps.All(b => b.Hidden))
                return null;

            // todo: remove the need for this.
            foreach (var b in beatmapSet.Beatmaps)
            {
                if (b.Metadata == null)
                    b.Metadata = beatmapSet.Metadata;
            }

            var set = new CarouselBeatmapSet(beatmapSet);

            foreach (var c in set.Beatmaps)
            {
                c.State.ValueChanged += v =>
                {
                    if (v == CarouselItemState.Selected)
                    {
                        selectedBeatmapSet = set;
                        SelectionChanged?.Invoke(c.Beatmap);

                        itemsCache.Invalidate();
                        scrollPositionCache.Invalidate();
                    }
                };
            }

            return set;
        }

        /// <summary>
        /// Computes the target Y positions for every item in the carousel.
        /// </summary>
        /// <returns>The Y position of the currently selected item.</returns>
        private void updateItems()
        {
            Items = root.Drawables.ToList();

            yPositions.Clear();

            float currentY = DrawHeight / 2;
            DrawableCarouselBeatmapSet lastSet = null;

            scrollTarget = null;

            foreach (DrawableCarouselItem d in Items)
            {
                if (d.IsPresent)
                {
                    switch (d)
                    {
                        case DrawableCarouselBeatmapSet set:
                            lastSet = set;

                            set.MoveToX(set.Item.State == CarouselItemState.Selected ? -100 : 0, 500, Easing.OutExpo);
                            set.MoveToY(currentY, 750, Easing.OutExpo);
                            break;
                        case DrawableCarouselBeatmap beatmap:
                            if (beatmap.Item.State.Value == CarouselItemState.Selected)
                                scrollTarget = currentY + beatmap.DrawHeight / 2 - DrawHeight / 2;

                            void performMove(float y, float? startY = null)
                            {
                                if (startY != null) beatmap.MoveTo(new Vector2(0, startY.Value));
                                beatmap.MoveToX(beatmap.Item.State == CarouselItemState.Selected ? -50 : 0, 500, Easing.OutExpo);
                                beatmap.MoveToY(y, 750, Easing.OutExpo);
                            }

                            Debug.Assert(lastSet != null);

                            float? setY = null;
                            if (!d.IsLoaded || beatmap.Alpha == 0) // can't use IsPresent due to DrawableCarouselItem override.
                                // ReSharper disable once PossibleNullReferenceException (resharper broken?)
                                setY = lastSet.Y + lastSet.DrawHeight + 5;

                            if (d.IsLoaded)
                                performMove(currentY, setY);
                            else
                            {
                                float y = currentY;
                                d.OnLoadComplete = _ => performMove(y, setY);
                            }

                            break;
                    }
                }

                yPositions.Add(currentY);

                if (d.Item.Visible)
                    currentY += d.DrawHeight + 5;
            }

            currentY += DrawHeight / 2;
            scrollableContent.Height = currentY;

            if (initialLoadComplete && (selectedBeatmapSet == null || selectedBeatmap == null || selectedBeatmapSet.State.Value != CarouselItemState.Selected))
            {
                selectedBeatmapSet = null;
                SelectionChanged?.Invoke(null);
            }

            itemsCache.Validate();
        }

        private void updateScrollPosition()
        {
            if (scrollTarget != null) ScrollTo(scrollTarget.Value);
            scrollPositionCache.Validate();
        }

        /// <summary>
        /// Computes the x-offset of currently visible items. Makes the carousel appear round.
        /// </summary>
        /// <param name="dist">
        /// Vertical distance from the center of the carousel container
        /// ranging from -1 to 1.
        /// </param>
        /// <param name="halfHeight">Half the height of the carousel container.</param>
        private static float offsetX(float dist, float halfHeight)
        {
            // The radius of the circle the carousel moves on.
            const float circle_radius = 3;
            double discriminant = Math.Max(0, circle_radius * circle_radius - dist * dist);
            float x = (circle_radius - (float)Math.Sqrt(discriminant)) * halfHeight;

            return 125 + x;
        }

        /// <summary>
        /// Update a item's x position and multiplicative alpha based on its y position and
        /// the current scroll position.
        /// </summary>
        /// <param name="p">The item to be updated.</param>
        /// <param name="halfHeight">Half the draw height of the carousel container.</param>
        private void updateItem(DrawableCarouselItem p, float halfHeight)
        {
            var height = p.IsPresent ? p.DrawHeight : 0;

            float itemDrawY = p.Position.Y - Current + height / 2;
            float dist = Math.Abs(1f - itemDrawY / halfHeight);

            // Setting the origin position serves as an additive position on top of potential
            // local transformation we may want to apply (e.g. when a item gets selected, we
            // may want to smoothly transform it leftwards.)
            p.OriginPosition = new Vector2(-offsetX(dist, halfHeight), 0);

            // We are applying a multiplicative alpha (which is internally done by nesting an
            // additional container and setting that container's alpha) such that we can
            // layer transformations on top, with a similar reasoning to the previous comment.
            p.SetMultiplicativeAlpha(MathHelper.Clamp(1.75f - 1.5f * dist, 0, 1));
        }
    }
}
