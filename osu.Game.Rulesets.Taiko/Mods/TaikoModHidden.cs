// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects.Drawables;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModHidden : ModHidden, IApplicableToDifficulty
    {
        public override string Description => @"Beats fade out before you hit them!";
        public override double ScoreMultiplier => 1.06;

        /// <summary>
        /// In osu-stable, the hit position is 160, so the active playfield is essentially 160 pixels shorter
        /// than the actual screen width. The normalized playfield height is 480, so on a 4:3 screen the
        /// playfield ratio of the active area up to the hit position will actually be (640 - 160) / 480 = 1.
        /// For custom resolutions/aspect ratios (x:y), the screen width given the normalized height becomes 480 * x / y instead,
        /// and the playfield ratio becomes (480 * x / y - 160) / 480 = x / y - 1/3.
        /// This constant is equal to the playfield ratio on 4:3 screens divided by the playfield ratio on 16:9 screens.
        /// </summary>
        private const double hd_sv_scale = (4.0 / 3.0 - 1.0 / 3.0) / (16.0 / 9.0 - 1.0 / 3.0);

        private double originalSliderMultiplier;

        private ControlPointInfo controlPointInfo;

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            ApplyNormalVisibilityState(hitObject, state);
        }

        protected double MultiplierAt(double position)
        {
            double beatLength = controlPointInfo.TimingPointAt(position).BeatLength;
            double speedMultiplier = controlPointInfo.DifficultyPointAt(position).SpeedMultiplier;

            return originalSliderMultiplier * speedMultiplier * TimingControlPoint.DEFAULT_BEAT_LENGTH / beatLength;
        }

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            switch (hitObject)
            {
                case DrawableDrumRollTick _:
                case DrawableHit _:
                    double preempt = 10000 / MultiplierAt(hitObject.HitObject.StartTime);
                    double start = hitObject.HitObject.StartTime - preempt * 0.6;
                    double duration = preempt * 0.3;

                    using (hitObject.BeginAbsoluteSequence(start))
                    {
                        hitObject.FadeOut(duration);

                        // DrawableHitObject sets LifetimeEnd to LatestTransformEndTime if it isn't manually changed.
                        // in order for the object to not be killed before its actual end time (as the latest transform ends earlier), set lifetime end explicitly.
                        hitObject.LifetimeEnd = state == ArmedState.Idle || !hitObject.AllJudged
                            ? hitObject.HitObject.GetEndTime() + hitObject.HitObject.HitWindows.WindowFor(HitResult.Miss)
                            : hitObject.HitStateUpdateTime;
                    }

                    break;
            }
        }

        public void ReadFromDifficulty(BeatmapDifficulty difficulty)
        {
        }

        public void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            // needs to be read after all processing has been run (TaikoBeatmapConverter applies an adjustment which would otherwise be omitted).
            originalSliderMultiplier = difficulty.SliderMultiplier;

            // osu-stable has an added playfield cover that essentially forces a 4:3 playfield ratio, by cutting off all objects past that size.
            // This is not yet implemented; instead a playfield adjustment container is present which maintains a 16:9 ratio.
            // For now, increase the slider multiplier proportionally so that the notes stay on the screen for the same amount of time as on stable.
            // Note that this means that the notes will scroll faster as they have a longer distance to travel on the screen in that same amount of time.
            difficulty.SliderMultiplier /= hd_sv_scale;
        }

        public override void ApplyToBeatmap(IBeatmap beatmap)
        {
            controlPointInfo = beatmap.ControlPointInfo;
        }
    }
}
