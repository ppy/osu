// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Rulesets;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections
{
    public class RulesetSection : SettingsSection
    {
        public override LocalisableString Header => RulesetSettingsStrings.Rulesets;

        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = FontAwesome.Solid.Chess
        };

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            foreach (Ruleset ruleset in rulesets.AvailableRulesets.Select(info => info.CreateInstance()))
            {
                try
                {
                    SettingsSubsection section = ruleset.CreateSettings();

                    if (section != null)
                        Add(section);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Failed to load ruleset settings");
                }
            }
        }
    }
}
