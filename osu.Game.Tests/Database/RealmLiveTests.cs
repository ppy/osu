// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Models;
using Realms;

#nullable enable

namespace osu.Game.Tests.Database
{
    public class RealmLiveTests : RealmTest
    {
        [Test]
        public void TestLiveEquality()
        {
            RunTestWithRealm((realmFactory, _) =>
            {
                ILive<RealmBeatmap> beatmap = realmFactory.CreateContext().Write(r => r.Add(new RealmBeatmap(CreateRuleset(), new RealmBeatmapDifficulty(), new RealmBeatmapMetadata()))).ToLive(realmFactory);

                ILive<RealmBeatmap> beatmap2 = realmFactory.CreateContext().All<RealmBeatmap>().First().ToLive(realmFactory);

                Assert.AreEqual(beatmap, beatmap2);
            });
        }

        [Test]
        public void TestAccessAfterStorageMigrate()
        {
            RunTestWithRealm((realmFactory, storage) =>
            {
                var beatmap = new RealmBeatmap(CreateRuleset(), new RealmBeatmapDifficulty(), new RealmBeatmapMetadata());

                ILive<RealmBeatmap> liveBeatmap;

                using (var context = realmFactory.CreateContext())
                {
                    context.Write(r => r.Add(beatmap));

                    liveBeatmap = beatmap.ToLive(realmFactory);
                }

                using (var migratedStorage = new TemporaryNativeStorage("realm-test-migration-target"))
                {
                    migratedStorage.DeleteDirectory(string.Empty);

                    storage.Migrate(migratedStorage);

                    Assert.IsFalse(liveBeatmap.PerformRead(l => l.Hidden));
                }
            });
        }

        [Test]
        public void TestAccessAfterAttach()
        {
            RunTestWithRealm((realmFactory, _) =>
            {
                var beatmap = new RealmBeatmap(CreateRuleset(), new RealmBeatmapDifficulty(), new RealmBeatmapMetadata());

                var liveBeatmap = beatmap.ToLive(realmFactory);

                using (var context = realmFactory.CreateContext())
                    context.Write(r => r.Add(beatmap));

                Assert.IsFalse(liveBeatmap.PerformRead(l => l.Hidden));
            });
        }

        [Test]
        public void TestAccessNonManaged()
        {
            var beatmap = new RealmBeatmap(CreateRuleset(), new RealmBeatmapDifficulty(), new RealmBeatmapMetadata());
            var liveBeatmap = beatmap.ToLiveUnmanaged();

            Assert.IsFalse(beatmap.Hidden);
            Assert.IsFalse(liveBeatmap.Value.Hidden);
            Assert.IsFalse(liveBeatmap.PerformRead(l => l.Hidden));

            Assert.Throws<InvalidOperationException>(() => liveBeatmap.PerformWrite(l => l.Hidden = true));

            Assert.IsFalse(beatmap.Hidden);
            Assert.IsFalse(liveBeatmap.Value.Hidden);
            Assert.IsFalse(liveBeatmap.PerformRead(l => l.Hidden));
        }

