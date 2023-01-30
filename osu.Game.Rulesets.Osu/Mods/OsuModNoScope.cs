// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModNoScope : ModNoScope, IUpdatableByPlayfield, IApplicableToBeatmap
    {
        public override LocalisableString Description => "Where's the cursor?";

        private PeriodTracker spinnerPeriods = null!;

        public override BindableInt HiddenComboCount { get; } = new BindableInt(10)
        {
            MinValue = 0,
            MaxValue = 50,
        };

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            spinnerPeriods = new PeriodTracker(beatmap.HitObjects.OfType<Spinner>().Select(b => new Period(b.StartTime - TRANSITION_DURATION, b.EndTime)));
        }

        public void Update(Playfield playfield)
        {
            var osuPlayfield = (OsuPlayfield)playfield;
            Debug.Assert(osuPlayfield.Cursor != null);

            bool shouldAlwaysShowCursor = IsBreakTime.Value || spinnerPeriods.IsInAny(osuPlayfield.Clock.CurrentTime);
            float targetAlpha = shouldAlwaysShowCursor ? 1 : ComboBasedAlpha;
            float currentAlpha = (float)Interpolation.Lerp(osuPlayfield.Cursor.Alpha, targetAlpha, Math.Clamp(osuPlayfield.Time.Elapsed / TRANSITION_DURATION, 0, 1));

            osuPlayfield.Cursor.Alpha = currentAlpha;
            osuPlayfield.Smoke.Alpha = currentAlpha;
        }
    }
}
