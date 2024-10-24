// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Game.Configuration;
using osu.Game.Rulesets.Configuration;
using osu.Game.Screens.Play.PlayerSettings;

namespace osu.Game.Rulesets.UI
{
    public partial class ReplayAnalysisSettings : PlayerSettingsGroup
    {
        private readonly Ruleset ruleset;

        protected IRulesetConfigManager Config;

        public ReplayAnalysisSettings(Ruleset ruleset)
            : base("Analysis Settings")
        {
            this.ruleset = ruleset;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRange(this.CreateSettingsControls());
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            Config = dependencies.Get<IRulesetConfigCache>().GetConfigFor(ruleset);
            if (Config is not null)
                dependencies.Cache(Config);

            return dependencies;
        }
    }
}
