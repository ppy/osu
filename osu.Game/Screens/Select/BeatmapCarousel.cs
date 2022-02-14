// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Layout;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Input.Bindings;
using osu.Game.Screens.Select.Carousel;
using osuTK;
using osuTK.Input;
using Realms;

namespace osu.Game.Screens.Select
{
    public class BeatmapCarousel : CompositeDrawable, IKeyBindingHandler<GlobalAction>
    {
        /// <summary>
        /// Height of the area above the carousel that should be treated as visible due to transparency of elements in front of it.
        /// </summary>
        public float BleedTop { get; set; }

        /// <summary>
        /// Height of the area below the carousel that should be treated as visible due to transparency of elements in front of it.
        /// </summary>
        public float BleedBottom { get; set; }

        /// <summary>
        /// Triggered when the <see cref="BeatmapSets"/> loaded change and are completely loaded.
        /// </summary>
        public Action BeatmapSetsChanged;

        /// <summary>
        /// The currently selected beatmap.
        /// </summary>
        public BeatmapInfo SelectedBeatmapInfo => selectedBeatmap?.BeatmapInfo;

        private CarouselBeatmap selectedBeatmap => selectedBeatmapSet?.Beatmaps.FirstOrDefault(s => s.State.Value == CarouselItemState.Selected);

        /// <summary>
        /// The currently selected beatmap set.
        /// </summary>
        public BeatmapSetInfo SelectedBeatmapSet => selectedBeatmapSet?.BeatmapSet;

        /// <summary>
        /// A function to optionally decide on a recommended difficulty from a beatmap set.
        /// </summary>
        public Func<IEnumerable<BeatmapInfo>, BeatmapInfo> GetRecommendedBeatmap;

        private CarouselBeatmapSet selectedBeatmapSet;

        /// <summary>
        /// Raised when the <see cref="SelectedBeatmapInfo"/> is changed.
        /// </summary>
        public Action<BeatmapInfo> SelectionChanged;

        public override bool HandleNonPositionalInput => AllowSelection;
        public override bool HandlePositionalInput => AllowSelection;

        public override bool PropagatePositionalInputSubTree => AllowSelection;
        public override bool PropagateNonPositionalInputSubTree => AllowSelection;

        private (int first, int last) displayedRange;

        /// <summary>
        /// Extend the range to retain already loaded pooled drawables.
        /// </summary>
        private const float distance_offscreen_before_unload = 1024;

        /// <summary>
        /// Extend the range to update positions / retrieve pooled drawables outside of visible range.
        /// </summary>
        private const float distance_offscreen_to_preload = 512; // todo: adjust this appropriately once we can make set panel contents load while off-screen.

        /// <summary>
        /// Whether carousel items have completed asynchronously loaded.
        /// </summary>
        public bool BeatmapSetsLoaded { get; private set; }

        protected readonly CarouselScrollContainer Scroll;

        private IEnumerable<CarouselBeatmapSet> beatmapSets => root.Children.OfType<CarouselBeatmapSet>();

        // todo: only used for testing, maybe remove.
        private bool loadedTestBeatmaps;

        public IEnumerable<BeatmapSetInfo> BeatmapSets
        {
            get => beatmapSets.Select(g => g.BeatmapSet);
            set
            {
                loadedTestBeatmaps = true;
                Schedule(() => loadBeatmapSets(value));
            }
        }

        private void loadBeatmapSets(IEnumerable<BeatmapSetInfo> beatmapSets)
        {
            CarouselRoot newRoot = new CarouselRoot(this);

            newRoot.AddChildren(beatmapSets.Select(s => createCarouselSet(s.Detach())).Where(g => g != null));

            root = newRoot;

            if (selectedBeatmapSet != null && !beatmapSets.Contains(selectedBeatmapSet.BeatmapSet))
                selectedBeatmapSet = null;

            Scroll.Clear(false);
            itemsCache.Invalidate();
            ScrollToSelected();

            applyActiveCriteria(false);

            if (loadedTestBeatmaps)
                signalBeatmapsLoaded();
        }

        private readonly List<CarouselItem> visibleItems = new List<CarouselItem>();

