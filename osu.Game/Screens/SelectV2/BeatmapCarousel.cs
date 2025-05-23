// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.Carousel;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Select;

namespace osu.Game.Screens.SelectV2
{
    [Cached]
    public partial class BeatmapCarousel : Carousel<BeatmapInfo>
    {
        public Action<BeatmapInfo>? RequestPresentBeatmap { private get; init; }

        /// <summary>
        /// From the provided beatmaps, return the most appropriate one for the user's skill.
        /// </summary>
        public Func<IEnumerable<BeatmapInfo>, BeatmapInfo>? ChooseRecommendedBeatmap { private get; init; }

        public const float SPACING = 3f;

        private IBindableList<BeatmapSetInfo> detachedBeatmaps = null!;

        private readonly LoadingLayer loading;

        private readonly BeatmapCarouselFilterMatching matching;
        private readonly BeatmapCarouselFilterGrouping grouping;

        /// <summary>
        /// Total number of beatmap difficulties displayed with the filter.
        /// </summary>
        public int MatchedBeatmapsCount => matching.BeatmapItemsCount;

        protected override float GetSpacingBetweenPanels(CarouselItem top, CarouselItem bottom)
        {
            // Group panels do not overlap with any other panel but should overlap with themselves.
            if ((top.Model is GroupDefinition) ^ (bottom.Model is GroupDefinition))
                return SPACING * 2;

            // Beatmap difficulty panels do not overlap with themselves or any other panel.
            if (grouping.BeatmapSetsGroupedTogether && (top.Model is BeatmapInfo || bottom.Model is BeatmapInfo))
                return SPACING;

            return -SPACING;
        }

        public BeatmapCarousel()
        {
            DebounceDelay = 100;
            DistanceOffscreenToPreload = 100;

            Filters = new ICarouselFilter[]
            {
                matching = new BeatmapCarouselFilterMatching(() => Criteria),
                new BeatmapCarouselFilterSorting(() => Criteria),
                grouping = new BeatmapCarouselFilterGrouping(() => Criteria),
            };

            AddInternal(loading = new LoadingLayer(dimBackground: true));
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapStore beatmapStore, CancellationToken? cancellationToken)
        {
            setupPools();
            setupBeatmaps(beatmapStore, cancellationToken);
        }

        #region Beatmap source hookup

        private void setupBeatmaps(BeatmapStore beatmapStore, CancellationToken? cancellationToken)
        {
            detachedBeatmaps = beatmapStore.GetBeatmapSets(cancellationToken);
            detachedBeatmaps.BindCollectionChanged(beatmapSetsChanged, true);
        }

        private void beatmapSetsChanged(object? beatmaps, NotifyCollectionChangedEventArgs changed)
        {
            // TODO: moving management of BeatmapInfo tracking to BeatmapStore might be something we want to consider.
            // right now we are managing this locally which is a bit of added overhead.
            IEnumerable<BeatmapSetInfo>? newItems = changed.NewItems?.Cast<BeatmapSetInfo>();
            IEnumerable<BeatmapSetInfo>? oldItems = changed.OldItems?.Cast<BeatmapSetInfo>();

            switch (changed.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Items.AddRange(newItems!.SelectMany(s => s.Beatmaps));
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var set in oldItems!)
                    {
                        foreach (var beatmap in set.Beatmaps)
                            Items.RemoveAll(i => i is BeatmapInfo bi && beatmap.Equals(bi));
                    }

                    break;

                case NotifyCollectionChangedAction.Move:
                    // We can ignore move operations as we are applying our own sort in all cases.
                    break;

                case NotifyCollectionChangedAction.Replace:
                    var oldSetBeatmaps = oldItems!.Single().Beatmaps;
                    var newSetBeatmaps = newItems!.Single().Beatmaps.ToList();

                    // Handling replace operations is a touch manual, as we need to locally diff the beatmaps of each version of the beatmap set.
                    // Matching is done based on online IDs, then difficulty names as these are the most stable thing between updates (which are usually triggered
                    // by users editing the beatmap or by difficulty/metadata recomputation).
                    //
                    // In the case of difficulty reprocessing, this will trigger multiple times per beatmap as it's always triggering a set update.
                    // We may want to look to improve this in the future either here or at the source (only trigger an update after all difficulties
                    // have been processed) if it becomes an issue for animation or performance reasons.
                    foreach (var beatmap in oldSetBeatmaps)
                    {
                        int previousIndex = Items.IndexOf(beatmap);
                        Debug.Assert(previousIndex >= 0);

                        BeatmapInfo? matchingNewBeatmap =
                            newSetBeatmaps.SingleOrDefault(b => b.OnlineID > 0 && b.OnlineID == beatmap.OnlineID) ??
                            newSetBeatmaps.SingleOrDefault(b => b.DifficultyName == beatmap.DifficultyName && b.Ruleset.Equals(beatmap.Ruleset));

                        if (matchingNewBeatmap != null)
                        {
                            // TODO: should this exist in song select instead of here?
                            // we need to ensure the global beatmap is also updated alongside changes.
                            if (CurrentSelection != null && CheckModelEquality(beatmap, CurrentSelection))
                                CurrentSelection = matchingNewBeatmap;

                            Items.ReplaceRange(previousIndex, 1, [matchingNewBeatmap]);
                            newSetBeatmaps.Remove(matchingNewBeatmap);
                        }
                        else
                        {
                            Items.RemoveAt(previousIndex);
                        }
                    }

                    // Add any items which weren't found in the previous pass (difficulty names didn't match).
                    foreach (var beatmap in newSetBeatmaps)
                        Items.Add(beatmap);

                    break;

                case NotifyCollectionChangedAction.Reset:
                    Items.Clear();
                    break;
            }
        }

