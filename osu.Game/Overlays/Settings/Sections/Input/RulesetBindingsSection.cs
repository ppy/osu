// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public partial class RulesetBindingsSection : SettingsSection
    {
        public override Drawable CreateIcon() => ruleset.CreateInstance().CreateIcon();

        public override LocalisableString Header => ruleset.Name;

        private readonly RulesetInfo ruleset;

        public RulesetBindingsSection(RulesetInfo ruleset)
        {
            this.ruleset = ruleset;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var r = ruleset.CreateInstance();

            foreach (int variant in r.AvailableVariants)
                Add(new VariantBindingsSubsection(ruleset, variant));
        }
    }
}
