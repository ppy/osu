// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Tests.Mods
{
    public class TestCustomisableModRuleset : Ruleset
    {
        public static RulesetInfo CreateTestRulesetInfo() => new TestCustomisableModRuleset().RulesetInfo;

        public override IEnumerable<Mod> GetModsFor(ModType type)
        {
            if (type == ModType.Conversion)
            {
                return new Mod[]
                {
                    new TestModCustomisable1(),
                    new TestModCustomisable2()
                };
            }

            return Array.Empty<Mod>();
        }

        public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod> mods = null) => throw new NotImplementedException();

        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => throw new NotImplementedException();

        public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => throw new NotImplementedException();

        public override string Description { get; } = "test";
        public override string ShortName { get; } = "tst";

        public class TestModCustomisable1 : TestModCustomisable
        {
            public override string Name => "Customisable Mod 1";

            public override string Acronym => "CM1";
        }

        public class TestModCustomisable2 : TestModCustomisable
        {
            public override string Name => "Customisable Mod 2";

            public override string Acronym => "CM2";

            public override bool RequiresConfiguration => true;
        }

        public abstract class TestModCustomisable : Mod, IApplicableMod
        {
            public override double ScoreMultiplier => 1.0;

            public override string Description => "This is a customisable test mod.";

            public override ModType Type => ModType.Conversion;

            [SettingSource("Sample float", "Change something for a mod")]
            public BindableFloat SliderBindable { get; } = new BindableFloat
            {
                MinValue = 0,
                MaxValue = 10,
                Default = 5,
                Value = 7
            };

            [SettingSource("Sample bool", "Clicking this changes a setting")]
            public BindableBool TickBindable { get; } = new BindableBool();
        }
    }
}