        [Test]
        public void TestScopedReadWithoutContext()
        {
            RunTestWithRealm((realmFactory, _) =>
            {
                ILive<RealmBeatmap>? liveBeatmap = null;
                Task.Factory.StartNew(() =>
                {
                    using (var threadContext = realmFactory.CreateContext())
                    {
                        var beatmap = threadContext.Write(r => r.Add(new RealmBeatmap(CreateRuleset(), new RealmBeatmapDifficulty(), new RealmBeatmapMetadata())));

                        liveBeatmap = beatmap.ToLive(realmFactory);
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
                ILive<RealmBeatmap>? liveBeatmap = null;
                Task.Factory.StartNew(() =>
                {
                    using (var threadContext = realmFactory.CreateContext())
                    {
                        var beatmap = threadContext.Write(r => r.Add(new RealmBeatmap(CreateRuleset(), new RealmBeatmapDifficulty(), new RealmBeatmapMetadata())));

                        liveBeatmap = beatmap.ToLive(realmFactory);
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
        public void TestValueAccessNonManaged()
        {
            RunTestWithRealm((realmFactory, _) =>
            {
                var beatmap = new RealmBeatmap(CreateRuleset(), new RealmBeatmapDifficulty(), new RealmBeatmapMetadata());
                var liveBeatmap = beatmap.ToLive(realmFactory);

                Assert.DoesNotThrow(() =>
                {
                    var __ = liveBeatmap.Value;
                });
            });
        }

        [Test]
        public void TestValueAccessWithOpenContextFails()
        {
            RunTestWithRealm((realmFactory, _) =>
            {
                ILive<RealmBeatmap>? liveBeatmap = null;

                Task.Factory.StartNew(() =>
                {
                    using (var threadContext = realmFactory.CreateContext())
                    {
                        var beatmap = threadContext.Write(r => r.Add(new RealmBeatmap(CreateRuleset(), new RealmBeatmapDifficulty(), new RealmBeatmapMetadata())));

                        liveBeatmap = beatmap.ToLive(realmFactory);
                    }
                }, TaskCreationOptions.LongRunning | TaskCreationOptions.HideScheduler).Wait();

                Debug.Assert(liveBeatmap != null);

                Task.Factory.StartNew(() =>
                {
                    // Can't be used, without a valid context.
                    Assert.Throws<InvalidOperationException>(() =>
                    {
                        var __ = liveBeatmap.Value;
                    });

                    // Can't be used, even from within a valid context.
                    using (realmFactory.CreateContext())
                    {
                        Assert.Throws<InvalidOperationException>(() =>
                        {
                            var __ = liveBeatmap.Value;
                        });
                    }
                }, TaskCreationOptions.LongRunning | TaskCreationOptions.HideScheduler).Wait();
            });
        }

        [Test]
        public void TestValueAccessWithoutOpenContextFails()
        {
            RunTestWithRealm((realmFactory, _) =>
            {
                ILive<RealmBeatmap>? liveBeatmap = null;
                Task.Factory.StartNew(() =>
                {
                    using (var threadContext = realmFactory.CreateContext())
                    {
                        var beatmap = threadContext.Write(r => r.Add(new RealmBeatmap(CreateRuleset(), new RealmBeatmapDifficulty(), new RealmBeatmapMetadata())));

                        liveBeatmap = beatmap.ToLive(realmFactory);
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
                    updateThreadContext.All<RealmBeatmap>().QueryAsyncWithNotifications(gotChange);
                    ILive<RealmBeatmap>? liveBeatmap = null;

                    Task.Factory.StartNew(() =>
                    {
                        using (var threadContext = realmFactory.CreateContext())
                        {
                            var ruleset = CreateRuleset();
                            var beatmap = threadContext.Write(r => r.Add(new RealmBeatmap(ruleset, new RealmBeatmapDifficulty(), new RealmBeatmapMetadata())));

                            // add a second beatmap to ensure that a full refresh occurs below.
                            // not just a refresh from the resolved Live.
                            threadContext.Write(r => r.Add(new RealmBeatmap(ruleset, new RealmBeatmapDifficulty(), new RealmBeatmapMetadata())));

                            liveBeatmap = beatmap.ToLive(realmFactory);
                        }
                    }, TaskCreationOptions.LongRunning | TaskCreationOptions.HideScheduler).Wait();

                    Debug.Assert(liveBeatmap != null);

                    // not yet seen by main context
                    Assert.AreEqual(0, updateThreadContext.All<RealmBeatmap>().Count());
                    Assert.AreEqual(0, changesTriggered);

                    liveBeatmap.PerformRead(resolved =>
                    {
                        // retrieval causes an implicit refresh. even changes that aren't related to the retrieval are fired at this point.
                        // ReSharper disable once AccessToDisposedClosure
                        Assert.AreEqual(2, updateThreadContext.All<RealmBeatmap>().Count());
                        Assert.AreEqual(1, changesTriggered);

                        // can access properties without a crash.
                        Assert.IsFalse(resolved.Hidden);

                        // ReSharper disable once AccessToDisposedClosure
                        updateThreadContext.Write(r =>
                        {
                            // can use with the main context.
                            r.Remove(resolved);
                        });
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
