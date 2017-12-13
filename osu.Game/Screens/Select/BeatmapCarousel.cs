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
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Screens.Select.Carousel;

namespace osu.Game.Screens.Select
{
    public class BeatmapCarousel : OsuScrollContainer
    {
        /// <summary>
        /// Triggered when the <see cref="Beatmaps"/> loaded change and are completely loaded.
        /// </summary>
        public Action BeatmapsChanged;

        /// <summary>
        /// The currently selected beatmap.
        /// </summary>
        public BeatmapInfo SelectedBeatmap => selectedBeatmap?.Beatmap;

        /// <summary>
        /// Raised when the <see cref="SelectedBeatmap"/> is changed.
        /// </summary>
        public Action<BeatmapInfo> SelectionChanged;

        public override bool HandleInput => AllowSelection;

        public IEnumerable<BeatmapSetInfo> Beatmaps
        {
            get { return carouselSets.Select(g => g.BeatmapSet); }
            set
            {
                Schedule(() =>
                {
                    scrollableContent.Clear(false);
                    items.Clear();
                    carouselSets.Clear();
                    yPositionsCache.Invalidate();
                });

                List<CarouselBeatmapSet> newSets = null;

                Task.Run(() =>
                {
                    newSets = value.Select(createGroup).Where(g => g != null).ToList();
                    newSets.ForEach(g => g.Filter(criteria));
                }).ContinueWith(t =>
                {
                    Schedule(() =>
                    {
                        carouselSets.AddRange(newSets);

                        root = new CarouselGroup(newSets.OfType<CarouselItem>().ToList());
                        items = root.Drawables.Value.ToList();

                        yPositionsCache.Invalidate();
                        BeatmapsChanged?.Invoke();
                    });
                });
            }
        }

        private readonly List<float> yPositions = new List<float>();
        private Cached yPositionsCache = new Cached();

        private readonly Container<DrawableCarouselItem> scrollableContent;

        private readonly List<CarouselBeatmapSet> carouselSets = new List<CarouselBeatmapSet>();

        private Bindable<RandomSelectAlgorithm> randomSelectAlgorithm;
        private readonly List<CarouselBeatmapSet> seenSets = new List<CarouselBeatmapSet>();

        private List<DrawableCarouselItem> items = new List<DrawableCarouselItem>();
        private CarouselGroup root = new CarouselGroup();

        private readonly Stack<CarouselBeatmap> randomSelectedBeatmaps = new Stack<CarouselBeatmap>();

        private CarouselBeatmap selectedBeatmap;

        public BeatmapCarousel()
        {
            Add(new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Child = scrollableContent = new Container<DrawableCarouselItem>
                {
                    RelativeSizeAxes = Axes.X,
                }
            });
        }

        public void RemoveBeatmapSet(BeatmapSetInfo beatmapSet)
        {
            Schedule(() => removeBeatmapSet(carouselSets.Find(b => b.BeatmapSet.ID == beatmapSet.ID)));
        }

        public void UpdateBeatmapSet(BeatmapSetInfo beatmapSet)
        {
            // todo: this method should be smarter as to not recreate items that haven't changed, etc.
            var oldGroup = carouselSets.Find(b => b.BeatmapSet.ID == beatmapSet.ID);

            bool hadSelection = oldGroup?.State == CarouselItemState.Selected;

            var newSet = createGroup(beatmapSet);

            int index = carouselSets.IndexOf(oldGroup);
            if (index >= 0)
                carouselSets.RemoveAt(index);

            if (newSet != null)
            {
                if (index >= 0)
                    carouselSets.Insert(index, newSet);
                //else
                //    addBeatmapSet(newSet);
            }

            if (hadSelection && newSet == null)
                SelectNext();

            Filter(null, false);

            //check if we can/need to maintain our current selection.
            if (hadSelection && newSet != null)
            {
                var newSelection = newSet.Beatmaps.Find(b => b.Beatmap.ID == selectedBeatmap?.Beatmap.ID);

                if (newSelection == null && selectedBeatmap != null)
                    newSelection = newSet.Beatmaps[Math.Min(newSet.Beatmaps.Count - 1, oldGroup.Beatmaps.IndexOf(selectedBeatmap))];

                select(newSelection);
            }
        }

        public void SelectBeatmap(BeatmapInfo beatmap, bool animated = true)
        {
            if (beatmap == null || beatmap.Hidden)
            {
                SelectNext();
                return;
            }

            if (beatmap == SelectedBeatmap) return;

            foreach (CarouselBeatmapSet group in carouselSets)
            {
                var item = group.Beatmaps.FirstOrDefault(p => p.Beatmap.Equals(beatmap));
                if (item != null)
                {
                    select(item);
                    return;
                }
            }
        }

        private void selectNullBeatmap()
        {
            selectedBeatmap = null;
            SelectionChanged?.Invoke(null);
        }

