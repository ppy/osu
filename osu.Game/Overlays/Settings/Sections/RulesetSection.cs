// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.Settings.Sections
{
    public partial class RulesetSection : SettingsSection
    {
        private readonly Ruleset ruleset;

        public RulesetSection(Ruleset ruleset, SettingsSubsection section)
        {
            UseSmallerSidebarButton = true;

            this.ruleset = ruleset;

            Add(section);
        }

        public override LocalisableString Header => ruleset.RulesetInfo.Name;

        public override Drawable CreateIcon() => ruleset.CreateIcon();
    }
}
