// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Game.Beatmaps;
using osu.Game.Scoring;

namespace osu.Game.Tests.Database
{
    [TestFixture]
    public class ReplayBookmarkDatabaseTests : RealmTest
    {
        [Test]
        public void TestBookmarksPersist()
        {
            RunTestWithRealmAsync(async (realm, _) =>
            {
                Guid id = Guid.NewGuid();

                await realm.WriteAsync(r =>
                {
                    var ruleset = CreateRuleset();
                    r.Add(ruleset);
                    r.Add(new ScoreInfo(ruleset: ruleset, beatmap: new BeatmapInfo(ruleset: ruleset)) { ID = id });
                });

                await realm.WriteAsync(r =>
                {
                    var si = r.Find<ScoreInfo>(id)!;
                    si.ReplayBookmarks.Add(1000);
                    si.ReplayBookmarks.Add(5000);
                    si.ReplayBookmarks.Add(12000);
                });

                realm.Run(r => r.Refresh());

                int[] bookmarks = realm.Run(r => r.Find<ScoreInfo>(id)!.ReplayBookmarks.ToArray());
                Assert.That(bookmarks, Is.EqualTo(new[] { 1000, 5000, 12000 }));
            });
        }

        [Test]
        public void TestBookmarksClearedAndReplaced()
        {
            RunTestWithRealmAsync(async (realm, _) =>
            {
                Guid id = Guid.NewGuid();

                await realm.WriteAsync(r =>
                {
                    var ruleset = CreateRuleset();
                    r.Add(ruleset);
                    r.Add(new ScoreInfo(ruleset: ruleset, beatmap: new BeatmapInfo(ruleset: ruleset)) { ID = id });
                });

                await realm.WriteAsync(r =>
                {
                    var si = r.Find<ScoreInfo>(id)!;
                    si.ReplayBookmarks.Add(1000);
                    si.ReplayBookmarks.Add(2000);
                });

                await realm.WriteAsync(r =>
                {
                    var si = r.Find<ScoreInfo>(id)!;
                    si.ReplayBookmarks.Clear();
                    si.ReplayBookmarks.Add(9000);
                });

                realm.Run(r => r.Refresh());

                int[] bookmarks = realm.Run(r => r.Find<ScoreInfo>(id)!.ReplayBookmarks.ToArray());
                Assert.That(bookmarks, Is.EqualTo(new[] { 9000 }));
            });
        }

        [Test]
        public void TestNoBookmarksReturnsEmpty()
        {
            RunTestWithRealmAsync(async (realm, _) =>
            {
                Guid id = Guid.NewGuid();

                await realm.WriteAsync(r =>
                {
                    var ruleset = CreateRuleset();
                    r.Add(ruleset);
                    r.Add(new ScoreInfo(ruleset: ruleset, beatmap: new BeatmapInfo(ruleset: ruleset)) { ID = id });
                });

                realm.Run(r => r.Refresh());

                int[] bookmarks = realm.Run(r => r.Find<ScoreInfo>(id)!.ReplayBookmarks.ToArray());
                Assert.That(bookmarks, Is.Empty);
            });
        }
    }
}