        private readonly Cached itemsCache = new Cached();
        private PendingScrollOperation pendingScrollOperation = PendingScrollOperation.None;

        public Bindable<bool> RightClickScrollingEnabled = new Bindable<bool>();

        public Bindable<RandomSelectAlgorithm> RandomAlgorithm = new Bindable<RandomSelectAlgorithm>();
        private readonly List<CarouselBeatmapSet> previouslyVisitedRandomSets = new List<CarouselBeatmapSet>();
        private readonly Stack<CarouselBeatmap> randomSelectedBeatmaps = new Stack<CarouselBeatmap>();

        private CarouselRoot root;

        private IDisposable subscriptionSets;
        private IDisposable subscriptionDeletedSets;
        private IDisposable subscriptionBeatmaps;
        private IDisposable subscriptionHiddenBeatmaps;

        private readonly DrawablePool<DrawableCarouselBeatmapSet> setPool = new DrawablePool<DrawableCarouselBeatmapSet>(100);

        private Sample spinSample;
        private Sample randomSelectSample;

        private int visibleSetsCount;

        public BeatmapCarousel()
        {
            root = new CarouselRoot(this);
            InternalChild = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    setPool,
                    Scroll = new CarouselScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, AudioManager audio)
        {
            spinSample = audio.Samples.Get("SongSelect/random-spin");
            randomSelectSample = audio.Samples.Get(@"SongSelect/select-random");

            config.BindWith(OsuSetting.RandomSelectAlgorithm, RandomAlgorithm);
            config.BindWith(OsuSetting.SongSelectRightMouseScroll, RightClickScrollingEnabled);

            RightClickScrollingEnabled.ValueChanged += enabled => Scroll.RightMouseScrollbar = enabled.NewValue;
            RightClickScrollingEnabled.TriggerChange();

            if (!loadedTestBeatmaps)
            {
                realm.Run(r => loadBeatmapSets(getBeatmapSets(r)));
            }
        }

        [Resolved]
        private RealmAccess realm { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            subscriptionSets = realm.RegisterForNotifications(getBeatmapSets, beatmapSetsChanged);
            subscriptionBeatmaps = realm.RegisterForNotifications(r => r.All<BeatmapInfo>().Where(b => !b.Hidden), beatmapsChanged);

            // Can't use main subscriptions because we can't lookup deleted indices.
            // https://github.com/realm/realm-dotnet/discussions/2634#discussioncomment-1605595.
            subscriptionDeletedSets = realm.RegisterForNotifications(r => r.All<BeatmapSetInfo>().Where(s => s.DeletePending && !s.Protected), deletedBeatmapSetsChanged);
            subscriptionHiddenBeatmaps = realm.RegisterForNotifications(r => r.All<BeatmapInfo>().Where(b => b.Hidden), beatmapsChanged);
        }

        private void deletedBeatmapSetsChanged(IRealmCollection<BeatmapSetInfo> sender, ChangeSet changes, Exception error)
        {
            // If loading test beatmaps, avoid overwriting with realm subscription callbacks.
            if (loadedTestBeatmaps)
                return;

            if (changes == null)
                return;

            foreach (int i in changes.InsertedIndices)
                removeBeatmapSet(sender[i].ID);
        }

        private void beatmapSetsChanged(IRealmCollection<BeatmapSetInfo> sender, ChangeSet changes, Exception error)
        {
            // If loading test beatmaps, avoid overwriting with realm subscription callbacks.
            if (loadedTestBeatmaps)
                return;

            if (changes == null)
            {
                // During initial population, we must manually account for the fact that our original query was done on an async thread.
                // Since then, there may have been imports or deletions.
                // Here we manually catch up on any changes.
                var realmSets = new HashSet<Guid>();

                for (int i = 0; i < sender.Count; i++)
                    realmSets.Add(sender[i].ID);

                foreach (var id in realmSets)
                {
                    if (!root.BeatmapSetsByID.ContainsKey(id))
                        UpdateBeatmapSet(realm.Realm.Find<BeatmapSetInfo>(id).Detach());
                }

                foreach (var id in root.BeatmapSetsByID.Keys)
                {
                    if (!realmSets.Contains(id))
                        removeBeatmapSet(id);
                }

                signalBeatmapsLoaded();
                return;
            }

            foreach (int i in changes.NewModifiedIndices)
                UpdateBeatmapSet(sender[i].Detach());

            foreach (int i in changes.InsertedIndices)
                UpdateBeatmapSet(sender[i].Detach());
        }

