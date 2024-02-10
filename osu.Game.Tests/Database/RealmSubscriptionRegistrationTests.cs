// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets;
using osu.Game.Tests.Resources;
using Realms;

namespace osu.Game.Tests.Database
{
    [TestFixture]
    public class RealmSubscriptionRegistrationTests : RealmTest
    {
        [Test]
        public void TestSubscriptionCollectionAndPropertyChanges()
        {
            int collectionChanges = 0;
            int propertyChanges = 0;

            ChangeSet? lastChanges = null;

            RunTestWithRealm((realm, _) =>
            {
                var registration = realm.RegisterForNotifications(r => r.All<BeatmapSetInfo>(), onChanged);

                realm.Run(r => r.Refresh());

                realm.Write(r => r.Add(TestResources.CreateTestBeatmapSetInfo()));
                realm.Run(r => r.Refresh());

                Assert.That(collectionChanges, Is.EqualTo(1));
                Assert.That(propertyChanges, Is.EqualTo(0));
                Assert.That(lastChanges?.InsertedIndices, Has.One.Items);
                Assert.That(lastChanges?.ModifiedIndices, Is.Empty);
                Assert.That(lastChanges?.NewModifiedIndices, Is.Empty);

                realm.Write(r => r.All<BeatmapSetInfo>().First().Beatmaps.First().CountdownOffset = 5);
                realm.Run(r => r.Refresh());

                Assert.That(collectionChanges, Is.EqualTo(1));
                Assert.That(propertyChanges, Is.EqualTo(1));
                Assert.That(lastChanges?.InsertedIndices, Is.Empty);
                Assert.That(lastChanges?.ModifiedIndices, Has.One.Items);
                Assert.That(lastChanges?.NewModifiedIndices, Has.One.Items);

                registration.Dispose();
            });

            void onChanged(IRealmCollection<BeatmapSetInfo> sender, ChangeSet? changes)
            {
                lastChanges = changes;

                if (changes == null)
                    return;

                if (changes.HasCollectionChanges())
                {
                    Interlocked.Increment(ref collectionChanges);
                }
                else
                {
                    Interlocked.Increment(ref propertyChanges);
                }
            }
        }

        [Test]
        public void TestSubscriptionWithAsyncWrite()
        {
            ChangeSet? lastChanges = null;

            RunTestWithRealm((realm, _) =>
            {
                var registration = realm.RegisterForNotifications(r => r.All<BeatmapSetInfo>(), onChanged);

                realm.Run(r => r.Refresh());

                realm.WriteAsync(r => r.Add(TestResources.CreateTestBeatmapSetInfo())).WaitSafely();

                realm.Run(r => r.Refresh());

                Assert.That(lastChanges?.InsertedIndices, Has.One.Items);

                registration.Dispose();
            });

            void onChanged(IRealmCollection<BeatmapSetInfo> sender, ChangeSet? changes) => lastChanges = changes;
        }

        [Test]
        public void TestPropertyChangedSubscription()
        {
            RunTestWithRealm((realm, _) =>
            {
                bool? receivedValue = null;

                realm.Write(r => r.Add(TestResources.CreateTestBeatmapSetInfo()));

                using (realm.SubscribeToPropertyChanged(r => r.All<BeatmapSetInfo>().First(), setInfo => setInfo.Protected, val => receivedValue = val))
                {
                    Assert.That(receivedValue, Is.False);

                    realm.Write(r => r.All<BeatmapSetInfo>().First().Protected = true);

                    realm.Run(r => r.Refresh());

                    Assert.That(receivedValue, Is.True);
                }
            });
        }

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

                using (realm.BlockAllOperations("testing"))
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
                using (realm.BlockAllOperations("testing"))
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

            void onChanged(IRealmCollection<BeatmapSetInfo> sender, ChangeSet? changes)
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

                using (realm.BlockAllOperations("testing"))
                {
                    // custom disposal action fired when context lost.
                    Assert.That(beatmapSetInfo, Is.Null);
                }

                // re-registration after context restore.
                realm.Run(r => r.Refresh());
                Assert.That(beatmapSetInfo, Is.Not.Null);

                subscription.Dispose();

                Assert.That(beatmapSetInfo, Is.Null);

                using (realm.BlockAllOperations("testing"))
                    Assert.That(beatmapSetInfo, Is.Null);

                realm.Run(r => r.Refresh());
                Assert.That(beatmapSetInfo, Is.Null);
            });
        }

        [Test]
        public void TestPropertyChangedSubscriptionWithContextLoss()
        {
            RunTestWithRealm((realm, _) =>
            {
                bool? receivedValue = null;

                realm.Write(r => r.Add(TestResources.CreateTestBeatmapSetInfo()));

                var subscription = realm.SubscribeToPropertyChanged(
                    r => r.All<BeatmapSetInfo>().First(),
                    setInfo => setInfo.Protected,
                    val => receivedValue = val);

                Assert.That(receivedValue, Is.Not.Null);
                receivedValue = null;

                using (realm.BlockAllOperations("testing"))
                {
                }

                // re-registration after context restore.
                realm.Run(r => r.Refresh());
                Assert.That(receivedValue, Is.Not.Null);

                subscription.Dispose();
                receivedValue = null;

                using (realm.BlockAllOperations("testing"))
                    Assert.That(receivedValue, Is.Null);

                realm.Run(r => r.Refresh());
                Assert.That(receivedValue, Is.Null);
            });
        }
    }
}
