// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Input.Bindings;
using osu.Framework.Testing;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Game.Tests.Visual;
using osuTK.Input;

namespace osu.Game.Tests.Input
{
    [HeadlessTest]
    public partial class RealmKeyBindingTest : OsuTestScene
    {
        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Test]
        public void TestUnmapGlobalAction()
        {
            var keyBinding = new RealmKeyBinding(GlobalAction.ToggleReplaySettings, KeyCombination.FromKey(Key.Z));

            AddAssert("action is integer", () => keyBinding.Action, () => Is.EqualTo((int)GlobalAction.ToggleReplaySettings));
            AddAssert("action unmaps correctly", () => keyBinding.GetAction(rulesets), () => Is.EqualTo(GlobalAction.ToggleReplaySettings));
        }

        [TestCase(typeof(OsuRuleset), OsuAction.Smoke, null)]
        [TestCase(typeof(TaikoRuleset), TaikoAction.LeftCentre, null)]
        [TestCase(typeof(CatchRuleset), CatchAction.MoveRight, null)]
        [TestCase(typeof(ManiaRuleset), ManiaAction.Key7, 7)]
        public void TestUnmapRulesetActions(Type rulesetType, object action, int? variant)
        {
            string rulesetName = ((Ruleset)Activator.CreateInstance(rulesetType)!).ShortName;
            var keyBinding = new RealmKeyBinding(action, KeyCombination.FromKey(Key.Z), rulesetName, variant);

            AddAssert("action is integer", () => keyBinding.Action, () => Is.EqualTo((int)action));
            AddAssert("action unmaps correctly", () => keyBinding.GetAction(rulesets), () => Is.EqualTo(action));
        }
    }
}