        /// <summary>
        /// Increment selection in the carousel in a chosen direction.
        /// </summary>
        /// <param name="direction">The direction to increment. Negative is backwards.</param>
        /// <param name="skipDifficulties">Whether to skip individual difficulties and only increment over full groups.</param>
        public void SelectNext(int direction = 1, bool skipDifficulties = true)
        {
            // todo: we may want to refactor and remove this as an optimisation in the future.
            if (carouselSets.All(g => g.State == CarouselItemState.Hidden))
            {
                selectNullBeatmap();
                return;
            }

            int originalIndex = items.IndexOf(selectedBeatmap?.Drawables.Value.First());
            int currentIndex = originalIndex;

            // local function to increment the index in the required direction, wrapping over extremities.
            int incrementIndex() => currentIndex = (currentIndex + direction + items.Count) % items.Count;

            while (incrementIndex() != originalIndex)
            {
                var item = items[currentIndex].Item;

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

        private IEnumerable<CarouselBeatmapSet> getVisibleGroups() => carouselSets.Where(select => select.State != CarouselItemState.NotSelected);

        public void SelectNextRandom()
        {
            if (carouselSets.Count == 0)
                return;

            var visibleGroups = getVisibleGroups();
            if (!visibleGroups.Any())
                return;

            if (selectedBeatmap != null)
                randomSelectedBeatmaps.Push(selectedBeatmap);

            CarouselBeatmapSet group;

            if (randomSelectAlgorithm == RandomSelectAlgorithm.RandomPermutation)
            {
                var notSeenGroups = visibleGroups.Except(seenSets);
                if (!notSeenGroups.Any())
                {
                    seenSets.Clear();
                    notSeenGroups = visibleGroups;
                }

                group = notSeenGroups.ElementAt(RNG.Next(notSeenGroups.Count()));
                seenSets.Add(group);
            }
            else
                group = visibleGroups.ElementAt(RNG.Next(visibleGroups.Count()));

            CarouselBeatmap item = group.Beatmaps[RNG.Next(group.Beatmaps.Count)];

            select(item);
        }

        public void SelectPreviousRandom()
        {
            if (!randomSelectedBeatmaps.Any())
                return;

            while (randomSelectedBeatmaps.Any())
            {
                var beatmap = randomSelectedBeatmaps.Pop();

                if (beatmap.Visible)
                {
                    select(beatmap);
                    break;
                }
            }
        }

        private FilterCriteria criteria = new FilterCriteria();

        private ScheduledDelegate filterTask;

        public bool AllowSelection = true;

        public void FlushPendingFilters()
        {
            if (filterTask?.Completed == false)
                Filter(null, false);
        }

        public void Filter(FilterCriteria newCriteria = null, bool debounce = true)
        {
            if (newCriteria != null)
                criteria = newCriteria;

            Action perform = delegate
            {
                filterTask = null;

                carouselSets.ForEach(s => s.Filter(criteria));

                yPositionsCache.Invalidate();

                if (selectedBeatmap?.Visible != true)
                    SelectNext();
                else
                    select(selectedBeatmap);
            };

            filterTask?.Cancel();
            filterTask = null;

            if (debounce)
                filterTask = Scheduler.AddDelayed(perform, 250);
            else
                perform();
        }

        public void ScrollToSelected(bool animated = true)
        {
            float selectedY = computeYPositions(animated);
            ScrollTo(selectedY, animated);
        }

        private CarouselBeatmapSet createGroup(BeatmapSetInfo beatmapSet)
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
                        selectedBeatmap = c;
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
            randomSelectAlgorithm = config.GetBindable<RandomSelectAlgorithm>(OsuSetting.RandomSelectAlgorithm);
        }

        private void removeBeatmapSet(CarouselBeatmapSet set)
        {
            if (set == null)
                return;

            carouselSets.Remove(set);

            foreach (var d in set.Drawables.Value)
            {
                items.Remove(d);
                scrollableContent.Remove(d);
            }

            if (set.State == CarouselItemState.Selected)
                SelectNext();

            yPositionsCache.Invalidate();
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

            foreach (DrawableCarouselItem d in items)
            {
                switch (d)
                {
                    case DrawableCarouselBeatmapSet set:
                        set.MoveToX(set.Item.State == CarouselItemState.Selected ? -100 : 0, 500, Easing.OutExpo);
                        lastSetY = set.Position.Y;
                        break;
                    case DrawableCarouselBeatmap beatmap:
                        beatmap.MoveToX(beatmap.Item.State == CarouselItemState.Selected ? -50 : 0, 500, Easing.OutExpo);

                        if (beatmap.Item == selectedBeatmap)
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
            Trace.Assert(items.Count == yPositions.Count);

            int firstIndex = yPositions.BinarySearch(Current - DrawableCarouselItem.MAX_HEIGHT);
            if (firstIndex < 0) firstIndex = ~firstIndex;
            int lastIndex = yPositions.BinarySearch(Current + drawHeight);
            if (lastIndex < 0)
            {
                lastIndex = ~lastIndex;

                // Add the first item of the last visible beatmap group to preload its data.
                if (lastIndex != 0 && items[lastIndex - 1] is DrawableCarouselBeatmapSet)
                    lastIndex++;
            }

            // Add those items within the previously found index range that should be displayed.
            for (int i = firstIndex; i < lastIndex; ++i)
            {
                DrawableCarouselItem item = items[i];

                // Only add if we're not already part of the content.
                if (!scrollableContent.Contains(item))
                {
                    // Makes sure headers are always _below_ items,
                    // and depth flows downward.
                    item.Depth = i + (item is DrawableCarouselBeatmapSet ? items.Count : 0);

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
