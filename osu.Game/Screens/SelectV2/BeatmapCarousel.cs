// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Localisation;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Carousel;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;
using Realms;

namespace osu.Game.Screens.SelectV2
{
    [Cached]
    public partial class BeatmapCarousel : Carousel<BeatmapInfo>
    {
        public Action<BeatmapInfo>? RequestPresentBeatmap { private get; init; }

        /// <summary>
        /// From the provided beatmaps, select the most appropriate one for the user's skill.
        /// </summary>
        public required Action<IEnumerable<GroupedBeatmap>> RequestRecommendedSelection { private get; init; }

        /// <summary>
        /// Selection requested for the provided beatmap.
        /// </summary>
        public required Action<GroupedBeatmap> RequestSelection { private get; init; }

        public const float SPACING = 3f;

        private IBindableList<BeatmapSetInfo> detachedBeatmaps = null!;

        private readonly LoadingLayer loading;

        private readonly BeatmapCarouselFilterGrouping grouping;

        /// <summary>
        /// Total number of beatmap difficulties displayed with the filter.
        /// </summary>
        public int MatchedBeatmapsCount => Filters.Last().BeatmapItemsCount;

        protected override float GetSpacingBetweenPanels(CarouselItem top, CarouselItem bottom)
        {
            // Group panels do not overlap with any other panel but should overlap with themselves.
            if ((top.Model is GroupDefinition) ^ (bottom.Model is GroupDefinition))
                return SPACING * 2;

            if (grouping.BeatmapSetsGroupedTogether)
            {
                // Give some space around the expanded beatmap set, at the top..
                if (bottom.Model is GroupedBeatmapSet && bottom.IsExpanded)
                    return SPACING * 2;

                // ..and the bottom.
                if (top.Model is GroupedBeatmap && bottom.Model is GroupedBeatmapSet)
                    return SPACING * 2;

                // Beatmap difficulty panels do not overlap with themselves or any other panel.
                if (top.Model is GroupedBeatmap || bottom.Model is GroupedBeatmap)
                    return SPACING;
            }
            else
            {
                if (CurrentSelection != null && (top == CurrentSelectionItem || bottom == CurrentSelectionItem))
                    return SPACING * 2;
            }

            return -SPACING;
        }

        public BeatmapCarousel()
        {
            DebounceDelay = 100;
            DistanceOffscreenToPreload = 100;

            // Account for the osu! logo being in the way.
            Scroll.ScrollbarPaddingBottom = 70;

            Filters = new ICarouselFilter[]
            {
                new BeatmapCarouselFilterMatching(() => Criteria!),
                new BeatmapCarouselFilterSorting(() => Criteria!),
                grouping = new BeatmapCarouselFilterGrouping
                {
                    GetCriteria = () => Criteria!,
                    GetCollections = GetAllCollections,
                    GetLocalUserTopRanks = GetBeatmapInfoGuidToTopRankMapping,
                    GetFavouriteBeatmapSets = GetFavouriteBeatmapSets,
                }
            };

            AddInternal(loading = new LoadingLayer());
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapStore beatmapStore, AudioManager audio, OsuConfigManager config, CancellationToken? cancellationToken)
        {
            setupPools();
            detachedBeatmaps = beatmapStore.GetBeatmapSets(cancellationToken);
            loadSamples(audio);

            config.BindWith(OsuSetting.RandomSelectAlgorithm, randomAlgorithm);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            detachedBeatmaps.BindCollectionChanged(beatmapSetsChanged, true);
        }

        #region Beatmap source hookup

