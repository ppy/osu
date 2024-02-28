// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.UI;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Rulesets
{
    [HeadlessTest]
    public partial class TestSceneBrokenRulesetHandling : OsuTestScene
    {
        [Resolved]
        private OsuGameBase gameBase { get; set; } = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("reset ruleset", () => Ruleset.Value = new OsuRuleset().RulesetInfo);
        }

        [Test]
        public void TestNullModsReturnedByRulesetAreIgnored()
        {
            AddStep("set ruleset with null mods", () => Ruleset.Value = new TestRulesetWithNullMods().RulesetInfo);
            AddAssert("no null mods in available mods", () => gameBase.AvailableMods.Value.SelectMany(kvp => kvp.Value).All(mod => mod != null));
        }

        [Test]
        public void TestRulesetRevertedIfModsCannotBeRetrieved()
        {
            RulesetInfo ruleset = null!;

            AddStep("store current ruleset", () => ruleset = Ruleset.Value);

            AddStep("set API incompatible ruleset", () => Ruleset.Value = new TestAPIIncompatibleRuleset().RulesetInfo);
            AddAssert("ruleset not changed", () => Ruleset.Value.Equals(ruleset));
        }

#nullable disable // purposefully disabling nullability to simulate broken or unannotated API user code.

        private class TestRulesetWithNullMods : Ruleset
        {
            public override string ShortName => "nullmods";
            public override string Description => "nullmods";

            public override IEnumerable<Mod> GetModsFor(ModType type) => new Mod[] { null };

            public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod> mods = null) => null!;
            public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => null!;
            public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => null!;
        }

        private class TestAPIIncompatibleRuleset : Ruleset
        {
            public override string ShortName => "incompatible";
            public override string Description => "incompatible";

            // simulate API incompatibility by throwing similar exceptions.
            public override IEnumerable<Mod> GetModsFor(ModType type) => throw new MissingMethodException();

            public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod> mods = null) => null!;
            public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => null!;
            public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => null!;
        }
    }
}
