// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Input.Bindings;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Input;
using osu.Game.Input.Bindings;

namespace osu.Game.Tests.Database
{
    [TestFixture]
    public class TestRealmKeyBindingStore
    {
        private NativeStorage storage;

        private RealmKeyBindingStore keyBindingStore;

        private RealmContextFactory realmContextFactory;

        [SetUp]
        public void SetUp()
        {
            var directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));

            storage = new NativeStorage(directory.FullName);

            realmContextFactory = new RealmContextFactory(storage);
            keyBindingStore = new RealmKeyBindingStore(realmContextFactory);
        }

        [Test]
        public void TestDefaultsPopulationAndQuery()
        {
            Assert.That(query().Count, Is.EqualTo(0));

            KeyBindingContainer testContainer = new TestKeyBindingContainer();

            keyBindingStore.Register(testContainer);

            Assert.That(query().Count, Is.EqualTo(3));

            Assert.That(query().Where(k => k.Action == (int)GlobalAction.Back).Count, Is.EqualTo(1));
            Assert.That(query().Where(k => k.Action == (int)GlobalAction.Select).Count, Is.EqualTo(2));
        }

        private IQueryable<RealmKeyBinding> query() => realmContextFactory.Get().All<RealmKeyBinding>();

        [Test]
        public void TestUpdateViaQueriedReference()
        {
            KeyBindingContainer testContainer = new TestKeyBindingContainer();

            keyBindingStore.Register(testContainer);

            var backBinding = query().Single(k => k.Action == (int)GlobalAction.Back);

            Assert.That(((IKeyBinding)backBinding).KeyCombination.Keys, Is.EquivalentTo(new[] { InputKey.Escape }));

            keyBindingStore.Update(backBinding, binding => binding.KeyCombination = new KeyCombination(InputKey.BackSpace));

            Assert.That(((IKeyBinding)backBinding).KeyCombination.Keys, Is.EquivalentTo(new[] { InputKey.BackSpace }));

            // check still correct after re-query.
            backBinding = query().Single(k => k.Action == (int)GlobalAction.Back);
            Assert.That(((IKeyBinding)backBinding).KeyCombination.Keys, Is.EquivalentTo(new[] { InputKey.BackSpace }));
        }

        [TearDown]
        public void TearDown()
        {
            storage.DeleteDirectory(string.Empty);
        }

        public class TestKeyBindingContainer : KeyBindingContainer
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