        #endregion

        #region Selection handling

        private GroupDefinition? lastSelectedGroup;
        private BeatmapInfo? lastSelectedBeatmap;

        protected override void HandleItemActivated(CarouselItem item)
        {
            switch (item.Model)
            {
                case GroupDefinition group:
                    // Special case – collapsing an open group.
                    if (lastSelectedGroup == group)
                    {
                        setExpansionStateOfGroup(lastSelectedGroup, false);
                        lastSelectedGroup = null;
                        return;
                    }

                    setExpandedGroup(group);
                    return;

                case BeatmapSetInfo setInfo:
                    // Selecting a set isn't valid – let's re-select the first visible difficulty.
                    if (grouping.SetItems.TryGetValue(setInfo, out var items))
                    {
                        var beatmaps = items.Select(i => i.Model).OfType<BeatmapInfo>();
                        CurrentSelection = ChooseRecommendedBeatmap?.Invoke(beatmaps) ?? beatmaps.First();
                    }

                    return;

                case BeatmapInfo beatmapInfo:
                    if (CurrentSelection != null && CheckModelEquality(CurrentSelection, beatmapInfo))
                    {
                        RequestPresentBeatmap?.Invoke(beatmapInfo);
                        return;
                    }

                    CurrentSelection = beatmapInfo;
                    return;
            }
        }

        protected override void HandleItemSelected(object? model)
        {
            base.HandleItemSelected(model);

            switch (model)
            {
                case BeatmapSetInfo:
                case GroupDefinition:
                    throw new InvalidOperationException("Groups should never become selected");

                case BeatmapInfo beatmapInfo:
                    // Find any containing group. There should never be too many groups so iterating is efficient enough.
                    GroupDefinition? containingGroup = grouping.GroupItems.SingleOrDefault(kvp => kvp.Value.Any(i => CheckModelEquality(i.Model, beatmapInfo))).Key;

                    if (containingGroup != null)
                        setExpandedGroup(containingGroup);
                    setExpandedSet(beatmapInfo);
                    break;
            }
        }

        protected override bool CheckValidForGroupSelection(CarouselItem item)
        {
            switch (item.Model)
            {
                case BeatmapSetInfo:
                    return true;

                case BeatmapInfo:
                    return !grouping.BeatmapSetsGroupedTogether;

                case GroupDefinition:
                    return false;

                default:
                    throw new ArgumentException($"Unsupported model type {item.Model}");
            }
        }

        private void setExpandedGroup(GroupDefinition group)
        {
            if (lastSelectedGroup != null)
                setExpansionStateOfGroup(lastSelectedGroup, false);
            lastSelectedGroup = group;
            setExpansionStateOfGroup(group, true);
        }

        private void setExpansionStateOfGroup(GroupDefinition group, bool expanded)
        {
            if (grouping.GroupItems.TryGetValue(group, out var items))
            {
                if (expanded)
                {
                    foreach (var i in items)
                    {
                        switch (i.Model)
                        {
                            case GroupDefinition:
                                i.IsExpanded = true;
                                break;

                            case BeatmapSetInfo set:
                                // Case where there are set headers, header should be visible
                                // and items should use the set's expanded state.
                                i.IsVisible = true;
                                setExpansionStateOfSetItems(set, i.IsExpanded);
                                break;

                            default:
                                // Case where there are no set headers, all items should be visible.
                                if (!grouping.BeatmapSetsGroupedTogether)
                                    i.IsVisible = true;
                                break;
                        }
                    }
                }
                else
                {
                    foreach (var i in items)
                    {
                        switch (i.Model)
                        {
                            case GroupDefinition:
                                i.IsExpanded = false;
                                break;

                            default:
                                i.IsVisible = false;
                                break;
                        }
                    }
                }
            }
        }

