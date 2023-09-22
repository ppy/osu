// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapSet;
using osu.Game.Rulesets;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Testing;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.BeatmapSet.Scores;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.Select.Details;
using APIUser = osu.Game.Online.API.Requests.Responses.APIUser;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneBeatmapSetOverlay : OsuManualInputManagerTestScene
    {
        private readonly TestBeatmapSetOverlay overlay;

        private int nextBeatmapSetId = 1;

        public TestSceneBeatmapSetOverlay()
        {
            Add(overlay = new TestBeatmapSetOverlay());
        }

        [Resolved]
        private IRulesetStore rulesets { get; set; } = null!;

        [SetUp]
        public void SetUp() => Schedule(() => SelectedMods.Value = Array.Empty<Mod>());

        [Test]
        public void TestLoading()
        {
            AddStep(@"show loading", () => overlay.ShowBeatmapSet(null));
        }

        [Test]
        public void TestLocalBeatmaps()
        {
            AddStep(@"show first", () =>
            {
                overlay.ShowBeatmapSet(new APIBeatmapSet
                {
                    Genre = new BeatmapSetOnlineGenre { Id = 15, Name = "Future genre" },
                    Language = new BeatmapSetOnlineLanguage { Id = 15, Name = "Future language" },
                    OnlineID = 1235,
                    Title = @"an awesome beatmap",
                    Artist = @"naru narusegawa",
                    Source = @"hinata sou",
                    Tags = @"test tag tag more tag",
                    Author = new APIUser
                    {
                        Username = @"BanchoBot",
                        Id = 3,
                    },
                    Preview = @"https://b.ppy.sh/preview/12345.mp3",
                    PlayCount = 123,
                    FavouriteCount = 456,
                    Submitted = DateTime.Now,
                    Ranked = DateTime.Now,
                    BPM = 111,
                    HasVideo = true,
                    Ratings = Enumerable.Range(0, 11).ToArray(),
                    HasStoryboard = true,
                    Covers = new BeatmapSetOnlineCovers(),
                    Beatmaps = new[]
                    {
                        new APIBeatmap
                        {
                            StarRating = 9.99,
                            DifficultyName = @"TEST",
                            Length = 456000,
                            HitLength = 400000,
                            RulesetID = 3,
                            CircleSize = 1,
                            DrainRate = 2.3f,
                            OverallDifficulty = 4.5f,
                            ApproachRate = 6,
                            CircleCount = 111,
                            SliderCount = 12,
                            PlayCount = 222,
                            PassCount = 21,
                            FailTimes = new APIFailTimes
                            {
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                            },
                        },
                    },
                });
            });

            downloadAssert(true);

            AddStep("show many difficulties", () => overlay.ShowBeatmapSet(createManyDifficultiesBeatmapSet()));
            downloadAssert(true);

            AddAssert("status is loved", () => overlay.ChildrenOfType<BeatmapSetOnlineStatusPill>().Single().Status == BeatmapOnlineStatus.Loved);
            AddAssert("scores container is visible", () => overlay.ChildrenOfType<ScoresContainer>().Single().Alpha == 1);
            AddAssert("mod selector is visible", () => overlay.ChildrenOfType<LeaderboardModSelector>().Single().Alpha == 1);

            AddStep("go to second beatmap", () => overlay.ChildrenOfType<BeatmapPicker.DifficultySelectorButton>().ElementAt(1).TriggerClick());

            AddAssert("status is graveyard", () => overlay.ChildrenOfType<BeatmapSetOnlineStatusPill>().Single().Status == BeatmapOnlineStatus.Graveyard);
            AddAssert("scores container is hidden", () => overlay.ChildrenOfType<ScoresContainer>().Single().Alpha == 0);
        }

        [Test]
        public void TestAvailability()
        {
            AddStep(@"show undownloadable", () =>
            {
                var set = getBeatmapSet();

                set.Availability = new BeatmapSetOnlineAvailability
                {
                    DownloadDisabled = true,
                    ExternalLink = "https://osu.ppy.sh",
                };

                overlay.ShowBeatmapSet(set);
            });

            downloadAssert(false);
        }

        [Test]
        public void TestMultipleRulesets()
        {
            AddStep("show multiple rulesets beatmap", () =>
            {
                var beatmaps = new List<APIBeatmap>();

                foreach (var ruleset in rulesets.AvailableRulesets.Skip(1))
                {
                    beatmaps.Add(new APIBeatmap
                    {
                        DifficultyName = ruleset.Name,
                        RulesetID = ruleset.OnlineID,
                        FailTimes = new APIFailTimes
                        {
                            Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                            Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                        },
                    });
                }

                var set = getBeatmapSet();

                set.Beatmaps = beatmaps.ToArray();

                overlay.ShowBeatmapSet(set);
            });

            AddAssert("shown beatmaps of current ruleset", () => overlay.Header.HeaderContent.Picker.Difficulties.All(b => b.Beatmap.Ruleset.OnlineID == overlay.Header.RulesetSelector.Current.Value.OnlineID));
            AddAssert("left-most beatmap selected", () => overlay.Header.HeaderContent.Picker.Difficulties.First().State == BeatmapPicker.DifficultySelectorState.Selected);
        }

        [Test]
        public void TestExplicitBeatmap()
        {
            AddStep("show explicit map", () =>
            {
                var beatmapSet = getBeatmapSet();
                beatmapSet.HasExplicitContent = true;
                overlay.ShowBeatmapSet(beatmapSet);
            });
        }

        [Test]
        public void TestSpotlightBeatmap()
        {
            AddStep("show spotlight map", () =>
            {
                var beatmapSet = getBeatmapSet();
                beatmapSet.FeaturedInSpotlight = true;
                overlay.ShowBeatmapSet(beatmapSet);
            });
        }

        [Test]
        public void TestFeaturedBeatmap()
        {
            AddStep("show featured map", () =>
            {
                var beatmapSet = getBeatmapSet();
                beatmapSet.TrackId = 1;
                overlay.ShowBeatmapSet(beatmapSet);
            });
        }

        [Test]
        public void TestAllBadgesBeatmap()
        {
            AddStep("show map with all badges", () =>
            {
                var beatmapSet = getBeatmapSet();
                beatmapSet.HasExplicitContent = true;
                beatmapSet.FeaturedInSpotlight = true;
                beatmapSet.TrackId = 1;
                overlay.ShowBeatmapSet(beatmapSet);
            });
        }

        [Test]
        public void TestSelectedModsDontAffectStatistics()
        {
            AddStep("show map", () => overlay.ShowBeatmapSet(getBeatmapSet()));
            AddAssert("AR displayed as 0", () => overlay.ChildrenOfType<AdvancedStats.StatisticRow>().Single(s => s.Title == BeatmapsetsStrings.ShowStatsAr).Value == (0, null));
            AddStep("set AR10 diff adjust", () => SelectedMods.Value = new[]
            {
                new OsuModDifficultyAdjust
                {
                    ApproachRate = { Value = 10 }
                }
            });
            AddAssert("AR still displayed as 0", () => overlay.ChildrenOfType<AdvancedStats.StatisticRow>().Single(s => s.Title == BeatmapsetsStrings.ShowStatsAr).Value == (0, null));
        }

        [Test]
        public void TestHide()
        {
            AddStep(@"hide", overlay.Hide);
        }

        [Test]
        public void TestShowWithNoReload()
        {
            AddStep(@"show without reload", overlay.Show);
        }

        [TestCase(BeatmapSetLookupType.BeatmapId)]
        [TestCase(BeatmapSetLookupType.SetId)]
        public void TestFetchLookupType(BeatmapSetLookupType lookupType)
        {
            string type = string.Empty;

            AddStep("register request handling", () =>
            {
                ((DummyAPIAccess)API).HandleRequest = req =>
                {
                    switch (req)
                    {
                        case GetBeatmapSetRequest getBeatmapSet:
                            type = getBeatmapSet.Type.ToString();
                            return true;
                    }

                    return false;
                };
            });

            AddStep(@"fetch", () =>
            {
                switch (lookupType)
                {
                    case BeatmapSetLookupType.BeatmapId:
                        overlay.FetchAndShowBeatmap(55);
                        break;

                    case BeatmapSetLookupType.SetId:
                        overlay.FetchAndShowBeatmapSet(55);
                        break;
                }
            });

            AddAssert(@"type is correct", () => type == lookupType.ToString());
        }

        [Test]
        public void TestBeatmapSetWithGuestDifficulty()
        {
            AddStep("show map", () => overlay.ShowBeatmapSet(createBeatmapSetWithGuestDifficulty()));
            AddStep("move mouse to host difficulty", () =>
            {
                InputManager.MoveMouseTo(overlay.ChildrenOfType<DifficultyIcon>().ElementAt(0));
            });
            AddAssert("guest mapper information not shown", () => overlay.ChildrenOfType<BeatmapPicker>().Single().ChildrenOfType<OsuSpriteText>().All(s => s.Text != "BanchoBot"));
            AddStep("move mouse to guest difficulty", () =>
            {
                InputManager.MoveMouseTo(overlay.ChildrenOfType<DifficultyIcon>().ElementAt(1));
            });
            AddAssert("guest mapper information shown", () => overlay.ChildrenOfType<BeatmapPicker>().Single().ChildrenOfType<OsuSpriteText>().Any(s => s.Text == "BanchoBot"));
        }

        private APIBeatmapSet createManyDifficultiesBeatmapSet()
        {
            var set = getBeatmapSet();

            var beatmaps = new List<APIBeatmap>();

            for (int i = 1; i < 41; i++)
            {
                beatmaps.Add(new APIBeatmap
                {
                    OnlineID = i * 10,
                    DifficultyName = $"Test #{i}",
                    RulesetID = Ruleset.Value.OnlineID,
                    StarRating = 2 + i * 0.1,
                    OverallDifficulty = 3.5f,
                    FailTimes = new APIFailTimes
                    {
                        Fails = Enumerable.Range(1, 100).Select(j => j % 12 - 6).ToArray(),
                        Retries = Enumerable.Range(-2, 100).Select(j => j % 12 - 6).ToArray(),
                    },
                    Status = i % 2 == 0 ? BeatmapOnlineStatus.Graveyard : BeatmapOnlineStatus.Loved,
                });
            }

            set.Beatmaps = beatmaps.ToArray();

            return set;
        }

        private APIBeatmapSet getBeatmapSet()
        {
            var beatmapSet = CreateAPIBeatmapSet(Ruleset.Value);

            // Make sure the overlay is reloaded (see `BeatmapSetInfo.Equals`).
            beatmapSet.OnlineID = nextBeatmapSetId++;

            return beatmapSet;
        }

        private APIBeatmapSet createBeatmapSetWithGuestDifficulty()
        {
            var set = getBeatmapSet();

            var beatmaps = new List<APIBeatmap>();

            var guestUser = new APIUser
            {
                Username = @"BanchoBot",
                Id = 3,
            };

            set.RelatedUsers = new[]
            {
                set.Author, guestUser
            };

            beatmaps.Add(new APIBeatmap
            {
                OnlineID = 1145,
                DifficultyName = "Host Diff",
                RulesetID = Ruleset.Value.OnlineID,
                StarRating = 1.4,
                OverallDifficulty = 3.5f,
                AuthorID = set.AuthorID,
                FailTimes = new APIFailTimes
                {
                    Fails = Enumerable.Range(1, 100).Select(j => j % 12 - 6).ToArray(),
                    Retries = Enumerable.Range(-2, 100).Select(j => j % 12 - 6).ToArray(),
                },
                Status = BeatmapOnlineStatus.Graveyard
            });

            beatmaps.Add(new APIBeatmap
            {
                OnlineID = 1919,
                DifficultyName = "Guest Diff",
                RulesetID = Ruleset.Value.OnlineID,
                StarRating = 8.1,
                OverallDifficulty = 3.5f,
                AuthorID = 3,
                FailTimes = new APIFailTimes
                {
                    Fails = Enumerable.Range(1, 100).Select(j => j % 12 - 6).ToArray(),
                    Retries = Enumerable.Range(-2, 100).Select(j => j % 12 - 6).ToArray(),
                },
                Status = BeatmapOnlineStatus.Graveyard
            });

            set.Beatmaps = beatmaps.ToArray();

            return set;
        }

        private void downloadAssert(bool shown)
        {
            AddAssert($"is download button {(shown ? "shown" : "hidden")}", () => overlay.Header.HeaderContent.DownloadButtonsVisible == shown);
        }

        private partial class TestBeatmapSetOverlay : BeatmapSetOverlay
        {
            public new BeatmapSetHeader Header => base.Header;
        }
    }
}