        private void beatmapsChanged(IRealmCollection<BeatmapInfo> sender, ChangeSet changes, Exception error)
        {
            // we only care about actual changes in hidden status.
            if (changes == null)
                return;

            foreach (int i in changes.InsertedIndices)
            {
                var beatmapInfo = sender[i];
                var beatmapSet = beatmapInfo.BeatmapSet;

                Debug.Assert(beatmapSet != null);

                // Only require to action here if the beatmap is missing.
                // This avoids processing these events unnecessarily when new beatmaps are imported, for example.
                if (root.BeatmapSetsByID.TryGetValue(beatmapSet.ID, out var existingSet)
                    && existingSet.BeatmapSet.Beatmaps.All(b => b.ID != beatmapInfo.ID))
                {
                    UpdateBeatmapSet(beatmapSet.Detach());
                }
            }
        }

        private IQueryable<BeatmapSetInfo> getBeatmapSets(Realm realm) => realm.All<BeatmapSetInfo>().Where(s => !s.DeletePending && !s.Protected);

        public void RemoveBeatmapSet(BeatmapSetInfo beatmapSet) =>
            removeBeatmapSet(beatmapSet.ID);

        private void removeBeatmapSet(Guid beatmapSetID) => Schedule(() =>
        {
            if (!root.BeatmapSetsByID.TryGetValue(beatmapSetID, out var existingSet))
                return;

            root.RemoveChild(existingSet);
            itemsCache.Invalidate();

            if (!Scroll.UserScrolling)
                ScrollToSelected(true);
        });

        public void UpdateBeatmapSet(BeatmapSetInfo beatmapSet) => Schedule(() =>
        {
            Guid? previouslySelectedID = null;

            // If the selected beatmap is about to be removed, store its ID so it can be re-selected if required
            if (selectedBeatmapSet?.BeatmapSet.ID == beatmapSet.ID)
                previouslySelectedID = selectedBeatmap?.BeatmapInfo.ID;

            var newSet = createCarouselSet(beatmapSet);

            root.RemoveChild(beatmapSet.ID);

            if (newSet != null)
            {
                root.AddChild(newSet);

                // check if we can/need to maintain our current selection.
                if (previouslySelectedID != null)
                    select((CarouselItem)newSet.Beatmaps.FirstOrDefault(b => b.BeatmapInfo.ID == previouslySelectedID) ?? newSet);
            }

            itemsCache.Invalidate();

            if (!Scroll.UserScrolling)
                ScrollToSelected(true);

            BeatmapSetsChanged?.Invoke();
        });

