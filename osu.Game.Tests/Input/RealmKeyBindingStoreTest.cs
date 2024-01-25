// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Input.Bindings;
using osu.Game.Input;
using osu.Game.Input.Bindings;
using osuTK.Input;

namespace osu.Game.Tests.Input
{
    [TestFixture]
    public class RealmKeyBindingStoreTest
    {
        [Test]
        public void TestBindingsWithoutDuplicatesAreNotModified()
        {
            var bindings = new List<RealmKeyBinding>
            {
                new RealmKeyBinding(GlobalAction.Back, KeyCombination.FromKey(Key.Escape)),
                new RealmKeyBinding(GlobalAction.Back, KeyCombination.FromMouseButton(MouseButton.Button1)),
                new RealmKeyBinding(GlobalAction.MusicPrev, KeyCombination.FromKey(Key.F1)),
                new RealmKeyBinding(GlobalAction.MusicNext, KeyCombination.FromKey(Key.F5))
            };

            int countCleared = RealmKeyBindingStore.ClearDuplicateBindings(bindings);

            Assert.Multiple(() =>
            {
                Assert.That(countCleared, Is.Zero);

                Assert.That(bindings[0].Action, Is.EqualTo((int)GlobalAction.Back));
                Assert.That(bindings[0].KeyCombination, Is.EqualTo(new KeyCombination(InputKey.Escape)));

                Assert.That(bindings[1].Action, Is.EqualTo((int)GlobalAction.Back));
                Assert.That(bindings[1].KeyCombination, Is.EqualTo(new KeyCombination(InputKey.ExtraMouseButton1)));

                Assert.That(bindings[2].Action, Is.EqualTo((int)GlobalAction.MusicPrev));
                Assert.That(bindings[2].KeyCombination, Is.EqualTo(new KeyCombination(InputKey.F1)));

                Assert.That(bindings[3].Action, Is.EqualTo((int)GlobalAction.MusicNext));
                Assert.That(bindings[3].KeyCombination, Is.EqualTo(new KeyCombination(InputKey.F5)));
            });
        }

        [Test]
        public void TestDuplicateBindingsAreCleared()
        {
            var bindings = new List<RealmKeyBinding>
            {
                new RealmKeyBinding(GlobalAction.Back, KeyCombination.FromKey(Key.Escape)),
                new RealmKeyBinding(GlobalAction.Back, KeyCombination.FromMouseButton(MouseButton.Button1)),
                new RealmKeyBinding(GlobalAction.MusicPrev, KeyCombination.FromKey(Key.F1)),
                new RealmKeyBinding(GlobalAction.IncreaseVolume, KeyCombination.FromKey(Key.Escape)),
                new RealmKeyBinding(GlobalAction.MusicNext, KeyCombination.FromKey(Key.F5)),
                new RealmKeyBinding(GlobalAction.ExportReplay, KeyCombination.FromKey(Key.F1)),
                new RealmKeyBinding(GlobalAction.TakeScreenshot, KeyCombination.FromKey(Key.PrintScreen)),
            };

            int countCleared = RealmKeyBindingStore.ClearDuplicateBindings(bindings);

            Assert.Multiple(() =>
            {
                Assert.That(countCleared, Is.EqualTo(4));

                Assert.That(bindings[0].Action, Is.EqualTo((int)GlobalAction.Back));
                Assert.That(bindings[0].KeyCombination, Is.EqualTo(new KeyCombination(InputKey.None)));

                Assert.That(bindings[1].Action, Is.EqualTo((int)GlobalAction.Back));
                Assert.That(bindings[1].KeyCombination, Is.EqualTo(new KeyCombination(InputKey.ExtraMouseButton1)));

                Assert.That(bindings[2].Action, Is.EqualTo((int)GlobalAction.MusicPrev));
                Assert.That(bindings[2].KeyCombination, Is.EqualTo(new KeyCombination(InputKey.None)));

                Assert.That(bindings[3].Action, Is.EqualTo((int)GlobalAction.IncreaseVolume));
                Assert.That(bindings[3].KeyCombination, Is.EqualTo(new KeyCombination(InputKey.None)));

                Assert.That(bindings[4].Action, Is.EqualTo((int)GlobalAction.MusicNext));
                Assert.That(bindings[4].KeyCombination, Is.EqualTo(new KeyCombination(InputKey.F5)));

                Assert.That(bindings[5].Action, Is.EqualTo((int)GlobalAction.ExportReplay));
                Assert.That(bindings[5].KeyCombination, Is.EqualTo(new KeyCombination(InputKey.None)));

                Assert.That(bindings[6].Action, Is.EqualTo((int)GlobalAction.TakeScreenshot));
                Assert.That(bindings[6].KeyCombination, Is.EqualTo(new KeyCombination(InputKey.PrintScreen)));
            });
        }

        [Test]
        public void TestDuplicateBindingsAllowedIfBoundToSameAction()
        {
            var bindings = new List<RealmKeyBinding>
            {
                new RealmKeyBinding(GlobalAction.Back, KeyCombination.FromKey(Key.Escape)),
                new RealmKeyBinding(GlobalAction.Back, KeyCombination.FromKey(Key.Escape)),
                new RealmKeyBinding(GlobalAction.MusicPrev, KeyCombination.FromKey(Key.F1)),
            };

            int countCleared = RealmKeyBindingStore.ClearDuplicateBindings(bindings);

            Assert.Multiple(() =>
            {
                Assert.That(countCleared, Is.EqualTo(0));

                Assert.That(bindings[0].Action, Is.EqualTo((int)GlobalAction.Back));
                Assert.That(bindings[0].KeyCombination, Is.EqualTo(new KeyCombination(InputKey.Escape)));

                Assert.That(bindings[1].Action, Is.EqualTo((int)GlobalAction.Back));
                Assert.That(bindings[1].KeyCombination, Is.EqualTo(new KeyCombination(InputKey.Escape)));

                Assert.That(bindings[2].Action, Is.EqualTo((int)GlobalAction.MusicPrev));
                Assert.That(bindings[2].KeyCombination, Is.EqualTo(new KeyCombination(InputKey.F1)));
            });
        }
    }
}
