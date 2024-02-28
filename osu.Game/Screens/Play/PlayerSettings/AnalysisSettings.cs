// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Replays;
using osu.Game.Rulesets.UI;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public abstract partial class AnalysisSettings : PlayerSettingsGroup
    {
        protected DrawableRuleset DrawableRuleset;

        protected AnalysisSettings(DrawableRuleset drawableRuleset)
            : base("Analysis Settings")
        {
            DrawableRuleset = drawableRuleset;
        }

        public abstract AnalysisContainer CreateAnalysisContainer(Replay replay);
    }
}
