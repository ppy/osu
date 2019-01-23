// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Configuration;
using osu.Game.Rulesets.Configuration;

namespace osu.Game.Rulesets.Osu.Configuration
{
    public class OsuConfigManager : RulesetConfigManager<OsuSetting>
    {
        public OsuConfigManager(SettingsStore settings, RulesetInfo ruleset, int? variant = null)
            : base(settings, ruleset, variant)
        {
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            Set(OsuSetting.SnakingInSliders, true);
            Set(OsuSetting.SnakingOutSliders, true);
            Set(OsuSetting.ShowCursorTrail, true);
        }
    }

    public enum OsuSetting
    {
        SnakingInSliders,
        SnakingOutSliders,
        ShowCursorTrail
    }
}
