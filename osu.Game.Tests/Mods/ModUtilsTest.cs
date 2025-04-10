// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using osu.Framework.Localisation;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Utils;

namespace osu.Game.Tests.Mods
{
    [TestFixture]
    public class ModUtilsTest
    {
        [Test]
        public void TestModIsNotCompatibleWithItself()
        {
            var mod = new Mock<CustomMod1>();
            Assert.That(ModUtils.CheckCompatibleSet(new[] { mod.Object, mod.Object }, out var invalid), Is.False);
            Assert.That(invalid, Is.EquivalentTo(new[] { mod.Object }));
        }

        [Test]
        public void TestModIsCompatibleByItself()
        {
            var mod = new Mock<CustomMod1>();
            Assert.That(ModUtils.CheckCompatibleSet(new[] { mod.Object }));
        }

        [Test]
        public void TestModIsCompatibleByItselfWithIncompatibleInterface()
        {
            var mod = new Mock<CustomMod1>();
            mod.Setup(m => m.IncompatibleMods).Returns(new[] { typeof(IModCompatibilitySpecification) });
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
        public void TestIncompatibleThroughInterface()
        {
            var mod1 = new Mock<CustomMod1>();
            var mod2 = new Mock<CustomMod2>();

            mod1.Setup(m => m.IncompatibleMods).Returns(new[] { typeof(IModCompatibilitySpecification) });
            mod2.Setup(m => m.IncompatibleMods).Returns(new[] { typeof(IModCompatibilitySpecification) });

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
                new Mod[] { new OsuModHidden(), new OsuModApproachDifferent() },
                new[] { typeof(OsuModHidden), typeof(OsuModApproachDifferent) }
            },
            // incompatible pair with derived class.
            new object[]
            {
                new Mod[] { new OsuModDeflate(), new OsuModApproachDifferent() },
                new[] { typeof(OsuModDeflate), typeof(OsuModApproachDifferent) }
            },
            // system mod not applicable in lazer.
            new object[]
            {
                new Mod[] { new OsuModHidden(), new ModScoreV2() },
                new[] { typeof(ModScoreV2) }
            },
            // multi mod.
            new object[]
            {
                new Mod[] { new MultiMod(new OsuModSuddenDeath(), new OsuModPerfect()) },
                new[] { typeof(MultiMod) }
            },
            // invalid multiplayer mod is valid for local.
            new object[]
            {
                new Mod[] { new OsuModHidden(), new InvalidMultiplayerMod() },
                Array.Empty<Type>()
            },
            // invalid free mod is valid for local.
            new object[]
            {
                new Mod[] { new OsuModHidden(), new InvalidMultiplayerFreeMod() },
                Array.Empty<Type>()
            },
            // valid pair.
            new object[]
            {
                new Mod[] { new OsuModHidden(), new OsuModHardRock() },
                Array.Empty<Type>()
            },
        };

        private static readonly object[] invalid_multiplayer_mod_test_scenarios =
        {
            // incompatible pair.
            new object[]
            {
                new Mod[] { new OsuModHidden(), new OsuModApproachDifferent() },
                new[] { typeof(OsuModHidden), typeof(OsuModApproachDifferent) }
            },
            // incompatible pair with derived class.
            new object[]
            {
                new Mod[] { new OsuModDeflate(), new OsuModApproachDifferent() },
                new[] { typeof(OsuModDeflate), typeof(OsuModApproachDifferent) }
            },
            // system mod.
            new object[]
            {
                new Mod[] { new OsuModHidden(), new OsuModTouchDevice() },
                new[] { typeof(OsuModTouchDevice) }
            },
            // multi mod.
            new object[]
            {
                new Mod[] { new MultiMod(new OsuModSuddenDeath(), new OsuModPerfect()) },
                new[] { typeof(MultiMod) }
            },
            // invalid multiplayer mod.
            new object[]
            {
                new Mod[] { new OsuModHidden(), new InvalidMultiplayerMod() },
                new[] { typeof(InvalidMultiplayerMod) }
            },
            // invalid free mod is valid for multiplayer global.
            new object[]
            {
                new Mod[] { new OsuModHidden(), new InvalidMultiplayerFreeMod() },
                Array.Empty<Type>()
            },
            // valid pair.
            new object[]
            {
                new Mod[] { new OsuModHidden(), new OsuModHardRock() },
                Array.Empty<Type>()
            },
        };

