// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.KeyBinding
{
    public class RulesetBindingsSection : SettingsSection
    {
        public override FontAwesome Icon => (ruleset.CreateInstance().CreateIcon() as SpriteIcon)?.Icon ?? FontAwesome.fa_osu_hot;
        public override string Header => ruleset.Name;

        private readonly RulesetInfo ruleset;

        public RulesetBindingsSection(RulesetInfo ruleset)
        {
            this.ruleset = ruleset;

            var r = ruleset.CreateInstance();

            foreach (var variant in r.AvailableVariants)
                Add(new VariantBindingsSubsection(ruleset, variant));
        }
    }
}
