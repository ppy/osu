// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Framework.Utils;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModNoScope : ModNoScope, IApplicableToBeatmap, IApplicableToDrawableRuleset<CatchHitObject>
    {
        public override string Description => "Where's the catcher?";

        public PeriodTracker BananaShowerPeriods;

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            BananaShowerPeriods = new PeriodTracker(beatmap.HitObjects.OfType<BananaShower>().Select(b => new Period(b.StartTime - TRANSITION_DURATION, b.EndTime)));
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var playfield = (CatchPlayfield)drawableRuleset.Playfield;
            playfield.OnUpdate += _ =>
            {
                bool shouldAlwaysShowCatcher = IsBreakTime.Value || BananaShowerPeriods.IsInAny(playfield.Clock.CurrentTime);
                float targetAlpha = shouldAlwaysShowCatcher ? 1 : ComboBasedAlpha;
                playfield.CatcherArea.Alpha = (float)Interpolation.Lerp(playfield.CatcherArea.Alpha, targetAlpha, Math.Clamp(playfield.Time.Elapsed / TRANSITION_DURATION, 0, 1));
            };
        }
    }
}