        private static readonly object[] invalid_free_mod_test_scenarios =
        {
            // system mod.
            new object[]
            {
                new Mod[] { new OsuModHidden(), new OsuModTouchDevice() },
                new[] { typeof(OsuModTouchDevice) }
            },
            // multi mod.
            new object[]
            {
                new Mod[] { new MultiMod(new OsuModSuddenDeath(), new OsuModPerfect()) },
                new[] { typeof(MultiMod) }
            },
            // invalid multiplayer mod.
            new object[]
            {
                new Mod[] { new OsuModHidden(), new InvalidMultiplayerMod() },
                new[] { typeof(InvalidMultiplayerMod) }
            },
            // invalid free mod.
            new object[]
            {
                new Mod[] { new OsuModHidden(), new InvalidMultiplayerFreeMod() },
                new[] { typeof(InvalidMultiplayerFreeMod) }
            },
            // incompatible pair is valid for free mods.
            new object[]
            {
                new Mod[] { new OsuModHidden(), new OsuModApproachDifferent() },
                Array.Empty<Type>(),
            },
            // incompatible pair with derived class is valid for free mods.
            new object[]
            {
                new Mod[] { new OsuModDeflate(), new OsuModSpinIn() },
                Array.Empty<Type>(),
            },
            // valid pair.
            new object[]
            {
                new Mod[] { new OsuModHidden(), new OsuModHardRock() },
                Array.Empty<Type>()
            },
        };

        private static readonly object[] invalid_freestyle_mod_test_scenarios =
        {
            // system mod.
            new object[]
            {
                new Mod[] { new OsuModHidden(), new OsuModTouchDevice() },
                new[] { typeof(OsuModTouchDevice) }
            },
            // multi mod.
            new object[]
            {
                new Mod[] { new MultiMod(new OsuModSuddenDeath(), new OsuModPerfect()) },
                new[] { typeof(MultiMod) }
            },
            // invalid freestyle mod.
            new object[]
            {
                new Mod[] { new OsuModHidden(), new OsuModNoScope(), new InvalidFreestyleMod() },
                new[] { typeof(OsuModNoScope), typeof(InvalidFreestyleMod) }
            },
            // valid pair.
            new object[]
            {
                new Mod[] { new OsuModHidden(), new OsuModHardRock() },
                Array.Empty<Type>()
            },
        };

        [TestCaseSource(nameof(invalid_mod_test_scenarios))]
        public void TestInvalidModScenarios(Mod[] inputMods, Type[] expectedInvalid)
        {
            bool isValid = ModUtils.CheckValidForGameplay(inputMods, out var invalid);

            Assert.That(isValid, Is.EqualTo(expectedInvalid.Length == 0));

            if (isValid)
                Assert.IsNull(invalid);
            else
                Assert.That(invalid?.Select(t => t.GetType()), Is.EquivalentTo(expectedInvalid));
        }

        [TestCaseSource(nameof(invalid_multiplayer_mod_test_scenarios))]
        public void TestInvalidMultiplayerModScenarios(Mod[] inputMods, Type[] expectedInvalid)
        {
            bool isValid = ModUtils.CheckValidRequiredModsForMultiplayer(inputMods, out var invalid);

            Assert.That(isValid, Is.EqualTo(expectedInvalid.Length == 0));

            if (isValid)
                Assert.IsNull(invalid);
            else
                Assert.That(invalid?.Select(t => t.GetType()), Is.EquivalentTo(expectedInvalid));
        }

        [TestCaseSource(nameof(invalid_freestyle_mod_test_scenarios))]
        public void TestInvalidFreestyleModScenarios(Mod[] inputMods, Type[] expectedInvalid)
        {
            bool isValid = ModUtils.CheckValidModsForFreestyle(inputMods, out var invalid);

            Assert.That(isValid, Is.EqualTo(expectedInvalid.Length == 0));

            if (isValid)
                Assert.IsNull(invalid);
            else
                Assert.That(invalid?.Select(t => t.GetType()), Is.EquivalentTo(expectedInvalid));
        }

        [TestCaseSource(nameof(invalid_free_mod_test_scenarios))]
        public void TestInvalidFreeModScenarios(Mod[] inputMods, Type[] expectedInvalid)
        {
            bool isValid = ModUtils.CheckValidFreeModsForMultiplayer(inputMods, out var invalid);

            Assert.That(isValid, Is.EqualTo(expectedInvalid.Length == 0));

            if (isValid)
                Assert.IsNull(invalid);
            else
                Assert.That(invalid?.Select(t => t.GetType()), Is.EquivalentTo(expectedInvalid));
        }

