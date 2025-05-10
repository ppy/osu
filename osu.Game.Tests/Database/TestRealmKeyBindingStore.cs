// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Input;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets;
using Realms;

namespace osu.Game.Tests.Database
{
    [TestFixture]
    public partial class TestRealmKeyBindingStore
    {
        private NativeStorage storage;

        private RealmKeyBindingStore keyBindingStore;

        private RealmAccess realm;

        [SetUp]
        public void SetUp()
        {
            var directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));

            storage = new NativeStorage(directory.FullName);

            realm = new RealmAccess(storage, "test");
            keyBindingStore = new RealmKeyBindingStore(realm, new ReadableKeyCombinationProvider());
        }

        [Test]
        public void TestDefaultsPopulationAndQuery()
        {
            Assert.That(queryCount(), Is.EqualTo(0));

            KeyBindingContainer testContainer = new TestKeyBindingContainer();

            keyBindingStore.Register(testContainer, Enumerable.Empty<RulesetInfo>());

            Assert.That(queryCount(), Is.EqualTo(3));

            Assert.That(queryCount(GlobalAction.Back), Is.EqualTo(1));
            Assert.That(queryCount(GlobalAction.Select), Is.EqualTo(2));
        }

        [Test]
        public void TestDefaultsPopulationRemovesExcess()
        {
            Assert.That(queryCount(), Is.EqualTo(0));

            KeyBindingContainer testContainer = new TestKeyBindingContainer();

            // Add some excess bindings for an action which only supports 1.
            realm.Write(r =>
            {
                r.Add(new RealmKeyBinding(GlobalAction.Back, new KeyCombination(InputKey.A)));
                r.Add(new RealmKeyBinding(GlobalAction.Back, new KeyCombination(InputKey.S)));
                r.Add(new RealmKeyBinding(GlobalAction.Back, new KeyCombination(InputKey.D)));
            });

            Assert.That(queryCount(GlobalAction.Back), Is.EqualTo(3));

            keyBindingStore.Register(testContainer, Enumerable.Empty<RulesetInfo>());

            Assert.That(queryCount(GlobalAction.Back), Is.EqualTo(1));
        }

        private int queryCount(GlobalAction? match = null)
        {
            return realm.Run(r =>
            {
                var results = r.All<RealmKeyBinding>();
                if (match.HasValue)
                    results = results.Where(k => k.ActionInt == (int)match.Value);
                return results.Count();
            });
        }

        [Test]
        public void TestUpdateViaQueriedReference()
        {
            KeyBindingContainer testContainer = new TestKeyBindingContainer();

            keyBindingStore.Register(testContainer, Enumerable.Empty<RulesetInfo>());

            realm.Run(outerRealm =>
            {
                var backBinding = outerRealm.All<RealmKeyBinding>().Single(k => k.ActionInt == (int)GlobalAction.Back);

                Assert.That(backBinding.KeyCombination.Keys, Is.EquivalentTo(new[] { InputKey.Escape }));

                var tsr = ThreadSafeReference.Create(backBinding);

                realm.Run(innerRealm =>
                {
                    var binding = innerRealm.ResolveReference(tsr)!;
                    innerRealm.Write(() => binding.KeyCombination = new KeyCombination(InputKey.BackSpace));
                });

                Assert.That(backBinding.KeyCombination.Keys, Is.EquivalentTo(new[] { InputKey.BackSpace }));

                // check still correct after re-query.
                backBinding = outerRealm.All<RealmKeyBinding>().Single(k => k.ActionInt == (int)GlobalAction.Back);
                Assert.That(backBinding.KeyCombination.Keys, Is.EquivalentTo(new[] { InputKey.BackSpace }));
            });
        }

        [TearDown]
        public void TearDown()
        {
            realm.Dispose();
            storage.DeleteDirectory(string.Empty);
        }

        public partial class TestKeyBindingContainer : KeyBindingContainer
        {
            public override IEnumerable<IKeyBinding> DefaultKeyBindings =>
                new[]
                {
                    new KeyBinding(InputKey.Escape, GlobalAction.Back),
                    new KeyBinding(InputKey.Enter, GlobalAction.Select),
                    new KeyBinding(InputKey.Space, GlobalAction.Select),
                };
        }
    }
}
