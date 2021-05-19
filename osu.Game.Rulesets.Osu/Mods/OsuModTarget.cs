// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModTarget : ModWithVisibilityAdjustment, IApplicableToDrawableRuleset<OsuHitObject>,
        IApplicableToHealthProcessor, IApplicableToDifficulty
    {
        public override string Name => "Target";
        public override string Acronym => "TP";
        public override ModType Type => ModType.Conversion;
        public override IconUsage? Icon => OsuIcon.ModTarget;
        public override string Description => @"Practice keeping up with the beat of the song.";
        public override double ScoreMultiplier => 1;

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            // Sudden death
            healthProcessor.FailConditions += (_, result)
                => result.Type.AffectsCombo()
                && !result.IsHit;
        }

        // Maximum distance to jump
        public const float MAX_DISTANCE = 250f;

        public override void ApplyToBeatmap(IBeatmap beatmap)
        {
            base.ApplyToBeatmap(beatmap);

            var osuBeatmap = (OsuBeatmap)beatmap;
            var origHitObjects = osuBeatmap.HitObjects.OrderBy(x => x.StartTime).ToList();

            var hitObjects = new List<OsuHitObject>();

            // Only place circles between startTime and endTime
            var startTime = origHitObjects.First().StartTime;
            double endTime;
            var endObj = origHitObjects.Last();
            switch (endObj)
            {
                case Slider slider:
                    endTime = slider.EndTime;
                    break;
                case Spinner spinner:
                    endTime = spinner.EndTime;
                    break;
                default:
                    endTime = endObj.StartTime;
                    break;
            }

            var comboStarts = origHitObjects.Where(x => x.NewCombo).Select(x => x.StartTime).ToList();

            TimingControlPoint currentTimingPoint = osuBeatmap.ControlPointInfo.TimingPointAt(startTime);
            var currentTime = currentTimingPoint.Time;
            int currentCombo = -1;
            IList<HitSampleInfo> lastSamples = null;

            float direction = MathHelper.TwoPi * RNG.NextSingle();
            float distance = 40f;

            while (!Precision.AlmostBigger(currentTime, endTime))
            {
                // Place a circle

                // Don't place any circles before the start of the first hit object of the map
                // Don't place any circles from the start of break time to the start of the first hit object after the break
                if (!Precision.AlmostBigger(startTime, currentTime)
                    && !osuBeatmap.Breaks.Any(x => Precision.AlmostBigger(currentTime, x.StartTime)
                    && Precision.AlmostBigger(origHitObjects.First(y => Precision.AlmostBigger(y.StartTime, x.EndTime)).StartTime, currentTime)))
                {
                    var newCircle = new HitCircle();
                    newCircle.ApplyDefaults(osuBeatmap.ControlPointInfo, osuBeatmap.BeatmapInfo.BaseDifficulty);
                    newCircle.StartTime = currentTime;

                    // Determine circle position
                    if (hitObjects.Count == 0)
                    {
                        newCircle.Position = new Vector2(RNG.NextSingle(OsuPlayfield.BASE_SIZE.X), RNG.NextSingle(OsuPlayfield.BASE_SIZE.Y));
                    }
                    else
                    {
                        var relativePos = new Vector2(
                            distance * (float)Math.Cos(direction),
                           distance * (float)Math.Sin(direction)
                        );
                        relativePos = getRotatedVector(hitObjects.Last().Position, relativePos);
                        direction = (float)Math.Atan2(relativePos.Y, relativePos.X);

                        var newPosition = Vector2.Add(hitObjects.Last().Position, relativePos);

                        if (newPosition.Y < 0)
                            newPosition.Y = 0;
                        else if (newPosition.Y > OsuPlayfield.BASE_SIZE.Y)
                            newPosition.Y = OsuPlayfield.BASE_SIZE.Y;
                        if (newPosition.X < 0)
                            newPosition.X = 0;
                        else if (newPosition.X > OsuPlayfield.BASE_SIZE.X)
                            newPosition.X = OsuPlayfield.BASE_SIZE.X;

                        newCircle.Position = newPosition;
                        // Add a random nudge to the direction
                        direction += distance / MAX_DISTANCE * (RNG.NextSingle() * MathHelper.TwoPi - MathHelper.Pi);
                    }

                    // Determine samples to use
                    var samples = getSamplesAtTime(origHitObjects, currentTime);
                    if (samples == null)
                    {
                        if (lastSamples != null)
                            newCircle.Samples = lastSamples;
                    }
                    else
                    {
                        newCircle.Samples = samples;
                        lastSamples = samples;
                    }

                    // Determine combo
                    if (comboStarts.Count > currentCombo + 1 && !Precision.AlmostBigger(comboStarts[currentCombo + 1], currentTime))
                    {
                        currentCombo++;
                        newCircle.NewCombo = true;
                        newCircle.IndexInCurrentCombo = 0;
                        if (hitObjects.Count > 0)
                            hitObjects.Last().LastInCombo = true;

                        // Increase distance for every combo
                        distance = Math.Min(distance * 1.05f, MAX_DISTANCE);
                        // Randomize direction as well
                        direction = MathHelper.TwoPi * RNG.NextSingle();
                    }
                    else
                    {
                        if (hitObjects.Count > 0)
                            newCircle.IndexInCurrentCombo = hitObjects.Last().IndexInCurrentCombo + 1;
                    }
                    newCircle.ComboIndex = currentCombo;

                    // Remove the last circle if it is too close in time to this one
                    if (hitObjects.Count > 0
                        && Precision.AlmostBigger(currentTimingPoint.BeatLength / 2, newCircle.StartTime - hitObjects.Last().StartTime))
                    {
                        hitObjects.RemoveAt(hitObjects.Count - 1);
                    }

                    hitObjects.Add(newCircle);
                }

                // Advance to the next beat and check for timing point changes

                currentTime += currentTimingPoint.BeatLength;

                var newTimingPoint = osuBeatmap.ControlPointInfo.TimingPointAt(currentTime);
                if (newTimingPoint != currentTimingPoint)
                {
                    currentTimingPoint = newTimingPoint;
                    currentTime = currentTimingPoint.Time;
                }
            }

            osuBeatmap.HitObjects = hitObjects;
        }

        /// <summary>
        /// Get samples (if any) for a specific point in time.
        /// </summary>
        /// <remarks>
        /// Samples will be returned if a hit circle or a slider node exists at that point of time.
        /// </remarks>
        /// <param name="hitObjects">The list of hit objects in a beatmap, ordered by StartTime</param>
        /// <param name="time">The point in time to get samples for</param>
        /// <returns>Hit samples</returns>
        private IList<HitSampleInfo> getSamplesAtTime(List<OsuHitObject> hitObjects, double time)
        {
            var sampleObj = hitObjects.FirstOrDefault(x =>
            {
                if (Precision.AlmostEquals(time, x.StartTime)) return true;
                if (x is Slider slider
                    && Precision.AlmostBigger(time, slider.StartTime)
                    && Precision.AlmostBigger(slider.EndTime, time))
                {
                    if (Precision.AlmostEquals((time - slider.StartTime) % slider.SpanDuration, 0))
                    {
                        return true;
                    }
                }
                return false;
            });
            if (sampleObj == null) return null;

            IList<HitSampleInfo> samples = null;
            if (sampleObj is Slider slider)
            {
                samples = slider.NodeSamples[(int)Math.Round((time - slider.StartTime) % slider.SpanDuration)];
            }
            else
            {
                samples = sampleObj.Samples;
            }
            return samples;
        }

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
        }

        protected override void ApplyNormalVisibilityState(DrawableHitObject drawable, ArmedState state)
        {
            if (drawable is DrawableSpinner)
                return;

            var h = (OsuHitObject)drawable.HitObject;

            // apply grow and fade effect
            if (drawable is DrawableHitCircle circle)
            {
                using (drawable.BeginAbsoluteSequence(h.StartTime - h.TimePreempt))
                {
                    drawable.ScaleTo(0.4f)
                        .Then().ScaleTo(1.6f, h.TimePreempt * 2);
                    drawable.FadeTo(0.5f)
                        .Then().Delay(h.TimeFadeIn).FadeTo(1f);

                    // remove approach circles
                    circle.ApproachCircle.Hide();
                }
            }
        }

        public void ReadFromDifficulty(BeatmapDifficulty difficulty)
        {
        }

        public void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            // Decrease AR to increase preempt time
            difficulty.ApproachRate *= 0.5f;
        }

        // The distances from the hit objects to the borders of the playfield they start to "turn around" and curve towards the middle.
        // The closer the hit objects draw to the border, the sharper the turn
        private const byte border_distance_x = 192;
        private const byte border_distance_y = 144;

        /// <summary>
        /// Determines the position of the current hit object relative to the previous one.
        /// </summary>
        /// <returns>The position of the current hit object relative to the previous one</returns>
        private Vector2 getRotatedVector(Vector2 prevPosChanged, Vector2 posRelativeToPrev)
        {
            var relativeRotationDistance = 0f;
            var playfieldMiddle = Vector2.Divide(OsuPlayfield.BASE_SIZE, 2);

            if (prevPosChanged.X < playfieldMiddle.X)
            {
                relativeRotationDistance = Math.Max(
                    (border_distance_x - prevPosChanged.X) / border_distance_x,
                    relativeRotationDistance
                );
            }
            else
            {
                relativeRotationDistance = Math.Max(
                    (prevPosChanged.X - (OsuPlayfield.BASE_SIZE.X - border_distance_x)) / border_distance_x,
                    relativeRotationDistance
                );
            }

            if (prevPosChanged.Y < playfieldMiddle.Y)
            {
                relativeRotationDistance = Math.Max(
                    (border_distance_y - prevPosChanged.Y) / border_distance_y,
                    relativeRotationDistance
                );
            }
            else
            {
                relativeRotationDistance = Math.Max(
                    (prevPosChanged.Y - (OsuPlayfield.BASE_SIZE.Y - border_distance_y)) / border_distance_y,
                    relativeRotationDistance
                );
            }

            return rotateVectorTowardsVector(
                posRelativeToPrev,
                Vector2.Subtract(playfieldMiddle, prevPosChanged),
                relativeRotationDistance / 2
            );
        }

        /// <summary>
        /// Rotates vector "initial" towards vector "destinantion"
        /// </summary>
        /// <param name="initial">Vector to rotate to "destination"</param>
        /// <param name="destination">Vector "initial" should be rotated to</param>
        /// <param name="relativeDistance">The angle the vector should be rotated relative to the difference between the angles of the the two vectors.</param>
        /// <returns>Resulting vector</returns>
        private Vector2 rotateVectorTowardsVector(Vector2 initial, Vector2 destination, float relativeDistance)
        {
            var initialAngleRad = Math.Atan2(initial.Y, initial.X);
            var destAngleRad = Math.Atan2(destination.Y, destination.X);

            var diff = destAngleRad - initialAngleRad;

            while (diff < -Math.PI)
            {
                diff += 2 * Math.PI;
            }

            while (diff > Math.PI)
            {
                diff -= 2 * Math.PI;
            }

            var finalAngleRad = initialAngleRad + relativeDistance * diff;

            return new Vector2(
                initial.Length * (float)Math.Cos(finalAngleRad),
                initial.Length * (float)Math.Sin(finalAngleRad)
            );
        }

        // Background metronome

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            drawableRuleset.Overlays.Add(new TargetBeatContainer());
        }

        public class TargetBeatContainer : BeatSyncedContainer
        {
            private PausableSkinnableSound sample;

            public TargetBeatContainer()
            {
                Divisor = 1;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren = new Drawable[]
                {
                    sample = new PausableSkinnableSound(new SampleInfo("spinnerbonus")), // TODO: use another sample?
                };
            }

            protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
            {
                base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

                if (!IsBeatSyncedWithTrack) return;

                sample?.Play();
            }
        }
    }
}