        private void beatmapSetsChanged(object? beatmaps, NotifyCollectionChangedEventArgs changed) => Schedule(() =>
        {
            // This callback is scheduled to ensure there's no added overhead during gameplay.
            // If this ever becomes an issue, it's important to note that the actual carousel filtering is already
            // implemented in a way it will only run when at song select.
            //
            // The overhead we are avoiding here is that of this method directly – things like Items.IndexOf calls
            // that can be slow for very large beatmap libraries. There are definitely ways to optimise this further.

            // TODO: moving management of BeatmapInfo tracking to BeatmapStore might be something we want to consider.
            // right now we are managing this locally which is a bit of added overhead.
            IEnumerable<BeatmapSetInfo>? newItems = changed.NewItems?.Cast<BeatmapSetInfo>();
            IEnumerable<BeatmapSetInfo>? oldItems = changed.OldItems?.Cast<BeatmapSetInfo>();

            switch (changed.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (!newItems!.Any())
                        return;

                    Items.AddRange(newItems!.SelectMany(s => s.Beatmaps));
                    break;

                case NotifyCollectionChangedAction.Remove:
                    bool selectedSetDeleted = false;

                    foreach (var set in oldItems!)
                    {
                        foreach (var beatmap in set.Beatmaps)
                        {
                            Items.RemoveAll(i => i is BeatmapInfo bi && beatmap.Equals(bi));
                            selectedSetDeleted |= CheckModelEquality((CurrentSelection as GroupedBeatmap)?.Beatmap, beatmap);
                        }
                    }

                    // After removing all items in this batch, we want to make an immediate reselection
                    // based on adjacency to the previous selection if it was deleted.
                    //
                    // This needs to be done immediately to avoid song select making a random selection.
                    // This needs to be done in this class because we need to know final display order.
                    // This needs to be done with attention to detail of which beatmaps have not been deleted.
                    if (selectedSetDeleted && CurrentSelectionIndex != null)
                    {
                        var items = GetCarouselItems()!;
                        if (items.Count == 0)
                            break;

                        bool success = false;

                        // Try selecting forwards first
                        for (int i = CurrentSelectionIndex.Value + 1; i < items.Count; i++)
                        {
                            if (attemptSelection(items[i]))
                            {
                                success = true;
                                break;
                            }
                        }

                        if (success)
                            break;

                        // Then try backwards (we might be at the end of available items).
                        for (int i = Math.Min(items.Count - 1, CurrentSelectionIndex.Value); i >= 0; i--)
                        {
                            if (attemptSelection(items[i]))
                                break;
                        }

                        bool attemptSelection(CarouselItem item)
                        {
                            if (CheckValidForSetSelection(item))
                            {
                                if (item.Model is GroupedBeatmap groupedBeatmap)
                                {
                                    // check the new selection wasn't deleted above
                                    if (!Items.Contains(groupedBeatmap.Beatmap))
                                        return false;

                                    RequestSelection(groupedBeatmap);
                                    return true;
                                }

                                if (item.Model is GroupedBeatmapSet groupedSet)
                                {
                                    if (oldItems.Contains(groupedSet.BeatmapSet))
                                        return false;

                                    selectRecommendedDifficultyForBeatmapSet(groupedSet);
                                    return true;
                                }
                            }

                            return false;
                        }
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

                        // we're intentionally being lenient with there being two difficulties with equal online ID or difficulty name.
                        // this can be the case when the user modifies the beatmap using the editor's "external edit" feature.
                        BeatmapInfo? matchingNewBeatmap =
                            newSetBeatmaps.FirstOrDefault(b => b.OnlineID > 0 && b.OnlineID == beatmap.OnlineID) ??
                            newSetBeatmaps.FirstOrDefault(b => b.DifficultyName == beatmap.DifficultyName && b.Ruleset.Equals(beatmap.Ruleset));

                        // The matching beatmap may have been deleted or invalidated in some way since this event was fired.
                        // Let's make sure we have the most up-to-date realm state.
                        if (matchingNewBeatmap?.ID is Guid matchingID)
                            matchingNewBeatmap = realm.Run(r => r.FindWithRefresh<BeatmapInfo>(matchingID)?.Detach());

                        if (matchingNewBeatmap != null)
                        {
                            // TODO: should this exist in song select instead of here?
                            // we need to ensure the global beatmap is also updated alongside changes.
                            if (CurrentBeatmap != null && beatmap.Equals(CurrentBeatmap))
                                // we don't know in which group the matching new beatmap is, but that's fine - we can keep the previous one for now.
                                // we are about to modify `Items`, which - if required - will trigger a re-filter,
                                // which will pick a correct group - if one is present - via `HandleFilterCompleted()`.
                                RequestSelection(new GroupedBeatmap(CurrentGroupedBeatmap?.Group, matchingNewBeatmap));

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
        });

        #endregion

        #region Selection handling

        protected GroupDefinition? ExpandedGroup { get; private set; }

        protected GroupedBeatmapSet? ExpandedBeatmapSet { get; private set; }

        protected override bool ShouldActivateOnKeyboardSelection(CarouselItem item) =>
            grouping.BeatmapSetsGroupedTogether && item.Model is GroupedBeatmap;

        /// <summary>
        /// The currently selected <see cref="GroupedBeatmap"/>.
        /// </summary>
        /// <remarks>
        /// The selection is never reset due to not existing. It can be set to anything.
        /// If no matching carousel item exists, there will be no visually selected item while waiting for potential new item which matches.
        /// </remarks>
        public GroupedBeatmap? CurrentGroupedBeatmap
        {
            get => CurrentSelection as GroupedBeatmap;
            set => CurrentSelection = value;
        }

        /// <summary>
        /// The currently selected <see cref="BeatmapInfo"/>.
        /// </summary>
        /// <remarks>
        /// This is a property mostly dedicated to external consumers who only care about showing some particular copy of a beatmap
        /// (there could be multiple panels for one beatmap due to grouping).
        /// Through this property, the carousel basically figures out what group to use internally.
        /// </remarks>
        public BeatmapInfo? CurrentBeatmap
        {
            get => CurrentGroupedBeatmap?.Beatmap;
            set
            {
                if (value == null)
                {
                    CurrentGroupedBeatmap = null;
                    return;
                }

                if (CurrentGroupedBeatmap != null && value.Equals(CurrentGroupedBeatmap.Beatmap))
                    return;

                // it is not universally guaranteed that the carousel items will be materialised at the time this is set.
                // therefore, in cases where it is known that they will not be, default to a null group.
                // even if grouping is active, this will be rectified to a correct group on the next invocation of `HandleFilterCompleted()`.
                CurrentGroupedBeatmap = IsLoaded && !IsFiltering
                    ? GetCarouselItems()?.Select(item => item.Model).OfType<GroupedBeatmap>().FirstOrDefault(gb => gb.Beatmap.Equals(value))
                    : new GroupedBeatmap(null, value);
            }
        }

        /// <summary>
        /// Tracks whether the user has manually requested to collapse an open group.
        /// In this case, refilters should not forcibly expand groups until the user expands a group again themselves.
        /// </summary>
        private bool userCollapsedGroup;

        protected override void HandleItemActivated(CarouselItem item)
        {
            try
            {
                switch (item.Model)
                {
                    case GroupDefinition group:
                        // Special case – collapsing an open group.
                        if (ExpandedGroup == group)
                        {
                            setExpansionStateOfGroup(ExpandedGroup, false);
                            ExpandedGroup = null;
                            userCollapsedGroup = true;
                            return;
                        }

                        setExpandedGroup(group);

                        if (userCollapsedGroup)
                        {
                            if (grouping.BeatmapSetsGroupedTogether && CurrentGroupedBeatmap != null && CheckModelEquality(group, CurrentGroupedBeatmap.Group))
                                setExpandedSet(new GroupedBeatmapSet(CurrentGroupedBeatmap.Group, CurrentGroupedBeatmap.Beatmap.BeatmapSet!));
                            userCollapsedGroup = false;
                        }

                        // If the active selection is within this group, it should get keyboard focus immediately.
                        if (CurrentSelectionItem?.IsVisible == true && CurrentSelection is GroupedBeatmap gb)
                            RequestSelection(gb);

                        return;

                    case GroupedBeatmapSet groupedSet:
                        selectRecommendedDifficultyForBeatmapSet(groupedSet);
                        return;

                    case GroupedBeatmap groupedBeatmap:
                        if (CurrentSelection != null && CheckModelEquality(CurrentSelection, groupedBeatmap))
                        {
                            RequestPresentBeatmap?.Invoke(groupedBeatmap.Beatmap);
                            return;
                        }

                        RequestSelection(groupedBeatmap);
                        return;
                }
            }
            finally
            {
                playActivationSound(item);
            }
        }

        protected override void HandleItemSelected(object? model)
        {
            base.HandleItemSelected(model);

            switch (model)
            {
                case GroupedBeatmapSet:
                case GroupDefinition:
                    throw new InvalidOperationException("Groups should never become selected");

                case GroupedBeatmap groupedBeatmap:
                    if (userCollapsedGroup)
                        break;

                    setExpandedGroup(groupedBeatmap.Group);

                    if (grouping.BeatmapSetsGroupedTogether)
                        setExpandedSet(new GroupedBeatmapSet(groupedBeatmap.Group, groupedBeatmap.Beatmap.BeatmapSet!));
                    break;
            }
        }

        protected override bool HandleItemsChanged(NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Reset:
                    return true;

                case NotifyCollectionChangedAction.Replace:
                    var oldBeatmaps = args.OldItems!.OfType<BeatmapInfo>().ToList();
                    var newBeatmaps = args.NewItems!.OfType<BeatmapInfo>().ToList();

                    for (int i = 0; i < oldBeatmaps.Count; i++)
                    {
                        var oldBeatmap = oldBeatmaps[i];
                        var newBeatmap = newBeatmaps[i];

                        // Ignore changes which don't concern us.
                        //
                        // Here are some examples of things that can go wrong:
                        // - Background difficulty calculation runs and causes a realm update.
                        //   We use `BeatmapDifficultyCache` and don't want to know about these.
                        // - Background user tag population runs and causes a realm update.
                        //   We don't display user tags so want to ignore this.
                        bool equalForDisplayPurposes =
                            // covers metadata changes
                            oldBeatmap.Hash == newBeatmap.Hash &&
                            // sanity check
                            oldBeatmap.OnlineID == newBeatmap.OnlineID &&
                            // displayed on panel
                            oldBeatmap.Status == newBeatmap.Status &&
                            // displayed on panel
                            oldBeatmap.DifficultyName == newBeatmap.DifficultyName &&
                            // hidden changed, needs re-filter
                            oldBeatmap.Hidden == newBeatmap.Hidden &&
                            // might be used for grouping, returning from gameplay
                            oldBeatmap.LastPlayed == newBeatmap.LastPlayed;

                        if (equalForDisplayPurposes)
                            return false;
                    }

                    return true;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void HandleFilterCompleted()
        {
            base.HandleFilterCompleted();

            attemptSelectSingleFilteredResult();

            // Store selected group before handling selection (it may implicitly change the expanded group).
            var groupForReselection = ExpandedGroup;

            var currentGroupedBeatmap = CurrentSelection as GroupedBeatmap;

            // The filter might have changed the set of available groups, which means that the current selection may point to a stale group.
            // Check whether that is the case.
            bool groupingRemainsOff = currentGroupedBeatmap?.Group == null && grouping.GroupItems.Count == 0;
            bool groupStillExists = currentGroupedBeatmap?.Group != null && grouping.GroupItems.ContainsKey(currentGroupedBeatmap.Group);

            if (groupingRemainsOff || groupStillExists)
            {
                // Only update the visual state of the selected item.
                HandleItemSelected(currentGroupedBeatmap);
            }
            else if (currentGroupedBeatmap != null)
            {
                // If the group no longer exists, grab an arbitrary other instance of the beatmap under the first group encountered.
                var newSelection = GetCarouselItems()?.Select(i => i.Model).OfType<GroupedBeatmap>().FirstOrDefault(gb => gb.Beatmap.Equals(currentGroupedBeatmap.Beatmap));
                // Only change the selection if we actually got a positive hit.
                // This is necessary so that selection isn't lost if the panel reappears later due to e.g. unapplying some filter criteria that made it disappear in the first place.
                if (newSelection != null)
                    CurrentSelection = newSelection;
            }

            // If a group was selected that is not the one containing the selection, attempt to reselect it.
            // If the original group was not found, ExpandedGroup will already have been updated to a valid value in `HandleItemSelected` above.
            if (groupForReselection != null && grouping.GroupItems.TryGetValue(groupForReselection, out _))
                setExpandedGroup(groupForReselection);
        }

        private void selectRecommendedDifficultyForBeatmapSet(GroupedBeatmapSet set)
        {
            // Selecting a set isn't valid – let's re-select the first visible difficulty.
            if (grouping.SetItems.TryGetValue(set, out var items))
            {
                var beatmaps = items.Select(i => i.Model).OfType<GroupedBeatmap>();
                RequestRecommendedSelection(beatmaps);
            }
        }

        /// <summary>
        /// If we don't have a selection and there's a single beatmap set returned, select it for the user.
        /// </summary>
        private void attemptSelectSingleFilteredResult()
        {
            var items = GetCarouselItems();

            if (items == null || items.Count == 0) return;

            BeatmapSetInfo? beatmapSetInfo = null;

            foreach (var item in items)
            {
                if (item.Model is GroupedBeatmap groupedBeatmap)
                {
                    var beatmapInfo = groupedBeatmap.Beatmap;

                    if (beatmapSetInfo == null)
                    {
                        beatmapSetInfo = beatmapInfo.BeatmapSet!;
                        continue;
                    }

                    // Found a beatmap with a different beatmap set, abort.
                    if (!beatmapSetInfo.Equals(beatmapInfo.BeatmapSet))
                        return;
                }
            }

            var beatmaps = items.Select(i => i.Model).OfType<GroupedBeatmap>();

            // do not request recommended selection if the user already had selected a difficulty within the single filtered beatmap set,
            // as it could change the difficulty that will be selected
            var preexistingSelection = beatmaps.FirstOrDefault(b => b.Equals(CurrentSelection as GroupedBeatmap));

            if (preexistingSelection != null)
            {
                // the selection might not have an item associated with it, if it was fully filtered away previously
                // in this case, request to reselect it
                if (CurrentSelectionItem == null)
                    RequestSelection(preexistingSelection);

                return;
            }

            RequestRecommendedSelection(beatmaps);
        }

        protected override bool CheckValidForGroupSelection(CarouselItem item) => item.Model is GroupDefinition;

        protected override bool CheckValidForSetSelection(CarouselItem item)
        {
            switch (item.Model)
            {
                case GroupedBeatmapSet:
                    return true;

                case GroupedBeatmap:
                    return !grouping.BeatmapSetsGroupedTogether;

                case GroupDefinition:
                    return false;

                default:
                    throw new ArgumentException($"Unsupported model type {item.Model}");
            }
        }

        private void setExpandedGroup(GroupDefinition? group)
        {
            if (ExpandedGroup != null)
                setExpansionStateOfGroup(ExpandedGroup, false);

            ExpandedGroup = group;

            if (ExpandedGroup != null)
                setExpansionStateOfGroup(ExpandedGroup, true);
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

                            case GroupedBeatmapSet groupedSet:
                                // Case where there are set headers, header should be visible
                                // and items should use the set's expanded state.
                                i.IsVisible = true;
                                setExpansionStateOfSetItems(groupedSet, i.IsExpanded);
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

        private void setExpandedSet(GroupedBeatmapSet set)
        {
            if (ExpandedBeatmapSet != null)
                setExpansionStateOfSetItems(ExpandedBeatmapSet, false);
            ExpandedBeatmapSet = set;
            setExpansionStateOfSetItems(ExpandedBeatmapSet, true);
        }

        private void setExpansionStateOfSetItems(GroupedBeatmapSet set, bool expanded)
        {
            if (grouping.SetItems.TryGetValue(set, out var items))
            {
                foreach (var i in items)
                {
                    if (i.Model is GroupedBeatmapSet)
                        i.IsExpanded = expanded;
                    else
                        i.IsVisible = expanded;
                }
            }
        }

        public void ExpandGroupForCurrentSelection()
        {
            if (CurrentGroupedBeatmap?.Group == null)
                return;

            if (CheckModelEquality(ExpandedGroup, CurrentGroupedBeatmap.Group))
                return;

            var groupItem = GetCarouselItems()?.FirstOrDefault(i => CheckModelEquality(i.Model, CurrentGroupedBeatmap.Group));
            if (groupItem != null)
                HandleItemActivated(groupItem);
        }

        protected override double? GetScrollTarget()
        {
            double? target = base.GetScrollTarget();

            // if the base implementation returned null, it means that the keyboard selection has been filtered out and is no longer visible
            // attempt a fallback to other possibly expanded panels (set first, then group)
            if (target == null)
            {
                var items = GetCarouselItems();
                var targetItem = items?.FirstOrDefault(i => CheckModelEquality(i.Model, ExpandedBeatmapSet))
                                 ?? items?.FirstOrDefault(i => CheckModelEquality(i.Model, ExpandedGroup));

                target = targetItem?.CarouselYPosition;
            }

            return target;
        }

        #endregion

        #region Audio

        private Sample? sampleChangeDifficulty;
        private Sample? sampleChangeSet;
        private Sample? sampleToggleGroup;

        private double audioFeedbackLastPlaybackTime;

        private void loadSamples(AudioManager audio)
        {
            sampleChangeDifficulty = audio.Samples.Get(@"SongSelect/select-difficulty");
            sampleChangeSet = audio.Samples.Get(@"SongSelect/select-expand");
            sampleToggleGroup = audio.Samples.Get(@"SongSelect/select-group");

            spinSample = audio.Samples.Get("SongSelect/random-spin");
            randomSelectSample = audio.Samples.Get(@"SongSelect/select-random");
        }

        private void playActivationSound(CarouselItem item)
        {
            if (Time.Current - audioFeedbackLastPlaybackTime >= OsuGameBase.SAMPLE_DEBOUNCE_TIME)
            {
                switch (item.Model)
                {
                    case GroupDefinition:
                        sampleToggleGroup?.Play();
                        return;

                    case GroupedBeatmapSet:
                        sampleChangeSet?.Play();
                        return;

                    case GroupedBeatmap:
                        sampleChangeDifficulty?.Play();
                        return;
                }

                audioFeedbackLastPlaybackTime = Time.Current;
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

        public FilterCriteria? Criteria { get; private set; }

        private ScheduledDelegate? loadingDebounce;

        public void Filter(FilterCriteria criteria, bool showLoadingImmediately = false)
        {
            bool resetDisplay = grouping.BeatmapSetsGroupedTogether != BeatmapCarouselFilterGrouping.ShouldGroupBeatmapsTogether(criteria);

            Criteria = criteria;

            if (criteria.Group == GroupMode.None)
                userCollapsedGroup = false;

            loadingDebounce ??= Scheduler.AddDelayed(() =>
            {
                if (loading.State.Value == Visibility.Visible)
                    return;

                Scroll.FadeColour(OsuColour.Gray(0.5f), 1000, Easing.OutQuint);
                loading.Show();
            }, showLoadingImmediately ? 0 : 250);

            FilterAsync(resetDisplay).ContinueWith(_ => Schedule(() =>
            {
                loadingDebounce?.Cancel();
                loadingDebounce = null;

                Scroll.FadeColour(OsuColour.Gray(1f), 500, Easing.OutQuint);
                loading.Hide();
            }));
        }

        protected override Task<IEnumerable<CarouselItem>> FilterAsync(bool clearExistingPanels = false)
        {
            if (Criteria == null)
                return Task.FromResult(Enumerable.Empty<CarouselItem>());

            return base.FilterAsync(clearExistingPanels);
        }

        #endregion

        #region Fetches for grouping support

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        /// <remarks>
        /// FOOTGUN WARNING: this being sorted on the realm side before detaching is IMPORTANT.
        /// realm supports sorting as an internal operation, and realm's implementation of string sorting does NOT match dotnet's
        /// with respect to treatment of punctuation characters like <c>-</c> or <c>_</c>, among others.
        /// All other places that show lists of collections also use the realm-side sorting implementation,
        /// because they use the sorting operation inside subscription queries for efficient drawable management,
        /// so this usage kind of has to follow suit.
        /// </remarks>
        protected virtual List<BeatmapCollection> GetAllCollections() => realm.Run(r => r.All<BeatmapCollection>().OrderBy(c => c.Name).AsEnumerable().Detach());

        protected virtual Dictionary<Guid, ScoreRank> GetBeatmapInfoGuidToTopRankMapping(FilterCriteria criteria) => realm.Run(r =>
        {
            var topRankMapping = new Dictionary<Guid, ScoreRank>();

            var allLocalScores = r.GetAllLocalScoresForUser(criteria.LocalUserId)
                                  .Filter($@"{nameof(ScoreInfo.Ruleset)}.{nameof(RulesetInfo.ShortName)} == $0", criteria.Ruleset?.ShortName)
                                  .OrderByDescending(s => s.TotalScore)
                                  .ThenBy(s => s.Date);

            foreach (var score in allLocalScores)
            {
                Debug.Assert(score.BeatmapInfo != null);

                if (topRankMapping.ContainsKey(score.BeatmapInfo.ID))
                    continue;

                topRankMapping[score.BeatmapInfo.ID] = score.Rank;
            }

            return topRankMapping;
        });

        /// <remarks>
        /// Note that calling <c>.ToHashSet()</c> below has two purposes:
        /// one being performance of contain checks in filtering code,
        /// another being slightly better thread safety (as <see cref="ILocalUserState.FavouriteBeatmapSets"/> could be mutated during async filtering).
        /// </remarks>
        protected HashSet<int> GetFavouriteBeatmapSets() => api.LocalUserState.FavouriteBeatmapSets.ToHashSet();

        #endregion

        #region Drawable pooling

        private readonly DrawablePool<PanelBeatmap> beatmapPanelPool = new DrawablePool<PanelBeatmap>(100);
        private readonly DrawablePool<PanelBeatmapStandalone> standalonePanelPool = new DrawablePool<PanelBeatmapStandalone>(100);
        private readonly DrawablePool<PanelBeatmapSet> setPanelPool = new DrawablePool<PanelBeatmapSet>(100);
        private readonly DrawablePool<PanelGroup> groupPanelPool = new DrawablePool<PanelGroup>(100);
        private readonly DrawablePool<PanelGroupStarDifficulty> starsGroupPanelPool = new DrawablePool<PanelGroupStarDifficulty>(11);
        private readonly DrawablePool<PanelGroupRankDisplay> ranksGroupPanelPool = new DrawablePool<PanelGroupRankDisplay>(9);
        private readonly DrawablePool<PanelGroupRankedStatus> statusGroupPanelPool = new DrawablePool<PanelGroupRankedStatus>(8);

        private void setupPools()
        {
            AddInternal(statusGroupPanelPool);
            AddInternal(ranksGroupPanelPool);
            AddInternal(starsGroupPanelPool);
            AddInternal(groupPanelPool);
            AddInternal(beatmapPanelPool);
            AddInternal(standalonePanelPool);
            AddInternal(setPanelPool);
        }

        protected override bool CheckModelEquality(object? x, object? y)
        {
            // In the confines of the carousel logic, we assume that CurrentSelection (and all items) are using non-stale
            // BeatmapInfo reference, and that we can match based on beatmap / beatmapset (GU)IDs.
            //
            // If there's a case where updates don't come in as expected, diagnosis should start from BeatmapStore, ensuring
            // it is doing a Replace operation on the list. If it is, then check the local handling in beatmapSetsChanged
            // before changing matching requirements here.

            if (x is GroupedBeatmapSet groupedSetX && y is GroupedBeatmapSet groupedSetY)
                return groupedSetX.Equals(groupedSetY);

            if (x is GroupedBeatmap groupedBeatmapX && y is GroupedBeatmap groupedBeatmapY)
                return groupedBeatmapX.Equals(groupedBeatmapY);

            // `BeatmapInfo` is no longer used directly in carousel items, but in rare circumstances still is used for model equality comparisons
            // (see `beatmapSetsChanged()` deletion handling logic, which aims to find a beatmap close to the just-deleted one, disregarding grouping concerns)
            if (x is BeatmapInfo beatmapInfoX && y is BeatmapInfo beatmapInfoY)
                return beatmapInfoX.Equals(beatmapInfoY);

            if (x is GroupDefinition groupX && y is GroupDefinition groupY)
                return groupX.Equals(groupY);

            if (x is StarDifficultyGroupDefinition starX && y is StarDifficultyGroupDefinition starY)
                return starX.Equals(starY);

            if (x is RankDisplayGroupDefinition rankX && y is RankDisplayGroupDefinition rankY)
                return rankX.Equals(rankY);

            if (x is RankedStatusGroupDefinition statusX && y is RankedStatusGroupDefinition statusY)
                return statusX.Equals(statusY);

            return base.CheckModelEquality(x, y);
        }

        protected override Drawable GetDrawableForDisplay(CarouselItem item)
        {
            switch (item.Model)
            {
                case RankedStatusGroupDefinition:
                    return statusGroupPanelPool.Get();

                case StarDifficultyGroupDefinition:
                    return starsGroupPanelPool.Get();

                case RankDisplayGroupDefinition:
                    return ranksGroupPanelPool.Get();

                case GroupDefinition:
                    return groupPanelPool.Get();

                case GroupedBeatmap:
                    if (!grouping.BeatmapSetsGroupedTogether)
                        return standalonePanelPool.Get();

                    return beatmapPanelPool.Get();

                case GroupedBeatmapSet:
                    return setPanelPool.Get();
            }

            throw new InvalidOperationException();
        }

        #endregion

        #region Random selection handling

        private readonly Bindable<RandomSelectAlgorithm> randomAlgorithm = new Bindable<RandomSelectAlgorithm>();
        private readonly HashSet<BeatmapInfo> previouslyVisitedRandomBeatmaps = new HashSet<BeatmapInfo>();
        private readonly List<GroupedBeatmap> randomHistory = new List<GroupedBeatmap>();

        private Sample? spinSample;
        private Sample? randomSelectSample;

        public bool NextRandom()
        {
            var carouselItems = GetCarouselItems();

            if (carouselItems?.Any() != true)
                return false;

            var selectionBefore = CurrentSelectionItem;
            var beatmapBefore = selectionBefore?.Model as GroupedBeatmap;

            bool success;

            if (beatmapBefore != null)
            {
                // keep track of visited beatmaps and sets for rewind
                randomHistory.Add(beatmapBefore);
                // keep track of visited beatmaps for "RandomPermutation" random tracking.
                // note that this is reset when we run out of beatmaps, while `randomHistory` is not.
                previouslyVisitedRandomBeatmaps.Add(beatmapBefore.Beatmap);
            }

            if (grouping.BeatmapSetsGroupedTogether)
                success = nextRandomSet();
            else
                success = nextRandomBeatmap();

            if (!success)
            {
                if (beatmapBefore != null)
                    randomHistory.RemoveAt(randomHistory.Count - 1);
                return false;
            }

            // CurrentSelectionItem won't be valid until UpdateAfterChildren.
            // We probably want to fix this at some point since a few places are working-around this quirk.
            ScheduleAfterChildren(() =>
            {
                if (selectionBefore != null && CurrentSelectionItem != null)
                    playSpinSample(visiblePanelCountBetweenItems(selectionBefore, CurrentSelectionItem));
            });

            return true;
        }

        private bool nextRandomBeatmap()
        {
            ICollection<GroupedBeatmap> visibleBeatmaps = ExpandedGroup != null
                // In the case of grouping, users expect random to only operate on the expanded group.
                // This is going to incur some overhead as we don't have a group-beatmapset mapping currently.
                //
                // If this becomes an issue, we could either store a mapping, or run the random algorithm many times
                // using the `SetItems` method until we get a group HIT.
                ? grouping.GroupItems[ExpandedGroup].Select(i => i.Model).OfType<GroupedBeatmap>().ToArray()
                : GetCarouselItems()!.Select(i => i.Model).OfType<GroupedBeatmap>().ToArray();

            GroupedBeatmap beatmap;

            switch (randomAlgorithm.Value)
            {
                case RandomSelectAlgorithm.RandomPermutation:
                {
                    ICollection<GroupedBeatmap> notYetVisitedBeatmaps = visibleBeatmaps.ExceptBy(previouslyVisitedRandomBeatmaps, gb => gb.Beatmap).ToList();

                    if (!notYetVisitedBeatmaps.Any())
                    {
                        previouslyVisitedRandomBeatmaps.ExceptWith(visibleBeatmaps.Select(b => b.Beatmap));
                        notYetVisitedBeatmaps = visibleBeatmaps;
                        if (CurrentSelection is GroupedBeatmap groupedBeatmap)
                            notYetVisitedBeatmaps = notYetVisitedBeatmaps.Except([groupedBeatmap]).ToList();
                    }

                    if (notYetVisitedBeatmaps.Count == 0)
                        return false;

                    beatmap = notYetVisitedBeatmaps.ElementAt(RNG.Next(notYetVisitedBeatmaps.Count));
                    break;
                }

                case RandomSelectAlgorithm.Random:
                    beatmap = visibleBeatmaps.ElementAt(RNG.Next(visibleBeatmaps.Count));
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            RequestSelection(beatmap);
            return true;
        }

        private bool nextRandomSet()
        {
            ICollection<GroupedBeatmapSet> visibleGroupedSets = ExpandedGroup != null && grouping.GroupItems.TryGetValue(ExpandedGroup, out var groupItems)
                // In the case of grouping, users expect random to only operate on the expanded group.
                // This is going to incur some overhead as we don't have a group-beatmapset mapping currently.
                //
                // If this becomes an issue, we could either store a mapping, or run the random algorithm many times
                // using the `SetItems` method until we get a group HIT.
                ? groupItems.Select(i => i.Model).OfType<GroupedBeatmapSet>().ToArray()
                // This is the fastest way to retrieve sets for randomisation.
                : grouping.SetItems.Keys;

            GroupedBeatmapSet set;

            switch (randomAlgorithm.Value)
            {
                case RandomSelectAlgorithm.RandomPermutation:
                {
                    ICollection<GroupedBeatmapSet> notYetVisitedSets =
                        visibleGroupedSets.ExceptBy(previouslyVisitedRandomBeatmaps.Select(b => b.BeatmapSet!), groupedSet => groupedSet.BeatmapSet).ToList();

                    if (!notYetVisitedSets.Any())
                    {
                        previouslyVisitedRandomBeatmaps.ExceptWith(visibleGroupedSets.SelectMany(setUnderGrouping => setUnderGrouping.BeatmapSet.Beatmaps));
                        notYetVisitedSets = visibleGroupedSets;
                        if (CurrentSelection is GroupedBeatmap groupedBeatmap)
                            notYetVisitedSets = notYetVisitedSets.ExceptBy([groupedBeatmap.Beatmap.BeatmapSet!], groupedSet => groupedSet.BeatmapSet).ToList();
                    }

                    if (notYetVisitedSets.Count == 0)
                        return false;

                    set = notYetVisitedSets.ElementAt(RNG.Next(notYetVisitedSets.Count));
                    break;
                }

                case RandomSelectAlgorithm.Random:
                    set = visibleGroupedSets.ElementAt(RNG.Next(visibleGroupedSets.Count));
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            selectRecommendedDifficultyForBeatmapSet(set);
            return true;
        }

        public bool PreviousRandom()
        {
            var carouselItems = GetCarouselItems();

            if (carouselItems?.Any() != true)
                return false;

            while (randomHistory.Any())
            {
                var previousBeatmap = randomHistory[^1];
                randomHistory.RemoveAt(randomHistory.Count - 1);

                // when going back through rewind history, we may no longer be in the same grouping mode.
                // the user wants to go back to the beatmap first and foremost, so the most important thing is to find a panel that corresponds to the beatmap.
                // going back to the same group is a nice-to-have, but a secondary concern.
                var previousBeatmapItem = carouselItems.Where(i => i.Model is GroupedBeatmap gb && gb.Beatmap.Equals(previousBeatmap.Beatmap))
                                                       .MaxBy(i => ((GroupedBeatmap)i.Model).Group == previousBeatmap.Group);

                if (previousBeatmapItem == null)
                    return false;

                if (CurrentSelection is GroupedBeatmap groupedBeatmap)
                {
                    if (randomAlgorithm.Value == RandomSelectAlgorithm.RandomPermutation)
                        previouslyVisitedRandomBeatmaps.Remove(groupedBeatmap.Beatmap);

                    if (CurrentSelectionItem == null)
                        playSpinSample(0);
                    else
                        playSpinSample(visiblePanelCountBetweenItems(previousBeatmapItem, CurrentSelectionItem));
                }

                RequestSelection((GroupedBeatmap)previousBeatmapItem.Model);
                return true;
            }

            return false;
        }

        private double visiblePanelCountBetweenItems(CarouselItem item1, CarouselItem item2) => Math.Ceiling(Math.Abs(item1.CarouselYPosition - item2.CarouselYPosition) / PanelBeatmapSet.HEIGHT);

        private void playSpinSample(double distance)
        {
            var chan = spinSample?.GetChannel();

            if (chan != null)
            {
                chan.Frequency.Value = 1f + Math.Clamp(distance / 200, 0, 1);
                chan.Play();
            }

            randomSelectSample?.Play();
        }

        #endregion
    }

    /// <summary>
    /// Defines a grouping header for a set of carousel items.
    /// </summary>
    public record GroupDefinition
    {
        /// <summary>
        /// The order of this group in the carousel, sorted using ascending order.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// The title of this group.
        /// </summary>
        public LocalisableString Title { get; }

        private readonly string uncasedTitle;

        public GroupDefinition(int order, LocalisableString title)
        {
            Order = order;
            Title = title;
            uncasedTitle = title.ToLower().GetLocalised(LocalisationParameters.DEFAULT);
        }

        public virtual bool Equals(GroupDefinition? other) => uncasedTitle == other?.uncasedTitle;

        public override int GetHashCode() => HashCode.Combine(uncasedTitle);
    }

    /// <summary>
    /// Defines a grouping header for a set of carousel items grouped by star difficulty.
    /// </summary>
    public record StarDifficultyGroupDefinition(int Order, LocalisableString Title, StarDifficulty Difficulty) : GroupDefinition(Order, Title);

    /// <summary>
    /// Defines a grouping header for a set of carousel items grouped by achieved rank.
    /// </summary>
    public record RankDisplayGroupDefinition(ScoreRank Rank) : GroupDefinition(-(int)Rank, Rank.GetLocalisableDescription());

    /// <summary>
    /// Defines a grouping header for a set of carousel items grouped by ranked status.
    /// </summary>
    public record RankedStatusGroupDefinition(int Order, BeatmapOnlineStatus Status) : GroupDefinition(Order, Status.GetLocalisableDescription());

    /// <summary>
    /// Used to represent a portion of a <see cref="BeatmapSetInfo"/> under a <see cref="GroupDefinition"/>.
    /// The purpose of this model is to support splitting beatmap sets apart when the active grouping mode demands it.
    /// </summary>
    public record GroupedBeatmapSet([UsedImplicitly] GroupDefinition? Group, BeatmapSetInfo BeatmapSet);

    /// <summary>
    /// Used to represent a <see cref="Beatmap"/> under a <see cref="GroupDefinition"/>.
    /// The purpose of this model is to support showing multiple copies of a beatmap, which can occur if a beatmap appears in multiple groups
    /// (most prominently, collections group mode).
    /// </summary>
    public record GroupedBeatmap(GroupDefinition? Group, BeatmapInfo Beatmap);
}
