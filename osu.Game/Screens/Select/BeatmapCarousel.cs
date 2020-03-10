// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Configuration;
using osuTK.Input;
using osu.Framework.Utils;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Threading;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Input.Bindings;
using osu.Game.Screens.Select.Carousel;

namespace osu.Game.Screens.Select
{
    public class BeatmapCarousel : CompositeDrawable, IKeyBindingHandler<GlobalAction>
    {
        private const float bleed_top = FilterControl.HEIGHT;
        private const float bleed_bottom = Footer.HEIGHT;

        /// <summary>
        /// Triggered when the <see cref="BeatmapSets"/> loaded change and are completely loaded.
        /// </summary>
        public Action BeatmapSetsChanged;

        /// <summary>
        /// The currently selected beatmap.
        /// </summary>
        public BeatmapInfo SelectedBeatmap => selectedBeatmap?.Beatmap;

        private CarouselBeatmap selectedBeatmap => selectedBeatmapSet?.Beatmaps.FirstOrDefault(s => s.State.Value == CarouselItemState.Selected);

        /// <summary>
        /// The currently selected beatmap set.
        /// </summary>
        public BeatmapSetInfo SelectedBeatmapSet => selectedBeatmapSet?.BeatmapSet;

        private CarouselBeatmapSet selectedBeatmapSet;

        /// <summary>
        /// Raised when the <see cref="SelectedBeatmap"/> is changed.
        /// </summary>
        public Action<BeatmapInfo> SelectionChanged;

        public override bool HandleNonPositionalInput => AllowSelection;
        public override bool HandlePositionalInput => AllowSelection;

        public override bool PropagatePositionalInputSubTree => AllowSelection;
        public override bool PropagateNonPositionalInputSubTree => AllowSelection;

        /// <summary>
        /// Whether carousel items have completed asynchronously loaded.
        /// </summary>
        public bool BeatmapSetsLoaded { get; private set; }

        private readonly OsuScrollContainer scroll;

        private IEnumerable<CarouselBeatmapSet> beatmapSets => root.Children.OfType<CarouselBeatmapSet>();

        // todo: only used for testing, maybe remove.
        public IEnumerable<BeatmapSetInfo> BeatmapSets
        {
            get => beatmapSets.Select(g => g.BeatmapSet);
            set => loadBeatmapSets(value);
        }

        private void loadBeatmapSets(IEnumerable<BeatmapSetInfo> beatmapSets)
        {
            CarouselRoot newRoot = new CarouselRoot(this);

            beatmapSets.Select(createCarouselSet).Where(g => g != null).ForEach(newRoot.AddChild);
            newRoot.Filter(activeCriteria);

            // preload drawables as the ctor overhead is quite high currently.
            _ = newRoot.Drawables;

            root = newRoot;
            if (selectedBeatmapSet != null && !beatmapSets.Contains(selectedBeatmapSet.BeatmapSet))
                selectedBeatmapSet = null;

            scrollableContent.Clear(false);
            itemsCache.Invalidate();
            scrollPositionCache.Invalidate();

            // Run on late scheduler want to ensure this runs after all pending UpdateBeatmapSet / RemoveBeatmapSet operations are run.
            SchedulerAfterChildren.Add(() =>
            {
                BeatmapSetsChanged?.Invoke();
                BeatmapSetsLoaded = true;
            });
        }

        private readonly List<float> yPositions = new List<float>();
        private readonly Cached itemsCache = new Cached();
        private readonly Cached scrollPositionCache = new Cached();

        private readonly Container<DrawableCarouselItem> scrollableContent;

        public Bindable<bool> RightClickScrollingEnabled = new Bindable<bool>();

        public Bindable<RandomSelectAlgorithm> RandomAlgorithm = new Bindable<RandomSelectAlgorithm>();
        private readonly List<CarouselBeatmapSet> previouslyVisitedRandomSets = new List<CarouselBeatmapSet>();
        private readonly Stack<CarouselBeatmap> randomSelectedBeatmaps = new Stack<CarouselBeatmap>();

        protected List<DrawableCarouselItem> Items = new List<DrawableCarouselItem>();
        private CarouselRoot root;