        /// <summary>
        /// Selects a given beatmap on the carousel.
        /// </summary>
        /// <param name="beatmapInfo">The beatmap to select.</param>
        /// <param name="bypassFilters">Whether to select the beatmap even if it is filtered (i.e., not visible on carousel).</param>
        /// <returns>True if a selection was made, False if it wasn't.</returns>
        public bool SelectBeatmap(BeatmapInfo beatmapInfo, bool bypassFilters = true)
        {
            // ensure that any pending events from BeatmapManager have been run before attempting a selection.
            Scheduler.Update();

            if (beatmapInfo?.Hidden != false)
                return false;

            foreach (CarouselBeatmapSet set in beatmapSets)
            {
                if (!bypassFilters && set.Filtered.Value)
                    continue;

                var item = set.Beatmaps.FirstOrDefault(p => p.BeatmapInfo.Equals(beatmapInfo));

                if (item == null)
                    // The beatmap that needs to be selected doesn't exist in this set
                    continue;

                if (!bypassFilters && item.Filtered.Value)
                    return false;

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

            return false;
        }

        /// <summary>
        /// Increment selection in the carousel in a chosen direction.
        /// </summary>
        /// <param name="direction">The direction to increment. Negative is backwards.</param>
        /// <param name="skipDifficulties">Whether to skip individual difficulties and only increment over full groups.</param>
        public void SelectNext(int direction = 1, bool skipDifficulties = true)
        {
            if (beatmapSets.All(s => s.Filtered.Value))
                return;

            if (skipDifficulties)
                selectNextSet(direction, true);
            else
                selectNextDifficulty(direction);
        }

        private void selectNextSet(int direction, bool skipDifficulties)
        {
            var unfilteredSets = beatmapSets.Where(s => !s.Filtered.Value).ToList();

            var nextSet = unfilteredSets[(unfilteredSets.IndexOf(selectedBeatmapSet) + direction + unfilteredSets.Count) % unfilteredSets.Count];

            if (skipDifficulties)
                select(nextSet);
            else
                select(direction > 0 ? nextSet.Beatmaps.First(b => !b.Filtered.Value) : nextSet.Beatmaps.Last(b => !b.Filtered.Value));
        }

        private void selectNextDifficulty(int direction)
        {
            if (selectedBeatmap == null)
                return;

            var unfilteredDifficulties = selectedBeatmapSet.Children.Where(s => !s.Filtered.Value).ToList();

            int index = unfilteredDifficulties.IndexOf(selectedBeatmap);

            if (index + direction < 0 || index + direction >= unfilteredDifficulties.Count)
                selectNextSet(direction, false);
            else
                select(unfilteredDifficulties[index + direction]);
        }

        /// <summary>
        /// Select the next beatmap in the random sequence.
        /// </summary>
        /// <returns>True if a selection could be made, else False.</returns>
        public bool SelectNextRandom()
        {
            if (!AllowSelection)
                return false;

            var visibleSets = beatmapSets.Where(s => !s.Filtered.Value).ToList();

            visibleSetsCount = visibleSets.Count;

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

            if (selectedBeatmapSet != null)
                playSpinSample(distanceBetween(set, selectedBeatmapSet));

            select(set);
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

                    if (selectedBeatmapSet != null)
                        playSpinSample(distanceBetween(beatmap, selectedBeatmapSet));

                    select(beatmap);
                    break;
                }
            }
        }

        private double distanceBetween(CarouselItem item1, CarouselItem item2) => Math.Ceiling(Math.Abs(item1.CarouselYPosition - item2.CarouselYPosition) / DrawableCarouselItem.MAX_HEIGHT);

        private void playSpinSample(double distance)
        {
            var chan = spinSample.GetChannel();
            chan.Frequency.Value = 1f + Math.Min(1f, distance / visibleSetsCount);
            chan.Play();

            randomSelectSample?.Play();
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
        private float visibleHalfHeight => (DrawHeight + BleedBottom + BleedTop) / 2;

        /// <summary>
        /// The position of the lower visible bound with respect to the current scroll position.
        /// </summary>
        private float visibleBottomBound => Scroll.Current + DrawHeight + BleedBottom;

        /// <summary>
        /// The position of the upper visible bound with respect to the current scroll position.
        /// </summary>
        private float visibleUpperBound => Scroll.Current - BleedTop;

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

        private void applyActiveCriteria(bool debounce, bool alwaysResetScrollPosition = true)
        {
            PendingFilter?.Cancel();
            PendingFilter = null;

            if (debounce)
                PendingFilter = Scheduler.AddDelayed(perform, 250);
            else
            {
                // if initial load is not yet finished, this will be run inline in loadBeatmapSets to ensure correct order of operation.
                if (!BeatmapSetsLoaded)
                    PendingFilter = Schedule(perform);
                else
                    perform();
            }

            void perform()
            {
                PendingFilter = null;

                root.Filter(activeCriteria);
                itemsCache.Invalidate();

                if (alwaysResetScrollPosition || !Scroll.UserScrolling)
                    ScrollToSelected(true);
            }
        }

        private void signalBeatmapsLoaded()
        {
            if (!BeatmapSetsLoaded)
            {
                BeatmapSetsChanged?.Invoke();
                BeatmapSetsLoaded = true;
            }

            itemsCache.Invalidate();
        }

        private float? scrollTarget;

        /// <summary>
        /// Scroll to the current <see cref="SelectedBeatmapInfo"/>.
        /// </summary>
        /// <param name="immediate">
        /// Whether the scroll position should immediately be shifted to the target, delegating animation to visible panels.
        /// This should be true for operations like filtering - where panels are changing visibility state - to avoid large jumps in animation.
        /// </param>
        public void ScrollToSelected(bool immediate = false) =>
            pendingScrollOperation = immediate ? PendingScrollOperation.Immediate : PendingScrollOperation.Standard;

        #region Key / button selection logic

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    SelectNext(-1);
                    return true;

                case Key.Right:
                    SelectNext();
                    return true;
            }

