// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.UI;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public partial class AnalysisSettings : PlayerSettingsGroup
    {
        protected DrawableRuleset drawableRuleset;

        public AnalysisSettings(DrawableRuleset drawableRuleset)
            : base("Analysis Settings")
        {
            this.drawableRuleset = drawableRuleset;
        }
    }
}