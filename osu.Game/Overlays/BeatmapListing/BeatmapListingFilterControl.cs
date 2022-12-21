// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Framework.Threading;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Configuration;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapListing
{
    public partial class BeatmapListingFilterControl : CompositeDrawable
    {
        /// <summary>
        /// Fired when a search finishes.
        /// </summary>
        public Action<SearchResult> SearchFinished;

        /// <summary>
        /// Fired when search criteria change.
        /// </summary>
        public Action SearchStarted;

        /// <summary>
        /// Any time the search text box receives key events (even while masked).
        /// </summary>
        public Action TypingStarted;

        /// <summary>
        /// True when pagination has reached the end of available results.
        /// </summary>
        private bool noMoreResults;

        /// <summary>
        /// The current page fetched of results (zero index).
        /// </summary>
        public int CurrentPage { get; private set; }

        /// <summary>
        /// The currently selected <see cref="BeatmapCardSize"/>.
        /// </summary>
        public IBindable<BeatmapCardSize> CardSize => cardSize;

        private readonly Bindable<BeatmapCardSize> cardSize = new Bindable<BeatmapCardSize>();

        private readonly BeatmapListingSearchControl searchControl;
        private readonly BeatmapListingSortTabControl sortControl;
        private readonly Box sortControlBackground;

        private ScheduledDelegate queryChangedDebounce;

        private SearchBeatmapSetsRequest getSetsRequest;
        private SearchBeatmapSetsResponse lastResponse;

        [Resolved]
        private IAPIProvider api { get; set; }

        private IBindable<APIUser> apiUser;

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
                        Child = searchControl = new BeatmapListingSearchControl
                        {
                            TypingStarted = () => TypingStarted?.Invoke()
                        }
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
                            },
                            new BeatmapListingCardSizeTabControl
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                Margin = new MarginPadding { Right = 20 },
                                Current = { BindTarget = CardSize }
                            }
                        }
                    }
                }
            };
        }

        [Resolved]
        private OsuConfigManager config { get; set; }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, IAPIProvider api)
        {
            sortControlBackground.Colour = colourProvider.Background4;
        }

        public void Search(string query)
            => Schedule(() => searchControl.Query.Value = query);

        public void FilterGenre(SearchGenre genre)
            => Schedule(() => searchControl.Genre.Value = genre);

        public void FilterLanguage(SearchLanguage language)
            => Schedule(() => searchControl.Language.Value = language);

        protected override void LoadComplete()
        {
            base.LoadComplete();

            config.BindWith(OsuSetting.BeatmapListingCardSize, cardSize);

            searchControl.Query.BindValueChanged(_ =>
            {
                resetSortControl();
                queueUpdateSearch(true);
            });

            searchControl.Category.BindValueChanged(_ =>
            {
                resetSortControl();
                queueUpdateSearch();
            });

            searchControl.General.CollectionChanged += (_, _) => queueUpdateSearch();
            searchControl.Ruleset.BindValueChanged(_ => queueUpdateSearch());
            searchControl.Genre.BindValueChanged(_ => queueUpdateSearch());
            searchControl.Language.BindValueChanged(_ => queueUpdateSearch());
            searchControl.Extra.CollectionChanged += (_, _) => queueUpdateSearch();
            searchControl.Ranks.CollectionChanged += (_, _) => queueUpdateSearch();
            searchControl.Played.BindValueChanged(_ => queueUpdateSearch());
            searchControl.ExplicitContent.BindValueChanged(_ => queueUpdateSearch());

            sortControl.Current.BindValueChanged(_ => queueUpdateSearch());
            sortControl.SortDirection.BindValueChanged(_ => queueUpdateSearch());

            apiUser = api.LocalUser.GetBoundCopy();
            apiUser.BindValueChanged(_ => queueUpdateSearch());
        }

        public void TakeFocus() => searchControl.TakeFocus();

        /// <summary>
        /// Fetch the next page of results. May result in a no-op if a fetch is already in progress, or if there are no results left.
        /// </summary>
        public void FetchNextPage()
        {
            // there may be no results left.
            if (noMoreResults)
                return;

            // there may already be an active request.
            if (getSetsRequest != null)
                return;

            if (lastResponse != null)
                CurrentPage++;

            performRequest();
        }

        private void resetSortControl() => sortControl.Reset(searchControl.Category.Value, !string.IsNullOrEmpty(searchControl.Query.Value));

        private void queueUpdateSearch(bool queryTextChanged = false)
        {
            SearchStarted?.Invoke();

            resetSearch();

            if (!api.IsLoggedIn)
                return;

            queryChangedDebounce = Scheduler.AddDelayed(() =>
            {
                resetSearch();
                FetchNextPage();
            }, queryTextChanged ? 500 : 100);
        }

        private void performRequest()
        {
            getSetsRequest = new SearchBeatmapSetsRequest(
                searchControl.Query.Value,
                searchControl.Ruleset.Value,
                lastResponse?.Cursor,
                searchControl.General,
                searchControl.Category.Value,
                sortControl.Current.Value,
                sortControl.SortDirection.Value,
                searchControl.Genre.Value,
                searchControl.Language.Value,
                searchControl.Extra,
                searchControl.Ranks,
                searchControl.Played.Value,
                searchControl.ExplicitContent.Value);

            getSetsRequest.Success += response =>
            {
                var sets = response.BeatmapSets.ToList();

                // If the previous request returned a null cursor, the API is indicating we can't paginate further (maybe there are no more beatmaps left).
                if (sets.Count == 0 || response.Cursor == null)
                    noMoreResults = true;

                if (CurrentPage == 0)
                    searchControl.BeatmapSet = sets.FirstOrDefault();

                lastResponse = response;
                getSetsRequest = null;

                // check if a non-supporter used supporter-only filters
                if (!api.LocalUser.Value.IsSupporter)
                {
                    List<LocalisableString> filters = new List<LocalisableString>();

                    if (searchControl.Played.Value != SearchPlayed.Any)
                        filters.Add(BeatmapsStrings.ListingSearchFiltersPlayed);

                    if (searchControl.Ranks.Any())
                        filters.Add(BeatmapsStrings.ListingSearchFiltersRank);

                    if (filters.Any())
                    {
                        var supporterOnlyFilters = SearchResult.SupporterOnlyFilters(filters);
                        SearchFinished?.Invoke(supporterOnlyFilters);
                        return;
                    }
                }

                var resultsReturned = SearchResult.ResultsReturned(sets);
                SearchFinished?.Invoke(resultsReturned);
            };

            api.Queue(getSetsRequest);
        }

        private void resetSearch()
        {
            noMoreResults = false;
            CurrentPage = 0;

            lastResponse = null;

            getSetsRequest?.Cancel();
            getSetsRequest = null;

            queryChangedDebounce?.Cancel();
        }

        protected override void Dispose(bool isDisposing)
        {
            resetSearch();

            base.Dispose(isDisposing);
        }

        /// <summary>
        /// Indicates the type of result of a user-requested beatmap search.
        /// </summary>
        public enum SearchResultType
        {
            /// <summary>
            /// Actual results have been returned from API.
            /// </summary>
            ResultsReturned,

            /// <summary>
            /// The user is not a supporter, but used supporter-only search filters.
            /// </summary>
            SupporterOnlyFilters
        }

        /// <summary>
        /// Describes the result of a user-requested beatmap search.
        /// </summary>
        public struct SearchResult
        {
            public SearchResultType Type { get; private set; }

            /// <summary>
            /// Contains the beatmap sets returned from API.
            /// Valid for read if and only if <see cref="Type"/> is <see cref="SearchResultType.ResultsReturned"/>.
            /// </summary>
            public List<APIBeatmapSet> Results { get; private set; }

            /// <summary>
            /// Contains the names of supporter-only filters requested by the user.
            /// Valid for read if and only if <see cref="Type"/> is <see cref="SearchResultType.SupporterOnlyFilters"/>.
            /// </summary>
            public List<LocalisableString> SupporterOnlyFiltersUsed { get; private set; }

            public static SearchResult ResultsReturned(List<APIBeatmapSet> results) => new SearchResult
            {
                Type = SearchResultType.ResultsReturned,
                Results = results,
            };

            public static SearchResult SupporterOnlyFilters(List<LocalisableString> filters) => new SearchResult
            {
                Type = SearchResultType.SupporterOnlyFilters,
                SupporterOnlyFiltersUsed = filters
            };
        }
    }
}
