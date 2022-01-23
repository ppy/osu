// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Tests.Resources;
using Realms;

#nullable enable

namespace osu.Game.Tests.Database
{
    [TestFixture]
    public class RealmSubscriptionRegistrationTests : RealmTest
    {
        [Test]
        public void TestSubscriptionWithContextLoss()
        {
            IEnumerable<BeatmapSetInfo>? resolvedItems = null;
            ChangeSet? lastChanges = null;

            RunTestWithRealm((realmFactory, _) =>
            {
                realmFactory.Write(realm => realm.Add(TestResources.CreateTestBeatmapSetInfo()));

                var registration = realmFactory.Register(realm => realm.All<BeatmapSetInfo>(), onChanged);

                testEventsArriving(true);

                // All normal until here.
                // Now let's yank the main realm context.
                resolvedItems = null;
                lastChanges = null;

                using (realmFactory.BlockAllOperations())
                    Assert.That(resolvedItems, Is.Empty);

                realmFactory.Write(realm => realm.Add(TestResources.CreateTestBeatmapSetInfo()));

                testEventsArriving(true);

                // Now let's try unsubscribing.
                resolvedItems = null;
                lastChanges = null;

                registration.Dispose();

                realmFactory.Write(realm => realm.Add(TestResources.CreateTestBeatmapSetInfo()));

                testEventsArriving(false);

                // And make sure even after another context loss we don't get firings.
                using (realmFactory.BlockAllOperations())
                    Assert.That(resolvedItems, Is.Null);

                realmFactory.Write(realm => realm.Add(TestResources.CreateTestBeatmapSetInfo()));

                testEventsArriving(false);

                void testEventsArriving(bool shouldArrive)
                {
                    realmFactory.Run(realm => realm.Refresh());

                    if (shouldArrive)
                        Assert.That(resolvedItems, Has.One.Items);
                    else
                        Assert.That(resolvedItems, Is.Null);

                    realmFactory.Write(realm =>
                    {
                        realm.RemoveAll<BeatmapSetInfo>();
                        realm.RemoveAll<RulesetInfo>();
                    });

                    realmFactory.Run(realm => realm.Refresh());

                    if (shouldArrive)
                        Assert.That(lastChanges?.DeletedIndices, Has.One.Items);
                    else
                        Assert.That(lastChanges, Is.Null);
                }
            });

            void onChanged(IRealmCollection<BeatmapSetInfo> sender, ChangeSet? changes, Exception error)
            {
                if (changes == null)
                    resolvedItems = sender;

                lastChanges = changes;
            }
        }

        [Test]
        public void TestCustomRegisterWithContextLoss()
        {
            RunTestWithRealm((realmFactory, _) =>
            {
                BeatmapSetInfo? beatmapSetInfo = null;

                realmFactory.Write(realm => realm.Add(TestResources.CreateTestBeatmapSetInfo()));

                var subscription = realmFactory.Register(realm =>
                {
                    beatmapSetInfo = realm.All<BeatmapSetInfo>().First();

                    return new InvokeOnDisposal(() => beatmapSetInfo = null);
                });

                Assert.That(beatmapSetInfo, Is.Not.Null);

                using (realmFactory.BlockAllOperations())
                {
                    // custom disposal action fired when context lost.
                    Assert.That(beatmapSetInfo, Is.Null);
                }

                // re-registration after context restore.
                realmFactory.Run(realm => realm.Refresh());
                Assert.That(beatmapSetInfo, Is.Not.Null);

                subscription.Dispose();

                Assert.That(beatmapSetInfo, Is.Null);

                using (realmFactory.BlockAllOperations())
                    Assert.That(beatmapSetInfo, Is.Null);

                realmFactory.Run(realm => realm.Refresh());
                Assert.That(beatmapSetInfo, Is.Null);
            });
        }
    }
}
