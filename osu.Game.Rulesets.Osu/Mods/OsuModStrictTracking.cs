// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModStrictTracking : Mod, IApplicableToDifficulty, IApplicableToDrawableHitObject, IApplicableToHitObject
    {
        public override string Name => @"Strict Tracking";
        public override string Acronym => @"ST";
        public override IconUsage? Icon => FontAwesome.Solid.PenFancy;
        public override ModType Type => ModType.DifficultyIncrease;
        public override string Description => @"Follow circles just got serious...";
        public override double ScoreMultiplier => 1.0;
        public override Type[] IncompatibleMods => new[] { typeof(ModClassic) };

        public void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            difficulty.SliderTickRate = 0.0;
        }

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            if (drawable is DrawableSlider slider)
            {
                slider.Tracking.ValueChanged += e =>
                {
                    if (e.NewValue || slider.Judged) return;

                    slider.MissForcefully();

                    foreach (var o in slider.NestedHitObjects)
                    {
                        if (o is DrawableOsuHitObject h && !o.Judged)
                            h.MissForcefully();
                    }
                };
            }
        }

        public void ApplyToHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Slider slider:
                    slider.TailCircle.JudgeAsSliderTick = true;
                    break;
            }
        }
    }
}
