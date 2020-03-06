// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Direct;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapListing
{
    public class BeatmapListingSearchHandler : CompositeDrawable
    {
        public Action<List<BeatmapSetInfo>> SearchFinished;
        public Action SearchStarted;

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        private readonly BeatmapListingSearchSection searchSection;
        private readonly BeatmapListingSortTabControl sortControl;
        private readonly Box sortControlBackground;

        private SearchBeatmapSetsRequest getSetsRequest;

        public BeatmapListingSearchHandler()
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
                        Child = searchSection = new BeatmapListingSearchSection(),
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

            searchSection.Query.BindValueChanged(query =>
            {
                sortCriteria.Value = string.IsNullOrEmpty(query.NewValue) ? DirectSortCriteria.Ranked : DirectSortCriteria.Relevance;
                sortDirection.Value = SortDirection.Descending;
                queueUpdateSearch(true);
            });

            searchSection.Ruleset.BindValueChanged(_ => queueUpdateSearch());
            searchSection.Category.BindValueChanged(_ => queueUpdateSearch());
            searchSection.Genre.BindValueChanged(_ => queueUpdateSearch());
            searchSection.Language.BindValueChanged(_ => queueUpdateSearch());

            sortCriteria.BindValueChanged(_ => queueUpdateSearch());
            sortDirection.BindValueChanged(_ => queueUpdateSearch());
        }

        private ScheduledDelegate queryChangedDebounce;

        private void queueUpdateSearch(bool queryTextChanged = false)
        {
            SearchStarted?.Invoke();

            getSetsRequest?.Cancel();

            queryChangedDebounce?.Cancel();
            queryChangedDebounce = Scheduler.AddDelayed(updateSearch, queryTextChanged ? 500 : 100);
        }

        private void updateSearch()
        {
            getSetsRequest = new SearchBeatmapSetsRequest(searchSection.Query.Value, searchSection.Ruleset.Value)
            {
                SearchCategory = searchSection.Category.Value,
                SortCriteria = sortControl.Current.Value,
                SortDirection = sortControl.SortDirection.Value,
                Genre = searchSection.Genre.Value,
                Language = searchSection.Language.Value
            };

            getSetsRequest.Success += response => Schedule(() => onSearchFinished(response));

            api.Queue(getSetsRequest);
        }

        private void onSearchFinished(SearchBeatmapSetsResponse response)
        {
            var beatmaps = response.BeatmapSets.Select(r => r.ToBeatmapSet(rulesets)).ToList();

            searchSection.BeatmapSet = response.Total == 0 ? null : beatmaps.First();

            SearchFinished?.Invoke(beatmaps);
        }

        protected override void Dispose(bool isDisposing)
        {
            getSetsRequest?.Cancel();
            queryChangedDebounce?.Cancel();

            base.Dispose(isDisposing);
        }
    }
}
