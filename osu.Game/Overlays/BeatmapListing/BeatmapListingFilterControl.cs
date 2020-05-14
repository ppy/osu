// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapListing
{
    public class BeatmapListingFilterControl : CompositeDrawable
    {
        public Action<List<BeatmapSetInfo>> SearchFinished;
        public Action SearchStarted;
        private List<BeatmapSetInfo> currentBeatmaps;

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        private readonly BeatmapListingSearchControl searchControl;
        private readonly BeatmapListingSortTabControl sortControl;
        private readonly Box sortControlBackground;

        private BeatmapListingPager beatmapListingPager;

        private ScheduledDelegate queryChangedDebounce;
        private ScheduledDelegate queryPagingDebounce;

        public BeatmapListingFilterControl()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 10),
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Masking = true,
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Colour = Color4.Black.Opacity(0.25f),
                            Type = EdgeEffectType.Shadow,
                            Radius = 3,
                            Offset = new Vector2(0f, 1f),
                        },
                        Child = searchControl = new BeatmapListingSearchControl(),
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 40,
                        Children = new Drawable[]
                        {
                            sortControlBackground = new Box
                            {
                                RelativeSizeAxes = Axes.Both
                            },
                            sortControl = new BeatmapListingSortTabControl
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Margin = new MarginPadding { Left = 20 }
                            }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            sortControlBackground.Colour = colourProvider.Background5;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var sortCriteria = sortControl.Current;
            var sortDirection = sortControl.SortDirection;

            searchControl.Query.BindValueChanged(query =>
            {
                sortCriteria.Value = string.IsNullOrEmpty(query.NewValue) ? SortCriteria.Ranked : SortCriteria.Relevance;
                sortDirection.Value = SortDirection.Descending;
                queueUpdateSearch(true);
            });

            searchControl.Ruleset.BindValueChanged(_ => queueUpdateSearch());
            searchControl.Category.BindValueChanged(_ => queueUpdateSearch());
            searchControl.Genre.BindValueChanged(_ => queueUpdateSearch());
            searchControl.Language.BindValueChanged(_ => queueUpdateSearch());

            sortCriteria.BindValueChanged(_ => queueUpdateSearch());
            sortDirection.BindValueChanged(_ => queueUpdateSearch());
        }

        public void TakeFocus() => searchControl.TakeFocus();

        public void ShowMore()
        {
            if (beatmapListingPager == null || !beatmapListingPager.CanFetchNextPage)
                return;

            if (queryPagingDebounce != null)
                return;

            beatmapListingPager.FetchNextPage();
        }

        private void queueUpdateSearch(bool queryTextChanged = false)
        {
            SearchStarted?.Invoke();

            cancelSearch();

            queryChangedDebounce = Scheduler.AddDelayed(updateSearch, queryTextChanged ? 500 : 100);
        }

        private void updateSearch()
        {
            cancelSearch();

            beatmapListingPager = new BeatmapListingPager(
                api,
                rulesets,
                searchControl.Query.Value,
                searchControl.Ruleset.Value,
                searchControl.Category.Value,
                sortControl.Current.Value,
                sortControl.SortDirection.Value
            );

            beatmapListingPager.PageFetched += onSearchFinished;

            ShowMore();
        }

        private void cancelSearch()
        {
            beatmapListingPager?.Reset();
            queryChangedDebounce?.Cancel();

            queryPagingDebounce?.Cancel();
            queryPagingDebounce = null;
        }

        private void onSearchFinished(List<BeatmapSetInfo> beatmaps)
        {
            queryPagingDebounce = Scheduler.AddDelayed(() => queryPagingDebounce = null, 1000);

            if (currentBeatmaps == null || !beatmapListingPager.IsPastFirstPage)
                currentBeatmaps = beatmaps;
            else
                currentBeatmaps.AddRange(beatmaps);

            SearchFinished?.Invoke(beatmaps);
        }

        protected override void Dispose(bool isDisposing)
        {
            cancelSearch();

            base.Dispose(isDisposing);
        }
    }
}
