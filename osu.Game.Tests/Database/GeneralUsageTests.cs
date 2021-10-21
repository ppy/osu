// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

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
