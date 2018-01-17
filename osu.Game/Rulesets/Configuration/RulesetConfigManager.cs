// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Platform;

namespace osu.Game.Rulesets.Configuration
{
    public abstract class RulesetConfigManager<T> : ConfigManager<T>, IRulesetConfigManager
        where T : struct
    {
        protected override string Filename => ruleset?.ShortName;
        private readonly Ruleset ruleset;

        protected RulesetConfigManager(Ruleset ruleset, Storage storage)
            : base(storage)
        {
            this.ruleset = ruleset;

            // Re-load with the ruleset
            Load();
        }
    }
}
