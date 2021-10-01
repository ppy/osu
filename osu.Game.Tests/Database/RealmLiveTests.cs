// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Models;
using Realms;

#nullable enable

namespace osu.Game.Tests.Database
{
    public class RealmLiveTests : RealmTest
    {
        [Test]
        public void TestLiveCastability()
        {
            RunTestWithRealm((realmFactory, _) =>
            {
                RealmLive<RealmBeatmap> beatmap = realmFactory.CreateContext().Write(r => r.Add(new RealmBeatmap(CreateRuleset(), new RealmBeatmapDifficulty(), new RealmBeatmapMetadata()))).ToLive();

                ILive<IBeatmapInfo> iBeatmap = beatmap;

                Assert.AreEqual(0, iBeatmap.Value.Length);
            });
        }

        [Test]
        public void TestValueAccessWithOpenContext()
        {
            RunTestWithRealm((realmFactory, _) =>
            {
                RealmLive<RealmBeatmap>? liveBeatmap = null;
                Task.Factory.StartNew(() =>
                {
                    using (var threadContext = realmFactory.CreateContext())
                    {
                        var beatmap = threadContext.Write(r => r.Add(new RealmBeatmap(CreateRuleset(), new RealmBeatmapDifficulty(), new RealmBeatmapMetadata())));

                        liveBeatmap = beatmap.ToLive();
                    }
                }, TaskCreationOptions.LongRunning | TaskCreationOptions.HideScheduler).Wait();

                Debug.Assert(liveBeatmap != null);

                Task.Factory.StartNew(() =>
                {
                    Assert.DoesNotThrow(() =>
                    {
                        using (realmFactory.CreateContext())
                        {
                            var resolved = liveBeatmap.Value;

                            Assert.IsTrue(resolved.Realm.IsClosed);
                            Assert.IsTrue(resolved.IsValid);

                            // can access properties without a crash.
                            Assert.IsFalse(resolved.Hidden);
                        }
                    });
                }, TaskCreationOptions.LongRunning | TaskCreationOptions.HideScheduler).Wait();
            });
        }

        [Test]
        public void TestScopedReadWithoutContext()
        {
            RunTestWithRealm((realmFactory, _) =>
            {
                RealmLive<RealmBeatmap>? liveBeatmap = null;
                Task.Factory.StartNew(() =>
                {
                    using (var threadContext = realmFactory.CreateContext())
                    {
                        var beatmap = threadContext.Write(r => r.Add(new RealmBeatmap(CreateRuleset(), new RealmBeatmapDifficulty(), new RealmBeatmapMetadata())));

                        liveBeatmap = beatmap.ToLive();
                    }
                }, TaskCreationOptions.LongRunning | TaskCreationOptions.HideScheduler).Wait();

                Debug.Assert(liveBeatmap != null);

                Task.Factory.StartNew(() =>
                {
                    liveBeatmap.PerformRead(beatmap =>
                    {
                        Assert.IsTrue(beatmap.IsValid);
                        Assert.IsFalse(beatmap.Hidden);
                    });
                }, TaskCreationOptions.LongRunning | TaskCreationOptions.HideScheduler).Wait();
            });
        }

        [Test]
        public void TestScopedWriteWithoutContext()
        {
            RunTestWithRealm((realmFactory, _) =>
            {
                RealmLive<RealmBeatmap>? liveBeatmap = null;
                Task.Factory.StartNew(() =>
                {
                    using (var threadContext = realmFactory.CreateContext())
                    {
                        var beatmap = threadContext.Write(r => r.Add(new RealmBeatmap(CreateRuleset(), new RealmBeatmapDifficulty(), new RealmBeatmapMetadata())));

                        liveBeatmap = beatmap.ToLive();
                    }
                }, TaskCreationOptions.LongRunning | TaskCreationOptions.HideScheduler).Wait();

                Debug.Assert(liveBeatmap != null);

                Task.Factory.StartNew(() =>
                {
                    liveBeatmap.PerformWrite(beatmap => { beatmap.Hidden = true; });
                    liveBeatmap.PerformRead(beatmap => { Assert.IsTrue(beatmap.Hidden); });
                }, TaskCreationOptions.LongRunning | TaskCreationOptions.HideScheduler).Wait();
            });
        }

        [Test]
        public void TestValueAccessWithoutOpenContextFails()
        {
            RunTestWithRealm((realmFactory, _) =>
            {
                RealmLive<RealmBeatmap>? liveBeatmap = null;
                Task.Factory.StartNew(() =>
                {
                    using (var threadContext = realmFactory.CreateContext())
                    {
                        var beatmap = threadContext.Write(r => r.Add(new RealmBeatmap(CreateRuleset(), new RealmBeatmapDifficulty(), new RealmBeatmapMetadata())));

                        liveBeatmap = beatmap.ToLive();
                    }
                }, TaskCreationOptions.LongRunning | TaskCreationOptions.HideScheduler).Wait();

                Debug.Assert(liveBeatmap != null);

                Task.Factory.StartNew(() =>
                {
                    Assert.Throws<InvalidOperationException>(() =>
                    {
                        var unused = liveBeatmap.Value;
                    });
                }, TaskCreationOptions.LongRunning | TaskCreationOptions.HideScheduler).Wait();
            });
        }

        [Test]
        public void TestLiveAssumptions()
        {
            RunTestWithRealm((realmFactory, _) =>
            {
                int changesTriggered = 0;

                using (var updateThreadContext = realmFactory.CreateContext())
                {
                    updateThreadContext.All<RealmBeatmap>().SubscribeForNotifications(gotChange);
                    RealmLive<RealmBeatmap>? liveBeatmap = null;

                    Task.Factory.StartNew(() =>
                    {
                        using (var threadContext = realmFactory.CreateContext())
                        {
                            var ruleset = CreateRuleset();
                            var beatmap = threadContext.Write(r => r.Add(new RealmBeatmap(ruleset, new RealmBeatmapDifficulty(), new RealmBeatmapMetadata())));

                            // add a second beatmap to ensure that a full refresh occurs below.
                            // not just a refresh from the resolved Live.
                            threadContext.Write(r => r.Add(new RealmBeatmap(ruleset, new RealmBeatmapDifficulty(), new RealmBeatmapMetadata())));

                            liveBeatmap = beatmap.ToLive();
                        }
                    }, TaskCreationOptions.LongRunning | TaskCreationOptions.HideScheduler).Wait();

                    Debug.Assert(liveBeatmap != null);

                    // not yet seen by main context
                    Assert.AreEqual(0, updateThreadContext.All<RealmBeatmap>().Count());
                    Assert.AreEqual(0, changesTriggered);

                    var resolved = liveBeatmap.Value;

                    // retrieval causes an implicit refresh. even changes that aren't related to the retrieval are fired at this point.
                    Assert.AreEqual(2, updateThreadContext.All<RealmBeatmap>().Count());
                    Assert.AreEqual(1, changesTriggered);

                    // even though the realm that this instance was resolved for was closed, it's still valid.
                    Assert.IsTrue(resolved.Realm.IsClosed);
                    Assert.IsTrue(resolved.IsValid);

                    // can access properties without a crash.
                    Assert.IsFalse(resolved.Hidden);

                    updateThreadContext.Write(r =>
                    {
                        // can use with the main context.
                        r.Remove(resolved);
                    });
                }

                void gotChange(IRealmCollection<RealmBeatmap> sender, ChangeSet changes, Exception error)
                {
                    changesTriggered++;
                }
            });
        }
    }
}
