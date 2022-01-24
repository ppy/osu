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
            RunTestWithRealm((realmFactory, _) => { realmFactory.Run(realm => realm.Refresh()); });
        }

        [Test]
        public void TestBlockOperations()
        {
            RunTestWithRealm((realmFactory, _) =>
            {
                using (realmFactory.BlockAllOperations())
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
            RunTestWithRealm((realmFactory, _) =>
            {
                bool callbackRan = false;

                realmFactory.Run(realm =>
                {
                    var subscription = realm.All<BeatmapInfo>().QueryAsyncWithNotifications((sender, changes, error) =>
                    {
                        realmFactory.Run(_ =>
                        {
                            callbackRan = true;
                        });
                    });

                    // Force the callback above to run.
                    realmFactory.Run(r => r.Refresh());

                    subscription?.Dispose();
                });

                Assert.IsTrue(callbackRan);
            });
        }

        [Test]
        public void TestBlockOperationsWithContention()
        {
            RunTestWithRealm((realmFactory, _) =>
            {
                ManualResetEventSlim stopThreadedUsage = new ManualResetEventSlim();
                ManualResetEventSlim hasThreadedUsage = new ManualResetEventSlim();

                Task.Factory.StartNew(() =>
                {
                    realmFactory.Run(_ =>
                    {
                        hasThreadedUsage.Set();

                        stopThreadedUsage.Wait();
                    });
                }, TaskCreationOptions.LongRunning | TaskCreationOptions.HideScheduler);

                hasThreadedUsage.Wait();

                Assert.Throws<TimeoutException>(() =>
                {
                    using (realmFactory.BlockAllOperations())
                    {
                    }
                });

                stopThreadedUsage.Set();
            });
        }
    }
}
