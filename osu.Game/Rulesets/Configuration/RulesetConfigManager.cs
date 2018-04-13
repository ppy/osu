// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Configuration;

namespace osu.Game.Rulesets.Configuration
{
    public abstract class RulesetConfigManager<T> : DatabasedConfigManager<T>, IRulesetConfigManager
        where T : struct
    {
        protected RulesetConfigManager(SettingsStore settings, RulesetInfo ruleset, int variant) : base(settings, ruleset, variant)
        {
        }
    }
}