            return false;
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
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

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        #endregion

        protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
        {
            // handles the vertical size of the carousel changing (ie. on window resize when aspect ratio has changed).
            if ((invalidation & Invalidation.Layout) > 0)
                itemsCache.Invalidate();

            return base.OnInvalidate(invalidation, source);
        }

        protected override void Update()
        {
            base.Update();

            bool revalidateItems = !itemsCache.IsValid;

            // First we iterate over all non-filtered carousel items and populate their
            // vertical position data.
            if (revalidateItems)
                updateYPositions();

            // if there is a pending scroll action we apply it without animation and transfer the difference in position to the panels.
            // this is intentionally applied before updating the visible range below, to avoid animating new items (sourced from pool) from locations off-screen, as it looks bad.
            if (pendingScrollOperation != PendingScrollOperation.None)
                updateScrollPosition();

            // This data is consumed to find the currently displayable range.
            // This is the range we want to keep drawables for, and should exceed the visible range slightly to avoid drawable churn.
            var newDisplayRange = getDisplayRange();

            // If the filtered items or visible range has changed, pooling requirements need to be checked.
            // This involves fetching new items from the pool, returning no-longer required items.
            if (revalidateItems || newDisplayRange != displayedRange)
            {
                displayedRange = newDisplayRange;

                if (visibleItems.Count > 0)
                {
                    var toDisplay = visibleItems.GetRange(displayedRange.first, displayedRange.last - displayedRange.first + 1);

                    foreach (var panel in Scroll.Children)
                    {
                        if (toDisplay.Remove(panel.Item))
                        {
                            // panel already displayed.
                            continue;
                        }

                        // panel loaded as drawable but not required by visible range.
                        // remove but only if too far off-screen
                        if (panel.Y + panel.DrawHeight < visibleUpperBound - distance_offscreen_before_unload || panel.Y > visibleBottomBound + distance_offscreen_before_unload)
                        {
                            // may want a fade effect here (could be seen if a huge change happens, like a set with 20 difficulties becomes selected).
                            panel.ClearTransforms();
                            panel.Expire();
                        }
                    }

                    // Add those items within the previously found index range that should be displayed.
                    foreach (var item in toDisplay)
                    {
                        var panel = setPool.Get(p => p.Item = item);

                        panel.Depth = item.CarouselYPosition;
                        panel.Y = item.CarouselYPosition;

                        Scroll.Add(panel);
                    }
                }
            }

            // Update externally controlled state of currently visible items (e.g. x-offset and opacity).
            // This is a per-frame update on all drawable panels.
            foreach (DrawableCarouselItem item in Scroll.Children)
            {
                updateItem(item);

                if (item is DrawableCarouselBeatmapSet set)
                {
                    foreach (var diff in set.DrawableBeatmaps)
                        updateItem(diff, item);
                }
            }
        }

        private readonly CarouselBoundsItem carouselBoundsItem = new CarouselBoundsItem();

