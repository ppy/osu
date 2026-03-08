// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapListing;
using osu.Game.Scoring;
using osuTK.Input;
using APIUser = osu.Game.Online.API.Requests.Responses.APIUser;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneBeatmapListingOverlay : OsuManualInputManagerTestScene
    {
        private const int downloaded_filter_auto_fetch_cap = 5;

        private readonly List<APIBeatmapSet> setsForResponse = new List<APIBeatmapSet>();
        private readonly Queue<(List<APIBeatmapSet> beatmaps, bool hasNextPage)> queuedResponses = new Queue<(List<APIBeatmapSet> beatmaps, bool hasNextPage)>();

        private BeatmapListingOverlay overlay;

        private BeatmapListingFilterControl filterControl => overlay.ChildrenOfType<BeatmapListingFilterControl>().Single();

        private BeatmapListingSearchControl searchControl => overlay.ChildrenOfType<BeatmapListingSearchControl>().Single();

        private OsuConfigManager localConfig;

        private bool returnCursorOnResponse;
        private bool consumeQueuedResponses;
        private int searchRequestsHandled;

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.Cache(localConfig = new OsuConfigManager(LocalStorage));
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("setup overlay", () =>
            {
                Child = overlay = new BeatmapListingOverlay { State = { Value = Visibility.Visible } };
                setsForResponse.Clear();
                queuedResponses.Clear();
                consumeQueuedResponses = false;
                searchRequestsHandled = 0;
            });

            AddStep("initialize dummy", () =>
            {
                var api = (DummyAPIAccess)API;

                api.HandleRequest = req =>
                {
                    if (!(req is SearchBeatmapSetsRequest searchBeatmapSetsRequest))
                        return false;

                    searchRequestsHandled++;

                    List<APIBeatmapSet> beatmaps;
                    bool hasNextPage;

                    if (consumeQueuedResponses && queuedResponses.TryDequeue(out var queuedResponse))
                    {
                        beatmaps = queuedResponse.beatmaps;
                        hasNextPage = queuedResponse.hasNextPage;
                    }
                    else
                    {
                        beatmaps = setsForResponse;
                        hasNextPage = returnCursorOnResponse;
                    }

                    searchBeatmapSetsRequest.TriggerSuccess(new SearchBeatmapSetsResponse
                    {
                        BeatmapSets = beatmaps,
                        Cursor = hasNextPage ? new Cursor() : null,
                    });

                    return true;
                };

                // non-supporter user
                api.LocalUser.Value = new APIUser
                {
                    Username = "TestBot",
                    Id = API.LocalUser.Value.Id + 1,
                };
            });

            AddStep("reset size", () => localConfig.SetValue(OsuSetting.BeatmapListingCardSize, BeatmapCardSize.Normal));
        }

        [Test]
        public void TestFeaturedArtistFilter()
        {
            AddAssert("is visible", () => overlay.State.Value == Visibility.Visible);
            AddAssert("featured artist filter is on", () => overlay.ChildrenOfType<BeatmapSearchGeneralFilterRow>().First().Current.Contains(SearchGeneral.FeaturedArtists));
            AddStep("toggle featured artist filter", () => overlay.ChildrenOfType<FilterTabItem<SearchGeneral>>().First(i => i.Value == SearchGeneral.FeaturedArtists).TriggerClick());
            AddAssert("featured artist filter is off", () => !overlay.ChildrenOfType<BeatmapSearchGeneralFilterRow>().First().Current.Contains(SearchGeneral.FeaturedArtists));
        }

        [Test]
        public void TestHideViaBack()
        {
            AddAssert("is visible", () => overlay.State.Value == Visibility.Visible);
            AddStep("hide", () => InputManager.Key(Key.Escape));
            AddUntilStep("is hidden", () => overlay.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestHideViaBackWithSearch()
        {
            AddAssert("is visible", () => overlay.State.Value == Visibility.Visible);

            AddStep("search something", () => overlay.ChildrenOfType<SearchTextBox>().First().Text = "search");

            AddStep("kill search", () => InputManager.Key(Key.Escape));

            AddAssert("search textbox empty", () => string.IsNullOrEmpty(overlay.ChildrenOfType<SearchTextBox>().First().Text));
            AddAssert("is visible", () => overlay.State.Value == Visibility.Visible);

            AddStep("hide", () => InputManager.Key(Key.Escape));
            AddUntilStep("is hidden", () => overlay.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestHideViaBackWithScrolledSearch()
        {
            AddAssert("is visible", () => overlay.State.Value == Visibility.Visible);

            AddStep("show many results", () => fetchFor(getManyBeatmaps(100).ToArray()));

            AddUntilStep("placeholder hidden", () => !overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().Any(d => d.IsPresent));

            AddStep("scroll to bottom", () => overlay.ChildrenOfType<OverlayScrollContainer>().First().ScrollToEnd());

            AddStep("kill search", () => InputManager.Key(Key.Escape));

            AddUntilStep("search textbox empty", () => string.IsNullOrEmpty(overlay.ChildrenOfType<SearchTextBox>().First().Text));
            AddUntilStep("is scrolled to top", () => overlay.ChildrenOfType<OverlayScrollContainer>().First().Current == 0);
            AddAssert("is visible", () => overlay.State.Value == Visibility.Visible);

            AddStep("hide", () => InputManager.Key(Key.Escape));
            AddUntilStep("is hidden", () => overlay.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestCorrectOldContentExpiration()
        {
            AddAssert("is visible", () => overlay.State.Value == Visibility.Visible);

            AddStep("show many results", () => fetchFor(getManyBeatmaps(100).ToArray()));
            assertAllCardsOfType<BeatmapCardNormal>(100);

            AddStep("show more results", () => fetchFor(getManyBeatmaps(30).ToArray()));
            assertAllCardsOfType<BeatmapCardNormal>(30);
        }

        [Test]
        public void TestCardSizeSwitching([Values] bool viaConfig)
        {
            AddAssert("is visible", () => overlay.State.Value == Visibility.Visible);

            AddStep("show many results", () => fetchFor(getManyBeatmaps(100).ToArray()));
            assertAllCardsOfType<BeatmapCardNormal>(100);

            setCardSize(BeatmapCardSize.Extra, viaConfig);
            assertAllCardsOfType<BeatmapCardExtra>(100);

            setCardSize(BeatmapCardSize.Normal, viaConfig);
            assertAllCardsOfType<BeatmapCardNormal>(100);

            AddStep("fetch for 0 beatmaps", () => fetchFor());
            AddUntilStep("placeholder shown", () => overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().SingleOrDefault()?.IsPresent == true);

            setCardSize(BeatmapCardSize.Extra, viaConfig);
            AddAssert("placeholder shown", () => overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().SingleOrDefault()?.IsPresent == true);
        }

        [Test]
        public void TestNoBeatmapsPlaceholder()
        {
            AddStep("fetch for 0 beatmaps", () => fetchFor());
            placeholderShown();

            AddStep("show many results", () => fetchFor(getManyBeatmaps(100).ToArray()));
            AddUntilStep("wait for loaded", () => this.ChildrenOfType<BeatmapCard>().Count() == 100);
            AddUntilStep("placeholder hidden", () => !overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().Any(d => d.IsPresent));

            AddStep("fetch for 0 beatmaps", () => fetchFor());
            placeholderShown();

            // fetch once more to ensure nothing happens in displaying placeholder again when it already is present.
            AddStep("fetch for 0 beatmaps again", () => fetchFor());
            placeholderShown();

            void placeholderShown() =>
                AddUntilStep("placeholder shown", () =>
                {
                    var notFoundDrawable = overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().SingleOrDefault();
                    return notFoundDrawable != null && notFoundDrawable.IsPresent && notFoundDrawable.Parent!.DrawHeight > 0;
                });
        }

        /// <summary>
        /// During pagination, the first beatmap of the second page may be a duplicate of the last beatmap from the previous page.
        /// This is currently the case with osu!web API due to ES relevance score's presence in the response cursor.
        /// See: https://github.com/ppy/osu-web/issues/9270
        /// </summary>
        [Test]
        public void TestDuplicatedBeatmapOnlyShowsOnce()
        {
            APIBeatmapSet beatmapSet = null;

            AddStep("show many results", () =>
            {
                beatmapSet = CreateAPIBeatmapSet(Ruleset.Value);
                beatmapSet.Title = "last beatmap of first page";

                fetchFor(getManyBeatmaps(49).Append(new APIBeatmapSet { Title = "last beatmap of first page", OnlineID = beatmapSet.OnlineID }).ToArray(), true);
            });
            AddUntilStep("wait for loaded", () => this.ChildrenOfType<BeatmapCard>().Count() == 50);

            AddStep("set next page", () => setSearchResponse(getManyBeatmaps(49).Prepend(new APIBeatmapSet { Title = "this shouldn't show up", OnlineID = beatmapSet.OnlineID }).ToArray(), false));
            AddStep("scroll to end", () => overlay.ChildrenOfType<OverlayScrollContainer>().Single().ScrollToEnd());
            AddUntilStep("wait for loaded", () => this.ChildrenOfType<BeatmapCard>().Count() >= 99);

            AddAssert("beatmap not duplicated", () => overlay.ChildrenOfType<BeatmapCard>().Count(c => c.BeatmapSet.Equals(beatmapSet)) == 1);
        }

        [Test]
        public void TestExcludeDownloadedFilterHidesLocalBeatmaps()
        {
            int downloadedSetId = Math.Max(1, Guid.NewGuid().GetHashCode());

            AddStep("mark local set as downloaded", () => addLocalBeatmapSet(downloadedSetId));
            setHideDownloadedFilter(true);

            AddStep("show one downloaded and one not-downloaded result", () =>
            {
                var downloaded = CreateAPIBeatmapSet(Ruleset.Value);
                downloaded.OnlineID = downloadedSetId;
                downloaded.Title = "Downloaded set";

                var notDownloaded = CreateAPIBeatmapSet(Ruleset.Value);
                notDownloaded.Title = "Not downloaded set";

                fetchFor(downloaded, notDownloaded);
            });

            AddUntilStep("only one card shown", () => this.ChildrenOfType<BeatmapCard>().Count() == 1);
            AddAssert("downloaded set filtered out", () => this.ChildrenOfType<BeatmapCard>().Single().BeatmapSet.OnlineID != downloadedSetId);
        }

        [Test]
        public void TestExcludeDownloadedFilterCanPaginatePastFilteredFirstPage()
        {
            int downloadedSetId = Math.Max(1, Guid.NewGuid().GetHashCode());
            int expectedSetId = Math.Max(1, Guid.NewGuid().GetHashCode());

            if (expectedSetId == downloadedSetId)
                expectedSetId++;

            AddStep("mark local set as downloaded", () => addLocalBeatmapSet(downloadedSetId));

            AddStep("set paged search responses", () =>
            {
                var downloaded = CreateAPIBeatmapSet(Ruleset.Value);
                downloaded.OnlineID = downloadedSetId;

                var notDownloaded = CreateAPIBeatmapSet(Ruleset.Value);
                notDownloaded.OnlineID = expectedSetId;

                setSearchResponses(
                    (new[] { downloaded }, true),
                    (new[] { notDownloaded }, false));
            });

            int requestsBeforeSearch = 0;
            AddStep("capture request baseline", () => requestsBeforeSearch = searchRequestsHandled);
            AddStep("enable queued responses", () => consumeQueuedResponses = true);
            setHideDownloadedFilter(true);
            AddUntilStep("non-downloaded result loaded", () => this.ChildrenOfType<BeatmapCard>().SingleOrDefault()?.BeatmapSet.OnlineID == expectedSetId);
            AddAssert("paged search requests were made", () => searchRequestsHandled >= requestsBeforeSearch + 2);
            noPlaceholderShown();
        }

        [Test]
        public void TestExcludeDownloadedFilterAutoPaginatesWhenFirstPageHasSingleVisibleResult()
        {
            int downloadedSetId = Math.Max(1, Guid.NewGuid().GetHashCode());
            int firstVisibleSetId = Math.Max(1, Guid.NewGuid().GetHashCode());
            int secondVisibleSetId = Math.Max(1, Guid.NewGuid().GetHashCode());

            if (firstVisibleSetId == downloadedSetId)
                firstVisibleSetId++;

            if (secondVisibleSetId == downloadedSetId || secondVisibleSetId == firstVisibleSetId)
                secondVisibleSetId += 2;

            AddStep("mark one set as downloaded", () => addLocalBeatmapSet(downloadedSetId));
            AddStep("set paged search responses", () =>
            {
                setSearchResponses(
                    (new[]
                    {
                        new APIBeatmapSet { OnlineID = firstVisibleSetId },
                        new APIBeatmapSet { OnlineID = downloadedSetId },
                    }, true),
                    (new[]
                    {
                        new APIBeatmapSet { OnlineID = secondVisibleSetId },
                    }, false));
            });

            int requestsBeforeSearch = 0;
            AddStep("capture request baseline", () => requestsBeforeSearch = searchRequestsHandled);
            AddStep("enable queued responses", () => consumeQueuedResponses = true);
            setHideDownloadedFilter(true);

            AddUntilStep("both visible results loaded", () =>
                this.ChildrenOfType<BeatmapCard>().Select(c => c.BeatmapSet.OnlineID).Distinct().Count() == 2);
            AddAssert("second page was requested", () => searchRequestsHandled >= requestsBeforeSearch + 2);
        }

        [Test]
        public void TestExcludeDownloadedFilterStopsFetchingWhenAllPagedResultsAreDownloaded()
        {
            int[] downloadedSetIds =
            {
                Math.Max(1, Guid.NewGuid().GetHashCode()),
                Math.Max(1, Guid.NewGuid().GetHashCode()),
                Math.Max(1, Guid.NewGuid().GetHashCode()),
            };

            AddStep("mark all paged sets as downloaded", () =>
            {
                foreach (int id in downloadedSetIds)
                    addLocalBeatmapSet(id);
            });

            AddStep("set paged search responses to only downloaded sets", () =>
            {
                setSearchResponses(
                    (new[] { new APIBeatmapSet { OnlineID = downloadedSetIds[0] } }, true),
                    (new[] { new APIBeatmapSet { OnlineID = downloadedSetIds[1] } }, true),
                    (new[] { new APIBeatmapSet { OnlineID = downloadedSetIds[2] } }, false));
            });

            int requestsBeforeSearch = 0;
            AddStep("capture request baseline", () => requestsBeforeSearch = searchRequestsHandled);
            AddStep("enable queued responses", () => consumeQueuedResponses = true);

            setHideDownloadedFilter(true);

            notFoundPlaceholderShown();

            AddAssert("one request per available page", () => searchRequestsHandled == requestsBeforeSearch + downloadedSetIds.Length);
            AddWaitStep("wait for any runaway requests", 1000);
            AddAssert("request count stable after cursor end", () => searchRequestsHandled == requestsBeforeSearch + downloadedSetIds.Length);
        }

        [Test]
        public void TestExcludeDownloadedFilterAutoFetchIsCapped()
        {
            AddStep("set many paged downloaded-only responses", () =>
            {
                var responses = Enumerable.Range(0, downloaded_filter_auto_fetch_cap * 2)
                                          .Select(_ =>
                                          {
                                              int onlineId = Math.Max(1, Guid.NewGuid().GetHashCode());
                                              addLocalBeatmapSet(onlineId);
                                              return (beatmaps: (IEnumerable<APIBeatmapSet>)new[] { new APIBeatmapSet { OnlineID = onlineId } }, hasNextPage: true);
                                          })
                                          .ToArray();

                setSearchResponses(responses);
            });

            int requestsBeforeSearch = 0;
            AddStep("capture request baseline", () => requestsBeforeSearch = searchRequestsHandled);
            AddStep("enable queued responses", () => consumeQueuedResponses = true);
            setHideDownloadedFilter(true);

            filteredLimitPlaceholderShown();

            int requestsAfterCompletion = 0;
            AddStep("capture request count after completion", () => requestsAfterCompletion = searchRequestsHandled);
            AddAssert("autofetch is bounded", () => requestsAfterCompletion <= requestsBeforeSearch + downloaded_filter_auto_fetch_cap);
        }

        [Test]
        public void TestExcludeDownloadedFilterCanLoadMoreAfterAutoFetchCap()
        {
            const int expectedSetId = int.MaxValue - 12345;

            AddStep("set paged downloaded-only responses then a non-downloaded one", () =>
            {
                var downloadedResponses = Enumerable.Range(0, downloaded_filter_auto_fetch_cap)
                                                   .Select(_ =>
                                                   {
                                                       int onlineId = Math.Max(1, Guid.NewGuid().GetHashCode());
                                                       addLocalBeatmapSet(onlineId);
                                                       return (beatmaps: (IEnumerable<APIBeatmapSet>)new[] { new APIBeatmapSet { OnlineID = onlineId } }, hasNextPage: true);
                                                   });

                var notDownloaded = new APIBeatmapSet { OnlineID = expectedSetId };

                setSearchResponses(downloadedResponses.Append((new[] { notDownloaded }, false)).ToArray());
            });

            AddStep("enable queued responses", () => consumeQueuedResponses = true);
            int requestsBeforeSearch = 0;
            AddStep("capture request baseline", () => requestsBeforeSearch = searchRequestsHandled);
            setHideDownloadedFilter(true);

            AddUntilStep("autofetch capped", () => searchRequestsHandled >= requestsBeforeSearch + downloaded_filter_auto_fetch_cap);
            AddStep("load more", () => filterControl.FetchMoreAfterFilteredLimit());
            AddUntilStep("non-downloaded result loaded", () => this.ChildrenOfType<BeatmapCard>().SingleOrDefault()?.BeatmapSet.OnlineID == expectedSetId);
        }

        [Test]
        public void TestExcludeDownloadedFilterShowsPromptWhenCapHitAfterResultsAreVisible()
        {
            int[] downloadedIds = Enumerable.Range(0, downloaded_filter_auto_fetch_cap)
                                            .Select(_ => Math.Max(1, Guid.NewGuid().GetHashCode()))
                                            .ToArray();

            AddStep("mark later pages as downloaded", () =>
            {
                foreach (int id in downloadedIds)
                    addLocalBeatmapSet(id);
            });

            AddStep("set responses with visible first page then downloaded pages", () =>
            {
                var firstPageVisible = getManyBeatmaps(60);

                var responses = new List<(IEnumerable<APIBeatmapSet> beatmaps, bool hasNextPage)>
                {
                    (firstPageVisible, true),
                };

                responses.AddRange(downloadedIds.Select(id =>
                    ((IEnumerable<APIBeatmapSet>)new[] { new APIBeatmapSet { OnlineID = id } }, true)));

                setSearchResponses(responses.ToArray());
            });

            AddStep("enable queued responses", () => consumeQueuedResponses = true);
            setHideDownloadedFilter(true);

            AddUntilStep("initial results visible", () => this.ChildrenOfType<BeatmapCard>().Any());
            AddStep("scroll to end", () => overlay.ChildrenOfType<OverlayScrollContainer>().Single().ScrollToEnd());
            AddUntilStep("\"filtered limit reached\" prompt added", () => this.ChildrenOfType<BeatmapListingOverlay.FilteredResultsLimitReachedDrawable>().Any());
            AddAssert("results remain visible", () => this.ChildrenOfType<BeatmapCard>().Any());
        }

        [Test]
        public void TestUserWithoutSupporterUsesSupporterOnlyFiltersWithoutResults()
        {
            AddStep("fetch for 0 beatmaps", () => fetchFor());

            AddStep("set dummy as non-supporter", () => ((DummyAPIAccess)API).LocalUser.Value.IsSupporter = false);

            // only Rank Achieved filter
            setRankAchievedFilter(new[] { ScoreRank.XH });
            supporterRequiredPlaceholderShown();

            setRankAchievedFilter(Array.Empty<ScoreRank>());
            notFoundPlaceholderShown();

            // only Played filter
            setPlayedFilter(SearchPlayed.Played);
            supporterRequiredPlaceholderShown();

            setPlayedFilter(SearchPlayed.Any);
            notFoundPlaceholderShown();

            // both RankAchieved and Played filters
            setRankAchievedFilter(new[] { ScoreRank.XH });
            setPlayedFilter(SearchPlayed.Played);
            supporterRequiredPlaceholderShown();

            setRankAchievedFilter(Array.Empty<ScoreRank>());
            setPlayedFilter(SearchPlayed.Any);
            notFoundPlaceholderShown();
        }

        [Test]
        public void TestUserWithSupporterUsesSupporterOnlyFiltersWithoutResults()
        {
            AddStep("fetch for 0 beatmaps", () => fetchFor());
            AddStep("set dummy as supporter", () => ((DummyAPIAccess)API).LocalUser.Value.IsSupporter = true);

            // only Rank Achieved filter
            setRankAchievedFilter(new[] { ScoreRank.XH });
            notFoundPlaceholderShown();

            setRankAchievedFilter(Array.Empty<ScoreRank>());
            notFoundPlaceholderShown();

            // only Played filter
            setPlayedFilter(SearchPlayed.Played);
            notFoundPlaceholderShown();

            setPlayedFilter(SearchPlayed.Any);
            notFoundPlaceholderShown();

            // both Rank Achieved and Played filters
            setRankAchievedFilter(new[] { ScoreRank.XH });
            setPlayedFilter(SearchPlayed.Played);
            notFoundPlaceholderShown();

            setRankAchievedFilter(Array.Empty<ScoreRank>());
            setPlayedFilter(SearchPlayed.Any);
            notFoundPlaceholderShown();
        }

        [Test]
        public void TestUserWithoutSupporterUsesSupporterOnlyFiltersWithResults()
        {
            AddStep("fetch for 1 beatmap", () => fetchFor(CreateAPIBeatmapSet(Ruleset.Value)));

            noPlaceholderShown();

            AddStep("set dummy as non-supporter", () => ((DummyAPIAccess)API).LocalUser.Value.IsSupporter = false);

            // only Rank Achieved filter
            setRankAchievedFilter(new[] { ScoreRank.XH });
            supporterRequiredPlaceholderShown();

            setRankAchievedFilter(Array.Empty<ScoreRank>());
            noPlaceholderShown();

            // only Played filter
            setPlayedFilter(SearchPlayed.Played);
            supporterRequiredPlaceholderShown();

            setPlayedFilter(SearchPlayed.Any);
            noPlaceholderShown();

            // both Rank Achieved and Played filters
            setRankAchievedFilter(new[] { ScoreRank.XH });
            setPlayedFilter(SearchPlayed.Played);
            supporterRequiredPlaceholderShown();

            setRankAchievedFilter(Array.Empty<ScoreRank>());
            setPlayedFilter(SearchPlayed.Any);
            noPlaceholderShown();
        }

        [Test]
        public void TestUserWithSupporterUsesSupporterOnlyFiltersWithResults()
        {
            AddStep("fetch for 1 beatmap", () => fetchFor(CreateAPIBeatmapSet(Ruleset.Value)));

            noPlaceholderShown();

            AddStep("set dummy as supporter", () => ((DummyAPIAccess)API).LocalUser.Value.IsSupporter = true);

            // only Rank Achieved filter
            setRankAchievedFilter(new[] { ScoreRank.XH });
            noPlaceholderShown();

            setRankAchievedFilter(Array.Empty<ScoreRank>());
            noPlaceholderShown();

            // only Played filter
            setPlayedFilter(SearchPlayed.Played);
            noPlaceholderShown();

            setPlayedFilter(SearchPlayed.Any);
            noPlaceholderShown();

            // both Rank Achieved and Played filters
            setRankAchievedFilter(new[] { ScoreRank.XH });
            setPlayedFilter(SearchPlayed.Played);
            noPlaceholderShown();

            setRankAchievedFilter(Array.Empty<ScoreRank>());
            setPlayedFilter(SearchPlayed.Any);
            noPlaceholderShown();
        }

        [Test]
        public void TestExpandedCardContentNotClipped()
        {
            AddAssert("is visible", () => overlay.State.Value == Visibility.Visible);

            AddStep("show result with many difficulties", () =>
            {
                var beatmapSet = CreateAPIBeatmapSet(Ruleset.Value);
                beatmapSet.Beatmaps = Enumerable.Repeat(beatmapSet.Beatmaps.First(), 100).ToArray();
                fetchFor(beatmapSet);
            });
            assertAllCardsOfType<BeatmapCardNormal>(1);

            AddStep("hover extra info row", () =>
            {
                var difficultyArea = this.ChildrenOfType<BeatmapCardExtraInfoRow>().Single();
                InputManager.MoveMouseTo(difficultyArea);
            });
            AddUntilStep("wait for expanded", () => this.ChildrenOfType<BeatmapCardNormal>().Single().Expanded.Value);
            AddAssert("expanded content not clipped", () =>
            {
                var cardContainer = this.ChildrenOfType<ReverseChildIDFillFlowContainer<BeatmapCard>>().Single().Parent;
                var expandedContent = this.ChildrenOfType<ExpandedContentScrollContainer>().Single();
                return expandedContent.ScreenSpaceDrawQuad.GetVertices().ToArray().All(v => cardContainer!.ScreenSpaceDrawQuad.Contains(v));
            });
        }

        private static int searchCount;

        private APIBeatmapSet[] getManyBeatmaps(int count) => Enumerable.Range(0, count).Select(_ => CreateAPIBeatmapSet(Ruleset.Value)).ToArray();

        private void fetchFor(params APIBeatmapSet[] beatmaps) => fetchFor(beatmaps, false);

        private void fetchFor(APIBeatmapSet[] beatmaps, bool hasNextPage)
        {
            setSearchResponse(beatmaps, hasNextPage);

            // trigger arbitrary change for fetching.
            searchControl.Query.Value = $"search {searchCount++}";
        }

        private void setSearchResponse(APIBeatmapSet[] beatmaps, bool hasNextPage)
        {
            queuedResponses.Clear();
            setsForResponse.Clear();
            setsForResponse.AddRange(beatmaps);
            returnCursorOnResponse = hasNextPage;
        }

        private void setSearchResponses(params (IEnumerable<APIBeatmapSet> beatmaps, bool hasNextPage)[] responses)
        {
            queuedResponses.Clear();

            foreach (var response in responses)
                queuedResponses.Enqueue((response.beatmaps.ToList(), response.hasNextPage));
        }

        private void setRankAchievedFilter(ScoreRank[] ranks)
        {
            AddStep($"set Rank Achieved filter to [{string.Join(',', ranks)}]", () =>
            {
                searchControl.Ranks.Clear();
                searchControl.Ranks.AddRange(ranks);
            });
        }

        private void setPlayedFilter(SearchPlayed played)
        {
            AddStep($"set Played filter to {played}", () => searchControl.Played.Value = played);
        }

        private void setHideDownloadedFilter(bool enabled)
        {
            AddStep($"set hide-downloaded filter to {enabled}", () =>
            {
                if (enabled)
                {
                    if (!searchControl.General.Contains(SearchGeneral.HideAlreadyDownloaded))
                        searchControl.General.Add(SearchGeneral.HideAlreadyDownloaded);
                }
                else
                {
                    searchControl.General.Remove(SearchGeneral.HideAlreadyDownloaded);
                }
            });
        }

        private void addLocalBeatmapSet(int onlineId)
        {
            Realm.Write(r => r.Add(new BeatmapSetInfo { OnlineID = onlineId }));
        }

        private void supporterRequiredPlaceholderShown()
        {
            AddUntilStep("\"supporter required\" placeholder shown", () => overlay.ChildrenOfType<BeatmapListingOverlay.SupporterRequiredDrawable>().SingleOrDefault()?.IsPresent == true);
        }

        private void notFoundPlaceholderShown()
        {
            AddUntilStep("\"no maps found\" placeholder shown", () => overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().SingleOrDefault()?.IsPresent == true);
        }

        private void noPlaceholderShown()
        {
            AddUntilStep("\"supporter required\" placeholder not shown", () => !overlay.ChildrenOfType<BeatmapListingOverlay.SupporterRequiredDrawable>().Any(d => d.IsPresent));
            AddUntilStep("\"no maps found\" placeholder not shown", () => !overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().Any(d => d.IsPresent));
        }

        private void filteredLimitPlaceholderShown()
        {
            AddUntilStep("\"filtered limit reached\" placeholder shown", () => overlay.ChildrenOfType<BeatmapListingOverlay.FilteredResultsLimitReachedDrawable>().Any());
        }

        private void setCardSize(BeatmapCardSize cardSize, bool viaConfig) => AddStep($"set card size to {cardSize}", () =>
        {
            if (viaConfig)
                localConfig.SetValue(OsuSetting.BeatmapListingCardSize, cardSize);
            else
                overlay.ChildrenOfType<BeatmapListingCardSizeTabControl>().Single().Current.Value = cardSize;
        });

        private void assertAllCardsOfType<T>(int expectedCount)
            where T : BeatmapCard =>
            AddUntilStep($"all loaded beatmap cards are {typeof(T)}", () =>
            {
                int loadedCorrectCount = this.ChildrenOfType<BeatmapCard>().Count(card => card.IsLoaded && card.GetType() == typeof(T));
                return loadedCorrectCount > 0 && loadedCorrectCount == expectedCount;
            });

        protected override void Dispose(bool isDisposing)
        {
            localConfig?.Dispose();
            base.Dispose(isDisposing);
        }
    }
}
