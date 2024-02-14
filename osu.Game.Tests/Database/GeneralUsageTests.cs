// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Database
{
    [TestFixture]
    public class GeneralUsageTests : RealmTest
    {
        /// <summary>
        /// Just test the construction of a new database works.
        /// </summary>
        [Test]
        public void TestConstructRealm()
        {
            RunTestWithRealm((realm, _) => { realm.Run(r => r.Refresh()); });
        }

        [Test]
        public void TestBlockOperations()
        {
            RunTestWithRealm((realm, _) =>
            {
                using (realm.BlockAllOperations("testing"))
                {
                }
            });
        }

        [Test]
        public void TestAsyncWriteAsync()
        {
            RunTestWithRealmAsync(async (realm, _) =>
            {
                await realm.WriteAsync(r => r.Add(TestResources.CreateTestBeatmapSetInfo()));

                realm.Run(r => r.Refresh());

                Assert.That(realm.Run(r => r.All<BeatmapSetInfo>().Count()), Is.EqualTo(1));
            });
        }

        [Test]
        public void TestAsyncWriteWhileBlocking()
        {
            RunTestWithRealm((realm, _) =>
            {
                Task writeTask;

                using (realm.BlockAllOperations("testing"))
                {
                    writeTask = realm.WriteAsync(r => r.Add(TestResources.CreateTestBeatmapSetInfo()));
                    Thread.Sleep(100);
                    Assert.That(writeTask.IsCompleted, Is.False);
                }

                writeTask.WaitSafely();

                realm.Run(r => r.Refresh());
                Assert.That(realm.Run(r => r.All<BeatmapSetInfo>().Count()), Is.EqualTo(1));
            });
        }

        [Test]
        public void TestAsyncWrite()
        {
            RunTestWithRealm((realm, _) =>
            {
                realm.WriteAsync(r => r.Add(TestResources.CreateTestBeatmapSetInfo())).WaitSafely();

                realm.Run(r => r.Refresh());

                Assert.That(realm.Run(r => r.All<BeatmapSetInfo>().Count()), Is.EqualTo(1));
            });
        }

        [Test]
        public void TestAsyncWriteAfterDisposal()
        {
            RunTestWithRealm((realm, _) =>
            {
                realm.Dispose();
                Assert.ThrowsAsync<ObjectDisposedException>(() => realm.WriteAsync(r => r.Add(TestResources.CreateTestBeatmapSetInfo())));
            });
        }

        [Test]
        public void TestAsyncWriteBeforeDisposal()
        {
            ManualResetEventSlim resetEvent = new ManualResetEventSlim();

            RunTestWithRealm((realm, _) =>
            {
                var writeTask = realm.WriteAsync(r =>
                {
                    // ensure that disposal blocks for our execution
                    Assert.That(resetEvent.Wait(100), Is.False);

                    r.Add(TestResources.CreateTestBeatmapSetInfo());
                });

                realm.Dispose();
                resetEvent.Set();

                writeTask.WaitSafely();
            });
        }

        /// <summary>
        /// Test to ensure that a `CreateContext` call nested inside a subscription doesn't cause any deadlocks
        /// due to context fetching semaphores.
        /// </summary>
        [Test]
        public void TestNestedContextCreationWithSubscription()
        {
            RunTestWithRealm((realm, _) =>
            {
                bool callbackRan = false;

                realm.RegisterCustomSubscription(r =>
                {
                    var subscription = r.All<BeatmapInfo>().QueryAsyncWithNotifications((_, _) =>
                    {
                        realm.Run(_ =>
                        {
                            callbackRan = true;
                        });
                    });

                    // Force the callback above to run.
                    realm.Run(rr => rr.Refresh());

                    subscription?.Dispose();
                    return null;
                });

                Assert.IsTrue(callbackRan);
            });
        }

        [Test]
        public void TestBlockOperationsWithContention()
        {
            RunTestWithRealm((realm, _) =>
            {
                ManualResetEventSlim stopThreadedUsage = new ManualResetEventSlim();
                ManualResetEventSlim hasThreadedUsage = new ManualResetEventSlim();

                Task.Factory.StartNew(() =>
                {
                    realm.Run(_ =>
                    {
                        hasThreadedUsage.Set();

                        stopThreadedUsage.Wait(60000);
                    });
                }, TaskCreationOptions.LongRunning | TaskCreationOptions.HideScheduler);

                hasThreadedUsage.Wait(60000);

                Assert.Throws<TimeoutException>(() =>
                {
                    using (realm.BlockAllOperations("testing"))
                    {
                    }
                });

                stopThreadedUsage.Set();

                // Ensure we can block a second time after the usage has ended.
                using (realm.BlockAllOperations("testing"))
                {
                }
            });
        }
    }
}
