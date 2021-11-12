// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Mods;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModNoScope : ModNoScope, IUpdatableByPlayfield
    {
        public override string Description => "Where's the catcher?";

        [SettingSource(
            "Hidden at combo",
            "The combo count at which the catcher becomes completely hidden",
            SettingControlType = typeof(SettingsSlider<int, HiddenComboSlider>)
        )]
        public override BindableInt HiddenComboCount { get; } = new BindableInt
        {
            Default = 10,
            Value = 10,
            MinValue = 0,
            MaxValue = 50,
        };

        public void Update(Playfield playfield)
        {
            var catchPlayfield = (CatchPlayfield)playfield;
            bool shouldAlwaysShowCatcher = IsBreakTime.Value;
            float targetAlpha = shouldAlwaysShowCatcher ? 1 : ComboBasedAlpha;
            catchPlayfield.CatcherArea.Alpha = (float)Interpolation.Lerp(catchPlayfield.CatcherArea.Alpha, targetAlpha, Math.Clamp(catchPlayfield.Time.Elapsed / TRANSITION_DURATION, 0, 1));
        }
    }
}
