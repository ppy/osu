// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Configuration;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public partial class AnalysisSettings : PlayerSettingsGroup
    {
        public AnalysisSettings()
            : base("Analysis Settings")
        {
            AddRange(this.CreateSettingsControls());
        }
    }
}
