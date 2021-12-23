// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Configuration;

namespace osu.Game.Overlays.Settings
{
    /// <summary>
    /// A <see cref="SettingsSubsection"/> which provides subclasses with the <see cref="IRulesetConfigManager"/>
    /// from the <see cref="Ruleset"/>'s <see cref="Ruleset.CreateConfig(SettingsStore)"/>.
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

            Config = dependencies.Get<IRulesetConfigCache>().GetConfigFor(ruleset);
            if (Config != null)
                dependencies.Cache(Config);

            return dependencies;
        }
    }
}
