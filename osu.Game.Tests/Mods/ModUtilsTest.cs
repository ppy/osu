// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Moq;
using NUnit.Framework;
using osu.Game.Rulesets.Mods;
using osu.Game.Utils;

namespace osu.Game.Tests.Mods
{
    [TestFixture]
    public class ModUtilsTest
    {
        [Test]
        public void TestModIsCompatibleByItself()
        {
            var mod = new Mock<Mod>();
            Assert.That(ModUtils.CheckCompatibleSet(new[] { mod.Object }));
        }

        [Test]
        public void TestIncompatibleThroughTopLevel()
        {
            var mod1 = new Mock<Mod>();
            var mod2 = new Mock<Mod>();

            mod1.Setup(m => m.IncompatibleMods).Returns(new[] { mod2.Object.GetType() });

            // Test both orderings.
            Assert.That(ModUtils.CheckCompatibleSet(new[] { mod1.Object, mod2.Object }), Is.False);
            Assert.That(ModUtils.CheckCompatibleSet(new[] { mod2.Object, mod1.Object }), Is.False);
        }

        [Test]
        public void TestMultiModIncompatibleWithTopLevel()
        {
            var mod1 = new Mock<Mod>();

            // The nested mod.
            var mod2 = new Mock<Mod>();
            mod2.Setup(m => m.IncompatibleMods).Returns(new[] { mod1.Object.GetType() });

            var multiMod = new MultiMod(new MultiMod(mod2.Object));

            // Test both orderings.
            Assert.That(ModUtils.CheckCompatibleSet(new[] { multiMod, mod1.Object }), Is.False);
            Assert.That(ModUtils.CheckCompatibleSet(new[] { mod1.Object, multiMod }), Is.False);
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
            var mod1 = new Mock<Mod>();
            var mod2 = new Mock<Mod>();

            // Test both orderings.
            Assert.That(ModUtils.CheckCompatibleSet(new[] { mod1.Object, mod2.Object }), Is.True);
            Assert.That(ModUtils.CheckCompatibleSet(new[] { mod2.Object, mod1.Object }), Is.True);
        }

        [Test]
        public void TestIncompatibleThroughBaseType()
        {
            var mod1 = new Mock<Mod>();
            var mod2 = new Mock<Mod>();
            mod2.Setup(m => m.IncompatibleMods).Returns(new[] { mod1.Object.GetType().BaseType });

            // Test both orderings.
            Assert.That(ModUtils.CheckCompatibleSet(new[] { mod1.Object, mod2.Object }), Is.False);
            Assert.That(ModUtils.CheckCompatibleSet(new[] { mod2.Object, mod1.Object }), Is.False);
        }

        [Test]
        public void TestAllowedThroughMostDerivedType()
        {
            var mod = new Mock<Mod>();
            Assert.That(ModUtils.CheckAllowed(new[] { mod.Object }, new[] { mod.Object.GetType() }));
        }

        [Test]
        public void TestNotAllowedThroughBaseType()
        {
            var mod = new Mock<Mod>();
            Assert.That(ModUtils.CheckAllowed(new[] { mod.Object }, new[] { typeof(Mod) }), Is.False);
        }

        public abstract class CustomMod1 : Mod
        {
        }

        public abstract class CustomMod2 : Mod
        {
        }
    }
}
