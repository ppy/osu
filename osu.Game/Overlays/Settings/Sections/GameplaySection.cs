// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings.Sections.Gameplay;
using osu.Game.Rulesets;
using System.Linq;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Settings.Sections
{
    public class GameplaySection : SettingsSection
    {
        public override string Header => "Gameplay";
        public override IconUsage Icon => FontAwesome.Regular.Circle;

        public GameplaySection()
        {
            Children = new Drawable[]
            {
                new GeneralSettings(),
                new SongSelectSettings(),
                new ModsSettings(),
            };
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            foreach (Ruleset ruleset in rulesets.AvailableRulesets.Select(info => info.CreateInstance()))
            {
                SettingsSubsection section = ruleset.CreateSettings();
                if (section != null)
                    Add(section);
            }
        }
    }
}
