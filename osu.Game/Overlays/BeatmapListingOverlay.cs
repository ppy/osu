// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Threading;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.BeatmapListing;
using osu.Game.Overlays.Direct;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public class BeatmapListingOverlay : FullscreenOverlay
    {
        /// <summary>
        /// Scroll distance from bottom at which new beatmaps will be loaded, if possible.
        /// </summary>
        private const int pagination_scroll_distance = 500;

        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        private OverlayScrollContainer scroll;

        private BeatmapListingSearchSection searchSection;
        private BeatmapListingSortTabControl sortControl;
        private BeatmapListingResultsDisplay resultsDisplay;

        public BeatmapListingOverlay()
            : base(OverlayColourScheme.Blue)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourProvider.Background6
                },
                scroll = new OverlayScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollbarVisible = false,
                    Child = new ReverseChildIDFillFlowContainer<Drawable>
                    {
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 10),
                        Children = new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Y,
                                RelativeSizeAxes = Axes.X,
                                Direction = FillDirection.Vertical,
                                Masking = true,
                                EdgeEffect = new EdgeEffectParameters
                                {
                                    Colour = Color4.Black.Opacity(0.25f),
                                    Type = EdgeEffectType.Shadow,
                                    Radius = 3,
                                    Offset = new Vector2(0f, 1f),
                                },
                                Children = new Drawable[]
                                {
                                    new BeatmapListingHeader(),
                                    searchSection = new BeatmapListingSearchSection(),
                                }
                            },
                            new Container
                            {
                                AutoSizeAxes = Axes.Y,
                                RelativeSizeAxes = Axes.X,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = ColourProvider.Background4,
                                    },
                                    new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Children = new Drawable[]
                                        {
                                            new Container
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Height = 40,
                                                Children = new Drawable[]
                                                {
                                                    new Box
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                        Colour = ColourProvider.Background5
                                                    },
                                                    sortControl = new BeatmapListingSortTabControl
                                                    {
                                                        Anchor = Anchor.CentreLeft,
                                                        Origin = Anchor.CentreLeft,
                                                        Margin = new MarginPadding { Left = 20 }
                                                    }
                                                }
                                            },
                                            new Container
                                            {
                                                AutoSizeAxes = Axes.Y,
                                                RelativeSizeAxes = Axes.X,
                                                Padding = new MarginPadding { Horizontal = 20 },
                                                Children = new Drawable[]
                                                {
                                                    panelTarget = new Container
                                                    {
                                                        AutoSizeAxes = Axes.Y,
                                                        RelativeSizeAxes = Axes.X,
                                                        Child = resultsDisplay = new BeatmapListingResultsDisplay()
                                                    },
                                                    loadingLayer = new LoadingLayer(panelTarget),
                                                }
                                            },
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var sortCriteria = sortControl.Current;
            var sortDirection = sortControl.SortDirection;

            searchSection.Query.BindValueChanged(query =>
            {
                sortCriteria.Value = string.IsNullOrEmpty(query.NewValue) ? DirectSortCriteria.Ranked : DirectSortCriteria.Relevance;
                sortDirection.Value = SortDirection.Descending;

                queueUpdateSearch(true);
            });

            searchSection.Ruleset.BindValueChanged(_ => queueUpdateSearch());
            searchSection.Category.BindValueChanged(_ => queueUpdateSearch());
            sortCriteria.BindValueChanged(_ => queueUpdateSearch());
            sortDirection.BindValueChanged(_ => queueUpdateSearch());
        }

        private ScheduledDelegate queryChangedDebounce;
        private ScheduledDelegate addPageDebounce;

        private LoadingLayer loadingLayer;
        private Container panelTarget;

        [CanBeNull]
        private BeatmapSetPager beatmapSetPager;

        private bool shouldLoadNextPage => scroll.ScrollableExtent > 0 && scroll.IsScrolledToEnd(pagination_scroll_distance);

        private void queueUpdateSearch(bool queryTextChanged = false)
        {
            beatmapSetPager?.Reset();

            queryChangedDebounce?.Cancel();
            queryChangedDebounce = Scheduler.AddDelayed(updateSearch, queryTextChanged ? 500 : 100);
        }

        private void queueAddPage()
        {
            if (beatmapSetPager == null || !beatmapSetPager.CanFetchNextPage)
                return;

            if (addPageDebounce != null)
                return;

            beatmapSetPager.FetchNextPage();
        }

        private void updateSearch()
        {
            if (!IsLoaded)
                return;

            if (State.Value == Visibility.Hidden)
                return;

            if (API == null)
                return;

            previewTrackManager.StopAnyPlaying(this);

            loadingLayer.Show();

            beatmapSetPager?.Reset();
            beatmapSetPager = new BeatmapSetPager(
                API,
                rulesets,
                searchSection.Query.Value,
                searchSection.Ruleset.Value,
                searchSection.Category.Value,
                sortControl.Current.Value,
                sortControl.SortDirection.Value);

            beatmapSetPager.PageFetch += onPageFetch;

            addPageDebounce?.Cancel();
            addPageDebounce = null;

            queueAddPage();
        }

        private void onPageFetch(List<BeatmapSetInfo> beatmaps)
        {
            Schedule(() =>
            {
                loadingLayer.Hide();

                if (beatmapSetPager.IsPastFirstPage)
                {
                    resultsDisplay.AddBeatmaps(beatmaps);
                }
                else
                {
                    resultsDisplay.ReplaceBeatmaps(beatmaps);
                }

                addPageDebounce = Scheduler.AddDelayed(() => addPageDebounce = null, 1000);
            });
        }

        protected override void Update()
        {
            base.Update();

            if (shouldLoadNextPage)
                queueAddPage();
        }

        protected override void Dispose(bool isDisposing)
        {
            beatmapSetPager?.Reset();
            queryChangedDebounce?.Cancel();
            addPageDebounce?.Cancel();

            base.Dispose(isDisposing);
        }
    }
}
