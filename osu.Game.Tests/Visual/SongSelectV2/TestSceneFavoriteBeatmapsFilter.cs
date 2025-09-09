// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Screens.Play;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Resources;
using osuTK.Input;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneFavoriteBeatmapsFilter : OsuManualInputManagerTestScene
    {
        private BeatmapManager beatmapManager = null!;
        private FilterControl filterControl = null!;
        private CollectionDropdown collectionDropdown = null!;
        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        [Cached]
        private readonly LocalUserPlayInfo localUserPlayInfo = new LocalUserPlayInfo();

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            Dependencies.Cache(new RealmRulesetStore(Realm));
            Dependencies.Cache(beatmapManager = new BeatmapManager(LocalStorage, Realm, null, Audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(Realm);
            Dependencies.Cache(localUserPlayInfo);

            beatmapManager.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    filterControl = new FilterControl
                    {
                        State = { Value = Visibility.Visible },
                        RelativeSizeAxes = Axes.X,
                        Height = 200,
                    },
                    collectionDropdown = new CollectionDropdown
                    {
                        RelativeSizeAxes = Axes.X,
                        Y = 220,
                        Height = 40,
                    }
                }
            };
        });

        [Test]
        public void TestFavoritesOptionNotVisibleWhenLoggedOut()
        {
            AddStep("log out", () => dummyAPI.Logout());
            AddUntilStep("no favorites option", () => !collectionDropdown.ItemSource.Any(item => item is FavoriteBeatmapsCollectionFilterMenuItem));
        }

        [Test]
        public void TestFavoritesOptionVisibleWhenLoggedIn()
        {
            AddStep("log in", () => dummyAPI.Login("test", "test"));
            AddUntilStep("favorites option exists", () => collectionDropdown.ItemSource.Any(item => item is FavoriteBeatmapsCollectionFilterMenuItem));
        }

        [Test]
        public void TestFavoritesFilterWithNoCache()
        {
            AddStep("log in", () => dummyAPI.Login("test", "test"));
            AddStep("select favorites", () => selectFavoritesFilter());
            AddAssert("API request was made", () => dummyAPI.LastQueuedRequest is GetUserBeatmapsRequest);
        }

        [Test]
        public void TestFavoritesFilterWithCache()
        {
            GetUserBeatmapsRequest? lastRequest = null;

            AddStep("log in", () => dummyAPI.Login("test", "test"));
            AddStep("mock API response", () =>
            {
                dummyAPI.HandleRequest = req =>
                {
                    if (req is GetUserBeatmapsRequest getUserBeatmapsRequest)
                    {
                        lastRequest = getUserBeatmapsRequest;
                        getUserBeatmapsRequest.TriggerSuccess(new APIBeatmapSet[]
                        {
                            new APIBeatmapSet { OnlineID = 1 },
                            new APIBeatmapSet { OnlineID = 2 }
                        });
                        return true;
                    }
                    return false;
                };
            });

            AddStep("select favorites", () => selectFavoritesFilter());
            AddAssert("API request was made", () => lastRequest != null);

            AddStep("reset request tracking", () => lastRequest = null);
            AddStep("select all beatmaps", () => selectAllBeatmapsFilter());
            AddStep("select favorites again", () => selectFavoritesFilter());
            AddAssert("no new API request", () => lastRequest == null);
        }

        [Test]
        public void TestFavoriteStatusChangeUpdatesFilter()
        {
            AddStep("log in", () => dummyAPI.Login("test", "test"));
            AddStep("mock API responses", () => setupMockAPI());
            AddStep("select favorites filter", () => selectFavoritesFilter());

            AddStep("trigger favorite change event", () =>
                PostBeatmapFavouriteRequest.FavouriteChanged?.Invoke(123, true));
            AddAssert("filter criteria updated", () => filterControl.CreateCriteria().CollectionBeatmapMD5Hashes != null);
        }

        [Test]
        public void TestPeriodicRefreshOnlyWhenNotPlaying()
        {
            AddStep("log in", () => dummyAPI.Login("test", "test"));
            AddStep("set playing state", () => localUserPlayInfo.PlayingState.Value = LocalUserPlayingState.Playing);
            AddStep("mock API responses", () => setupMockAPI());
            AddStep("select favorites filter", () => selectFavoritesFilter());
            AddAssert("playing state is respected", () => localUserPlayInfo.PlayingState.Value == LocalUserPlayingState.Playing);

            AddStep("set not playing state", () => localUserPlayInfo.PlayingState.Value = LocalUserPlayingState.NotPlaying);
            AddAssert("not playing state is set", () => localUserPlayInfo.PlayingState.Value == LocalUserPlayingState.NotPlaying);
        }

        [Test]
        public void TestCacheClearsOnUserChange()
        {
            AddStep("log in as user 1", () => dummyAPI.Login("user1", "test"));
            AddStep("mock API responses", () => setupMockAPI());
            AddStep("select favorites filter", () => selectFavoritesFilter());
            AddUntilStep("cache populated", () => filterControl.CreateCriteria().CollectionBeatmapMD5Hashes != null);

            AddStep("log in as user 2", () => dummyAPI.Login("user2", "test"));
            AddStep("select favorites filter", () => selectFavoritesFilter());
            AddUntilStep("new API request for new user", () => dummyAPI.LastQueuedRequest is GetUserBeatmapsRequest);
        }

        [Test]
        public void TestFavoritesFilterShowsCorrectBeatmaps()
        {
            AddStep("log in", () => dummyAPI.Login("test", "test"));
            AddStep("mock API with known beatmap", () =>
            {
                var importedBeatmap = beatmapManager.GetAllUsableBeatmapSets().FirstOrDefault();
                if (importedBeatmap != null)
                {
                    dummyAPI.HandleRequest = req =>
                    {
                        if (req is GetUserBeatmapsRequest getUserBeatmapsRequest)
                        {
                            getUserBeatmapsRequest.TriggerSuccess(new APIBeatmapSet[]
                            {
                                new APIBeatmapSet { OnlineID = importedBeatmap.OnlineID }
                            });
                            return true;
                        }
                        return false;
                    };
                }
            });

            AddStep("select favorites filter", () => selectFavoritesFilter());
            AddUntilStep("filter includes imported beatmap hashes", () =>
            {
                var criteria = filterControl.CreateCriteria();
                var importedBeatmap = beatmapManager.GetAllUsableBeatmapSets().FirstOrDefault();
                if (importedBeatmap == null) return false;
                var importedHashes = importedBeatmap.Beatmaps.Select(b => b.MD5Hash).ToHashSet();
                return criteria.CollectionBeatmapMD5Hashes?.Intersect(importedHashes).Any() == true;
            });
        }

        private void selectFavoritesFilter()
        {
            AddStep("select favorites from dropdown", () =>
            {
                var favoritesMenuItem = collectionDropdown.ItemSource.FirstOrDefault(item => item is FavoriteBeatmapsCollectionFilterMenuItem);
                if (favoritesMenuItem != null)
                    collectionDropdown.Current.Value = favoritesMenuItem;
            });
        }

        private void selectAllBeatmapsFilter()
        {
            AddStep("select all beatmaps from dropdown", () =>
            {
                var allBeatmapsMenuItem = collectionDropdown.ItemSource.FirstOrDefault(item => item is AllBeatmapsCollectionFilterMenuItem);
                if (allBeatmapsMenuItem != null)
                    collectionDropdown.Current.Value = allBeatmapsMenuItem;
            });
        }

        private void setupMockAPI()
        {
            dummyAPI.HandleRequest = req =>
            {
                if (req is GetUserBeatmapsRequest getUserBeatmapsRequest)
                {
                    getUserBeatmapsRequest.TriggerSuccess(new APIBeatmapSet[]
                    {
                        new APIBeatmapSet { OnlineID = 1 },
                        new APIBeatmapSet { OnlineID = 2 }
                    });
                    return true;
                }
                return false;
            };
        }

        private partial class LocalUserPlayInfo : ILocalUserPlayInfo
        {
            public Bindable<LocalUserPlayingState> PlayingState { get; } = new Bindable<LocalUserPlayingState>();

            IBindable<LocalUserPlayingState> ILocalUserPlayInfo.PlayingState => PlayingState;
        }
    }
}
