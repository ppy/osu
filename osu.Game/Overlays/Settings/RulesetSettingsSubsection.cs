// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Configuration;

namespace osu.Game.Overlays.Settings
{
    /// <summary>
    /// A <see cref="SettingsSubsection"/> which provides subclasses with the <see cref="IRulesetConfigManager"/>
    /// from the <see cref="Ruleset"/>'s <see cref="Ruleset.CreateConfig()"/>.
    /// </summary>
    public abstract class RulesetSettingsSubsection : SettingsSubsection
    {
        private readonly Ruleset ruleset;

        protected IRulesetConfigManager Config;

        protected RulesetSettingsSubsection(Ruleset ruleset)
        {
            this.ruleset = ruleset;
        }

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            Config = dependencies.Get<RulesetConfigCache>().GetConfigFor(ruleset);
            if (Config != null)
                dependencies.Cache(Config);

            return dependencies;
        }
    }
}
