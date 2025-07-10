// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using osu.Framework.Extensions.TypeExtensions;
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

        private static readonly object[] multiplayer_mod_test_scenarios =
        {
            // valid - as allowed mod.
            new MultiplayerTestScenario(false, false, [new OsuModBarrelRoll()], []),
            new MultiplayerTestScenario(false, true, [new OsuModBarrelRoll()], []),
            // valid - as allowed mod (incompatible pair).
            new MultiplayerTestScenario(false, false, [new OsuModHardRock(), new OsuModEasy()], []),
            new MultiplayerTestScenario(false, true, [new OsuModHardRock(), new OsuModEasy()], []),
            // valid - as allowed mod (incompatible pair with derived classes).
            new MultiplayerTestScenario(false, false, [new OsuModDeflate(), new OsuModApproachDifferent()], []),
            new MultiplayerTestScenario(false, true, [new OsuModDeflate(), new OsuModApproachDifferent()], []),
            // valid - as allowed mod (not implemented in all rulesets).
            new MultiplayerTestScenario(false, false, [new OsuModBarrelRoll()], []),
            new MultiplayerTestScenario(false, true, [new OsuModBarrelRoll()], []),
            // valid - as required mod.
            new MultiplayerTestScenario(true, false, [new OsuModStrictTracking()], []),
            // valid - as required mod when not freestyle.
            new MultiplayerTestScenario(true, false, [new InvalidFreestyleRequiredMod()], []),
            // valid - as required mod when freestyle (implemented in all rulesets).
            new MultiplayerTestScenario(true, true, [new OsuModEasy()], []),
            new MultiplayerTestScenario(true, true, [new OsuModNoFail()], []),
            new MultiplayerTestScenario(true, true, [new OsuModHalfTime()], []),
            new MultiplayerTestScenario(true, true, [new OsuModDaycore()], []),
            new MultiplayerTestScenario(true, true, [new OsuModHardRock()], []),
            new MultiplayerTestScenario(true, true, [new OsuModSuddenDeath()], []),
            new MultiplayerTestScenario(true, true, [new OsuModPerfect()], []),
            new MultiplayerTestScenario(true, true, [new OsuModDoubleTime()], []),
            new MultiplayerTestScenario(true, true, [new OsuModNightcore()], []),
            new MultiplayerTestScenario(true, true, [new OsuModDifficultyAdjust()], []),
            new MultiplayerTestScenario(true, true, [new ModWindUp()], []),
            new MultiplayerTestScenario(true, true, [new ModWindDown()], []),
            new MultiplayerTestScenario(true, true, [new OsuModMuted()], []),

            // invalid - always (system mod)
            new MultiplayerTestScenario(false, false, [new OsuModTouchDevice()], [typeof(OsuModTouchDevice)]),
            new MultiplayerTestScenario(true, false, [new OsuModTouchDevice()], [typeof(OsuModTouchDevice)]),
            // invalid - always (multi mod).
            new MultiplayerTestScenario(false, false, [new MultiMod()], [typeof(MultiMod)]),
            new MultiplayerTestScenario(true, false, [new MultiMod()], [typeof(MultiMod)]),
            // invalid - always (disallowed by mod)
            new MultiplayerTestScenario(false, false, [new InvalidMultiplayerMod()], [typeof(InvalidMultiplayerMod)]),
            new MultiplayerTestScenario(true, false, [new InvalidMultiplayerMod()], [typeof(InvalidMultiplayerMod)]),
            new MultiplayerTestScenario(false, false, [new OsuModAutoplay()], [typeof(OsuModAutoplay)]),
            new MultiplayerTestScenario(true, false, [new OsuModAutoplay()], [typeof(OsuModAutoplay)]),
            // invalid - always (changes play length - for now not allowed in multiplayer).
            new MultiplayerTestScenario(false, false, [new ModAdaptiveSpeed()], [typeof(ModAdaptiveSpeed)]),
            new MultiplayerTestScenario(true, false, [new ModAdaptiveSpeed()], [typeof(ModAdaptiveSpeed)]),
            // invalid - as allowed mod (disallowed by mod).
            new MultiplayerTestScenario(false, false, [new InvalidMultiplayerFreeMod()], [typeof(InvalidMultiplayerFreeMod)]),
            new MultiplayerTestScenario(false, true, [new InvalidMultiplayerFreeMod()], [typeof(InvalidMultiplayerFreeMod)]),
            // invalid - as allowed mod (changes play length - for now not allowed in multiplayer).
            new MultiplayerTestScenario(false, false, [new OsuModHalfTime()], [typeof(OsuModHalfTime)]),
            new MultiplayerTestScenario(false, false, [new OsuModDaycore()], [typeof(OsuModDaycore)]),
            new MultiplayerTestScenario(false, false, [new OsuModDoubleTime()], [typeof(OsuModDoubleTime)]),
            new MultiplayerTestScenario(false, false, [new OsuModNightcore()], [typeof(OsuModNightcore)]),
            // invalid - as required mod (incompatible pair)
            new MultiplayerTestScenario(true, false, [new OsuModHidden(), new OsuModApproachDifferent()], [typeof(OsuModHidden), typeof(OsuModApproachDifferent)]),
            new MultiplayerTestScenario(true, true, [new OsuModHidden(), new OsuModApproachDifferent()], [typeof(OsuModHidden), typeof(OsuModApproachDifferent)]),
            new MultiplayerTestScenario(true, false, [new OsuModDeflate(), new OsuModApproachDifferent()], [typeof(OsuModDeflate), typeof(OsuModApproachDifferent)]),
            new MultiplayerTestScenario(true, true, [new OsuModDeflate(), new OsuModApproachDifferent()], [typeof(OsuModDeflate), typeof(OsuModApproachDifferent)]),
            // invalid - as required mod when freestyle (disallowed by mod).
            new MultiplayerTestScenario(true, true, [new InvalidFreestyleRequiredMod()], [typeof(InvalidFreestyleRequiredMod)]),
            // invalid - as required mod when freestyle (not implemented in all rulesets).
            new MultiplayerTestScenario(true, true, [new OsuModStrictTracking()], [typeof(OsuModStrictTracking)]),
            new MultiplayerTestScenario(true, true, [new OsuModBarrelRoll()], [typeof(OsuModBarrelRoll)]),
        };

        [TestCaseSource(nameof(multiplayer_mod_test_scenarios))]
        public void TestMultiplayerModScenarios(MultiplayerTestScenario scenario)
        {
            List<Mod>? invalidMods;
            bool isValid = scenario.IsRequired
                ? ModUtils.CheckValidRequiredModsForMultiplayer(scenario.Mods, scenario.IsFreestyle, out invalidMods)
                : ModUtils.CheckValidAllowedModsForMultiplayer(scenario.Mods, scenario.IsFreestyle, out invalidMods);

            Assert.That(isValid, Is.EqualTo(scenario.InvalidTypes.Length == 0));

            if (isValid)
                Assert.IsNull(invalidMods);
            else
                Assert.That(invalidMods?.Select(t => t.GetType()), Is.EquivalentTo(scenario.InvalidTypes));
        }

        [Test]
        public void TestPlaylistsModScenarios()
        {
            // The rest are tested by TestMultiplayerModScenarios.
            Assert.IsTrue(ModUtils.IsValidModForMatch(new OsuModHardRock(), false, MatchType.Playlists, false));
            Assert.IsTrue(ModUtils.IsValidModForMatch(new OsuModHardRock(), true, MatchType.Playlists, false));
            Assert.IsTrue(ModUtils.IsValidModForMatch(new OsuModDoubleTime(), false, MatchType.Playlists, false));
            Assert.IsTrue(ModUtils.IsValidModForMatch(new OsuModDoubleTime(), true, MatchType.Playlists, false));
            Assert.IsTrue(ModUtils.IsValidModForMatch(new ModAdaptiveSpeed(), false, MatchType.Playlists, false));
            Assert.IsTrue(ModUtils.IsValidModForMatch(new ModAdaptiveSpeed(), true, MatchType.Playlists, false));
        }

        [Test]
        public void TestFreestyleRulesetCompatibility()
        {
            HashSet<string> commonAcronyms = new HashSet<string>();

            commonAcronyms.UnionWith(new OsuRuleset().CreateAllMods().Select(m => m.Acronym));
            commonAcronyms.IntersectWith(new TaikoRuleset().CreateAllMods().Select(m => m.Acronym));
            commonAcronyms.IntersectWith(new CatchRuleset().CreateAllMods().Select(m => m.Acronym));
            commonAcronyms.IntersectWith(new ManiaRuleset().CreateAllMods().Select(m => m.Acronym));

            Assert.Multiple(() =>
            {
                foreach (var ruleset in new Ruleset[] { new OsuRuleset(), new TaikoRuleset(), new CatchRuleset(), new ManiaRuleset() })
                {
                    foreach (var mod in ruleset.CreateAllMods())
                    {
                        if (mod.ValidForFreestyleAsRequiredMod && !mod.UserPlayable)
                            Assert.Fail($"Mod {mod.GetType().ReadableName()} declares {nameof(Mod.ValidForFreestyleAsRequiredMod)} but is not playable!");

                        if (mod.ValidForFreestyleAsRequiredMod && !mod.HasImplementation)
                            Assert.Fail($"Mod {mod.GetType().ReadableName()} declares {nameof(Mod.ValidForFreestyleAsRequiredMod)} but is not implemented!");

                        if (mod.ValidForFreestyleAsRequiredMod && mod.UserPlayable && mod.HasImplementation && !commonAcronyms.Contains(mod.Acronym))
                            Assert.Fail($"{mod.GetType().ReadableName()} declares {nameof(Mod.ValidForFreestyleAsRequiredMod)} but does not exist in all four basic rulesets!");
                    }
                }
            });
        }

        [Test]
        public void TestModsValidForRequiredFreestyleAreConsistentlyCompatibleAcrossRulesets()
        {
            Dictionary<(string firstMod, string secondMod), bool> compatibilityMap = new Dictionary<(string, string), bool>();

            Assert.Multiple(() =>
            {
                for (int rulesetId = 0; rulesetId < 4; ++rulesetId)
                {
                    var rulesetStore = new AssemblyRulesetStore();
                    var ruleset = rulesetStore.GetRuleset(rulesetId)!.CreateInstance();

                    var modsValidForFreestyleAsRequired = ruleset.CreateAllMods().Where(m => m.ValidForFreestyleAsRequiredMod).OrderBy(m => m.Acronym).ToList();

                    for (int i = 0; i < modsValidForFreestyleAsRequired.Count; i++)
                    {
                        for (int j = i; j < modsValidForFreestyleAsRequired.Count; ++j)
                        {
                            var first = modsValidForFreestyleAsRequired[i];
                            var second = modsValidForFreestyleAsRequired[j];

                            bool compatible = ModUtils.CheckCompatibleSet([first, second]);

                            if (!compatibilityMap.TryGetValue((first.Acronym, second.Acronym), out bool previousCompatible))
                                compatibilityMap[(first.Acronym, second.Acronym)] = compatible;
                            else if (previousCompatible != compatible)
                                Assert.Fail($"{first.Acronym} and {second.Acronym} declare {nameof(Mod.ValidForFreestyleAsRequiredMod)} while not being consistently compatible in all four rulesets!");
                        }
                    }
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

        public class InvalidFreestyleRequiredMod : Mod
        {
            public override string Name => string.Empty;
            public override LocalisableString Description => string.Empty;
            public override double ScoreMultiplier => 1;
            public override string Acronym => string.Empty;
            public override bool HasImplementation => true;
            public override bool ValidForFreestyleAsRequiredMod => false;
        }

        public interface IModCompatibilitySpecification;

        public readonly record struct MultiplayerTestScenario(bool IsRequired, bool IsFreestyle, Mod[] Mods, Type[] InvalidTypes)
        {
            public override string ToString()
                => $"{IsRequired}, {IsFreestyle}, [{string.Join(',', Mods.Select(m => m.GetType().ReadableName()))}], [{string.Join(',', InvalidTypes.Select(t => t.ReadableName()))}]";
        }
    }
}
