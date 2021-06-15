// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
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

        [SettingSource("Fade-out Time", "The bigger this multiplier is, the sooner the notes will start fading out")]
        public BindableNumber<double> FadeOutTimeMultiplier { get; } = new BindableDouble
        {
            MinValue = 0.5,
            MaxValue = 1.5,
            Default = 1.0,
            Value = 1.0,
            Precision = 0.01,
        };

        /// <summary>
        /// In stable taiko, the hit position is 160, so the active playfield is essentially 160 pixels shorter
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
            var beatLength = controlPointInfo.TimingPointAt(position)?.BeatLength;
            var speedMultiplier = controlPointInfo.DifficultyPointAt(position)?.SpeedMultiplier;
            return originalSliderMultiplier * (speedMultiplier ?? 1.0) * TimingControlPoint.DEFAULT_BEAT_LENGTH / (beatLength ?? TimingControlPoint.DEFAULT_BEAT_LENGTH);
        }

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            switch (hitObject)
            {
                case DrawableDrumRollTick _:
                    break;

                case DrawableHit _:
                    break;

                default:
                    return;
            }

            var preempt = 10000 / MultiplierAt(hitObject.HitObject.StartTime) * FadeOutTimeMultiplier.Value;
            var start = hitObject.HitObject.StartTime - preempt * 0.6;
            var duration = preempt * 0.3;

            using (hitObject.BeginAbsoluteSequence(start))
            {
                hitObject.FadeOut(duration);

                // DrawableHitObject sets LifetimeEnd to LatestTransformEndTime if it isn't manually changed.
                // in order for the object to not be killed before its actual end time (as the latest transform ends earlier), set lifetime end explicitly.
                hitObject.LifetimeEnd = state == ArmedState.Idle
                    ? hitObject.HitObject.GetEndTime() + hitObject.HitObject.HitWindows.WindowFor(HitResult.Miss)
                    : hitObject.HitStateUpdateTime;
            }
        }

        public void ReadFromDifficulty(BeatmapDifficulty difficulty)
        {
        }

        public void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            originalSliderMultiplier = difficulty.SliderMultiplier;
            difficulty.SliderMultiplier /= hd_sv_scale;
        }

        public override void ApplyToBeatmap(IBeatmap beatmap)
        {
            controlPointInfo = beatmap.ControlPointInfo;
        }
    }
}