        public BeatmapCarousel()
        {
            root = new CarouselRoot(this);
            InternalChild = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = scroll = new CarouselScrollContainer
                {
                    Masking = false,
                    RelativeSizeAxes = Axes.Both,
                    Child = scrollableContent = new Container<DrawableCarouselItem>
                    {
                        RelativeSizeAxes = Axes.X,
                    }
                }
            };
        }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.RandomSelectAlgorithm, RandomAlgorithm);
            config.BindWith(OsuSetting.SongSelectRightMouseScroll, RightClickScrollingEnabled);

            RightClickScrollingEnabled.ValueChanged += enabled => scroll.RightMouseScrollbar = enabled.NewValue;
            RightClickScrollingEnabled.TriggerChange();

            beatmaps.ItemAdded += beatmapAdded;
            beatmaps.ItemRemoved += beatmapRemoved;
            beatmaps.BeatmapHidden += beatmapHidden;
            beatmaps.BeatmapRestored += beatmapRestored;

            loadBeatmapSets(beatmaps.GetAllUsableBeatmapSetsEnumerable());
        }

        public void RemoveBeatmapSet(BeatmapSetInfo beatmapSet) => Schedule(() =>
        {
            var existingSet = beatmapSets.FirstOrDefault(b => b.BeatmapSet.ID == beatmapSet.ID);

            if (existingSet == null)
                return;

            root.RemoveChild(existingSet);
            itemsCache.Invalidate();
        });

        public void UpdateBeatmapSet(BeatmapSetInfo beatmapSet) => Schedule(() =>
        {
            int? previouslySelectedID = null;
            CarouselBeatmapSet existingSet = beatmapSets.FirstOrDefault(b => b.BeatmapSet.ID == beatmapSet.ID);

            // If the selected beatmap is about to be removed, store its ID so it can be re-selected if required
            if (existingSet?.State?.Value == CarouselItemState.Selected)
                previouslySelectedID = selectedBeatmap?.Beatmap.ID;

            var newSet = createCarouselSet(beatmapSet);

            if (existingSet != null)
                root.RemoveChild(existingSet);

            if (newSet == null)
            {
                itemsCache.Invalidate();
                return;
            }

            root.AddChild(newSet);

            applyActiveCriteria(false);

            //check if we can/need to maintain our current selection.
            if (previouslySelectedID != null)
                select((CarouselItem)newSet.Beatmaps.FirstOrDefault(b => b.Beatmap.ID == previouslySelectedID) ?? newSet);

            itemsCache.Invalidate();
            Schedule(() => BeatmapSetsChanged?.Invoke());
        });

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
                if (!bypassFilters && set.Filtered.Value)
                    continue;

                var item = set.Beatmaps.FirstOrDefault(p => p.Beatmap.Equals(beatmap));

                if (item == null)
                    // The beatmap that needs to be selected doesn't exist in this set
                    continue;

                if (!bypassFilters && item.Filtered.Value)
                    // The beatmap exists in this set but is filtered, so look for the first unfiltered map in the set
                    item = set.Beatmaps.FirstOrDefault(b => !b.Filtered.Value);

                if (item != null)
                {
                    select(item);

                    // if we got here and the set is filtered, it means we were bypassing filters.
                    // in this case, reapplying the filter is necessary to ensure the panel is in the correct place
                    // (since it is forcefully being included in the carousel).
                    if (set.Filtered.Value)
                    {
                        Debug.Assert(bypassFilters);

                        applyActiveCriteria(false);
                    }

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
            var visibleItems = Items.Where(s => !s.Item.Filtered.Value).ToList();

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

                if (item.Filtered.Value || item.State.Value == CarouselItemState.Selected) continue;

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
                            select(direction > 0 ? set.Beatmaps.First(b => !b.Filtered.Value) : set.Beatmaps.Last(b => !b.Filtered.Value));
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
            var visibleSets = beatmapSets.Where(s => !s.Filtered.Value).ToList();
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

            if (RandomAlgorithm.Value == RandomSelectAlgorithm.RandomPermutation)
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

            var visibleBeatmaps = set.Beatmaps.Where(s => !s.Filtered.Value).ToList();
            select(visibleBeatmaps[RNG.Next(visibleBeatmaps.Count)]);
            return true;
        }

        public void SelectPreviousRandom()
        {
            while (randomSelectedBeatmaps.Any())
            {
                var beatmap = randomSelectedBeatmaps.Pop();

                if (!beatmap.Filtered.Value)
                {
                    if (RandomAlgorithm.Value == RandomSelectAlgorithm.RandomPermutation)
                        previouslyVisitedRandomSets.Remove(selectedBeatmapSet);
                    select(beatmap);
                    break;
                }
            }
        }

        private void select(CarouselItem item)
        {
            if (!AllowSelection)
                return;

            if (item == null) return;

            item.State.Value = CarouselItemState.Selected;
        }

        private FilterCriteria activeCriteria = new FilterCriteria();

        protected ScheduledDelegate PendingFilter;

        public bool AllowSelection = true;

        /// <summary>
        /// Half the height of the visible content.
        /// <remarks>
        /// This is different from the height of <see cref="ScrollContainer{T}"/>.displayableContent, since
        /// the beatmap carousel bleeds into the <see cref="FilterControl"/> and the <see cref="Footer"/>
        /// </remarks>
        /// </summary>
        private float visibleHalfHeight => (DrawHeight + bleed_bottom + bleed_top) / 2;

        /// <summary>
        /// The position of the lower visible bound with respect to the current scroll position.
        /// </summary>
        private float visibleBottomBound => scroll.Current + DrawHeight + bleed_bottom;

        /// <summary>
        /// The position of the upper visible bound with respect to the current scroll position.
        /// </summary>
        private float visibleUpperBound => scroll.Current - bleed_top;

        public void FlushPendingFilterOperations()
        {
            if (PendingFilter?.Completed == false)
            {
                applyActiveCriteria(false);
                Update();
            }
        }

        public void Filter(FilterCriteria newCriteria, bool debounce = true)
        {
            if (newCriteria != null)
                activeCriteria = newCriteria;

            applyActiveCriteria(debounce);
        }

        private void applyActiveCriteria(bool debounce)
        {
            if (root.Children.Any() != true) return;

            void perform()
            {
                PendingFilter = null;

                root.Filter(activeCriteria);
                itemsCache.Invalidate();
                scrollPositionCache.Invalidate();
            }

            PendingFilter?.Cancel();
            PendingFilter = null;

            if (debounce)
                PendingFilter = Scheduler.AddDelayed(perform, 250);
            else
                perform();
        }

        private float? scrollTarget;

        public void ScrollToSelected() => scrollPositionCache.Invalidate();

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    SelectNext(-1, true);
                    return true;

                case Key.Right:
                    SelectNext(1, true);
                    return true;
            }

            return false;
        }

        public bool OnPressed(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.SelectNext:
                    SelectNext(1, false);
                    return true;

                case GlobalAction.SelectPrevious:
                    SelectNext(-1, false);
                    return true;
            }

            return false;
        }

        public void OnReleased(GlobalAction action)
        {
        }

        protected override void Update()
        {
            base.Update();

            if (!itemsCache.IsValid)
                updateItems();

            // Remove all items that should no longer be on-screen
            scrollableContent.RemoveAll(p => p.Y < visibleUpperBound - p.DrawHeight || p.Y > visibleBottomBound || !p.IsPresent);

            // Find index range of all items that should be on-screen
            Trace.Assert(Items.Count == yPositions.Count);

            int firstIndex = yPositions.BinarySearch(visibleUpperBound - DrawableCarouselItem.MAX_HEIGHT);
            if (firstIndex < 0) firstIndex = ~firstIndex;
            int lastIndex = yPositions.BinarySearch(visibleBottomBound);
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
            foreach (DrawableCarouselItem p in scrollableContent.Children)
                updateItem(p);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (!scrollPositionCache.IsValid)
                updateScrollPosition();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (beatmaps != null)
            {
                beatmaps.ItemAdded -= beatmapAdded;
                beatmaps.ItemRemoved -= beatmapRemoved;
                beatmaps.BeatmapHidden -= beatmapHidden;
                beatmaps.BeatmapRestored -= beatmapRestored;
            }

            // aggressively dispose "off-screen" items to reduce GC pressure.
            foreach (var i in Items)
                i.Dispose();
        }

        private void beatmapRemoved(BeatmapSetInfo item) => RemoveBeatmapSet(item);

        private void beatmapAdded(BeatmapSetInfo item) => UpdateBeatmapSet(item);

        private void beatmapRestored(BeatmapInfo b) => UpdateBeatmapSet(beatmaps.QueryBeatmapSet(s => s.ID == b.BeatmapSetInfoID));

        private void beatmapHidden(BeatmapInfo b) => UpdateBeatmapSet(beatmaps.QueryBeatmapSet(s => s.ID == b.BeatmapSetInfoID));

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
                c.State.ValueChanged += state =>
                {
                    if (state.NewValue == CarouselItemState.Selected)
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

            float currentY = visibleHalfHeight;
            DrawableCarouselBeatmapSet lastSet = null;

            scrollTarget = null;

            foreach (DrawableCarouselItem d in Items)
            {
                if (d.IsPresent)
                {
                    switch (d)
                    {
                        case DrawableCarouselBeatmapSet set:
                        {
                            lastSet = set;

                            set.MoveToX(set.Item.State.Value == CarouselItemState.Selected ? -100 : 0, 500, Easing.OutExpo);
                            set.MoveToY(currentY, 750, Easing.OutExpo);
                            break;
                        }

                        case DrawableCarouselBeatmap beatmap:
                        {
                            if (beatmap.Item.State.Value == CarouselItemState.Selected)
                                scrollTarget = currentY + beatmap.DrawHeight / 2 - DrawHeight / 2;

                            void performMove(float y, float? startY = null)
                            {
                                if (startY != null) beatmap.MoveTo(new Vector2(0, startY.Value));
                                beatmap.MoveToX(beatmap.Item.State.Value == CarouselItemState.Selected ? -50 : 0, 500, Easing.OutExpo);
                                beatmap.MoveToY(y, 750, Easing.OutExpo);
                            }

                            Debug.Assert(lastSet != null);

                            float? setY = null;
                            if (!d.IsLoaded || beatmap.Alpha == 0) // can't use IsPresent due to DrawableCarouselItem override.
                                setY = lastSet.Y + lastSet.DrawHeight + 5;

                            if (d.IsLoaded)
                                performMove(currentY, setY);
                            else
                            {
                                float y = currentY;
                                d.OnLoadComplete += _ => performMove(y, setY);
                            }

                            break;
                        }
                    }
                }

                yPositions.Add(currentY);

                if (d.Item.Visible)
                    currentY += d.DrawHeight + 5;
            }

            currentY += visibleHalfHeight;
            scrollableContent.Height = currentY;

            if (BeatmapSetsLoaded && (selectedBeatmapSet == null || selectedBeatmap == null || selectedBeatmapSet.State.Value != CarouselItemState.Selected))
            {
                selectedBeatmapSet = null;
                SelectionChanged?.Invoke(null);
            }

            itemsCache.Validate();
        }

        private bool firstScroll = true;

        private void updateScrollPosition()
        {
            if (scrollTarget != null)
            {
                if (firstScroll)
                {
                    // reduce movement when first displaying the carousel.
                    scroll.ScrollTo(scrollTarget.Value - 200, false);
                    firstScroll = false;
                }

                scroll.ScrollTo(scrollTarget.Value);
                scrollPositionCache.Validate();
            }
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
            float discriminant = MathF.Max(0, circle_radius * circle_radius - dist * dist);
            float x = (circle_radius - MathF.Sqrt(discriminant)) * halfHeight;

            return 125 + x;
        }

        /// <summary>
        /// Update a item's x position and multiplicative alpha based on its y position and
        /// the current scroll position.
        /// </summary>
        /// <param name="p">The item to be updated.</param>
        private void updateItem(DrawableCarouselItem p)
        {
            float itemDrawY = p.Position.Y - visibleUpperBound + p.DrawHeight / 2;
            float dist = Math.Abs(1f - itemDrawY / visibleHalfHeight);

            // Setting the origin position serves as an additive position on top of potential
            // local transformation we may want to apply (e.g. when a item gets selected, we
            // may want to smoothly transform it leftwards.)
            p.OriginPosition = new Vector2(-offsetX(dist, visibleHalfHeight), 0);

            // We are applying a multiplicative alpha (which is internally done by nesting an
            // additional container and setting that container's alpha) such that we can
            // layer transformations on top, with a similar reasoning to the previous comment.
            p.SetMultiplicativeAlpha(Math.Clamp(1.75f - 1.5f * dist, 0, 1));
        }

        private class CarouselRoot : CarouselGroupEagerSelect
        {
            private readonly BeatmapCarousel carousel;

            public CarouselRoot(BeatmapCarousel carousel)
            {
                this.carousel = carousel;
            }

            protected override void PerformSelection()
            {
                if (LastSelected == null)
                    carousel.SelectNextRandom();
                else
                    base.PerformSelection();
            }
        }

        private class CarouselScrollContainer : OsuScrollContainer
        {
            private bool rightMouseScrollBlocked;

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                if (e.Button == MouseButton.Right)
                {
                    // we need to block right click absolute scrolling when hovering a carousel item so context menus can display.
                    // this can be reconsidered when we have an alternative to right click scrolling.
                    if (GetContainingInputManager().HoveredDrawables.OfType<DrawableCarouselItem>().Any())
                    {
                        rightMouseScrollBlocked = true;
                        return false;
                    }
                }

                rightMouseScrollBlocked = false;
                return base.OnMouseDown(e);
            }

            protected override bool OnDragStart(DragStartEvent e)
            {
                if (rightMouseScrollBlocked)
                    return false;

                return base.OnDragStart(e);
            }
        }
    }
}
