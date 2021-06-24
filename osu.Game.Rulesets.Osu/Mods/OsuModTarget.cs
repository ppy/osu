// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModTarget : ModWithVisibilityAdjustment, IApplicableToDrawableRuleset<OsuHitObject>,
                                IApplicableToHealthProcessor, IApplicableToDifficulty, IApplicableFailOverride,
                                IHasSeed, IMutateApproachCircles
    {
        public override string Name => "Target";
        public override string Acronym => "TP";
        public override ModType Type => ModType.Conversion;
        public override IconUsage? Icon => OsuIcon.ModTarget;
        public override string Description => @"Practice keeping up with the beat of the song.";
        public override double ScoreMultiplier => 1;

        public override Type[] IncompatibleMods => new[] { typeof(IMutateApproachCircles) };

        [SettingSource("Seed", "Use a custom seed instead of a random one", SettingControlType = typeof(SeedSettingsControl))]
        public Bindable<int?> Seed { get; } = new Bindable<int?>
        {
            Default = null,
            Value = null
        };

        #region Constants

        /// <summary>
        /// Jump distance for circles in the last combo
        /// </summary>
        private const float max_base_distance = 333f;

        /// <summary>
        /// The maximum allowed jump distance after multipliers are applied
        /// </summary>
        private const float distance_cap = 380f;

        // The distances from the hit objects to the borders of the playfield they start to "turn around" and curve towards the middle.
        // The closer the hit objects draw to the border, the sharper the turn
        private const byte border_distance_x = 192;
        private const byte border_distance_y = 144;

        /// <summary>
        /// The extent of rotation towards playfield centre when a circle is near the edge
        /// </summary>
        private const float edge_rotation_multiplier = 0.75f;

        /// <summary>
        /// Number of recent circles to check for overlap
        /// </summary>
        private const int overlap_check_count = 5;

        /// <summary>
        /// Duration of the undimming animation
        /// </summary>
        private const double undim_duration = 96;

        #endregion

        #region Private Fields

        private ControlPointInfo controlPointInfo;

        private List<OsuHitObject> origHitObjects;

        #endregion

        #region Sudden Death (IApplicableFailOverride)

        public bool PerformFail() => true;

        public bool RestartOnFail => false;

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            // Sudden death
            healthProcessor.FailConditions += (_, result)
                => result.Type.AffectsCombo()
                   && !result.IsHit;
        }

        #endregion

        #region Reduce AR (IApplicableToDifficulty)

        public void ReadFromDifficulty(BeatmapDifficulty difficulty)
        {
        }

        public void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            // Decrease AR to increase preempt time
            difficulty.ApproachRate *= 0.5f;
        }

        #endregion

        #region Circle Transforms (ModWithVisibilityAdjustment)

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject drawable, ArmedState state)
        {
        }

        protected override void ApplyNormalVisibilityState(DrawableHitObject drawable, ArmedState state)
        {
            if (!(drawable is DrawableHitCircle circle)) return;

            var h = (OsuHitObject)drawable.HitObject;

            using (drawable.BeginAbsoluteSequence(h.StartTime - h.TimePreempt))
            {
                drawable.ScaleTo(0.5f)
                        .Then().ScaleTo(1f, h.TimePreempt);

                var colour = drawable.Colour;

                var avgColour = colour.AverageColour.Linear;
                drawable.FadeColour(new Color4(avgColour.R * 0.45f, avgColour.G * 0.45f, avgColour.B * 0.45f, avgColour.A))
                        .Then().Delay(h.TimePreempt - controlPointInfo.TimingPointAt(h.StartTime).BeatLength - undim_duration)
                        .FadeColour(colour, undim_duration);

                // remove approach circles
                circle.ApproachCircle.Hide();
            }
        }

        #endregion

        #region Beatmap Generation (IApplicableToBeatmap)

        public override void ApplyToBeatmap(IBeatmap beatmap)
        {
            Seed.Value ??= RNG.Next();

            var osuBeatmap = (OsuBeatmap)beatmap;

            if (osuBeatmap.HitObjects.Count == 0) return;

            controlPointInfo = osuBeatmap.ControlPointInfo;
            origHitObjects = osuBeatmap.HitObjects.OrderBy(x => x.StartTime).ToList();

            var hitObjects = generateBeats(osuBeatmap)
                             .Select(x =>
                             {
                                 var newCircle = new HitCircle();
                                 newCircle.ApplyDefaults(controlPointInfo, osuBeatmap.BeatmapInfo.BaseDifficulty);
                                 newCircle.StartTime = x;
                                 return (OsuHitObject)newCircle;
                             }).ToList();

            addHitSamples(hitObjects);

            fixComboInfo(hitObjects);

            randomizeCirclePos(hitObjects);

            osuBeatmap.HitObjects = hitObjects;

            base.ApplyToBeatmap(beatmap);
        }

        private IEnumerable<double> generateBeats(IBeatmap beatmap)
        {
            var startTime = origHitObjects.First().StartTime;
            var endObj = origHitObjects.Last();
            var endTime = endObj.GetEndTime();

            var beats = beatmap.ControlPointInfo.TimingPoints
                               .Where(timingPoint => Precision.AlmostBigger(endTime, timingPoint.Time))
                               .SelectMany(timingPoint => getBeatsForTimingPoint(timingPoint, endTime))
                               .Where(beat => Precision.AlmostBigger(beat, startTime))
                               .Where(beat => !isInsideBreakPeriod(beatmap.Breaks, beat))
                               .ToList();

            // Remove beats that are too close to the next one (e.g. due to timing point changes)
            for (var i = beats.Count - 2; i >= 0; i--)
            {
                var beat = beats[i];

                if (Precision.AlmostBigger(beatmap.ControlPointInfo.TimingPointAt(beat).BeatLength / 2, beats[i + 1] - beat))
                    beats.RemoveAt(i);
            }

            return beats;
        }

        private void addHitSamples(IEnumerable<OsuHitObject> hitObjects)
        {
            var lastSampleIdx = 0;

            foreach (var obj in hitObjects)
            {
                var samples = getSamplesAtTime(origHitObjects, obj.StartTime);

                if (samples == null)
                {
                    // If samples aren't available at the exact start time of the object,
                    // use samples (without additions) in the closest original hit object instead

                    while (lastSampleIdx < origHitObjects.Count && origHitObjects[lastSampleIdx].StartTime <= obj.StartTime)
                        lastSampleIdx++;

                    if (lastSampleIdx >= origHitObjects.Count) continue;

                    if (lastSampleIdx > 0)
                    {
                        // get samples from the previous hit object if it is closer in time
                        if (obj.StartTime - origHitObjects[lastSampleIdx - 1].StartTime < origHitObjects[lastSampleIdx].StartTime - obj.StartTime)
                            lastSampleIdx--;
                    }

                    // Remove additions
                    obj.Samples = origHitObjects[lastSampleIdx].Samples.Where(s => !HitSampleInfo.AllAdditions.Contains(s.Name)).ToList();
                }
                else
                    obj.Samples = samples;
            }
        }

        private void fixComboInfo(List<OsuHitObject> hitObjects)
        {
            // First follow the combo indices in the original beatmap
            hitObjects.ForEach(x =>
            {
                var origObj = origHitObjects.FindLast(y => Precision.AlmostBigger(x.StartTime, y.StartTime));
                x.ComboIndex = origObj?.ComboIndex ?? 0;
            });
            // Then reprocess them to ensure continuity in the combo indices and add indices in current combo
            var combos = hitObjects.GroupBy(x => x.ComboIndex).ToList();

            for (var i = 0; i < combos.Count; i++)
            {
                var group = combos[i].ToList();
                group.First().NewCombo = true;
                group.Last().LastInCombo = true;

                for (var j = 0; j < group.Count; j++)
                {
                    var x = group[j];
                    x.ComboIndex = i;
                    x.IndexInCurrentCombo = j;
                }
            }
        }

        private void randomizeCirclePos(IReadOnlyList<OsuHitObject> hitObjects)
        {
            if (hitObjects.Count == 0) return;

            var rng = new Random(Seed.Value.GetValueOrDefault());

            float nextSingle(float max = 1f) => (float)(rng.NextDouble() * max);

            var direction = MathHelper.TwoPi * nextSingle();
            var maxComboIndex = hitObjects.Last().ComboIndex;

            for (var i = 0; i < hitObjects.Count; i++)
            {
                var obj = hitObjects[i];
                var lastPos = i == 0
                    ? Vector2.Divide(OsuPlayfield.BASE_SIZE, 2)
                    : hitObjects[i - 1].Position;

                var distance = maxComboIndex == 0
                    ? (float)obj.Radius
                    : map(obj.ComboIndex, 0, maxComboIndex, (float)obj.Radius, max_base_distance);
                if (obj.NewCombo) distance *= 1.5f;
                if (obj.Kiai) distance *= 1.2f;
                distance = Math.Min(distance_cap, distance);

                // Attempt to place the circle at a place that does not overlap with previous ones

                var tryCount = 0;

                do
                {
                    if (tryCount > 0) direction = MathHelper.TwoPi * nextSingle();

                    var relativePos = new Vector2(
                        distance * (float)Math.Cos(direction),
                        distance * (float)Math.Sin(direction)
                    );
                    relativePos = getRotatedVector(lastPos, relativePos);
                    direction = (float)Math.Atan2(relativePos.Y, relativePos.X);

                    var newPosition = Vector2.Add(lastPos, relativePos);

                    obj.Position = newPosition;

                    clampToPlayfield(obj);

                    tryCount++;
                    if (tryCount % 10 == 0) distance *= 0.9f;
                } while (distance >= obj.Radius * 2 && isOverlappingWithRecent(hitObjects, i));

                if (obj.LastInCombo)
                    direction = MathHelper.TwoPi * nextSingle();
                else
                    direction += distance / distance_cap * (nextSingle() * MathHelper.TwoPi - MathHelper.Pi);
            }
        }

        #endregion

        #region Metronome (IApplicableToDrawableRuleset)

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

            protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
            {
                base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

                if (!IsBeatSyncedWithTrack) return;

                sample?.Play();
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren = new Drawable[]
                {
                    sample = new PausableSkinnableSound(new SampleInfo("Gameplay/nightcore-hat")) // todo: use another sample
                };
            }
        }

        #endregion

        #region Helper Subroutines

        /// <summary>
        /// Check if a given time is inside a <see cref="BreakPeriod"/>.
        /// </summary>
        /// <remarks>
        /// The given time is also considered to be inside a break if it is earlier than the
        /// start time of the first original hit object after the break.
        /// </remarks>
        /// <param name="breaks">The breaks of the beatmap.</param>
        /// <param name="time">The time to be checked.</param>=
        private bool isInsideBreakPeriod(IEnumerable<BreakPeriod> breaks, double time)
        {
            return breaks.Any(breakPeriod =>
            {
                var firstObjAfterBreak = origHitObjects.First(obj => Precision.AlmostBigger(obj.StartTime, breakPeriod.EndTime));

                return Precision.AlmostBigger(time, breakPeriod.StartTime)
                       && Precision.AlmostBigger(firstObjAfterBreak.StartTime, time);
            });
        }

        private IEnumerable<double> getBeatsForTimingPoint(TimingControlPoint timingPoint, double mapEndTime)
        {
            var beats = new List<double>();
            int i = 0;
            var currentTime = timingPoint.Time;

            while (Precision.AlmostBigger(mapEndTime, currentTime) && controlPointInfo.TimingPointAt(currentTime) == timingPoint)
            {
                beats.Add(Math.Floor(currentTime));
                i++;
                currentTime = timingPoint.Time + i * timingPoint.BeatLength;
            }

            return beats;
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
        private IList<HitSampleInfo> getSamplesAtTime(IEnumerable<OsuHitObject> hitObjects, double time)
        {
            var sampleObj = hitObjects.FirstOrDefault(hitObject =>
            {
                if (Precision.AlmostEquals(time, hitObject.StartTime))
                    return true;

                if (!(hitObject is IHasPathWithRepeats s))
                    return false;
                if (!Precision.AlmostBigger(time, hitObject.StartTime)
                    || !Precision.AlmostBigger(s.EndTime, time))
                    return false;

                return nodeIndexFromTime(s, time - hitObject.StartTime) != -1;
            });
            if (sampleObj == null) return null;

            IList<HitSampleInfo> samples;

            if (sampleObj is IHasPathWithRepeats slider)
                samples = slider.NodeSamples[nodeIndexFromTime(slider, time - sampleObj.StartTime)];
            else
                samples = sampleObj.Samples;

            return samples;
        }

        /// <summary>
        /// Get the repeat node at a point in time.
        /// </summary>
        /// <param name="curve">The slider.</param>
        /// <param name="timeSinceStart">The time since the start time of the slider.</param>
        /// <returns>Index of the node. -1 if there isn't a node at the specific time.</returns>
        private int nodeIndexFromTime(IHasPathWithRepeats curve, double timeSinceStart)
        {
            double spanDuration = curve.Duration / curve.SpanCount();
            double nodeIndex = timeSinceStart / spanDuration;

            if (Precision.AlmostEquals(nodeIndex - Math.Round(nodeIndex), 0))
                return (int)Math.Round(nodeIndex);

            return -1;
        }

        private bool isOverlappingWithRecent(IReadOnlyList<OsuHitObject> hitObjects, int idx)
        {
            var target = hitObjects[idx];
            return hitObjects.SkipLast(hitObjects.Count - idx).TakeLast(overlap_check_count)
                             .Any(h => Vector2.Distance(h.Position, target.Position) < target.Radius * 2);
        }

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
                Math.Min(1, relativeRotationDistance * edge_rotation_multiplier)
            );
        }

        /// <summary>
        /// Rotates vector "initial" towards vector "destination"
        /// </summary>
        /// <param name="initial">Vector to rotate to "destination"</param>
        /// <param name="destination">Vector "initial" should be rotated to</param>
        /// <param name="relativeDistance">
        /// The angle the vector should be rotated relative to the difference between the angles of
        /// the the two vectors.
        /// </param>
        /// <returns>Resulting vector</returns>
        private Vector2 rotateVectorTowardsVector(Vector2 initial, Vector2 destination, float relativeDistance)
        {
            var initialAngleRad = Math.Atan2(initial.Y, initial.X);
            var destAngleRad = Math.Atan2(destination.Y, destination.X);

            var diff = destAngleRad - initialAngleRad;

            while (diff < -Math.PI) diff += 2 * Math.PI;

            while (diff > Math.PI) diff -= 2 * Math.PI;

            var finalAngleRad = initialAngleRad + relativeDistance * diff;

            return new Vector2(
                initial.Length * (float)Math.Cos(finalAngleRad),
                initial.Length * (float)Math.Sin(finalAngleRad)
            );
        }

        private void clampToPlayfield(OsuHitObject obj)
        {
            var position = obj.Position;
            var radius = (float)obj.Radius;

            if (position.Y < radius)
                position.Y = radius;
            else if (position.Y > OsuPlayfield.BASE_SIZE.Y - radius)
                position.Y = OsuPlayfield.BASE_SIZE.Y - radius;

            if (position.X < radius)
                position.X = radius;
            else if (position.X > OsuPlayfield.BASE_SIZE.X - radius)
                position.X = OsuPlayfield.BASE_SIZE.X - radius;

            obj.Position = position;
        }

        private static float map(float value, float fromLow, float fromHigh, float toLow, float toHigh)
        {
            return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
        }

        #endregion
    }
}
