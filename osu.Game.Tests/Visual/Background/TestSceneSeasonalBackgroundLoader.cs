// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Game.Configuration;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Tests.Visual.Background
{
    public class TestSceneSeasonalBackgroundLoader : ScreenTestScene
    {
        [Resolved]
        private OsuConfigManager config { get; set; }

        [Resolved]
        private SessionStatics statics { get; set; }

        [Cached(typeof(LargeTextureStore))]
        private LookupLoggingTextureStore textureStore = new LookupLoggingTextureStore();

        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        private SeasonalBackgroundLoader backgroundLoader;
        private Container backgroundContainer;

        // in real usages these would be online URLs, but correct execution of this test
        // shouldn't be coupled to existence of online assets.
        private static readonly List<string> seasonal_background_urls = new List<string>
        {
            "Backgrounds/bg2",
            "Backgrounds/bg4",
            "Backgrounds/bg3"
        };

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore wrappedStore)
        {
            textureStore.AddStore(wrappedStore);

            Add(backgroundContainer = new Container
            {
                RelativeSizeAxes = Axes.Both
            });
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            // reset API response in statics to avoid test crosstalk.
            statics.Set<APISeasonalBackgrounds>(Static.SeasonalBackgrounds, null);
            textureStore.PerformedLookups.Clear();
            dummyAPI.SetState(APIState.Online);

            backgroundContainer.Clear();
        });

        [TestCase(-5)]
        [TestCase(5)]
        public void TestAlwaysSeasonal(int daysOffset)
        {
            registerBackgroundsResponse(DateTimeOffset.Now.AddDays(daysOffset));
            setSeasonalBackgroundMode(SeasonalBackgroundMode.Always);

            createLoader();

            for (int i = 0; i < 4; ++i)
                loadNextBackground();

            AddAssert("all backgrounds cycled", () => new HashSet<string>(textureStore.PerformedLookups).SetEquals(seasonal_background_urls));
        }

        [TestCase(-5)]
        [TestCase(5)]
        public void TestNeverSeasonal(int daysOffset)
        {
            registerBackgroundsResponse(DateTimeOffset.Now.AddDays(daysOffset));
            setSeasonalBackgroundMode(SeasonalBackgroundMode.Never);

            createLoader();

            assertNoBackgrounds();
        }

        [Test]
        public void TestSometimesInSeason()
        {
            registerBackgroundsResponse(DateTimeOffset.Now.AddDays(5));
            setSeasonalBackgroundMode(SeasonalBackgroundMode.Sometimes);

            createLoader();

            assertAnyBackground();
        }

        [Test]
        public void TestSometimesOutOfSeason()
        {
            registerBackgroundsResponse(DateTimeOffset.Now.AddDays(-10));
            setSeasonalBackgroundMode(SeasonalBackgroundMode.Sometimes);

            createLoader();

            assertNoBackgrounds();
        }

        [Test]
        public void TestDelayedConnectivity()
        {
            registerBackgroundsResponse(DateTimeOffset.Now.AddDays(30));
            setSeasonalBackgroundMode(SeasonalBackgroundMode.Always);
            AddStep("go offline", () => dummyAPI.SetState(APIState.Offline));

            createLoader();
            assertNoBackgrounds();

            AddStep("go online", () => dummyAPI.SetState(APIState.Online));

            assertAnyBackground();
        }

        private void registerBackgroundsResponse(DateTimeOffset endDate)
            => AddStep("setup request handler", () =>
            {
                dummyAPI.HandleRequest = request =>
                {
                    if (dummyAPI.State.Value != APIState.Online || !(request is GetSeasonalBackgroundsRequest backgroundsRequest))
                        return;

                    backgroundsRequest.TriggerSuccess(new APISeasonalBackgrounds
                    {
                        Backgrounds = seasonal_background_urls.Select(url => new APISeasonalBackground { Url = url }).ToList(),
                        EndDate = endDate
                    });
                };
            });

        private void setSeasonalBackgroundMode(SeasonalBackgroundMode mode)
            => AddStep($"set seasonal mode to {mode}", () => config.Set(OsuSetting.SeasonalBackgroundMode, mode));

        private void createLoader()
            => AddStep("create loader", () =>
            {
                if (backgroundLoader != null)
                    Remove(backgroundLoader);

                Add(backgroundLoader = new SeasonalBackgroundLoader());
            });

        private void loadNextBackground()
        {
            SeasonalBackground background = null;

            AddStep("create next background", () =>
            {
                background = backgroundLoader.LoadNextBackground();
                LoadComponentAsync(background, bg => backgroundContainer.Child = bg);
            });

            AddUntilStep("background loaded", () => background.IsLoaded);
        }

        private void assertAnyBackground()
        {
            loadNextBackground();
            AddAssert("background looked up", () => textureStore.PerformedLookups.Any());
        }

        private void assertNoBackgrounds()
        {
            AddAssert("no background available", () => backgroundLoader.LoadNextBackground() == null);
            AddAssert("no lookups performed", () => !textureStore.PerformedLookups.Any());
        }

        private class LookupLoggingTextureStore : LargeTextureStore
        {
            public List<string> PerformedLookups { get; } = new List<string>();

            public override Texture Get(string name, WrapMode wrapModeS, WrapMode wrapModeT)
            {
                PerformedLookups.Add(name);
                return base.Get(name, wrapModeS, wrapModeT);
            }
        }
    }
}
