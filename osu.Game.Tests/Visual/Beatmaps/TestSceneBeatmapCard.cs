// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Beatmaps.Drawables.Cards.Buttons;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Beatmaps
{
    public partial class TestSceneBeatmapCard : OsuManualInputManagerTestScene
    {
        /// <summary>
        /// All cards on this scene use a common online ID to ensure that map download, preview tracks, etc. can be tested manually with online sources.
        /// </summary>
        private const int online_id = 163112;

        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        private APIBeatmapSet[] testCases;

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        #region Test case generation

        [BackgroundDependencyLoader]
        private void load()
        {
            var normal = CreateAPIBeatmapSet(Ruleset.Value);
            normal.HasVideo = true;
            normal.HasStoryboard = true;

            var withStatistics = CreateAPIBeatmapSet(Ruleset.Value);
            withStatistics.Title = withStatistics.TitleUnicode = "play favourite stats";
            withStatistics.Status = BeatmapOnlineStatus.Approved;
            withStatistics.FavouriteCount = 284_239;
            withStatistics.PlayCount = 999_001;
            withStatistics.Ranked = DateTimeOffset.Now.AddDays(-45);
            withStatistics.HypeStatus = new BeatmapSetHypeStatus
            {
                Current = 34,
                Required = 5
            };
            withStatistics.NominationStatus = new BeatmapSetNominationStatus
            {
                Current = 1,
                Required = 2
            };

            var undownloadable = getUndownloadableBeatmapSet();
            undownloadable.LastUpdated = DateTimeOffset.Now.AddYears(-1);

            var someDifficulties = getManyDifficultiesBeatmapSet(11);
            someDifficulties.Title = someDifficulties.TitleUnicode = "favourited";
            someDifficulties.Title = someDifficulties.TitleUnicode = "some difficulties";
            someDifficulties.Status = BeatmapOnlineStatus.Qualified;
            someDifficulties.HasFavourited = true;
            someDifficulties.FavouriteCount = 1;
            someDifficulties.NominationStatus = new BeatmapSetNominationStatus
            {
                Current = 2,
                Required = 2
            };

            var manyDifficulties = getManyDifficultiesBeatmapSet(100);
            manyDifficulties.Status = BeatmapOnlineStatus.Pending;

            var explicitMap = CreateAPIBeatmapSet(Ruleset.Value);
            explicitMap.Title = someDifficulties.TitleUnicode = "explicit beatmap";
            explicitMap.HasExplicitContent = true;

            var spotlightMap = CreateAPIBeatmapSet(Ruleset.Value);
            spotlightMap.Title = someDifficulties.TitleUnicode = "spotlight beatmap";
            spotlightMap.FeaturedInSpotlight = true;

            var featuredMap = CreateAPIBeatmapSet(Ruleset.Value);
            featuredMap.Title = someDifficulties.TitleUnicode = "featured artist beatmap";
            featuredMap.TrackId = 1;

            var allBadgesMap = CreateAPIBeatmapSet(Ruleset.Value);
            allBadgesMap.Title = someDifficulties.TitleUnicode = "all-badges beatmap";
            allBadgesMap.HasExplicitContent = true;
            allBadgesMap.FeaturedInSpotlight = true;
            allBadgesMap.TrackId = 2;

            var longName = CreateAPIBeatmapSet(Ruleset.Value);
            longName.Title = longName.TitleUnicode = "this track has an incredibly and implausibly long title";
            longName.Artist = longName.ArtistUnicode = "and this artist! who would have thunk it. it's really such a long name.";
            longName.Source = "wow. even the source field has an impossibly long string in it. this really takes the cake, doesn't it?";
            longName.HasExplicitContent = true;
            longName.FeaturedInSpotlight = true;
            longName.TrackId = 444;

            testCases = new[]
            {
                normal,
                withStatistics,
                undownloadable,
                someDifficulties,
                manyDifficulties,
                explicitMap,
                spotlightMap,
                featuredMap,
                allBadgesMap,
                longName
            };

            foreach (var testCase in testCases)
                testCase.OnlineID = online_id;
        }

        private APIBeatmapSet getUndownloadableBeatmapSet() => new APIBeatmapSet
        {
            OnlineID = 123,
            Title = "undownloadable beatmap",
            Artist = "test",
            Source = "more tests",
            Author = new APIUser
            {
                Username = "BanchoBot",
                Id = 3,
            },
            Availability = new BeatmapSetOnlineAvailability
            {
                DownloadDisabled = true,
            },
            Preview = @"https://b.ppy.sh/preview/12345.mp3",
            PlayCount = 123,
            FavouriteCount = 456,
            BPM = 111,
            HasVideo = true,
            HasStoryboard = true,
            Covers = new BeatmapSetOnlineCovers(),
            Beatmaps = new[]
            {
                new APIBeatmap
                {
                    RulesetID = Ruleset.Value.OnlineID,
                    DifficultyName = "Test",
                    StarRating = 6.42,
                }
            }
        };

        private static APIBeatmapSet getManyDifficultiesBeatmapSet(int count)
        {
            var beatmaps = new List<APIBeatmap>();

            for (int i = 0; i < count; i++)
            {
                beatmaps.Add(new APIBeatmap
                {
                    RulesetID = i % 4,
                    StarRating = 2 + i % 4 * 2,
                });
            }

            return new APIBeatmapSet
            {
                OnlineID = 1,
                Title = "many difficulties beatmap",
                Artist = "test",
                Author = new APIUser
                {
                    Username = "BanchoBot",
                    Id = 3,
                },
                HasVideo = true,
                HasStoryboard = true,
                Covers = new BeatmapSetOnlineCovers(),
                Beatmaps = beatmaps.ToArray(),
            };
        }

        #endregion

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("register request handling", () => dummyAPI.HandleRequest = request =>
            {
                if (!(request is PostBeatmapFavouriteRequest))
                    return false;

                request.TriggerSuccess();
                return true;
            });

            ensureSoleilyRemoved();
        }

        private void ensureSoleilyRemoved()
        {
            AddUntilStep("ensure manager loaded", () => beatmaps != null);
            AddStep("remove map", () =>
            {
                var beatmap = beatmaps.QueryBeatmapSet(b => b.OnlineID == online_id);

                if (beatmap != null) beatmaps.Delete(beatmap.Value);
            });
        }

        private Drawable createContent(OverlayColourScheme colourScheme, Func<APIBeatmapSet, Drawable> creationFunc)
        {
            var colourProvider = new OverlayColourProvider(colourScheme);

            return new DependencyProvidingContainer
            {
                RelativeSizeAxes = Axes.Both,
                CachedDependencies = new (Type, object)[]
                {
                    (typeof(OverlayColourProvider), colourProvider)
                },
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background5
                    },
                    new BasicScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = new ReverseChildIDFillFlowContainer<Drawable>
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Full,
                            Padding = new MarginPadding(10),
                            Spacing = new Vector2(10),
                            ChildrenEnumerable = testCases.Select(creationFunc)
                        }
                    }
                }
            };
        }

        private void createTestCase(Func<APIBeatmapSet, Drawable> creationFunc)
        {
            foreach (var scheme in Enum.GetValues(typeof(OverlayColourScheme)).Cast<OverlayColourScheme>())
                AddStep($"set {scheme} scheme", () => Child = createContent(scheme, creationFunc));
        }

        [Test]
        public void TestNano()
        {
            createTestCase(beatmapSetInfo => new BeatmapCardNano(beatmapSetInfo));
        }

        [Test]
        public void TestNormal()
        {
            createTestCase(beatmapSetInfo => new BeatmapCardNormal(beatmapSetInfo));
        }

        [Test]
        public void TestExtra()
        {
            createTestCase(beatmapSetInfo => new BeatmapCardExtra(beatmapSetInfo));
        }

        [Test]
        public void TestHoverState()
        {
            AddStep("create cards", () => Child = createContent(OverlayColourScheme.Blue, s => new BeatmapCardNormal(s)));

            AddStep("Hover card", () => InputManager.MoveMouseTo(firstCard()));
            AddWaitStep("wait for potential state change", 5);
            AddAssert("card is not expanded", () => !firstCard().Expanded.Value);

            AddStep("Hover spectrum display", () => InputManager.MoveMouseTo(firstCard().ChildrenOfType<DifficultySpectrumDisplay>().Single()));
            AddUntilStep("card is expanded", () => firstCard().Expanded.Value);

            AddStep("Hover difficulty content", () => InputManager.MoveMouseTo(firstCard().ChildrenOfType<BeatmapCardDifficultyList>().Single()));
            AddWaitStep("wait for potential state change", 5);
            AddAssert("card is still expanded", () => firstCard().Expanded.Value);

            AddStep("Hover main content again", () => InputManager.MoveMouseTo(firstCard()));
            AddWaitStep("wait for potential state change", 5);
            AddAssert("card is still expanded", () => firstCard().Expanded.Value);

            AddStep("Hover away", () => InputManager.MoveMouseTo(this.ChildrenOfType<BeatmapCardNormal>().Last()));
            AddUntilStep("card is not expanded", () => !firstCard().Expanded.Value);

            BeatmapCardNormal firstCard() => this.ChildrenOfType<BeatmapCardNormal>().First();
        }

        [Test]
        public void TestPlayButtonByTouchInput()
        {
            AddStep("create cards", () => Child = createContent(OverlayColourScheme.Blue, beatmapSetInfo => new BeatmapCardNormal(beatmapSetInfo)));

            // mimics touch input
            AddStep("touch play button area on first card", () =>
            {
                InputManager.MoveMouseTo(firstCard().ChildrenOfType<PlayButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("first card is playing", () => firstCard().ChildrenOfType<PlayButton>().Single().Playing.Value);

            BeatmapCardNormal firstCard() => this.ChildrenOfType<BeatmapCardNormal>().First();
        }
    }
}