        private (int firstIndex, int lastIndex) getDisplayRange()
        {
            // Find index range of all items that should be on-screen
            carouselBoundsItem.CarouselYPosition = visibleUpperBound - distance_offscreen_to_preload;
            int firstIndex = visibleItems.BinarySearch(carouselBoundsItem);
            if (firstIndex < 0) firstIndex = ~firstIndex;

            carouselBoundsItem.CarouselYPosition = visibleBottomBound + distance_offscreen_to_preload;
            int lastIndex = visibleItems.BinarySearch(carouselBoundsItem);
            if (lastIndex < 0) lastIndex = ~lastIndex;

            // as we can't be 100% sure on the size of individual carousel drawables,
            // always play it safe and extend bounds by one.
            firstIndex = Math.Max(0, firstIndex - 1);
            lastIndex = Math.Clamp(lastIndex + 1, firstIndex, Math.Max(0, visibleItems.Count - 1));

            return (firstIndex, lastIndex);
        }

        private CarouselBeatmapSet createCarouselSet(BeatmapSetInfo beatmapSet)
        {
            // This can be moved to the realm query if required using:
            // .Filter("DeletePending == false && Protected == false && ANY Beatmaps.Hidden == false")
            //
            // As long as we are detaching though, it makes more sense to do it here as adding to the realm query has an overhead
            // as seen at https://github.com/realm/realm-dotnet/discussions/2773#discussioncomment-2004275.
            if (beatmapSet.Beatmaps.All(b => b.Hidden))
                return null;

            var set = new CarouselBeatmapSet(beatmapSet)
            {
                GetRecommendedBeatmap = beatmaps => GetRecommendedBeatmap?.Invoke(beatmaps)
            };

            foreach (var c in set.Beatmaps)
            {
                c.State.ValueChanged += state =>
                {
                    if (state.NewValue == CarouselItemState.Selected)
                    {
                        selectedBeatmapSet = set;
                        SelectionChanged?.Invoke(c.BeatmapInfo);

                        itemsCache.Invalidate();
                        ScrollToSelected();
                    }
                };
            }

            return set;
        }

        private const float panel_padding = 5;

        /// <summary>
        /// Computes the target Y positions for every item in the carousel.
        /// </summary>
        /// <returns>The Y position of the currently selected item.</returns>
        private void updateYPositions()
        {
            visibleItems.Clear();

            float currentY = visibleHalfHeight;

            scrollTarget = null;

            foreach (CarouselItem item in root.Children)
            {
                if (item.Filtered.Value)
                    continue;

                switch (item)
                {
                    case CarouselBeatmapSet set:
                    {
                        visibleItems.Add(set);
                        set.CarouselYPosition = currentY;

                        if (item.State.Value == CarouselItemState.Selected)
                        {
                            // scroll position at currentY makes the set panel appear at the very top of the carousel's screen space
                            // move down by half of visible height (height of the carousel's visible extent, including semi-transparent areas)
                            // then reapply the top semi-transparent area (because carousel's screen space starts below it)
                            scrollTarget = currentY + DrawableCarouselBeatmapSet.HEIGHT - visibleHalfHeight + BleedTop;

                            foreach (var b in set.Beatmaps)
                            {
                                if (!b.Visible)
                                    continue;

                                if (b.State.Value == CarouselItemState.Selected)
                                {
                                    scrollTarget += b.TotalHeight / 2;
                                    break;
                                }

                                scrollTarget += b.TotalHeight;
                            }
                        }

                        currentY += set.TotalHeight + panel_padding;
                        break;
                    }
                }
            }

            currentY += visibleHalfHeight;

            Scroll.ScrollContent.Height = currentY;

            itemsCache.Validate();

            // update and let external consumers know about selection loss.
            if (BeatmapSetsLoaded)
            {
                bool selectionLost = selectedBeatmapSet != null && selectedBeatmapSet.State.Value != CarouselItemState.Selected;

                if (selectionLost)
                {
                    selectedBeatmapSet = null;
                    SelectionChanged?.Invoke(null);
                }
            }
        }

        private bool firstScroll = true;

