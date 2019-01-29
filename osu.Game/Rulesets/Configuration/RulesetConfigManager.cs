// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Configuration;

namespace osu.Game.Rulesets.Configuration
{
    public abstract class RulesetConfigManager<T> : DatabasedConfigManager<T>, IRulesetConfigManager
        where T : struct
    {
        protected RulesetConfigManager(SettingsStore settings, RulesetInfo ruleset, int? variant = null)
            : base(settings, ruleset, variant)
        {
        }
    }
}
