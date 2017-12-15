// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
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

        private CarouselBeatmapSet selectedBeatmapSet => beatmapSets.FirstOrDefault(s => s.State == CarouselItemState.Selected);

        /// <summary>
        /// Raised when the <see cref="SelectedBeatmap"/> is changed.
        /// </summary>
        public Action<BeatmapInfo> SelectionChanged;

        public override bool HandleInput => AllowSelection;

        private IEnumerable<CarouselBeatmapSet> beatmapSets => root.Children?.OfType<CarouselBeatmapSet>() ?? new CarouselBeatmapSet[] { };

        public IEnumerable<BeatmapSetInfo> BeatmapSets
        {
            get { return beatmapSets.Select(g => g.BeatmapSet); }
            set
            {
                List<CarouselBeatmapSet> newSets = null;

                CarouselGroup newRoot = new CarouselGroupEagerSelect();

                Task.Run(() =>
                {
                    value.Select(createCarouselSet).Where(g => g != null).ForEach(newRoot.AddChild);
                    newRoot.Filter(criteria);
                }).ContinueWith(t =>
                {
                    Schedule(() =>
                    {
                        root = newRoot;
                        updateItems();
                    });
                });
            }
        }

        /// <summary>
        /// Call after altering <see cref="BeatmapSets"/> in any way.
        /// </summary>
        private void updateItems()
        {
            scrollableContent.Clear(false);

            Items = root.Drawables.ToList();

            yPositionsCache.Invalidate();
            BeatmapSetsChanged?.Invoke();
        }

        private readonly List<float> yPositions = new List<float>();
        private Cached yPositionsCache = new Cached();

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

        public void RemoveBeatmapSet(BeatmapSetInfo beatmapSet)
        {
            var existingSet = beatmapSets.FirstOrDefault(b => b.BeatmapSet.ID == beatmapSet.ID);

            if (existingSet == null)
                return;

            root.RemoveChild(existingSet);
            updateItems();

            if (existingSet.State == CarouselItemState.Selected)
                SelectNext();
        }

        public void UpdateBeatmapSet(BeatmapSetInfo beatmapSet)
        {
            CarouselBeatmapSet existingSet = beatmapSets.FirstOrDefault(b => b.BeatmapSet.ID == beatmapSet.ID);

            bool hadSelection = existingSet?.State?.Value == CarouselItemState.Selected;

            var newSet = createCarouselSet(beatmapSet);

            if (existingSet != null)
                root.RemoveChild(existingSet);

            if (newSet == null)
            {
                updateItems();
                SelectNext();
                return;
            }

            root.AddChild(newSet);

            Filter(debounce: false);

            //check if we can/need to maintain our current selection.
            if (hadSelection)
                select((CarouselItem)newSet.Beatmaps.FirstOrDefault(b => b.Beatmap.ID == selectedBeatmap?.Beatmap.ID) ?? newSet);

            updateItems();
        }

        public void SelectBeatmap(BeatmapInfo beatmap, bool animated = true)
        {
            if (beatmap == null || beatmap.Hidden)
            {
                SelectNext();
                return;
            }

            if (beatmap == SelectedBeatmap) return;

            foreach (CarouselBeatmapSet group in beatmapSets)
            {
                var item = group.Beatmaps.FirstOrDefault(p => p.Beatmap.Equals(beatmap));
                if (item != null)
                {
                    select(item);
                    return;
                }
            }
        }

        /// <summary>
        /// Increment selection in the carousel in a chosen direction.
        /// </summary>
        /// <param name="direction">The direction to increment. Negative is backwards.</param>
        /// <param name="skipDifficulties">Whether to skip individual difficulties and only increment over full groups.</param>
        public void SelectNext(int direction = 1, bool skipDifficulties = true)
        {
            // todo: we may want to refactor and remove this as an optimisation in the future.
            if (beatmapSets.All(g => g.Filtered))
            {
                SelectionChanged?.Invoke(null);
                return;
            }

            int originalIndex = Items.IndexOf(selectedBeatmap?.Drawables.First());
            int currentIndex = originalIndex;

            // local function to increment the index in the required direction, wrapping over extremities.
            int incrementIndex() => currentIndex = (currentIndex + direction + Items.Count) % Items.Count;

            while (incrementIndex() != originalIndex)
            {
                var item = Items[currentIndex].Item;

                if (item.Filtered || item.State == CarouselItemState.Selected) continue;

                switch (item)
                {
                    case CarouselBeatmap beatmap:
                        if (skipDifficulties) continue;
                        select(beatmap);
                        return;
                    case CarouselBeatmapSet set:
                        select(set);
                        return;
                }
            }
        }

        private IEnumerable<CarouselBeatmapSet> getVisibleSets() => beatmapSets.Where(select => select.Visible);

        public void SelectNextRandom()
        {
            if (!beatmapSets.Any())
                return;

            var visible = getVisibleSets().ToList();
            if (!visible.Any())
                return;

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
                var notYetVisitedSets = visible.Except(previouslyVisitedRandomSets).ToList();
                if (!notYetVisitedSets.Any())
                {
                    previouslyVisitedRandomSets.Clear();
                    notYetVisitedSets = visible;
                }

                set = notYetVisitedSets.ElementAt(RNG.Next(notYetVisitedSets.Count));
                previouslyVisitedRandomSets.Add(set);
            }
            else
                set = visible.ElementAt(RNG.Next(visible.Count));

            select(set.Beatmaps.Skip(RNG.Next(set.Beatmaps.Count())).FirstOrDefault());
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

        private FilterCriteria criteria = new FilterCriteria();

        protected ScheduledDelegate FilterTask;

        public bool AllowSelection = true;

        public void FlushPendingFilters()
        {
            if (FilterTask?.Completed == false)
                Filter(debounce: false);
        }

        public void Filter(FilterCriteria newCriteria = null, bool debounce = true)
        {
            if (newCriteria != null)
                criteria = newCriteria;

            Action perform = delegate
            {
                FilterTask = null;

                var lastSet = selectedBeatmapSet;
                var lastBeatmap = selectedBeatmap;

                root.Filter(criteria);
                updateItems();

                if (selectedBeatmap != null && !selectedBeatmap.Filtered)
                    select(selectedBeatmap);
                else if (lastBeatmap != null && !lastSet.Filtered)
                {
                    var searchable = lastSet.Beatmaps.AsEnumerable();

                    // search forwards first then backwards if nothing found.
                    var nextAvailable =
                        searchable.SkipWhile(b => b != lastBeatmap).FirstOrDefault(b => !b.Filtered) ??
                        searchable.Reverse().SkipWhile(b => b != lastBeatmap).FirstOrDefault(b => !b.Filtered);

                    select(nextAvailable);
                }
                else
                    SelectNext();

                ScrollToSelected(false);
            };

            FilterTask?.Cancel();
            FilterTask = null;

            if (debounce)
                FilterTask = Scheduler.AddDelayed(perform, 250);
            else
                perform();
        }

        public void ScrollToSelected(bool animated = true)
        {
            float selectedY = computeYPositions(animated);
            ScrollTo(selectedY, animated);
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
                        SelectionChanged?.Invoke(c.Beatmap);
                        yPositionsCache.Invalidate();
                        Schedule(() => ScrollToSelected());
                    }
                };
            }

            return set;
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.RandomSelectAlgorithm, RandomAlgorithm);
        }

        /// <summary>
        /// Computes the target Y positions for every item in the carousel.
        /// </summary>
        /// <returns>The Y position of the currently selected item.</returns>
        private float computeYPositions(bool animated = true)
        {
            yPositions.Clear();

            float currentY = DrawHeight / 2;
            float selectedY = currentY;

            float lastSetY = 0;

            var selected = selectedBeatmap;

            foreach (DrawableCarouselItem d in Items)
            {
                switch (d)
                {
                    case DrawableCarouselBeatmapSet set:
                        set.MoveToX(set.Item.State == CarouselItemState.Selected ? -100 : 0, 500, Easing.OutExpo);
                        lastSetY = set.Position.Y;
                        break;
                    case DrawableCarouselBeatmap beatmap:
                        beatmap.MoveToX(beatmap.Item.State == CarouselItemState.Selected ? -50 : 0, 500, Easing.OutExpo);

                        if (beatmap.Item == selected)
                            selectedY = currentY + beatmap.DrawHeight / 2 - DrawHeight / 2;

                        // on first display we want to begin hidden under our group's header.
                        if (animated && !beatmap.IsPresent)
                            beatmap.MoveToY(lastSetY);
                        break;
                }

                yPositions.Add(currentY);
                d.MoveToY(currentY, animated ? 750 : 0, Easing.OutExpo);

                if (d.Item.Visible)
                    currentY += d.DrawHeight + 5;
            }

            currentY += DrawHeight / 2;
            scrollableContent.Height = currentY;

            yPositionsCache.Validate();

            return selectedY;
        }

        private void select(CarouselItem item)
        {
            if (item == null) return;
            item.State.Value = CarouselItemState.Selected;
        }

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

            float drawHeight = DrawHeight;

            if (!yPositionsCache.IsValid)
                computeYPositions();

            // Remove all items that should no longer be on-screen
            scrollableContent.RemoveAll(delegate(DrawableCarouselItem p)
            {
                float itemPosY = p.Position.Y;
                bool remove = itemPosY < Current - p.DrawHeight || itemPosY > Current + drawHeight || !p.IsPresent;
                return remove;
            });

            // Find index range of all items that should be on-screen
            Trace.Assert(Items.Count == yPositions.Count);

            int firstIndex = yPositions.BinarySearch(Current - DrawableCarouselItem.MAX_HEIGHT);
            if (firstIndex < 0) firstIndex = ~firstIndex;
            int lastIndex = yPositions.BinarySearch(Current + drawHeight);
            if (lastIndex < 0)
            {
                lastIndex = ~lastIndex;

                // Add the first item of the last visible beatmap group to preload its data.
                if (lastIndex != 0 && Items[lastIndex - 1] is DrawableCarouselBeatmapSet)
                    lastIndex++;
            }

            // Add those items within the previously found index range that should be displayed.
            for (int i = firstIndex; i < lastIndex; ++i)
            {
                DrawableCarouselItem item = Items[i];

                // Only add if we're not already part of the content.
                if (!scrollableContent.Contains(item))
                {
                    if (!item.Item.Visible) continue;

                    // Makes sure headers are always _below_ items,
                    // and depth flows downward.
                    item.Depth = i + (item is DrawableCarouselBeatmapSet ? Items.Count : 0);

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
            }

            // Update externally controlled state of currently visible items
            // (e.g. x-offset and opacity).
            float halfHeight = drawHeight / 2;
            foreach (DrawableCarouselItem p in scrollableContent.Children)
                updateItem(p, halfHeight);
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