        [Test]
        public void TestModBelongsToRuleset()
        {
            Assert.That(ModUtils.CheckModsBelongToRuleset(new OsuRuleset(), Array.Empty<Mod>()));
            Assert.That(ModUtils.CheckModsBelongToRuleset(new OsuRuleset(), new Mod[] { new OsuModDoubleTime() }));
            Assert.That(ModUtils.CheckModsBelongToRuleset(new OsuRuleset(), new Mod[] { new OsuModDoubleTime(), new OsuModAccuracyChallenge() }));
            Assert.That(ModUtils.CheckModsBelongToRuleset(new OsuRuleset(), new Mod[] { new OsuModDoubleTime(), new ModAccuracyChallenge() }), Is.False);
            Assert.That(ModUtils.CheckModsBelongToRuleset(new OsuRuleset(), new Mod[] { new OsuModDoubleTime(), new TaikoModFlashlight() }), Is.False);
        }

        [Test]
        public void TestFormatScoreMultiplier()
        {
            Assert.AreEqual(ModUtils.FormatScoreMultiplier(0.9999).ToString(), "0.99x");
            Assert.AreEqual(ModUtils.FormatScoreMultiplier(1.0).ToString(), "1.00x");
            Assert.AreEqual(ModUtils.FormatScoreMultiplier(1.0001).ToString(), "1.01x");

            Assert.AreEqual(ModUtils.FormatScoreMultiplier(0.899999999999999).ToString(), "0.90x");
            Assert.AreEqual(ModUtils.FormatScoreMultiplier(0.9).ToString(), "0.90x");
            Assert.AreEqual(ModUtils.FormatScoreMultiplier(0.900000000000001).ToString(), "0.90x");

            Assert.AreEqual(ModUtils.FormatScoreMultiplier(1.099999999999999).ToString(), "1.10x");
            Assert.AreEqual(ModUtils.FormatScoreMultiplier(1.1).ToString(), "1.10x");
            Assert.AreEqual(ModUtils.FormatScoreMultiplier(1.100000000000001).ToString(), "1.10x");

            Assert.AreEqual(ModUtils.FormatScoreMultiplier(1.045).ToString(), "1.05x");
            Assert.AreEqual(ModUtils.FormatScoreMultiplier(1.05).ToString(), "1.05x");
            Assert.AreEqual(ModUtils.FormatScoreMultiplier(1.055).ToString(), "1.06x");
        }

        [Test]
        public void TestRoomModValidity()
        {
            Assert.IsTrue(ModUtils.IsValidModForMatch(new OsuModHardRock(), MatchType.Playlists, true, false));
            Assert.IsTrue(ModUtils.IsValidModForMatch(new OsuModDoubleTime(), MatchType.Playlists, true, false));
            Assert.IsTrue(ModUtils.IsValidModForMatch(new ModAdaptiveSpeed(), MatchType.Playlists, true, false));
            Assert.IsFalse(ModUtils.IsValidModForMatch(new OsuModAutoplay(), MatchType.Playlists, true, false));
            Assert.IsFalse(ModUtils.IsValidModForMatch(new OsuModTouchDevice(), MatchType.Playlists, true, false));

            Assert.IsTrue(ModUtils.IsValidModForMatch(new OsuModHardRock(), MatchType.HeadToHead, true, false));
            Assert.IsTrue(ModUtils.IsValidModForMatch(new OsuModDoubleTime(), MatchType.HeadToHead, true, false));
            // For now, adaptive speed isn't allowed in multiplayer because it's a per-user rate adjustment.
            Assert.IsFalse(ModUtils.IsValidModForMatch(new ModAdaptiveSpeed(), MatchType.HeadToHead, true, false));
            Assert.IsFalse(ModUtils.IsValidModForMatch(new OsuModAutoplay(), MatchType.HeadToHead, true, false));
            Assert.IsFalse(ModUtils.IsValidModForMatch(new OsuModTouchDevice(), MatchType.HeadToHead, true, false));
        }

