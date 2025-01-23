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

        protected override void HandleItemSelected(object? model)
        {
            base.HandleItemSelected(model);

            // Selecting a set isn't valid – let's re-select the first difficulty.
            if (model is BeatmapSetInfo setInfo)
            {
                CurrentSelection = setInfo.Beatmaps.First();
                return;
            }

            if (model is BeatmapInfo beatmapInfo)
                setVisibilityOfSetItems(beatmapInfo.BeatmapSet!, true);
        }

        protected override void HandleItemDeselected(object? model)
        {
            base.HandleItemDeselected(model);

            if (model is BeatmapInfo beatmapInfo)
                setVisibilityOfSetItems(beatmapInfo.BeatmapSet!, false);
        }

        private void setVisibilityOfSetItems(BeatmapSetInfo set, bool visible)
        {
            if (grouping.SetItems.TryGetValue(set, out var group))
            {
                foreach (var i in group)
                    i.IsVisible = visible;
            }
        }

        protected override void HandleItemActivated(CarouselItem item)
        {
            base.HandleItemActivated(item);

            // TODO: maybe this should be handled by the panel itself?
            if (GetMaterialisedDrawableForItem(item) is BeatmapCarouselPanel drawable)
                drawable.FlashFromActivation();
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

        private readonly DrawablePool<BeatmapCarouselPanel> carouselPanelPool = new DrawablePool<BeatmapCarouselPanel>(100);

        private void setupPools()
        {
            AddInternal(carouselPanelPool);
        }

        protected override Drawable GetDrawableForDisplay(CarouselItem item) => carouselPanelPool.Get();

        #endregion
    }
}
