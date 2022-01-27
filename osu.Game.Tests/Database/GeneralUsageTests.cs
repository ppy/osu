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
using osu.Game.Rulesets;

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

        /// <summary>
        /// This test ensures that the <see cref="RealmExtensions.Write"/> and <see cref="RealmExtensions.Write{T}"/> methods
        /// can be used in a nested fashion.
        /// </summary>
        [Test]
        public void TestNestedWrites()
        {
            RunTestWithRealm((realm, __) =>
            {
                realm.Write(r =>
                {
                    r.Add(new RulesetInfo { ShortName = "ruleset1" });
                    checkCountOffThread(0, realm);

                    // check the non-generic variant. note that the braces are required to exercise this.
                    r.Write(r2 =>
                    {
                        r2.Add(new RulesetInfo { ShortName = "ruleset2" });
                    });
                    checkCountOffThread(0, realm);

                    // check the generic variant.
                    _ = r.Write(r3 => r3.Add(new RulesetInfo { ShortName = "ruleset3" }));
                    checkCountOffThread(0, realm);
                });

                int finalCount = realm.Run(r => r.All<RulesetInfo>().Count());
                Assert.AreEqual(3, finalCount);
                checkCountOffThread(3, realm);
            });

            void checkCountOffThread(int expectedCount, RealmAccess realmAccess)
            {
                int actualCount = -1;
                // run a refetch from another thread to check what is the actual count which will be seen by other threads.
                Task.Factory.StartNew(() => actualCount = realmAccess.Run(r1 => r1.All<RulesetInfo>().Count()), TaskCreationOptions.LongRunning | TaskCreationOptions.HideScheduler).WaitSafely();
                Assert.AreEqual(expectedCount, actualCount);
            }
        }
    }
}
