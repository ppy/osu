// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
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
                ILive<BeatmapInfo> beatmap = realmFactory.Run(realm => realm.Write(r => r.Add(new BeatmapInfo(CreateRuleset(), new BeatmapDifficulty(), new BeatmapMetadata()))).ToLive(realmFactory));

                ILive<BeatmapInfo> beatmap2 = realmFactory.Run(realm => realm.All<BeatmapInfo>().First().ToLive(realmFactory));

                Assert.AreEqual(beatmap, beatmap2);
            });
        }

        [Test]
        public void TestAccessAfterStorageMigrate()
        {
            RunTestWithRealm((realmFactory, storage) =>
            {
                var beatmap = new BeatmapInfo(CreateRuleset(), new BeatmapDifficulty(), new BeatmapMetadata());

                ILive<BeatmapInfo>? liveBeatmap = null;

                realmFactory.Run(realm =>
                {
                    realm.Write(r => r.Add(beatmap));

                    liveBeatmap = beatmap.ToLive(realmFactory);
                });

                using (realmFactory.BlockAllOperations())
                {
                    // recycle realm before migrating
                }

                using (var migratedStorage = new TemporaryNativeStorage("realm-test-migration-target"))
                {
                    migratedStorage.DeleteDirectory(string.Empty);

                    storage.Migrate(migratedStorage);

                    Assert.IsFalse(liveBeatmap?.PerformRead(l => l.Hidden));
                }
            });
        }

        [Test]
        public void TestAccessAfterAttach()
        {
            RunTestWithRealm((realmFactory, _) =>
            {
                var beatmap = new BeatmapInfo(CreateRuleset(), new BeatmapDifficulty(), new BeatmapMetadata());

                var liveBeatmap = beatmap.ToLive(realmFactory);

                realmFactory.Run(realm => realm.Write(r => r.Add(beatmap)));

                Assert.IsFalse(liveBeatmap.PerformRead(l => l.Hidden));
            });
        }

        [Test]
        public void TestAccessNonManaged()
        {
            var beatmap = new BeatmapInfo(CreateRuleset(), new BeatmapDifficulty(), new BeatmapMetadata());
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
                ILive<BeatmapInfo>? liveBeatmap = null;
                Task.Factory.StartNew(() =>
                {
                    realmFactory.Run(threadContext =>
                    {
                        var beatmap = threadContext.Write(r => r.Add(new BeatmapInfo(CreateRuleset(), new BeatmapDifficulty(), new BeatmapMetadata())));

                        liveBeatmap = beatmap.ToLive(realmFactory);
                    });
                }, TaskCreationOptions.LongRunning | TaskCreationOptions.HideScheduler).WaitSafely();

                Debug.Assert(liveBeatmap != null);

                Task.Factory.StartNew(() =>
                {
                    liveBeatmap.PerformRead(beatmap =>
                    {
                        Assert.IsTrue(beatmap.IsValid);
                        Assert.IsFalse(beatmap.Hidden);
                    });
                }, TaskCreationOptions.LongRunning | TaskCreationOptions.HideScheduler).WaitSafely();
            });
        }

        [Test]
        public void TestScopedWriteWithoutContext()
        {
            RunTestWithRealm((realmFactory, _) =>
            {
                ILive<BeatmapInfo>? liveBeatmap = null;
                Task.Factory.StartNew(() =>
                {
                    realmFactory.Run(threadContext =>
                    {
                        var beatmap = threadContext.Write(r => r.Add(new BeatmapInfo(CreateRuleset(), new BeatmapDifficulty(), new BeatmapMetadata())));

                        liveBeatmap = beatmap.ToLive(realmFactory);
                    });
                }, TaskCreationOptions.LongRunning | TaskCreationOptions.HideScheduler).WaitSafely();

                Debug.Assert(liveBeatmap != null);

                Task.Factory.StartNew(() =>
                {
                    liveBeatmap.PerformWrite(beatmap => { beatmap.Hidden = true; });
                    liveBeatmap.PerformRead(beatmap => { Assert.IsTrue(beatmap.Hidden); });
                }, TaskCreationOptions.LongRunning | TaskCreationOptions.HideScheduler).WaitSafely();
            });
        }

        [Test]
        public void TestValueAccessNonManaged()
        {
            RunTestWithRealm((realmFactory, _) =>
            {
                var beatmap = new BeatmapInfo(CreateRuleset(), new BeatmapDifficulty(), new BeatmapMetadata());
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
                ILive<BeatmapInfo>? liveBeatmap = null;

                Task.Factory.StartNew(() =>
                {
                    realmFactory.Run(threadContext =>
                    {
                        var beatmap = threadContext.Write(r => r.Add(new BeatmapInfo(CreateRuleset(), new BeatmapDifficulty(), new BeatmapMetadata())));

                        liveBeatmap = beatmap.ToLive(realmFactory);
                    });
                }, TaskCreationOptions.LongRunning | TaskCreationOptions.HideScheduler).WaitSafely();

                Debug.Assert(liveBeatmap != null);

                Task.Factory.StartNew(() =>
                {
                    // Can't be used, without a valid context.
                    Assert.Throws<InvalidOperationException>(() =>
                    {
                        var __ = liveBeatmap.Value;
                    });

                    // Can't be used, even from within a valid context.
                    realmFactory.Run(threadContext =>
                    {
                        Assert.Throws<InvalidOperationException>(() =>
                        {
                            var __ = liveBeatmap.Value;
                        });
                    });
                }, TaskCreationOptions.LongRunning | TaskCreationOptions.HideScheduler).WaitSafely();
            });
        }

        [Test]
        public void TestValueAccessWithoutOpenContextFails()
        {
            RunTestWithRealm((realmFactory, _) =>
            {
                ILive<BeatmapInfo>? liveBeatmap = null;
                Task.Factory.StartNew(() =>
                {
                    realmFactory.Run(threadContext =>
                    {
                        var beatmap = threadContext.Write(r => r.Add(new BeatmapInfo(CreateRuleset(), new BeatmapDifficulty(), new BeatmapMetadata())));

                        liveBeatmap = beatmap.ToLive(realmFactory);
                    });
                }, TaskCreationOptions.LongRunning | TaskCreationOptions.HideScheduler).WaitSafely();

                Debug.Assert(liveBeatmap != null);

                Task.Factory.StartNew(() =>
                {
                    Assert.Throws<InvalidOperationException>(() =>
                    {
                        var unused = liveBeatmap.Value;
                    });
                }, TaskCreationOptions.LongRunning | TaskCreationOptions.HideScheduler).WaitSafely();
            });
        }

        [Test]
        public void TestLiveAssumptions()
        {
            RunTestWithRealm((realmFactory, _) =>
            {
                int changesTriggered = 0;

                realmFactory.RegisterCustomSubscription(outerRealm =>
                {
                    outerRealm.All<BeatmapInfo>().QueryAsyncWithNotifications(gotChange);
                    ILive<BeatmapInfo>? liveBeatmap = null;

                    Task.Factory.StartNew(() =>
                    {
                        realmFactory.Run(innerRealm =>
                        {
                            var ruleset = CreateRuleset();
                            var beatmap = innerRealm.Write(r => r.Add(new BeatmapInfo(ruleset, new BeatmapDifficulty(), new BeatmapMetadata())));

                            // add a second beatmap to ensure that a full refresh occurs below.
                            // not just a refresh from the resolved Live.
                            innerRealm.Write(r => r.Add(new BeatmapInfo(ruleset, new BeatmapDifficulty(), new BeatmapMetadata())));

                            liveBeatmap = beatmap.ToLive(realmFactory);
                        });
                    }, TaskCreationOptions.LongRunning | TaskCreationOptions.HideScheduler).WaitSafely();

                    Debug.Assert(liveBeatmap != null);

                    // not yet seen by main context
                    Assert.AreEqual(0, outerRealm.All<BeatmapInfo>().Count());
                    Assert.AreEqual(0, changesTriggered);

                    liveBeatmap.PerformRead(resolved =>
                    {
                        // retrieval causes an implicit refresh. even changes that aren't related to the retrieval are fired at this point.
                        // ReSharper disable once AccessToDisposedClosure
                        Assert.AreEqual(2, outerRealm.All<BeatmapInfo>().Count());
                        Assert.AreEqual(1, changesTriggered);

                        // can access properties without a crash.
                        Assert.IsFalse(resolved.Hidden);

                        // ReSharper disable once AccessToDisposedClosure
                        outerRealm.Write(r =>
                        {
                            // can use with the main context.
                            r.Remove(resolved);
                        });
                    });

                    return null;
                });

                void gotChange(IRealmCollection<BeatmapInfo> sender, ChangeSet changes, Exception error)
                {
                    changesTriggered++;
                }
            });
        }
    }
}
