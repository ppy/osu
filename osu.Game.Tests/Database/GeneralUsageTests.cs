// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Game.Database;
using osu.Game.Models;
using Realms;

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
            RunTestWithRealm((realmFactory, _) => { realmFactory.CreateContext().Refresh(); });
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

        [Test]
        public void TestNestedContextCreation()
        {
            RunTestWithRealm((realmFactory, _) =>
            {
                var mainContext = realmFactory.Context;
                bool callbackRan = false;

                var subscription = mainContext.All<RealmBeatmap>().SubscribeForNotifications((sender, changes, error) =>
                {
                    realmFactory.CreateContext();
                    callbackRan = true;
                });

                Task.Factory.StartNew(() =>
                {
                    using (var threadContext = realmFactory.CreateContext())
                    {
                        threadContext.Write(r => r.Add(new RealmBeatmap(CreateRuleset(), new RealmBeatmapDifficulty(), new RealmBeatmapMetadata())));
                    }
                }, TaskCreationOptions.LongRunning | TaskCreationOptions.HideScheduler).Wait();

                // will create a context but also run the callback above (Refresh is implicitly run when getting a new context).
                realmFactory.CreateContext();

                Assert.IsTrue(callbackRan);

                subscription.Dispose();
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
                    using (realmFactory.CreateContext())
                    {
                        hasThreadedUsage.Set();

                        stopThreadedUsage.Wait();
                    }
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
