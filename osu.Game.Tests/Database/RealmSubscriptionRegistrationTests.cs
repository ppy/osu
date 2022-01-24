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

            RunTestWithRealm((realm, _) =>
            {
                realm.Write(r => r.Add(TestResources.CreateTestBeatmapSetInfo()));

                var registration = realm.RegisterForNotifications(r => r.All<BeatmapSetInfo>(), onChanged);

                testEventsArriving(true);

                // All normal until here.
                // Now let's yank the main realm context.
                resolvedItems = null;
                lastChanges = null;

                using (realm.BlockAllOperations())
                    Assert.That(resolvedItems, Is.Empty);

                realm.Write(r => r.Add(TestResources.CreateTestBeatmapSetInfo()));

                testEventsArriving(true);

                // Now let's try unsubscribing.
                resolvedItems = null;
                lastChanges = null;

                registration.Dispose();

                realm.Write(r => r.Add(TestResources.CreateTestBeatmapSetInfo()));

                testEventsArriving(false);

                // And make sure even after another context loss we don't get firings.
                using (realm.BlockAllOperations())
                    Assert.That(resolvedItems, Is.Null);

                realm.Write(r => r.Add(TestResources.CreateTestBeatmapSetInfo()));

                testEventsArriving(false);

                void testEventsArriving(bool shouldArrive)
                {
                    realm.Run(r => r.Refresh());

                    if (shouldArrive)
                        Assert.That(resolvedItems, Has.One.Items);
                    else
                        Assert.That(resolvedItems, Is.Null);

                    realm.Write(r =>
                    {
                        r.RemoveAll<BeatmapSetInfo>();
                        r.RemoveAll<RulesetInfo>();
                    });

                    realm.Run(r => r.Refresh());

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
            RunTestWithRealm((realm, _) =>
            {
                BeatmapSetInfo? beatmapSetInfo = null;

                realm.Write(r => r.Add(TestResources.CreateTestBeatmapSetInfo()));

                var subscription = realm.RegisterCustomSubscription(r =>
                {
                    beatmapSetInfo = r.All<BeatmapSetInfo>().First();

                    return new InvokeOnDisposal(() => beatmapSetInfo = null);
                });

                Assert.That(beatmapSetInfo, Is.Not.Null);

                using (realm.BlockAllOperations())
                {
                    // custom disposal action fired when context lost.
                    Assert.That(beatmapSetInfo, Is.Null);
                }

                // re-registration after context restore.
                realm.Run(r => r.Refresh());
                Assert.That(beatmapSetInfo, Is.Not.Null);

                subscription.Dispose();

                Assert.That(beatmapSetInfo, Is.Null);

                using (realm.BlockAllOperations())
                    Assert.That(beatmapSetInfo, Is.Null);

                realm.Run(r => r.Refresh());
                Assert.That(beatmapSetInfo, Is.Null);
            });
        }
    }
}
