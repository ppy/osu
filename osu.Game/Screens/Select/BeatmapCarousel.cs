// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Layout;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Graphics.Containers;
using osu.Game.Input.Bindings;
using osu.Game.Screens.Select.Carousel;
using osu.Game.Screens.Select.Filter;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Select
{
    public partial class BeatmapCarousel : CompositeDrawable, IKeyBindingHandler<GlobalAction>
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
        /// Triggered when <see cref="BeatmapSets"/> finish loading, or are subsequently changed.
        /// </summary>
        public Action? BeatmapSetsChanged;

        /// <summary>
        /// Triggered after filter conditions have finished being applied to the model hierarchy.
        /// </summary>
        public Action? FilterApplied;

        /// <summary>
        /// The currently selected beatmap.
        /// </summary>
        public BeatmapInfo? SelectedBeatmapInfo => selectedBeatmap?.BeatmapInfo;

        private CarouselBeatmap? selectedBeatmap => selectedBeatmapSet?.Beatmaps.FirstOrDefault(s => s.State.Value == CarouselItemState.Selected);

        /// <summary>
        /// The total count of non-filtered beatmaps displayed.
        /// </summary>
        public int CountDisplayed => beatmapSets.Where(s => !s.Filtered.Value).Sum(s => s.TotalItemsNotFiltered);

        /// <summary>
        /// The currently selected beatmap set.
        /// </summary>
        public BeatmapSetInfo? SelectedBeatmapSet => selectedBeatmapSet?.BeatmapSet;

        /// <summary>
        /// A function to optionally decide on a recommended difficulty from a beatmap set.
        /// </summary>
        public Func<IEnumerable<BeatmapInfo>, BeatmapInfo?>? GetRecommendedBeatmap;

        private CarouselBeatmapSet? selectedBeatmapSet;

        /// <summary>
        /// Raised when the <see cref="SelectedBeatmapInfo"/> is changed.
        /// </summary>
        public Action<BeatmapInfo?>? SelectionChanged;

        public override bool HandleNonPositionalInput => AllowSelection;
        public override bool HandlePositionalInput => AllowSelection;

        public override bool PropagatePositionalInputSubTree => AllowSelection;
        public override bool PropagateNonPositionalInputSubTree => AllowSelection;

        private (int first, int last) displayedRange;

        /// <summary>
        /// Extend the range to retain already loaded pooled drawables.
        /// </summary>
        private const float distance_offscreen_before_unload = 2048;

        /// <summary>
        /// Extend the range to update positions / retrieve pooled drawables outside of visible range.
        /// </summary>
        private const float distance_offscreen_to_preload = 768;

        /// <summary>
        /// Whether carousel items have completed asynchronously loaded.
        /// </summary>
        public bool BeatmapSetsLoaded { get; private set; }

        [Cached]
        protected readonly CarouselScrollContainer Scroll;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        private IBindableList<BeatmapSetInfo>? detachedBeatmapSets;

        private readonly NoResultsPlaceholder noResultsPlaceholder;

        private IEnumerable<CarouselBeatmapSet> beatmapSets => root.Items.OfType<CarouselBeatmapSet>();

        internal IEnumerable<BeatmapSetInfo> BeatmapSets => beatmapSets.Select(g => g.BeatmapSet);

        private void loadNewRoot()
        {
            beatmapsSplitOut = activeCriteria.Sort == SortMode.Difficulty;

            // Ensure no changes are made to the list while we are initialising items.
            // We'll catch up on changes via subscriptions anyway.
            BeatmapSetInfo[] loadableSets = detachedBeatmapSets!.ToArray();

            if (selectedBeatmapSet != null && !loadableSets.Contains(selectedBeatmapSet.BeatmapSet, EqualityComparer<BeatmapSetInfo>.Default))
                selectedBeatmapSet = null;

            var selectedBeatmapBefore = selectedBeatmap?.BeatmapInfo;

            CarouselRoot newRoot = new CarouselRoot(this);

            if (beatmapsSplitOut)
            {
                var carouselBeatmapSets = loadableSets.SelectMany(s => s.Beatmaps).Select(b =>
                {
                    return createCarouselSet(new BeatmapSetInfo(new[] { b })
                    {
                        ID = b.BeatmapSet!.ID,
                        OnlineID = b.BeatmapSet!.OnlineID,
                        Status = b.BeatmapSet!.Status,
                    });
                }).OfType<CarouselBeatmapSet>();

                newRoot.AddItems(carouselBeatmapSets);
            }
            else
            {
                var carouselBeatmapSets = loadableSets.Select(createCarouselSet).OfType<CarouselBeatmapSet>();

                newRoot.AddItems(carouselBeatmapSets);
            }

            root = newRoot;
            root.Filter(activeCriteria);

            Scroll.Clear(false);
            itemsCache.Invalidate();
            ScrollToSelected();

            // Restore selection
            if (selectedBeatmapBefore != null && newRoot.BeatmapSetsByID.TryGetValue(selectedBeatmapBefore.BeatmapSet!.ID, out var newSelectionCandidates))
            {
                CarouselBeatmap? found = newSelectionCandidates.SelectMany(s => s.Beatmaps).SingleOrDefault(b => b.BeatmapInfo.ID == selectedBeatmapBefore.ID);

                if (found != null)
                    found.State.Value = CarouselItemState.Selected;
            }

            Schedule(() =>
            {
                invalidateAfterChange();
                BeatmapSetsLoaded = true;
            });
        }

        private readonly List<CarouselItem> visibleItems = new List<CarouselItem>();

        private readonly Cached itemsCache = new Cached();
        private PendingScrollOperation pendingScrollOperation = PendingScrollOperation.None;

        public Bindable<RandomSelectAlgorithm> RandomAlgorithm = new Bindable<RandomSelectAlgorithm>();
        private readonly List<CarouselBeatmapSet> previouslyVisitedRandomSets = new List<CarouselBeatmapSet>();
        private readonly List<CarouselBeatmap> randomSelectedBeatmaps = new List<CarouselBeatmap>();

        private CarouselRoot root;

        private readonly DrawablePool<DrawableCarouselBeatmapSet> setPool = new DrawablePool<DrawableCarouselBeatmapSet>(100);

        private Sample? spinSample;
        private Sample? randomSelectSample;

        private int visibleSetsCount;

        public BeatmapCarousel(FilterCriteria initialCriteria)
        {
            root = new CarouselRoot(this);
            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    setPool,
                    Scroll = new CarouselScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    noResultsPlaceholder = new NoResultsPlaceholder()
                }
            };

            activeCriteria = initialCriteria;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, AudioManager audio, BeatmapStore beatmaps, CancellationToken? cancellationToken)
        {
            spinSample = audio.Samples.Get("SongSelect/random-spin");
            randomSelectSample = audio.Samples.Get(@"SongSelect/select-random");

            config.BindWith(OsuSetting.RandomSelectAlgorithm, RandomAlgorithm);

            detachedBeatmapSets = beatmaps.GetBeatmapSets(cancellationToken);
            detachedBeatmapSets.BindCollectionChanged(beatmapSetsChanged);
            loadNewRoot();
        }

        private readonly HashSet<BeatmapSetInfo> setsRequiringUpdate = new HashSet<BeatmapSetInfo>();
        private readonly HashSet<BeatmapSetInfo> setsRequiringRemoval = new HashSet<BeatmapSetInfo>();

        private void beatmapSetsChanged(object? beatmaps, NotifyCollectionChangedEventArgs changed)
        {
            IEnumerable<BeatmapSetInfo>? oldBeatmapSets = changed.OldItems?.Cast<BeatmapSetInfo>();
            HashSet<Guid> oldBeatmapSetIDs = oldBeatmapSets?.Select(s => s.ID).ToHashSet() ?? [];

            IEnumerable<BeatmapSetInfo>? newBeatmapSets = changed.NewItems?.Cast<BeatmapSetInfo>();
            HashSet<Guid> newBeatmapSetIDs = newBeatmapSets?.Select(s => s.ID).ToHashSet() ?? [];

            switch (changed.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    setsRequiringRemoval.RemoveWhere(s => newBeatmapSetIDs.Contains(s.ID));
                    setsRequiringUpdate.AddRange(newBeatmapSets!);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    setsRequiringUpdate.RemoveWhere(s => oldBeatmapSetIDs.Contains(s.ID));
                    setsRequiringRemoval.AddRange(oldBeatmapSets!);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    setsRequiringUpdate.RemoveWhere(s => oldBeatmapSetIDs.Contains(s.ID));
                    setsRequiringRemoval.AddRange(oldBeatmapSets!);

                    setsRequiringRemoval.RemoveWhere(s => newBeatmapSetIDs.Contains(s.ID));
                    setsRequiringUpdate.AddRange(newBeatmapSets!);
                    break;

                case NotifyCollectionChangedAction.Move:
                    setsRequiringUpdate.AddRange(newBeatmapSets!);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    setsRequiringRemoval.Clear();
                    setsRequiringUpdate.Clear();
                    loadNewRoot();
                    break;
            }

            Scheduler.AddOnce(processBeatmapChanges);
        }

        // All local operations must be scheduled.
        //
        // If we don't schedule, beatmaps getting changed while song select is suspended (ie. last played being updated)
        // will cause unexpected sounds and operations to occur in the background.
        private void processBeatmapChanges()
        {
            try
            {
                // To handle the beatmap update flow, attempt to track selection changes across delete-insert transactions.
                // When an update occurs, the previous beatmap set is either soft or hard deleted.
                // Check if the current selection was potentially deleted by re-querying its validity.
                bool selectedSetMarkedDeleted = SelectedBeatmapSet != null && fetchFromID(SelectedBeatmapSet.ID)?.DeletePending != false;

                foreach (var set in setsRequiringRemoval) removeBeatmapSet(set.ID);

                foreach (var set in setsRequiringUpdate) updateBeatmapSet(set);

                if (setsRequiringRemoval.Count > 0 && SelectedBeatmapInfo != null)
                {
                    // If SelectedBeatmapInfo is non-null, the set should also be non-null.
                    Debug.Assert(SelectedBeatmapSet != null);

                    if (selectedSetMarkedDeleted && setsRequiringUpdate.Any())
                    {
                        // If it is no longer valid, make the bold assumption that an updated version will be available in the modified/inserted indices.
                        // This relies on the full update operation being in a single transaction, so please don't change that.
                        foreach (var set in setsRequiringUpdate)
                        {
                            foreach (var beatmapInfo in set.Beatmaps)
                            {
                                if (!((IBeatmapMetadataInfo)beatmapInfo.Metadata).Equals(SelectedBeatmapInfo.Metadata)) continue;

                                // Best effort matching. We can't use ID because in the update flow a new version will get its own GUID.
                                if (beatmapInfo.DifficultyName == SelectedBeatmapInfo.DifficultyName)
                                {
                                    SelectBeatmap(beatmapInfo);
                                    return;
                                }
                            }
                        }

                        // If a direct selection couldn't be made, it's feasible that the difficulty name (or beatmap metadata) changed.
                        // Let's attempt to follow set-level selection anyway.
                        SelectBeatmap(setsRequiringUpdate.First().Beatmaps.First());
                    }
                }
            }
            finally
            {
                BeatmapSetsLoaded = true;
                invalidateAfterChange();
            }

            setsRequiringRemoval.Clear();
            setsRequiringUpdate.Clear();

            BeatmapSetInfo? fetchFromID(Guid id) => realm.Realm.Find<BeatmapSetInfo>(id);
        }

        public void RemoveBeatmapSet(BeatmapSetInfo beatmapSet) => Schedule(() =>
        {
            removeBeatmapSet(beatmapSet.ID);
            invalidateAfterChange();
        });

        private void removeBeatmapSet(Guid beatmapSetID)
        {
            if (!root.BeatmapSetsByID.TryGetValue(beatmapSetID, out var existingSets))
                return;

            foreach (var set in existingSets)
            {
                foreach (var beatmap in set.Beatmaps)
                    randomSelectedBeatmaps.Remove(beatmap);
                previouslyVisitedRandomSets.Remove(set);

                root.RemoveItem(set);
            }
        }

        public void UpdateBeatmapSet(BeatmapSetInfo beatmapSet) => Schedule(() =>
        {
            updateBeatmapSet(beatmapSet);
            invalidateAfterChange();
        });

        private void updateBeatmapSet(BeatmapSetInfo beatmapSet)
        {
            var newSets = new List<CarouselBeatmapSet>();

            if (beatmapsSplitOut)
            {
                foreach (var beatmap in beatmapSet.Beatmaps)
                {
                    var newSet = createCarouselSet(new BeatmapSetInfo(new[] { beatmap })
                    {
                        ID = beatmapSet.ID,
                        OnlineID = beatmapSet.OnlineID,
                        Status = beatmapSet.Status,
                    });

                    if (newSet != null)
                        newSets.Add(newSet);
                }
            }
            else
            {
                var newSet = createCarouselSet(beatmapSet);

                if (newSet != null)
                    newSets.Add(newSet);
            }

            var removedSets = root.ReplaceItem(beatmapSet, newSets);

            // If we don't remove these here, it may remain in a hidden state until scrolled off screen.
            // Doesn't really affect anything during actual user interaction, but makes testing annoying.
            foreach (var removedSet in removedSets)
            {
                var removedDrawable = Scroll.FirstOrDefault(c => c.Item == removedSet);
                if (removedDrawable != null)
                    expirePanelImmediately(removedDrawable);
            }
        }

        /// <summary>
        /// Selects a given beatmap on the carousel.
        /// </summary>
        /// <param name="beatmapInfo">The beatmap to select.</param>
        /// <param name="bypassFilters">Whether to select the beatmap even if it is filtered (i.e., not visible on carousel).</param>
        /// <returns>True if a selection was made, False if it wasn't.</returns>
        public bool SelectBeatmap(BeatmapInfo? beatmapInfo, bool bypassFilters = true)
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
            if (selectedBeatmap == null || selectedBeatmapSet == null)
                return;

            var unfilteredSets = beatmapSets.Where(s => !s.Filtered.Value).ToList();

            var nextSet = unfilteredSets[(unfilteredSets.IndexOf(selectedBeatmapSet) + direction + unfilteredSets.Count) % unfilteredSets.Count];

            if (skipDifficulties)
                select(nextSet);
            else
                select(direction > 0 ? nextSet.Beatmaps.First(b => !b.Filtered.Value) : nextSet.Beatmaps.Last(b => !b.Filtered.Value));
        }

        private void selectNextDifficulty(int direction)
        {
            if (selectedBeatmap == null || selectedBeatmapSet == null)
                return;

            var unfilteredDifficulties = selectedBeatmapSet.Items.Where(s => !s.Filtered.Value).ToList();

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

            if (selectedBeatmap != null && selectedBeatmapSet != null)
            {
                randomSelectedBeatmaps.Add(selectedBeatmap);

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
                var beatmap = randomSelectedBeatmaps[^1];
                randomSelectedBeatmaps.RemoveAt(randomSelectedBeatmaps.Count - 1);

                if (!beatmap.Filtered.Value && beatmap.BeatmapInfo.BeatmapSet?.DeletePending != true)
                {
                    if (selectedBeatmapSet != null)
                    {
                        if (RandomAlgorithm.Value == RandomSelectAlgorithm.RandomPermutation)
                            previouslyVisitedRandomSets.Remove(selectedBeatmapSet);

                        playSpinSample(distanceBetween(beatmap, selectedBeatmapSet));
                    }

                    select(beatmap);
                    break;
                }
            }
        }

        private double distanceBetween(CarouselItem item1, CarouselItem item2) => Math.Ceiling(Math.Abs(item1.CarouselYPosition - item2.CarouselYPosition) / DrawableCarouselItem.MAX_HEIGHT);

        private void playSpinSample(double distance)
        {
            var chan = spinSample?.GetChannel();

            if (chan != null)
            {
                chan.Frequency.Value = 1f + Math.Min(1f, distance / visibleSetsCount);
                chan.Play();
            }

            randomSelectSample?.Play();
        }

        private void select(CarouselItem? item)
        {
            if (!AllowSelection)
                return;

            if (item == null) return;

            item.State.Value = CarouselItemState.Selected;
        }

        private FilterCriteria activeCriteria;

        protected ScheduledDelegate? PendingFilter;

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
        private float visibleBottomBound => (float)(Scroll.Current + DrawHeight + BleedBottom);

        /// <summary>
        /// The position of the upper visible bound with respect to the current scroll position.
        /// </summary>
        private float visibleUpperBound => (float)(Scroll.Current - BleedTop);

        public void FlushPendingFilterOperations()
        {
            if (!IsLoaded)
                return;

            if (PendingFilter?.Completed == false)
            {
                applyActiveCriteria(false);
                Update();
            }
        }

        public void Filter(FilterCriteria? newCriteria)
        {
            if (newCriteria != null)
                activeCriteria = newCriteria;

            applyActiveCriteria(true);
        }

        private bool beatmapsSplitOut;

        private void applyActiveCriteria(bool debounce)
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

                if ((activeCriteria.Sort == SortMode.Difficulty) != beatmapsSplitOut)
                {
                    loadNewRoot();
                    return;
                }

                root.Filter(activeCriteria);
                itemsCache.Invalidate();

                ScrollToSelected(true);

                FilterApplied?.Invoke();
            }
        }

        private void invalidateAfterChange()
        {
            itemsCache.Invalidate();

            if (!Scroll.UserScrolling)
                ScrollToSelected(true);

            BeatmapSetsChanged?.Invoke();
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

        #region Button selection logic

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.SelectNext:
                case GlobalAction.ActivateNextSet:
                    SelectNext(1, e.Action == GlobalAction.ActivateNextSet);
                    return true;

                case GlobalAction.SelectPrevious:
                case GlobalAction.ActivatePreviousSet:
                    SelectNext(-1, e.Action == GlobalAction.ActivatePreviousSet);
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
            if (invalidation.HasFlag(Invalidation.DrawSize))
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
            {
                updateYPositions();

                if (visibleItems.Count == 0)
                {
                    noResultsPlaceholder.Filter = activeCriteria;
                    noResultsPlaceholder.Show();
                }
                else
                    noResultsPlaceholder.Hide();
            }

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

                    foreach (var panel in Scroll)
                    {
                        Debug.Assert(panel.Item != null);

                        if (toDisplay.Remove(panel.Item))
                        {
                            // panel already displayed.
                            continue;
                        }

                        // panel loaded as drawable but not required by visible range.
                        // remove but only if too far off-screen
                        if (panel.Y + panel.DrawHeight < visibleUpperBound - distance_offscreen_before_unload || panel.Y > visibleBottomBound + distance_offscreen_before_unload)
                            expirePanelImmediately(panel);
                    }

                    // Add those items within the previously found index range that should be displayed.
                    foreach (var item in toDisplay)
                    {
                        var panel = setPool.Get();

                        panel.Item = item;
                        panel.Y = item.CarouselYPosition;

                        Scroll.Add(panel);
                    }
                }
            }

            // Update externally controlled state of currently visible items (e.g. x-offset and opacity).
            // This is a per-frame update on all drawable panels.
            foreach (DrawableCarouselItem item in Scroll)
            {
                updateItem(item);

                Debug.Assert(item.Item != null);

                if (item.Item.Visible)
                {
                    bool isSelected = item.Item.State.Value == CarouselItemState.Selected;

                    bool hasPassedSelection = item.Item.CarouselYPosition < selectedBeatmapSet?.CarouselYPosition;

                    // Cheap way of doing animations when entering / exiting song select.
                    const double half_time = 50;
                    const float panel_x_offset_when_inactive = 200;

                    if (isSelected || AllowSelection)
                    {
                        item.Alpha = (float)Interpolation.DampContinuously(item.Alpha, 1, half_time, Clock.ElapsedFrameTime);
                        item.X = (float)Interpolation.DampContinuously(item.X, 0, half_time, Clock.ElapsedFrameTime);
                    }
                    else
                    {
                        item.Alpha = (float)Interpolation.DampContinuously(item.Alpha, 0, half_time, Clock.ElapsedFrameTime);
                        item.X = (float)Interpolation.DampContinuously(item.X, panel_x_offset_when_inactive, half_time, Clock.ElapsedFrameTime);
                    }

                    Scroll.ChangeChildDepth(item, hasPassedSelection ? -item.Item.CarouselYPosition : item.Item.CarouselYPosition);
                }

                if (item is DrawableCarouselBeatmapSet set)
                {
                    for (int i = 0; i < set.DrawableBeatmaps.Count; i++)
                        updateItem(set.DrawableBeatmaps[i], item);
                }
            }
        }

        private static void expirePanelImmediately(DrawableCarouselItem panel)
        {
            // may want a fade effect here (could be seen if a huge change happens, like a set with 20 difficulties becomes selected).
            panel.ClearTransforms();
            panel.Expire();
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

        private CarouselBeatmapSet? createCarouselSet(BeatmapSetInfo beatmapSet)
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

        /// <summary>
        /// Computes the target Y positions for every item in the carousel.
        /// </summary>
        /// <returns>The Y position of the currently selected item.</returns>
        private void updateYPositions()
        {
            visibleItems.Clear();

            float currentY = visibleHalfHeight;

            scrollTarget = null;

            foreach (CarouselItem item in root.Items)
            {
                if (item.Filtered.Value)
                    continue;

                switch (item)
                {
                    case CarouselBeatmapSet set:
                    {
                        bool isSelected = item.State.Value == CarouselItemState.Selected;

                        float padding = isSelected ? 5 : -5;

                        if (isSelected)
                            // double padding because we want to cancel the negative padding from the last item.
                            currentY += padding * 2;

                        visibleItems.Add(set);
                        set.CarouselYPosition = currentY;

                        if (isSelected)
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

                        currentY += set.TotalHeight + padding;
                        break;
                    }
                }
            }

            currentY += visibleHalfHeight;

            Scroll.ScrollContent.Height = currentY;

            itemsCache.Validate();

            // update and let external consumers know about selection loss.
            if (BeatmapSetsLoaded && AllowSelection)
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
                        float scrollChange = (float)(scrollTarget.Value - Scroll.Current);
                        Scroll.ScrollTo(scrollTarget.Value, false);
                        foreach (var i in Scroll)
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
        /// Update an item's x position and multiplicative alpha based on its y position and
        /// the current scroll position.
        /// </summary>
        /// <param name="item">The item to be updated.</param>
        /// <param name="parent">For nested items, the parent of the item to be updated.</param>
        private void updateItem(DrawableCarouselItem item, DrawableCarouselItem? parent = null)
        {
            Vector2 posInScroll = Scroll.ScrollContent.ToLocalSpace(item.Header.ScreenSpaceDrawQuad.Centre);
            float itemDrawY = posInScroll.Y - visibleUpperBound;
            float dist = Math.Abs(1f - itemDrawY / visibleHalfHeight);

            // adjusting the item's overall X position can cause it to become masked away when
            // child items (difficulties) are still visible.
            item.Header.X = offsetX(dist, visibleHalfHeight) - (parent?.X ?? 0);
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
            public override DrawableCarouselItem CreateDrawableRepresentation() => throw new NotImplementedException();
        }

        private class CarouselRoot : CarouselGroupEagerSelect
        {
            // May only be null during construction (State.Value set causes PerformSelection to be triggered).
            private readonly BeatmapCarousel? carousel;

            public readonly Dictionary<Guid, List<CarouselBeatmapSet>> BeatmapSetsByID = new Dictionary<Guid, List<CarouselBeatmapSet>>();

            public CarouselRoot(BeatmapCarousel carousel)
            {
                // root should always remain selected. if not, PerformSelection will not be called.
                State.Value = CarouselItemState.Selected;
                State.ValueChanged += _ => State.Value = CarouselItemState.Selected;

                this.carousel = carousel;
            }

            public override void AddItem(CarouselItem i)
            {
                CarouselBeatmapSet set = (CarouselBeatmapSet)i;
                if (BeatmapSetsByID.TryGetValue(set.BeatmapSet.ID, out var sets))
                    sets.Add(set);
                else
                    BeatmapSetsByID.Add(set.BeatmapSet.ID, new List<CarouselBeatmapSet> { set });

                base.AddItem(i);
            }

            /// <summary>
            /// A special method to handle replace operations (general for updating a beatmap).
            /// Avoids event-driven selection flip-flopping during the remove/add process.
            /// </summary>
            /// <param name="oldItem">The beatmap set to be replaced.</param>
            /// <param name="newItems">All new items to replace the removed beatmap set.</param>
            /// <returns>All removed items, for any further processing.</returns>
            public IEnumerable<CarouselBeatmapSet> ReplaceItem(BeatmapSetInfo oldItem, List<CarouselBeatmapSet> newItems)
            {
                var previousSelection = (LastSelected as CarouselBeatmapSet)?.Beatmaps
                                                                            .FirstOrDefault(s => s.State.Value == CarouselItemState.Selected)
                                                                            ?.BeatmapInfo;

                bool wasSelected = previousSelection?.BeatmapSet?.ID == oldItem.ID;

                // Without doing this, the removal of the old beatmap will cause carousel's eager selection
                // logic to invoke, causing one unnecessary selection.
                DisableSelection = true;
                var removedSets = RemoveItemsByID(oldItem.ID);
                DisableSelection = false;

                foreach (var set in newItems)
                    AddItem(set);

                // Check if we can/need to maintain our current selection.
                if (wasSelected)
                {
                    CarouselBeatmap? matchingBeatmap = newItems.SelectMany(s => s.Beatmaps)
                                                               .FirstOrDefault(b => b.BeatmapInfo.ID == previousSelection?.ID);

                    if (matchingBeatmap != null)
                        matchingBeatmap.State.Value = CarouselItemState.Selected;
                }

                return removedSets;
            }

            public IEnumerable<CarouselBeatmapSet> RemoveItemsByID(Guid beatmapSetID)
            {
                if (BeatmapSetsByID.TryGetValue(beatmapSetID, out var carouselBeatmapSets))
                {
                    foreach (var set in carouselBeatmapSets)
                        RemoveItem(set);

                    return carouselBeatmapSets;
                }

                return Enumerable.Empty<CarouselBeatmapSet>();
            }

            public override void RemoveItem(CarouselItem i)
            {
                CarouselBeatmapSet set = (CarouselBeatmapSet)i;
                BeatmapSetsByID.Remove(set.BeatmapSet.ID);

                base.RemoveItem(i);
            }

            protected override void PerformSelection()
            {
                if (LastSelected == null)
                    carousel?.SelectNextRandom();
                else
                    base.PerformSelection();
            }
        }

        public partial class CarouselScrollContainer : UserTrackingScrollContainer<DrawableCarouselItem>, IKeyBindingHandler<GlobalAction>
        {
            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

            public CarouselScrollContainer()
            {
                // size is determined by the carousel itself, due to not all content necessarily being loaded.
                ScrollContent.AutoSizeAxes = Axes.None;

                // the scroll container may get pushed off-screen by global screen changes, but we still want panels to display outside of the bounds.
                Masking = false;
            }

            #region Absolute scrolling

            private bool absoluteScrolling;

            protected override bool IsDragging => base.IsDragging || absoluteScrolling;

            public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
            {
                switch (e.Action)
                {
                    case GlobalAction.AbsoluteScrollSongList:
                        beginAbsoluteScrolling(e);
                        return true;
                }

                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
            {
                switch (e.Action)
                {
                    case GlobalAction.AbsoluteScrollSongList:
                        endAbsoluteScrolling();
                        break;
                }
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                if (e.Button == MouseButton.Right)
                {
                    // To avoid conflicts with context menus, disallow absolute scroll if it looks like things will fall over.
                    if (GetContainingInputManager()!.HoveredDrawables.OfType<IHasContextMenu>().Any())
                        return false;

                    beginAbsoluteScrolling(e);
                }

                return base.OnMouseDown(e);
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                if (e.Button == MouseButton.Right)
                    endAbsoluteScrolling();
                base.OnMouseUp(e);
            }

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                if (absoluteScrolling)
                {
                    ScrollToAbsolutePosition(e.CurrentState.Mouse.Position);
                    return true;
                }

                return base.OnMouseMove(e);
            }

            private void beginAbsoluteScrolling(UIEvent e)
            {
                ScrollToAbsolutePosition(e.CurrentState.Mouse.Position);
                absoluteScrolling = true;
            }

            private void endAbsoluteScrolling() => absoluteScrolling = false;

            #endregion

            protected override ScrollbarContainer CreateScrollbar(Direction direction)
            {
                return new PaddedScrollbar();
            }

            protected partial class PaddedScrollbar : OsuScrollbar
            {
                public PaddedScrollbar()
                    : base(Direction.Vertical)
                {
                }
            }

            private const float top_padding = 10;
            private const float bottom_padding = 70;

            protected override float ToScrollbarPosition(double scrollPosition)
            {
                if (Precision.AlmostEquals(0, ScrollableExtent))
                    return 0;

                return (float)(top_padding + (ScrollbarMovementExtent - (top_padding + bottom_padding)) * (scrollPosition / ScrollableExtent));
            }

            protected override float FromScrollbarPosition(float scrollbarPosition)
            {
                if (Precision.AlmostEquals(0, ScrollbarMovementExtent))
                    return 0;

                return (float)(ScrollableExtent * ((scrollbarPosition - top_padding) / (ScrollbarMovementExtent - (top_padding + bottom_padding))));
            }
        }
    }
}
