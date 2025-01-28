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
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Select;

namespace osu.Game.Screens.SelectV2
{
    [Cached]
    public partial class BeatmapCarousel : Carousel<BeatmapInfo>
    {
        private IBindableList<BeatmapSetInfo> detachedBeatmaps = null!;

        private readonly LoadingLayer loading;

        private readonly BeatmapCarouselFilterGrouping grouping;

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

        protected override bool HandleItemSelected(object? model)
        {
            base.HandleItemSelected(model);

            switch (model)
            {
                case GroupDefinition group:
                    if (lastSelectedGroup != null)
                        setVisibilityOfGroupItems(lastSelectedGroup, false);

                    // Collapsing an open group.
                    if (lastSelectedGroup == group)
                    {
                        lastSelectedGroup = null;
                        return false;
                    }

                    lastSelectedGroup = group;

                    setVisibilityOfGroupItems(group, true);

                    // In stable, you can kinda select a group (expand without changing selection)
                    // For simplicity, let's not do that for now and handle similar to a beatmap set header.
                    CurrentSelection = grouping.GroupItems[group].First().Model;
                    return false;

                case BeatmapSetInfo setInfo:
                    // Selecting a set isn't valid – let's re-select the first difficulty.
                    CurrentSelection = setInfo.Beatmaps.First();
                    return false;

                case BeatmapInfo beatmapInfo:
                    if (lastSelectedBeatmap != null)
                        setVisibilityOfSetItems(lastSelectedBeatmap.BeatmapSet!, false);
                    lastSelectedBeatmap = beatmapInfo;

                    // If we have groups, we need to account for them.
                    if (grouping.GroupItems.Count > 0)
                    {
                        // Find the containing group. There should never be too many groups so iterating is efficient enough.
                        var group = grouping.GroupItems.Single(kvp => kvp.Value.Any(i => ReferenceEquals(i.Model, beatmapInfo))).Key;
                        setVisibilityOfGroupItems(group, true);
                    }
                    else
                        setVisibilityOfSetItems(beatmapInfo.BeatmapSet!, true);

                    // Ensure the group containing this beatmap is also visible.
                    // TODO: need to update visibility of correct group?
                    return true;
            }

            return true;
        }

        private void setVisibilityOfGroupItems(GroupDefinition group, bool visible)
        {
            if (grouping.GroupItems.TryGetValue(group, out var items))
            {
                foreach (var i in items)
                    i.IsVisible = visible;
            }
        }

        private void setVisibilityOfSetItems(BeatmapSetInfo set, bool visible)
        {
            if (grouping.SetItems.TryGetValue(set, out var items))
            {
                foreach (var i in items)
                    i.IsVisible = visible;
            }
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

        private readonly DrawablePool<BeatmapPanel> beatmapPanelPool = new DrawablePool<BeatmapPanel>(100);
        private readonly DrawablePool<BeatmapSetPanel> setPanelPool = new DrawablePool<BeatmapSetPanel>(100);
        private readonly DrawablePool<GroupPanel> groupPanelPool = new DrawablePool<GroupPanel>(100);

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

    public record GroupDefinition(string Title);
}
