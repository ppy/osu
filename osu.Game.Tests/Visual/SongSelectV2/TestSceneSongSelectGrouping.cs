// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Extensions;
using osu.Game.Models;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Scoring;
using osu.Game.Screens.Select.Filter;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    /// <summary>
    /// Test suite for grouping modes which require the presence of API / realm.
    /// All other grouping modes are tested separately in <see cref="BeatmapCarouselFilterGroupingTest"/>.
    /// </summary>
    public partial class TestSceneSongSelectGrouping : SongSelectTestScene
    {
        private BeatmapCarouselFilterGrouping grouping => Carousel.Filters.OfType<BeatmapCarouselFilterGrouping>().Single();

        [SetUp]
        public void SetUp() => Schedule(() => API.Logout());

        #region Collection grouping

        [Test]
        public void TestCollectionGrouping()
        {
            ImportBeatmapForRuleset(0);
            ImportBeatmapForRuleset(0);
            ImportBeatmapForRuleset(0);

            BeatmapSetInfo[] beatmapSets = null!;

            AddStep("add collections", () =>
            {
                beatmapSets = Beatmaps.GetAllUsableBeatmapSets().OrderBy(b => b.OnlineID).ToArray();

                Realm.Write(r =>
                {
                    r.RemoveAll<BeatmapCollection>();
                    r.Add(new BeatmapCollection("My Collection #1", beatmapSets[0].Beatmaps.Select(b => b.MD5Hash).ToList()));
                    r.Add(new BeatmapCollection("My Collection #2", beatmapSets[1].Beatmaps.Select(b => b.MD5Hash).ToList()));
                    r.Add(new BeatmapCollection("My Collection #3"));
                });
            });

            LoadSongSelect();
            GroupBy(GroupMode.Collections);
            WaitForFiltering();

            assertGroupPresent("My Collection #1", () => new[] { beatmapSets[0] });
            assertGroupPresent("My Collection #2", () => new[] { beatmapSets[1] });
            assertGroupPresent("Not in collection", () => new[] { beatmapSets[2] });
            assertGroupsCount(3);
        }

        [Test]
        public void TestCollectionGroupingUpdatesOnChange()
        {
            ImportBeatmapForRuleset(0);

            BeatmapSetInfo beatmapSet = null!;

            AddStep("add collections", () =>
            {
                beatmapSet = Beatmaps.GetAllUsableBeatmapSets().Single();

                Realm.Write(r =>
                {
                    r.RemoveAll<BeatmapCollection>();
                    r.Add(new BeatmapCollection("My Collection #4"));
                });
            });

            LoadSongSelect();
            GroupBy(GroupMode.Collections);
            WaitForFiltering();

            AddAssert("collection not present", () => grouping.GroupItems.All(g => g.Key.Title != "My Collection #4"));

            AddAssert("no-collection group present", () =>
            {
                var group = grouping.GroupItems.Single(g => g.Key.Title == "Not in collection");
                return group.Value.Select(i => i.Model).OfType<GroupedBeatmapSet>().Single().BeatmapSet.Equals(beatmapSet);
            });

            AddStep("add beatmap to collection", () =>
            {
                Realm.Write(r =>
                {
                    var collection = r.All<BeatmapCollection>().Single();
                    collection.BeatmapMD5Hashes.AddRange(beatmapSet.Beatmaps.Select(b => b.MD5Hash));
                });
            });

            WaitForFiltering();

            assertGroupPresent("My Collection #4", () => new[] { beatmapSet });
            assertGroupsCount(1);
        }

        #endregion

        #region My Maps grouping

        [Test]
        public void TestMyMapsGrouping()
        {
            ImportBeatmapForRuleset(s => ((RealmUser)s.Metadata.Author).Username = "user1", 3, 0);
            ImportBeatmapForRuleset(s => ((RealmUser)s.Metadata.Author).Username = "user2", 3, 0);
            ImportBeatmapForRuleset(s => ((RealmUser)s.Metadata.Author).Username = "user3", 3, 0);

            BeatmapSetInfo[] beatmapSets = null!;

            AddStep("get beatmaps", () => beatmapSets = Beatmaps.GetAllUsableBeatmapSets().OrderBy(b => b.OnlineID).ToArray());

            AddStep("log in", () =>
            {
                API.Login("user1", string.Empty);
                API.AuthenticateSecondFactor("abcdefgh");
            });

            LoadSongSelect();
            GroupBy(GroupMode.MyMaps);
            WaitForFiltering();

            assertGroupPresent("My maps", () => new[] { beatmapSets[0] });
            assertGroupsCount(1);
        }

        [Test]
        public void TestMyMapsGroupingRenamedUsername()
        {
            ImportBeatmapForRuleset(s =>
            {
                ((RealmUser)s.Metadata.Author).Username = "user1_old";
                ((RealmUser)s.Metadata.Author).OnlineID = DummyAPIAccess.DUMMY_USER_ID;
            }, 3, 0);
            ImportBeatmapForRuleset(s => ((RealmUser)s.Metadata.Author).Username = "user2", 3, 0);
            ImportBeatmapForRuleset(s => ((RealmUser)s.Metadata.Author).Username = "user3", 3, 0);

            BeatmapSetInfo[] beatmapSets = null!;

            AddStep("get beatmaps", () => beatmapSets = Beatmaps.GetAllUsableBeatmapSets().OrderBy(b => b.OnlineID).ToArray());

            AddStep("log in", () =>
            {
                API.Login("user1", string.Empty);
                API.AuthenticateSecondFactor("abcdefgh");
            });

            LoadSongSelect();
            GroupBy(GroupMode.MyMaps);
            WaitForFiltering();

            assertGroupPresent("My maps", () => new[] { beatmapSets[0] });
            assertGroupsCount(1);
        }

        [Test]
        public void TestMyMapsGroupingUpdatesOnUserChange()
        {
            ImportBeatmapForRuleset(s => ((RealmUser)s.Metadata.Author).Username = "user1", 3, 0);
            ImportBeatmapForRuleset(s => ((RealmUser)s.Metadata.Author).Username = "user2", 3, 0);
            ImportBeatmapForRuleset(s => ((RealmUser)s.Metadata.Author).Username = new GuestUser().Username, 3, 0);

            BeatmapSetInfo[] beatmapSets = null!;

            AddStep("get beatmaps", () => beatmapSets = Beatmaps.GetAllUsableBeatmapSets().OrderBy(b => b.OnlineID).ToArray());

            // stay logged out

            LoadSongSelect();
            GroupBy(GroupMode.MyMaps);
            WaitForFiltering();

            AddUntilStep("wait for placeholder visible", () => getPlaceholder()?.State.Value == Visibility.Visible);
            checkMatchedBeatmaps(0);

            AddStep("log in", () =>
            {
                API.Login("user2", string.Empty);
                API.AuthenticateSecondFactor("abcdefgh");
            });

            WaitForFiltering();

            assertGroupPresent("My maps", () => new[] { beatmapSets[1] });
            assertGroupsCount(1);
        }

        #endregion

        #region Rank Achieved grouping

        [Test]
        public void TestRankAchievedGrouping()
        {
            ImportBeatmapForRuleset(_ => { }, 1, 0);
            ImportBeatmapForRuleset(_ => { }, 1, 0);
            ImportBeatmapForRuleset(_ => { }, 1, 0);
            ImportBeatmapForRuleset(_ => { }, 1, 0);
            ImportBeatmapForRuleset(_ => { }, 1, 0);

            AddStep("log in", () =>
            {
                API.Login("user1", string.Empty); // match username in test scores.
                API.AuthenticateSecondFactor("abcdefgh");
            });

            BeatmapSetInfo[] beatmapSets = null!;

            AddStep("add scores", () =>
            {
                beatmapSets = Beatmaps.GetAllUsableBeatmapSets().OrderBy(b => b.OnlineID).ToArray();

                ScoreManager.Import(createTestScoreInfo(beatmapSets[0].Beatmaps[0], ScoreRank.SH));
                ScoreManager.Import(createTestScoreInfo(beatmapSets[1].Beatmaps[0], ScoreRank.A));
                ScoreManager.Import(createTestScoreInfo(beatmapSets[2].Beatmaps[0], ScoreRank.C));

                // score belonging to another user on an unplayed beatmap.
                ScoreManager.Import(createTestScoreInfo(beatmapSets[3].Beatmaps[0], ScoreRank.XH, s => s.User = new APIUser { Id = 1337, Username = "user2" }));

                // score belonging to another user on a played beatmap.
                ScoreManager.Import(createTestScoreInfo(beatmapSets[0].Beatmaps[0], ScoreRank.XH, s => s.User = new APIUser { Id = 1337, Username = "user2" }));

                // score belonging to local user but with less rank.
                ScoreManager.Import(createTestScoreInfo(beatmapSets[0].Beatmaps[0], ScoreRank.D));
            });

            LoadSongSelect();
            GroupBy(GroupMode.RankAchieved);
            WaitForFiltering();

            assertGroupPresent("Silver S", () => new[] { beatmapSets[0] });
            assertGroupPresent("A", () => new[] { beatmapSets[1] });
            assertGroupPresent("C", () => new[] { beatmapSets[2] });
            assertGroupPresent("Unplayed", () => new[] { beatmapSets[3], beatmapSets[4] });
            assertGroupsCount(4);
        }

        #endregion

        #region Benchmarks

        [Test]
        [Explicit("Manual benchmark")]
        public void TestPerformance()
        {
            const int sets_count = 100;
            const int diffs_count = 100;

            AddStep("log in", () =>
            {
                API.Login("user1", string.Empty); // match username in test scores.
                API.AuthenticateSecondFactor("abcdefgh");
            });

            int count = 0;

            AddStep("populate database", () =>
            {
                count = 0;

                Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < sets_count; i++)
                    {
                        var liveSet = Beatmaps.Import(TestResources.CreateTestBeatmapSetInfo(diffs_count, Rulesets.AvailableRulesets.ToArray()))!;

                        liveSet.PerformRead(s =>
                        {
                            foreach (var beatmap in s.Beatmaps
                                                     .GroupBy(b => b.Ruleset.OnlineID)
                                                     .Select(g => g.OrderBy(_ => RNG.Next()).Take(4)) // take 4 difficulties from each ruleset randomly
                                                     .SelectMany(g => g))
                            {
                                for (int k = 0; k < 3; k++) // create 3 scores per difficulty
                                    ScoreManager.Import(createTestScoreInfo(beatmap));
                            }
                        });

                        count++;
                    }
                }, TaskCreationOptions.LongRunning);
            });

            AddUntilStep("wait for population", () => count, () => Is.GreaterThan(sets_count / 3));
            AddUntilStep("this takes a while", () => count, () => Is.GreaterThan(sets_count / 3 * 2));
            AddUntilStep("maybe they are done now", () => count, () => Is.EqualTo(sets_count));

            LoadSongSelect();
        }

        #endregion

        private void assertGroupsCount(int expected)
        {
            AddAssert($"groups = {expected}", () => grouping.GroupItems, () => Has.Count.EqualTo(expected));
        }

        private void assertGroupPresent(string name, Func<IEnumerable<BeatmapSetInfo>> getBeatmaps)
        {
            AddAssert($"\"{name}\" present", () =>
            {
                var group = grouping.GroupItems.Single(g => g.Key.Title.ToString() == name);
                var actualBeatmaps = group.Value.Select(i => i.Model).OfType<GroupedBeatmap>().Select(gb => gb.Beatmap).OrderBy(b => b.ID);
                var expectedBeatmaps = getBeatmaps().SelectMany(s => s.Beatmaps).OrderBy(b => b.ID);
                return actualBeatmaps.SequenceEqual(expectedBeatmaps);
            });
        }

        private NoResultsPlaceholder? getPlaceholder() => SongSelect.ChildrenOfType<NoResultsPlaceholder>().FirstOrDefault();

        private void checkMatchedBeatmaps(int expected) => AddUntilStep($"{expected} matching shown", () => Carousel.MatchedBeatmapsCount, () => Is.EqualTo(expected));

        private ScoreInfo createTestScoreInfo(BeatmapInfo beatmap, ScoreRank? rank = null, Action<ScoreInfo>? applyToScore = null)
        {
            var score = TestResources.CreateTestScoreInfo(beatmap);
            score.User = API.LocalUser.Value;
            score.Rank = rank ?? Enum.GetValues<ScoreRank>().MinBy(_ => RNG.Next());
            score.TotalScore = (long)(((double)score.Rank + 1) / (Enum.GetValues<ScoreRank>().Length + 1) * 1000000);
            score.Date = DateTimeOffset.Now;
            applyToScore?.Invoke(score);
            return score;
        }
    }
}
