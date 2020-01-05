// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Osu.Configuration;

namespace osu.Game.Rulesets.Osu.UI
{
    public class OsuSettingsSubsection : RulesetSettingsSubsection
    {
        protected override string Header => "osu!";

        public OsuSettingsSubsection(Ruleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (OsuRulesetConfigManager)Config;

            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "渐入滑条",
                    Bindable = config.GetBindable<bool>(OsuRulesetSetting.SnakingInSliders)
                },
                new SettingsCheckbox
                {
                    LabelText = "渐出滑条",
                    Bindable = config.GetBindable<bool>(OsuRulesetSetting.SnakingOutSliders)
                },
                new SettingsCheckbox
                {
                    LabelText = "光标轨迹",
                    Bindable = config.GetBindable<bool>(OsuRulesetSetting.ShowCursorTrail)
                },
            };
        }
    }
}
