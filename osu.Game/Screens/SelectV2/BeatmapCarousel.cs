// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Select;

namespace osu.Game.Screens.SelectV2
{
    [Cached]
    public partial class BeatmapCarousel : Carousel<BeatmapInfo>
    {
        public Action<BeatmapInfo>? RequestPresentBeatmap { private get; init; }

        public const float SPACING = 5f;

        private IBindableList<BeatmapSetInfo> detachedBeatmaps = null!;

        private readonly LoadingLayer loading;

        private readonly BeatmapCarouselFilterGrouping grouping;

        protected override float GetSpacingBetweenPanels(CarouselItem top, CarouselItem bottom)
        {
            if (top.Model is BeatmapInfo || bottom.Model is BeatmapInfo)
                // Beatmap difficulty panels do not overlap with themselves or any other panel.
                return SPACING;

            return -SPACING;
        }

        public BeatmapCarousel()
        {
            DebounceDelay = 100;
            DistanceOffscreenToPreload = 100;

            Filters = new ICarouselFilter[]
            {
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
            IEnumerable<BeatmapSetInfo>? newBeatmapSets = changed.NewItems?.Cast<BeatmapSetInfo>();
            IEnumerable<BeatmapSetInfo>? beatmapSetInfos = changed.OldItems?.Cast<BeatmapSetInfo>();

            switch (changed.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Items.AddRange(newBeatmapSets!.SelectMany(s => s.Beatmaps));
                    break;

                case NotifyCollectionChangedAction.Remove:

                    foreach (var set in beatmapSetInfos!)
                    {
                        foreach (var beatmap in set.Beatmaps)
                            Items.RemoveAll(i => i is BeatmapInfo bi && beatmap.Equals(bi));
                    }

                    break;

                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();

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
                    // Selecting a set isn't valid – let's re-select the first difficulty.
                    CurrentSelection = setInfo.Beatmaps.First();
                    return;

                case BeatmapInfo beatmapInfo:
                    if (ReferenceEquals(CurrentSelection, beatmapInfo))
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
                    GroupDefinition? containingGroup = grouping.GroupItems.SingleOrDefault(kvp => kvp.Value.Any(i => ReferenceEquals(i.Model, beatmapInfo))).Key;

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

        public void Filter(FilterCriteria criteria)
        {
            Criteria = criteria;
            loading.Show();
            FilterAsync().ContinueWith(_ => Schedule(() => loading.Hide()));
        }

        #endregion

        #region Drawable pooling

        private readonly DrawablePool<PanelBeatmap> beatmapPanelPool = new DrawablePool<PanelBeatmap>(100);
        private readonly DrawablePool<PanelBeatmapSet> setPanelPool = new DrawablePool<PanelBeatmapSet>(100);
        private readonly DrawablePool<PanelGroup> groupPanelPool = new DrawablePool<PanelGroup>(100);

        private void setupPools()
        {
            AddInternal(groupPanelPool);
            AddInternal(beatmapPanelPool);
            AddInternal(setPanelPool);
        }

        protected override Drawable GetDrawableForDisplay(CarouselItem item)
        {
            switch (item.Model)
            {
                case GroupDefinition:
                    return groupPanelPool.Get();

                case BeatmapInfo:
                    // TODO: if beatmap is a group selection target, it needs to be a different drawable
                    // with more information attached.
                    return beatmapPanelPool.Get();

                case BeatmapSetInfo:
                    return setPanelPool.Get();
            }

            throw new InvalidOperationException();
        }

        #endregion
    }

    public record GroupDefinition(object Data, string Title);
}
