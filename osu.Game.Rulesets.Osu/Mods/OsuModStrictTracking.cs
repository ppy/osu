// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Mods.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModStrictTracking : Mod, IApplicableAfterBeatmapConversion, IApplicableToDrawableHitObject, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override string Name => @"Strict Tracking";
        public override string Acronym => @"ST";
        public override IconUsage? Icon => FontAwesome.Solid.PenFancy;
        public override ModType Type => ModType.DifficultyIncrease;
        public override string Description => @"Follow circles just got serious...";
        public override double ScoreMultiplier => 1.0;
        public override Type[] IncompatibleMods => new[] { typeof(ModClassic) };

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            if (drawable is DrawableSlider slider)
            {
                slider.Tracking.ValueChanged += e =>
                {
                    if (e.NewValue || slider.Judged) return;

                    var tail = slider.NestedHitObjects.OfType<StrictTrackingDrawableSliderTail>().First();

                    if (!tail.Judged)
                        tail.MissForcefully();
                };
            }
        }

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var osuBeatmap = (OsuBeatmap)beatmap;

            if (osuBeatmap.HitObjects.Count == 0) return;

            var hitObjects = osuBeatmap.HitObjects.Select(ho =>
            {
                if (ho is Slider slider)
                {
                    var newSlider = new StrictTrackingSlider(slider);
                    return newSlider;
                }

                return ho;
            }).ToList();

            osuBeatmap.HitObjects = hitObjects;
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            drawableRuleset.Playfield.RegisterPool<StrictTrackingSliderTailCircle, StrictTrackingDrawableSliderTail>(10, 100);
        }
    }
}