        [Test]
        public void TestRoomFreeModValidity()
        {
            Assert.IsTrue(ModUtils.IsValidModForMatch(new OsuModHardRock(), MatchType.Playlists, false, false));
            Assert.IsTrue(ModUtils.IsValidModForMatch(new OsuModDoubleTime(), MatchType.Playlists, false, false));
            Assert.IsTrue(ModUtils.IsValidModForMatch(new ModAdaptiveSpeed(), MatchType.Playlists, false, false));
            Assert.IsFalse(ModUtils.IsValidModForMatch(new OsuModAutoplay(), MatchType.Playlists, false, false));
            Assert.IsFalse(ModUtils.IsValidModForMatch(new OsuModTouchDevice(), MatchType.Playlists, false, false));

            Assert.IsTrue(ModUtils.IsValidModForMatch(new OsuModHardRock(), MatchType.HeadToHead, false, false));
            // For now, all rate adjustment mods aren't allowed as free mods in multiplayer.
            Assert.IsFalse(ModUtils.IsValidModForMatch(new OsuModDoubleTime(), MatchType.HeadToHead, false, false));
            Assert.IsFalse(ModUtils.IsValidModForMatch(new ModAdaptiveSpeed(), MatchType.HeadToHead, false, false));
            Assert.IsFalse(ModUtils.IsValidModForMatch(new OsuModAutoplay(), MatchType.HeadToHead, false, false));
            Assert.IsFalse(ModUtils.IsValidModForMatch(new OsuModTouchDevice(), MatchType.HeadToHead, false, false));
        }

        [Test]
        public void TestFreestyleModValidity()
        {
            foreach (MatchType type in new[] { MatchType.Playlists, MatchType.HeadToHead })
            {
                foreach (bool required in new[] { false, true })
                {
                    Assert.IsTrue(ModUtils.IsValidModForMatch(new OsuModHardRock(), type, required, true));
                    Assert.IsTrue(ModUtils.IsValidModForMatch(new OsuModHardRock(), type, required, false));
                    Assert.IsFalse(ModUtils.IsValidModForMatch(new OsuModBarrelRoll(), type, required, true));
                    Assert.IsTrue(ModUtils.IsValidModForMatch(new OsuModBarrelRoll(), type, required, false));
                }
            }
        }

        [Test]
        public void TestFreestyleRulesetCompatibility()
        {
            Mod[] osuMods = ModUtils.FlattenMods(new OsuRuleset().CreateAllMods()).Where(m => m.ValidForFreestyle).ToArray();
            Ruleset[] otherRulesets = [new TaikoRuleset(), new CatchRuleset(), new ManiaRuleset()];

            EqualityComparer<Mod> validModComparer = EqualityComparer<Mod>.Create((a, b) =>
            {
                if (a == null || b == null)
                    return false;

                Type aType = a.GetType();

                while (aType != typeof(Mod))
                {
                    if (aType.IsInstanceOfType(b))
                        return string.Equals(a.Acronym, b.Acronym, StringComparison.Ordinal);

                    aType = aType.BaseType!;
                }

                return false;
            });

            Assert.Multiple(() =>
            {
                foreach (var ruleset in otherRulesets)
                {
                    Mod[] mods = ModUtils.FlattenMods(ruleset.CreateAllMods()).Where(m => m.ValidForFreestyle).ToArray();

                    foreach (var mod in mods)
                        Assert.That(osuMods, Contains.Item(mod).Using<Mod>(validModComparer));
                }
            });
        }

        public abstract class CustomMod1 : Mod, IModCompatibilitySpecification
        {
        }

        public abstract class CustomMod2 : Mod, IModCompatibilitySpecification
        {
        }

        private class InvalidMultiplayerMod : Mod
        {
            public override string Name => string.Empty;
            public override LocalisableString Description => string.Empty;
            public override string Acronym => string.Empty;
            public override double ScoreMultiplier => 1;
            public override bool HasImplementation => true;
            public override bool ValidForMultiplayer => false;
            public override bool ValidForMultiplayerAsFreeMod => false;
        }

        private class InvalidMultiplayerFreeMod : Mod
        {
            public override string Name => string.Empty;
            public override LocalisableString Description => string.Empty;
            public override string Acronym => string.Empty;
            public override double ScoreMultiplier => 1;
            public override bool HasImplementation => true;
            public override bool ValidForMultiplayerAsFreeMod => false;
        }

        public class InvalidFreestyleMod : Mod
        {
            public override string Name => string.Empty;
            public override LocalisableString Description => string.Empty;
            public override double ScoreMultiplier => 1;
            public override string Acronym => string.Empty;
            public override bool HasImplementation => true;
            public override bool ValidForFreestyle => false;
        }

        public interface IModCompatibilitySpecification
        {
        }
    }
}
