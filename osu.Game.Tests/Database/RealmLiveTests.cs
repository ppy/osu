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

namespace osu.Game.Tests.Database
{
    public class RealmLiveTests : RealmTest
    {
        [Test]
        public void TestLiveEquality()
        {
            RunTestWithRealm((realm, _) =>
            {
                Live<BeatmapInfo> beatmap = realm.Run(r => r.Write(_ => r.Add(new BeatmapInfo(CreateRuleset(), new BeatmapDifficulty(), new BeatmapMetadata()))).ToLive(realm));

                Live<BeatmapInfo> beatmap2 = realm.Run(r => r.All<BeatmapInfo>().First().ToLive(realm));

                Assert.AreEqual(beatmap, beatmap2);
            });
        }

        [Test]
        public void TestAccessAfterStorageMigrate()
        {
            using (var migratedStorage = new TemporaryNativeStorage("realm-test-migration-target"))
            {
                RunTestWithRealm((realm, storage) =>
                {
                    var beatmap = new BeatmapInfo(CreateRuleset(), new BeatmapDifficulty(), new BeatmapMetadata());

                    Live<BeatmapInfo>? liveBeatmap = null;

                    realm.Run(r =>
                    {
                        r.Write(_ => r.Add(beatmap));

                        liveBeatmap = beatmap.ToLive(realm);
                    });

                    migratedStorage.DeleteDirectory(string.Empty);

                    using (realm.BlockAllOperations("testing"))
                        storage.Migrate(migratedStorage);

                    Assert.IsFalse(liveBeatmap?.PerformRead(l => l.Hidden));
                });
            }
        }

        [Test]
        public void TestFailedWritePerformsRollback()
        {
            RunTestWithRealm((realm, _) =>
            {
                Assert.Throws<InvalidOperationException>(() =>
                {
                    realm.Write(r =>
                    {
                        r.Add(new BeatmapInfo(CreateRuleset(), new BeatmapDifficulty(), new BeatmapMetadata()));
                        throw new InvalidOperationException();
                    });
                });

                Assert.That(realm.Run(r => r.All<BeatmapInfo>()), Is.Empty);
            });
        }

        [Test]
        public void TestFailedNestedWritePerformsRollback()
        {
            RunTestWithRealm((realm, _) =>
            {
                Assert.Throws<InvalidOperationException>(() =>
                {
                    realm.Write(r =>
                    {
                        realm.Write(_ =>
                        {
                            r.Add(new BeatmapInfo(CreateRuleset(), new BeatmapDifficulty(), new BeatmapMetadata()));
                            throw new InvalidOperationException();
                        });
                    });
                });

                Assert.That(realm.Run(r => r.All<BeatmapInfo>()), Is.Empty);
            });
        }

        [Test]
        public void TestNestedWriteCalls()
        {
            RunTestWithRealm((realm, _) =>
            {
                var beatmap = new BeatmapInfo(CreateRuleset(), new BeatmapDifficulty(), new BeatmapMetadata());

                var liveBeatmap = beatmap.ToLive(realm);

                realm.Run(r =>
                    r.Write(_ =>
                        r.Write(_ =>
                            r.Add(beatmap)))
                );

                Assert.IsFalse(liveBeatmap.PerformRead(l => l.Hidden));
            });
        }

