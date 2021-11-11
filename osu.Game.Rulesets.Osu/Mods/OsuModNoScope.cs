// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModNoScope : ModNoScope, IUpdatableByPlayfield, IApplicableToBeatmap
    {
        public override string Description => "Where's the cursor?";

        private PeriodTracker spinnerPeriods;

        [SettingSource(
            "Hidden at combo",
            "The combo count at which the cursor becomes completely hidden",
            SettingControlType = typeof(SettingsSlider<int, HiddenComboSlider>)
        )]
        public override BindableInt HiddenComboCount { get; } = new BindableInt
        {
            Default = 10,
            Value = 10,
            MinValue = 0,
            MaxValue = 50,
        };

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            spinnerPeriods = new PeriodTracker(beatmap.HitObjects.OfType<Spinner>().Select(b => new Period(b.StartTime - TRANSITION_DURATION, b.EndTime)));
        }

        public void Update(Playfield playfield)
        {
            bool shouldAlwaysShowCursor = IsBreakTime.Value || spinnerPeriods.IsInAny(playfield.Clock.CurrentTime);
            float targetAlpha = shouldAlwaysShowCursor ? 1 : ComboBasedAlpha;
            playfield.Cursor.Alpha = (float)Interpolation.Lerp(playfield.Cursor.Alpha, targetAlpha, Math.Clamp(playfield.Time.Elapsed / TRANSITION_DURATION, 0, 1));
        }
    }
}
