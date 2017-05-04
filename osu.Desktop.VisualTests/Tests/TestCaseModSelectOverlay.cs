// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.Mods;
using osu.Framework.Testing;
using osu.Game.Database;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseModSelectOverlay : TestCase
    {
        public override string Description => @"Tests the mod select overlay";

        protected ModSelectOverlay ModSelect;
        private RulesetDatabase rulesets;

        [BackgroundDependencyLoader]
        private void load(RulesetDatabase rulesets)
        {
            this.rulesets = rulesets;
        }

        public override void Reset()
        {
            base.Reset();

            Add(ModSelect = new ModSelectOverlay
            {
                RelativeSizeAxes = Axes.X,
                Origin = Anchor.BottomCentre,
                Anchor = Anchor.BottomCentre,
            });

            AddStep("Toggle", ModSelect.ToggleVisibility);

            foreach (var ruleset in rulesets.AllRulesets)
                AddStep(ruleset.CreateInstance().Description, () => ModSelect.Ruleset.Value = ruleset);
        }
    }
}
