// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Configuration;
using osu.Game.Rulesets;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public partial class AnalysisSettings : PlayerSettingsGroup
    {
        protected Ruleset Ruleset;

        public AnalysisSettings(Ruleset ruleset)
            : base("Analysis Settings")
        {
            Ruleset = ruleset;

            AddRange(this.CreateSettingsControls());
        }
    }
}
