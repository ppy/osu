// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.Settings
{
    /// <summary>
    /// A <see cref="SettingsSubsection"/> which provides subclasses with the <see cref="IRulesetConfigManager"/>
    /// from the <see cref="Ruleset"/>'s <see cref="Ruleset.CreateConfig()"/>.
    /// </summary>
    public abstract class RulesetSettingsSubsection : SettingsSubsection
    {
        private readonly Ruleset ruleset;

        protected RulesetSettingsSubsection(Ruleset ruleset)
        {
            this.ruleset = ruleset;
        }

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent)
        {
            dependencies = new DependencyContainer(base.CreateLocalDependencies(parent));

            var config = dependencies.Get<RulesetConfigCache>().GetConfigFor(ruleset);
            if (config != null)
                dependencies.Cache(config);

            return dependencies;
        }
    }
}
