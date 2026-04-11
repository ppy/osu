// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Tests.Online.Multiplayer
{
    [TestFixture]
    public class TestMultiplayerModMultiplierApplicator
    {
        [Test]
        public void TestNoCustomMultipliers_UsesOriginal()
        {
            var mods = new[] { new OsuModEasy() };
            double result = MultiplayerModMultiplierApplicator.GetEffectiveMultiplier(mods, null);

            Assert.That(result, Is.EqualTo(new OsuModEasy().ScoreMultiplier).Within(0.001));
        }

        [Test]
        public void TestCustomMultiplierOverridesOriginal()
        {
            var mods = new[] { new OsuModEasy() };
            var custom = new Dictionary<string, double> { ["EZ"] = 1.0 };

            double result = MultiplayerModMultiplierApplicator.GetEffectiveMultiplier(mods, custom);

            Assert.That(result, Is.EqualTo(1.0).Within(0.001));
        }

        [Test]
        public void TestCustomMultiplierClampsToMin()
        {
            var mods = new[] { new OsuModEasy() };
            var custom = new Dictionary<string, double> { ["EZ"] = -5.0 };

            double result = MultiplayerModMultiplierApplicator.GetEffectiveMultiplier(mods, custom);

            Assert.That(result, Is.EqualTo(MultiplayerModMultiplierApplicator.MIN_MULTIPLIER).Within(0.001));
        }

        [Test]
        public void TestCustomMultiplierClampsToMax()
        {
            var mods = new[] { new OsuModEasy() };
            var custom = new Dictionary<string, double> { ["EZ"] = 999.0 };

            double result = MultiplayerModMultiplierApplicator.GetEffectiveMultiplier(mods, custom);

            Assert.That(result, Is.EqualTo(MultiplayerModMultiplierApplicator.MAX_MULTIPLIER).Within(0.001));
        }

        [Test]
        public void TestMultipleModsMultiplyCorrectly()
        {
            IEnumerable<Mod> mods = new List<Mod> { new OsuModEasy(), new OsuModNoFail() };
            var custom = new Dictionary<string, double>
            {
                ["EZ"] = 1.0,
                ["NF"] = 0.5,
            };

            double result = MultiplayerModMultiplierApplicator.GetEffectiveMultiplier(mods, custom);

            // 1.0 * 0.5 = 0.5
            Assert.That(result, Is.EqualTo(0.5).Within(0.001));
        }

        [Test]
        public void TestUnknownModAcronymIgnored()
        {
            IEnumerable<Mod> mods = new List<Mod> { new OsuModEasy() };
            var custom = new Dictionary<string, double> { ["UNKNOWN"] = 5.0 };

            double result = MultiplayerModMultiplierApplicator.GetEffectiveMultiplier(mods, custom);

            // Should fall back to original EZ multiplier
            Assert.That(result, Is.EqualTo(new OsuModEasy().ScoreMultiplier).Within(0.001));
        }

        [Test]
        public void TestSanitiseRemovesInvalidEntries()
        {
            var raw = new Dictionary<string, double>
            {
                ["EZ"] = 0.5,
                [""] = 2.0,   // empty key — should be removed
                ["NF"] = 999, // should be clamped to MAX
                ["HD"] = -1,  // should be clamped to MIN
            };

            var sanitised = MultiplayerModMultiplierApplicator.Sanitise(raw);

            Assert.That(sanitised.ContainsKey(""), Is.False);
            Assert.That(sanitised["EZ"], Is.EqualTo(0.5).Within(0.001));
            Assert.That(sanitised["NF"], Is.EqualTo(MultiplayerModMultiplierApplicator.MAX_MULTIPLIER).Within(0.001));
            Assert.That(sanitised["HD"], Is.EqualTo(MultiplayerModMultiplierApplicator.MIN_MULTIPLIER).Within(0.001));
        }

        [Test]
        public void TestRoomSettingsEquality_WithDifferentModMultipliers()
        {
            var settings1 = new MultiplayerRoomSettings
            {
                Name = "Room",
                ModMultipliers = new Dictionary<string, double> { ["EZ"] = 1.0 }
            };

            var settings2 = new MultiplayerRoomSettings
            {
                Name = "Room",
                ModMultipliers = new Dictionary<string, double> { ["EZ"] = 0.5 }
            };

            Assert.That(settings1.Equals(settings2), Is.False);
        }

        [Test]
        public void TestRoomSettingsEquality_WithSameModMultipliers()
        {
            var settings1 = new MultiplayerRoomSettings
            {
                Name = "Room",
                ModMultipliers = new Dictionary<string, double> { ["EZ"] = 1.0 }
            };

            var settings2 = new MultiplayerRoomSettings
            {
                Name = "Room",
                ModMultipliers = new Dictionary<string, double> { ["EZ"] = 1.0 }
            };

            Assert.That(settings1.Equals(settings2), Is.True);
        }

        [Test]
        public void TestRoomSettingsEquality_DifferentOrderSameContent()
        {
            var settings1 = new MultiplayerRoomSettings
            {
                Name = "Room",
                ModMultipliers = new Dictionary<string, double>
                {
                    ["EZ"] = 1.0,
                    ["HD"] = 1.06
                }
            };

            var settings2 = new MultiplayerRoomSettings
            {
                Name = "Room",
                ModMultipliers = new Dictionary<string, double>
                {
                    ["HD"] = 1.06,
                    ["EZ"] = 1.0
                }
            };

            // Should be equal regardless of insertion order
            Assert.That(settings1.Equals(settings2), Is.True);
        }

        [Test]
        public void TestRoomSettingsEquality_EmptyVsNonEmpty()
        {
            var settings1 = new MultiplayerRoomSettings { Name = "Room" };
            var settings2 = new MultiplayerRoomSettings
            {
                Name = "Room",
                ModMultipliers = new Dictionary<string, double> { ["EZ"] = 1.0 }
            };

            Assert.That(settings1.Equals(settings2), Is.False);
        }
    }
}