        private void setExpandedSet(BeatmapInfo beatmapInfo)
        {
            if (lastSelectedBeatmap != null)
                setExpansionStateOfSetItems(lastSelectedBeatmap.BeatmapSet!, false);
            lastSelectedBeatmap = beatmapInfo;
            setExpansionStateOfSetItems(beatmapInfo.BeatmapSet!, true);
        }

        private void setExpansionStateOfSetItems(BeatmapSetInfo set, bool expanded)
        {
            if (grouping.SetItems.TryGetValue(set, out var items))
            {
                foreach (var i in items)
                {
                    if (i.Model is BeatmapSetInfo)
                        i.IsExpanded = expanded;
                    else
                        i.IsVisible = expanded;
                }
            }
        }

        #endregion

        #region Animation

        /// <summary>
        /// Moves non-selected beatmaps to the right, hiding off-screen.
        /// </summary>
        public bool VisuallyFocusSelected { get; set; }

        private float selectionFocusOffset;

        protected override void Update()
        {
            base.Update();

            selectionFocusOffset = (float)Interpolation.DampContinuously(selectionFocusOffset, VisuallyFocusSelected ? 300 : 0, 100, Time.Elapsed);
        }

        protected override float GetPanelXOffset(Drawable panel)
        {
            return base.GetPanelXOffset(panel) + (((ICarouselPanel)panel).Selected.Value ? 0 : selectionFocusOffset);
        }

        #endregion

        #region Filtering

        public FilterCriteria Criteria { get; private set; } = new FilterCriteria();

        private ScheduledDelegate? loadingDebounce;

        public void Filter(FilterCriteria criteria)
        {
            Criteria = criteria;

            loadingDebounce ??= Scheduler.AddDelayed(() => loading.Show(), 250);

            FilterAsync().ContinueWith(_ => Schedule(() =>
            {
                loadingDebounce?.Cancel();
                loadingDebounce = null;

                loading.Hide();
            }));
        }

        #endregion

        #region Drawable pooling

        private readonly DrawablePool<PanelBeatmap> beatmapPanelPool = new DrawablePool<PanelBeatmap>(100);
        private readonly DrawablePool<PanelBeatmapStandalone> standalonePanelPool = new DrawablePool<PanelBeatmapStandalone>(100);
        private readonly DrawablePool<PanelBeatmapSet> setPanelPool = new DrawablePool<PanelBeatmapSet>(100);
        private readonly DrawablePool<PanelGroup> groupPanelPool = new DrawablePool<PanelGroup>(100);
        private readonly DrawablePool<PanelGroupStarDifficulty> starsGroupPanelPool = new DrawablePool<PanelGroupStarDifficulty>(11);

        private void setupPools()
        {
            AddInternal(starsGroupPanelPool);
            AddInternal(groupPanelPool);
            AddInternal(beatmapPanelPool);
            AddInternal(standalonePanelPool);
            AddInternal(setPanelPool);
        }

        protected override bool CheckModelEquality(object x, object y)
        {
            // In the confines of the carousel logic, we assume that CurrentSelection (and all items) are using non-stale
            // BeatmapInfo reference, and that we can match based on beatmap / beatmapset (GU)IDs.
            //
            // If there's a case where updates don't come in as expected, diagnosis should start from BeatmapStore, ensuring
            // it is doing a Replace operation on the list. If it is, then check the local handling in beatmapSetsChanged
            // before changing matching requirements here.

            if (x is BeatmapSetInfo beatmapSetX && y is BeatmapSetInfo beatmapSetY)
                return beatmapSetX.Equals(beatmapSetY);

            if (x is BeatmapInfo beatmapX && y is BeatmapInfo beatmapY)
                return beatmapX.Equals(beatmapY);

            return base.CheckModelEquality(x, y);
        }

        protected override Drawable GetDrawableForDisplay(CarouselItem item)
        {
            switch (item.Model)
            {
                case StarDifficultyGroupDefinition:
                    return starsGroupPanelPool.Get();

                case GroupDefinition:
                    return groupPanelPool.Get();

                case BeatmapInfo:
                    if (!grouping.BeatmapSetsGroupedTogether)
                        return standalonePanelPool.Get();

                    return beatmapPanelPool.Get();

                case BeatmapSetInfo:
                    return setPanelPool.Get();
            }

            throw new InvalidOperationException();
        }

        #endregion
    }

    /// <summary>
    /// Defines a grouping header for a set of carousel items.
    /// </summary>
    /// <param name="Order">The order of this group in the carousel, sorted using ascending order.</param>
    /// <param name="Title">The title of this group.</param>
    public record GroupDefinition(int Order, string Title);

    /// <summary>
    /// Defines a grouping header for a set of carousel items grouped by star difficulty.
    /// </summary>
    public record StarDifficultyGroupDefinition(int Order, string Title, StarDifficulty Difficulty) : GroupDefinition(Order, Title);
}
