// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Screens.Edit;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Mods
{
    /// <summary>
    /// Mod that colours <see cref="HitObject"/>s based on the musical division they are on
    /// </summary>
    public class CatchModSynesthesia : ModSynesthesia, IApplicableToBeatmap, IApplicableToDrawableHitObject
    {
        private readonly OsuColour colours = new OsuColour();

        private IBeatmap? currentBeatmap { get; set; }

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            //Store a reference to the current beatmap to look up the beat divisor when notes are drawn
            if (currentBeatmap != beatmap)
                currentBeatmap = beatmap;
        }

        public void ApplyToDrawableHitObject(DrawableHitObject d)
        {
            if (currentBeatmap == null) return;

            Color4? timingBasedColour = null;

            d.HitObjectApplied += _ =>
            {
                // Block bananas from getting coloured.
                if (d.HitObject is not Banana)
                {
                    timingBasedColour = BindableBeatDivisor.GetColourFor(currentBeatmap.ControlPointInfo.GetClosestBeatDivisor(d.HitObject.StartTime), colours);
                }

                // Colour droplets into a solid colour, as droplets aren't generated snapped to timeline ticks.
                if (d.HitObject is Droplet)
                {
                    timingBasedColour = Color4.LightGreen;
                }
            };

            // Need to set this every update to ensure it doesn't get overwritten by DrawableHitObject.OnApply() -> UpdateComboColour().
            d.OnUpdate += _ =>
            {
                if (timingBasedColour != null)
                    d.AccentColour.Value = timingBasedColour.Value;
            };
        }
    }
}
