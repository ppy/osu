// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Utils;

namespace osu.Game.Tests.Mods
{
    [TestFixture]
    public class ModUtilsTest
    {
        [Test]
        public void TestModIsCompatibleByItself()
        {
            var mod = new Mock<CustomMod1>();
            Assert.That(ModUtils.CheckCompatibleSet(new[] { mod.Object }));
        }

        [Test]
        public void TestIncompatibleThroughTopLevel()
        {
            var mod1 = new Mock<CustomMod1>();
            var mod2 = new Mock<CustomMod2>();

            mod1.Setup(m => m.IncompatibleMods).Returns(new[] { mod2.Object.GetType() });

            // Test both orderings.
            Assert.That(ModUtils.CheckCompatibleSet(new Mod[] { mod1.Object, mod2.Object }), Is.False);
            Assert.That(ModUtils.CheckCompatibleSet(new Mod[] { mod2.Object, mod1.Object }), Is.False);
        }

        [Test]
        public void TestMultiModIncompatibleWithTopLevel()
        {
            var mod1 = new Mock<CustomMod1>();

            // The nested mod.
            var mod2 = new Mock<CustomMod2>();
            mod2.Setup(m => m.IncompatibleMods).Returns(new[] { mod1.Object.GetType() });

            var multiMod = new MultiMod(new MultiMod(mod2.Object));

            // Test both orderings.
            Assert.That(ModUtils.CheckCompatibleSet(new Mod[] { multiMod, mod1.Object }), Is.False);
            Assert.That(ModUtils.CheckCompatibleSet(new Mod[] { mod1.Object, multiMod }), Is.False);
        }

        [Test]
        public void TestTopLevelIncompatibleWithMultiMod()
        {
            // The nested mod.
            var mod1 = new Mock<CustomMod1>();
            var multiMod = new MultiMod(new MultiMod(mod1.Object));

            var mod2 = new Mock<CustomMod2>();
            mod2.Setup(m => m.IncompatibleMods).Returns(new[] { typeof(CustomMod1) });

            // Test both orderings.
            Assert.That(ModUtils.CheckCompatibleSet(new Mod[] { multiMod, mod2.Object }), Is.False);
            Assert.That(ModUtils.CheckCompatibleSet(new Mod[] { mod2.Object, multiMod }), Is.False);
        }

        [Test]
        public void TestCompatibleMods()
        {
            var mod1 = new Mock<CustomMod1>();
            var mod2 = new Mock<CustomMod2>();

            // Test both orderings.
            Assert.That(ModUtils.CheckCompatibleSet(new Mod[] { mod1.Object, mod2.Object }), Is.True);
            Assert.That(ModUtils.CheckCompatibleSet(new Mod[] { mod2.Object, mod1.Object }), Is.True);
        }

        [Test]
        public void TestIncompatibleThroughBaseType()
        {
            var mod1 = new Mock<CustomMod1>();
            var mod2 = new Mock<CustomMod2>();
            mod2.Setup(m => m.IncompatibleMods).Returns(new[] { typeof(Mod) });

            // Test both orderings.
            Assert.That(ModUtils.CheckCompatibleSet(new Mod[] { mod1.Object, mod2.Object }), Is.False);
            Assert.That(ModUtils.CheckCompatibleSet(new Mod[] { mod2.Object, mod1.Object }), Is.False);
        }

        [Test]
        public void TestAllowedThroughMostDerivedType()
        {
            var mod = new Mock<CustomMod1>();
            Assert.That(ModUtils.CheckAllowed(new[] { mod.Object }, new[] { mod.Object.GetType() }));
        }

        [Test]
        public void TestNotAllowedThroughBaseType()
        {
            var mod = new Mock<CustomMod1>();
            Assert.That(ModUtils.CheckAllowed(new[] { mod.Object }, new[] { typeof(Mod) }), Is.False);
        }

        private static readonly object[] invalid_mod_test_scenarios =
        {
            // incompatible pair.
            new object[]
            {
                new Mod[] { new OsuModDoubleTime(), new OsuModHalfTime() },
                new[] { typeof(OsuModDoubleTime), typeof(OsuModHalfTime) }
            },
            // incompatible pair with derived class.
            new object[]
            {
                new Mod[] { new OsuModNightcore(), new OsuModHalfTime() },
                new[] { typeof(OsuModNightcore), typeof(OsuModHalfTime) }
            },
            // system mod.
            new object[]
            {
                new Mod[] { new OsuModDoubleTime(), new OsuModTouchDevice() },
                new[] { typeof(OsuModTouchDevice) }
            },
            // multi mod.
            new object[]
            {
                new Mod[] { new MultiMod(new OsuModHalfTime()), new OsuModHalfTime() },
                new[] { typeof(MultiMod) }
            },
            // valid pair.
            new object[]
            {
                new Mod[] { new OsuModDoubleTime(), new OsuModHardRock() },
                null
            }
        };

        [TestCaseSource(nameof(invalid_mod_test_scenarios))]
        public void TestInvalidModScenarios(Mod[] inputMods, Type[] expectedInvalid)
        {
            bool isValid = ModUtils.CheckValidForGameplay(inputMods, out var invalid);

            Assert.That(isValid, Is.EqualTo(expectedInvalid == null));

            if (isValid)
                Assert.IsNull(invalid);
            else
                Assert.That(invalid?.Select(t => t.GetType()), Is.EquivalentTo(expectedInvalid));
        }

        public abstract class CustomMod1 : Mod
        {
        }

        public abstract class CustomMod2 : Mod
        {
        }
    }
}
