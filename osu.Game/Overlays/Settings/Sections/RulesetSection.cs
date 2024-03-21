// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.Settings.Sections
{
    public partial class RulesetSection : SettingsSection
    {
        public override LocalisableString Header => RulesetSettingsStrings.Rulesets;

        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = OsuIcon.Rulesets
        };

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            foreach (Ruleset ruleset in rulesets.AvailableRulesets.Select(info => info.CreateInstance()))
            {
                try
                {
                    SettingsSubsection? section = ruleset.CreateSettings();

                    if (section != null)
                        Add(section);
                }
                catch
                {
                    Logger.Log($"Failed to load ruleset settings for {ruleset.RulesetInfo.Name}. Please check for an update from the developer.", level: LogLevel.Error);
                }
            }
        }
    }
}
