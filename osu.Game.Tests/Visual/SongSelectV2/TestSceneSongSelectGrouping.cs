// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Extensions;
using osu.Game.Models;
using osu.Game.Online.API;
using osu.Game.Screens.Select.Filter;
using osu.Game.Screens.SelectV2;

namespace osu.Game.Tests.Visual.SongSelectV2
{
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

            AddAssert("first collection present", () =>
            {
                var group = grouping.GroupItems.Single(g => g.Key.Title == "My Collection #1");
                return group.Value.Select(i => i.Model).OfType<BeatmapSetInfo>().Single().Equals(beatmapSets[0]);
            });

            AddAssert("second collection present", () =>
            {
                var group = grouping.GroupItems.Single(g => g.Key.Title == "My Collection #2");
                return group.Value.Select(i => i.Model).OfType<BeatmapSetInfo>().Single().Equals(beatmapSets[1]);
            });

            AddAssert("third collection not present", () => grouping.GroupItems.All(g => g.Key.Title != "My Collection #3"));

            AddAssert("no-collection group present", () =>
            {
                var group = grouping.GroupItems.Single(g => g.Key.Title == "Not in collection");
                return group.Value.Select(i => i.Model).OfType<BeatmapSetInfo>().Single().Equals(beatmapSets[2]);
            });
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
                return group.Value.Select(i => i.Model).OfType<BeatmapSetInfo>().Single().Equals(beatmapSet);
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

            AddAssert("collection present", () =>
            {
                var group = grouping.GroupItems.Single(g => g.Key.Title == "My Collection #4");
                return group.Value.Select(i => i.Model).OfType<BeatmapSetInfo>().Single().Equals(beatmapSet);
            });

            AddAssert("no-collection group not present", () => grouping.GroupItems.All(g => g.Key.Title != "Not in collection"));
        }

        #endregion

        #region My Maps grouping

        [Test]
        public void TestMyMapsGrouping()
        {
            ImportBeatmapForRuleset(s => ((RealmUser)s.Metadata.Author).Username = "user1", 0);
            ImportBeatmapForRuleset(s => ((RealmUser)s.Metadata.Author).Username = "user2", 0);
            ImportBeatmapForRuleset(s => ((RealmUser)s.Metadata.Author).Username = "user3", 0);

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

            AddAssert("'my maps' present", () =>
            {
                var group = grouping.GroupItems.Single();
                return group.Key.Title == "My maps" && group.Value.Select(i => i.Model).OfType<BeatmapSetInfo>().Single().Equals(beatmapSets[0]);
            });
        }

        [Test]
        public void TestMyMapsGroupingRenamedUsername()
        {
            ImportBeatmapForRuleset(s =>
            {
                ((RealmUser)s.Metadata.Author).Username = "user1_old";
                ((RealmUser)s.Metadata.Author).OnlineID = DummyAPIAccess.DUMMY_USER_ID;
            }, 0);
            ImportBeatmapForRuleset(s => ((RealmUser)s.Metadata.Author).Username = "user2", 0);
            ImportBeatmapForRuleset(s => ((RealmUser)s.Metadata.Author).Username = "user3", 0);

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

            AddAssert("'my maps' present", () =>
            {
                var group = grouping.GroupItems.Single();
                return group.Key.Title == "My maps" && group.Value.Select(i => i.Model).OfType<BeatmapSetInfo>().Single().Equals(beatmapSets[0]);
            });
        }

        [Test]
        public void TestMyMapsGroupingUpdatesOnUserChange()
        {
            ImportBeatmapForRuleset(s => ((RealmUser)s.Metadata.Author).Username = "user1", 0);
            ImportBeatmapForRuleset(s => ((RealmUser)s.Metadata.Author).Username = "user2", 0);
            ImportBeatmapForRuleset(s => ((RealmUser)s.Metadata.Author).Username = new GuestUser().Username, 0);

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

            AddAssert("'my maps' present", () =>
            {
                var group = grouping.GroupItems.Single();
                return group.Key.Title == "My maps" && group.Value.Select(i => i.Model).OfType<BeatmapSetInfo>().Single().Equals(beatmapSets[1]);
            });
        }

        #endregion

        private NoResultsPlaceholder? getPlaceholder() => SongSelect.ChildrenOfType<NoResultsPlaceholder>().FirstOrDefault();

        private void checkMatchedBeatmaps(int expected) => AddUntilStep($"{expected} matching shown", () => Carousel.MatchedBeatmapsCount, () => Is.EqualTo(expected));
    }
}