        [Test]
        public void TestAccessAfterAttach()
        {
            RunTestWithRealm((realm, _) =>
            {
                var beatmap = new BeatmapInfo(CreateRuleset(), new BeatmapDifficulty(), new BeatmapMetadata());

                var liveBeatmap = beatmap.ToLive(realm);

                realm.Run(r => r.Write(_ => r.Add(beatmap)));

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
        public void TestTransactionRolledBackOnException()
        {
            RunTestWithRealm((realm, _) =>
            {
                var beatmap = new BeatmapInfo(CreateRuleset(), new BeatmapDifficulty(), new BeatmapMetadata());

                realm.Run(r => r.Write(_ => r.Add(beatmap)));

                var liveBeatmap = beatmap.ToLive(realm);

                Assert.Throws<InvalidOperationException>(() => liveBeatmap.PerformWrite(l => throw new InvalidOperationException()));
                Assert.IsFalse(liveBeatmap.PerformRead(l => l.Hidden));

                liveBeatmap.PerformWrite(l => l.Hidden = true);
                Assert.IsTrue(liveBeatmap.PerformRead(l => l.Hidden));
            });
        }

        [Test]
        public void TestScopedReadWithoutContext()
        {
            RunTestWithRealm((realm, _) =>
            {
                Live<BeatmapInfo>? liveBeatmap = null;
                Task.Factory.StartNew(() =>
                {
                    realm.Run(threadContext =>
                    {
                        var beatmap = threadContext.Write(r => r.Add(new BeatmapInfo(CreateRuleset(), new BeatmapDifficulty(), new BeatmapMetadata())));

                        liveBeatmap = beatmap.ToLive(realm);
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
            RunTestWithRealm((realm, _) =>
            {
                Live<BeatmapInfo>? liveBeatmap = null;
                Task.Factory.StartNew(() =>
                {
                    realm.Run(threadContext =>
                    {
                        var beatmap = threadContext.Write(r => r.Add(new BeatmapInfo(CreateRuleset(), new BeatmapDifficulty(), new BeatmapMetadata())));

                        liveBeatmap = beatmap.ToLive(realm);
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
            RunTestWithRealm((realm, _) =>
            {
                var beatmap = new BeatmapInfo(CreateRuleset(), new BeatmapDifficulty(), new BeatmapMetadata());
                var liveBeatmap = beatmap.ToLive(realm);

                Assert.DoesNotThrow(() =>
                {
                    var __ = liveBeatmap.Value;
                });
            });
        }

        [Test]
        public void TestValueAccessWithOpenContextFails()
        {
            RunTestWithRealm((realm, _) =>
            {
                Live<BeatmapInfo>? liveBeatmap = null;

                Task.Factory.StartNew(() =>
                {
                    realm.Run(threadContext =>
                    {
                        var beatmap = threadContext.Write(r => r.Add(new BeatmapInfo(CreateRuleset(), new BeatmapDifficulty(), new BeatmapMetadata())));

                        liveBeatmap = beatmap.ToLive(realm);
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
                    realm.Run(_ =>
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
            RunTestWithRealm((realm, _) =>
            {
                Live<BeatmapInfo>? liveBeatmap = null;
                Task.Factory.StartNew(() =>
                {
                    realm.Run(threadContext =>
                    {
                        var beatmap = threadContext.Write(r => r.Add(new BeatmapInfo(CreateRuleset(), new BeatmapDifficulty(), new BeatmapMetadata())));

                        liveBeatmap = beatmap.ToLive(realm);
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
            RunTestWithRealm((realm, _) =>
            {
                int changesTriggered = 0;

                realm.RegisterCustomSubscription(outerRealm =>
                {
                    outerRealm.All<BeatmapInfo>().QueryAsyncWithNotifications(gotChange);
                    Live<BeatmapInfo>? liveBeatmap = null;

                    Task.Factory.StartNew(() =>
                    {
                        realm.Run(innerRealm =>
                        {
                            var ruleset = CreateRuleset();
                            var beatmap = innerRealm.Write(r => r.Add(new BeatmapInfo(ruleset, new BeatmapDifficulty(), new BeatmapMetadata())));

                            // add a second beatmap to ensure that a full refresh occurs below.
                            // not just a refresh from the resolved Live.
                            innerRealm.Write(r => r.Add(new BeatmapInfo(ruleset, new BeatmapDifficulty(), new BeatmapMetadata())));

                            liveBeatmap = beatmap.ToLive(realm);
                        });
                    }, TaskCreationOptions.LongRunning | TaskCreationOptions.HideScheduler).WaitSafely();

                    Debug.Assert(liveBeatmap != null);

                    // not yet seen by main context
                    Assert.AreEqual(0, outerRealm.All<BeatmapInfo>().Count());
                    Assert.AreEqual(0, changesTriggered);

                    liveBeatmap.PerformRead(resolved =>
                    {
                        // retrieval causes an implicit refresh. even changes that aren't related to the retrieval are fired at this point.
                        Assert.AreEqual(2, outerRealm.All<BeatmapInfo>().Count());
                        Assert.AreEqual(1, changesTriggered);

                        // can access properties without a crash.
                        Assert.IsFalse(resolved.Hidden);

                        outerRealm.Write(r =>
                        {
                            // can use with the main context.
                            r.Remove(resolved);
                        });
                    });

                    return null;
                });

                void gotChange(IRealmCollection<BeatmapInfo> sender, ChangeSet? changes)
                {
                    changesTriggered++;
                }
            });
        }
    }
}