        private void updateScrollPosition()
        {
            if (scrollTarget != null)
            {
                if (firstScroll)
                {
                    // reduce movement when first displaying the carousel.
                    Scroll.ScrollTo(scrollTarget.Value - 200, false);
                    firstScroll = false;
                }

                switch (pendingScrollOperation)
                {
                    case PendingScrollOperation.Standard:
                        Scroll.ScrollTo(scrollTarget.Value);
                        break;

                    case PendingScrollOperation.Immediate:

                        // in order to simplify animation logic, rather than using the animated version of ScrollTo,
                        // we take the difference in scroll height and apply to all visible panels.
                        // this avoids edge cases like when the visible panels is reduced suddenly, causing ScrollContainer
                        // to enter clamp-special-case mode where it animates completely differently to normal.
                        float scrollChange = scrollTarget.Value - Scroll.Current;
                        Scroll.ScrollTo(scrollTarget.Value, false);
                        foreach (var i in Scroll.Children)
                            i.Y += scrollChange;
                        break;
                }

                pendingScrollOperation = PendingScrollOperation.None;
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
        /// <param name="item">The item to be updated.</param>
        /// <param name="parent">For nested items, the parent of the item to be updated.</param>
        private void updateItem(DrawableCarouselItem item, DrawableCarouselItem parent = null)
        {
            Vector2 posInScroll = Scroll.ScrollContent.ToLocalSpace(item.Header.ScreenSpaceDrawQuad.Centre);
            float itemDrawY = posInScroll.Y - visibleUpperBound;
            float dist = Math.Abs(1f - itemDrawY / visibleHalfHeight);

            // adjusting the item's overall X position can cause it to become masked away when
            // child items (difficulties) are still visible.
            item.Header.X = offsetX(dist, visibleHalfHeight) - (parent?.X ?? 0);

            // We are applying a multiplicative alpha (which is internally done by nesting an
            // additional container and setting that container's alpha) such that we can
            // layer alpha transformations on top.
            item.SetMultiplicativeAlpha(Math.Clamp(1.75f - 1.5f * dist, 0, 1));
        }

        private enum PendingScrollOperation
        {
            None,
            Standard,
            Immediate,
        }

        /// <summary>
        /// A carousel item strictly used for binary search purposes.
        /// </summary>
        private class CarouselBoundsItem : CarouselItem
        {
            public override DrawableCarouselItem CreateDrawableRepresentation() =>
                throw new NotImplementedException();
        }

        private class CarouselRoot : CarouselGroupEagerSelect
        {
            private readonly BeatmapCarousel carousel;

            public readonly Dictionary<Guid, CarouselBeatmapSet> BeatmapSetsByID = new Dictionary<Guid, CarouselBeatmapSet>();

            public CarouselRoot(BeatmapCarousel carousel)
            {
                // root should always remain selected. if not, PerformSelection will not be called.
                State.Value = CarouselItemState.Selected;
                State.ValueChanged += state => State.Value = CarouselItemState.Selected;

                this.carousel = carousel;
            }

            public override void AddChild(CarouselItem i)
            {
                CarouselBeatmapSet set = (CarouselBeatmapSet)i;
                BeatmapSetsByID.Add(set.BeatmapSet.ID, set);

                base.AddChild(i);
            }

            public void RemoveChild(Guid beatmapSetID)
            {
                if (BeatmapSetsByID.TryGetValue(beatmapSetID, out var carouselBeatmapSet))
                    RemoveChild(carouselBeatmapSet);
            }

            public override void RemoveChild(CarouselItem i)
            {
                CarouselBeatmapSet set = (CarouselBeatmapSet)i;
                BeatmapSetsByID.Remove(set.BeatmapSet.ID);

                base.RemoveChild(i);
            }

            protected override void PerformSelection()
            {
                if (LastSelected == null || LastSelected.Filtered.Value)
                    carousel?.SelectNextRandom();
                else
                    base.PerformSelection();
            }
        }

        protected class CarouselScrollContainer : UserTrackingScrollContainer<DrawableCarouselItem>
        {
            private bool rightMouseScrollBlocked;

            public CarouselScrollContainer()
            {
                // size is determined by the carousel itself, due to not all content necessarily being loaded.
                ScrollContent.AutoSizeAxes = Axes.None;

                // the scroll container may get pushed off-screen by global screen changes, but we still want panels to display outside of the bounds.
                Masking = false;
            }

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

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            subscriptionSets?.Dispose();
            subscriptionDeletedSets?.Dispose();
            subscriptionBeatmaps?.Dispose();
            subscriptionHiddenBeatmaps?.Dispose();
        }
    }
}
