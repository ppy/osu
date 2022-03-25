// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Database;

#nullable enable

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
                using (realm.BlockAllOperations())
                {
                }
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
                    var subscription = r.All<BeatmapInfo>().QueryAsyncWithNotifications((sender, changes, error) =>
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

                        stopThreadedUsage.Wait();
                    });
                }, TaskCreationOptions.LongRunning | TaskCreationOptions.HideScheduler);

                hasThreadedUsage.Wait();

                Assert.Throws<TimeoutException>(() =>
                {
                    using (realm.BlockAllOperations())
                    {
                    }
                });

                stopThreadedUsage.Set();

                // Ensure we can block a second time after the usage has ended.
                using (realm.BlockAllOperations())
                {
                }
            });
        }
    }
}
