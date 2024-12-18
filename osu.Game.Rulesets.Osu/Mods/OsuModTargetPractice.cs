// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Osu.Utils;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModTargetPractice : ModWithVisibilityAdjustment, IApplicableToDrawableRuleset<OsuHitObject>,
                                        IApplicableToHealthProcessor, IApplicableToDifficulty, IApplicableFailOverride, IHasSeed, IHidesApproachCircles
    {
        public override string Name => "Target Practice";
        public override string Acronym => "TP";
        public override ModType Type => ModType.Conversion;
        public override IconUsage? Icon => OsuIcon.ModTarget;
        public override LocalisableString Description => @"Practice keeping up with the beat of the song.";
        public override double ScoreMultiplier => 0.1;

        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[]
        {
            typeof(IRequiresApproachCircles),
            typeof(OsuModRandom),
            typeof(OsuModSpunOut),
            typeof(OsuModStrictTracking),
            typeof(OsuModSuddenDeath),
            typeof(OsuModDepth)
        }).ToArray();

        [SettingSource("Seed", "Use a custom seed instead of a random one", SettingControlType = typeof(SettingsNumberBox))]
        public Bindable<int?> Seed { get; } = new Bindable<int?>();

        [SettingSource("Metronome ticks", "Whether a metronome beat should play in the background")]
        public Bindable<bool> Metronome { get; } = new BindableBool(true);

        #region Constants

        /// <summary>
        /// Jump distance for circles in the last combo
        /// </summary>
        private const float max_base_distance = 333f;

        /// <summary>
        /// The maximum allowed jump distance after multipliers are applied
        /// </summary>
        private const float distance_cap = 380f;

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

        /// <summary>
        /// Acceptable difference for timing comparisons
        /// </summary>
        private const double timing_precision = 1;

        #endregion

        #region Private Fields

        private ControlPointInfo controlPointInfo = null!;

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

        public void ReadFromDifficulty(IBeatmapDifficultyInfo difficulty)
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

            double startTime = circle.HitObject.StartTime;
            double preempt = circle.HitObject.TimePreempt;

            using (circle.BeginAbsoluteSequence(startTime - preempt))
            {
                // initial state
                circle.ScaleTo(0.5f)
                      .FadeColour(OsuColour.Gray(0.5f));

                // scale to final size
                circle.ScaleTo(1f, preempt);

                // Remove approach circles
                circle.ApproachCircle.Hide();
            }

            using (circle.BeginAbsoluteSequence(startTime - controlPointInfo.TimingPointAt(startTime).BeatLength - undim_duration))
                circle.FadeColour(Color4.White, undim_duration);
        }

        #endregion

        #region Beatmap Generation (IApplicableToBeatmap)

        public override void ApplyToBeatmap(IBeatmap beatmap)
        {
            Seed.Value ??= RNG.Next();

            var rng = new Random(Seed.Value.Value);

            var osuBeatmap = (OsuBeatmap)beatmap;

            if (osuBeatmap.HitObjects.Count == 0) return;

            controlPointInfo = osuBeatmap.ControlPointInfo;

            var originalHitObjects = osuBeatmap.HitObjects.OrderBy(x => x.StartTime).ToList();
            var hitObjects = generateBeats(osuBeatmap, originalHitObjects)
                             .Select(beat =>
                             {
                                 var newCircle = new HitCircle();
                                 newCircle.ApplyDefaults(controlPointInfo, osuBeatmap.Difficulty);
                                 newCircle.StartTime = beat;
                                 return (OsuHitObject)newCircle;
                             }).ToList();

            addHitSamples(hitObjects, originalHitObjects);

            fixComboInfo(hitObjects, originalHitObjects);

            randomizeCirclePos(hitObjects, rng);

            osuBeatmap.HitObjects = hitObjects;

            base.ApplyToBeatmap(beatmap);
        }

        private IEnumerable<double> generateBeats(IBeatmap beatmap, IReadOnlyCollection<OsuHitObject> originalHitObjects)
        {
            double startTime = beatmap.HitObjects.First().StartTime;
            double endTime = beatmap.GetLastObjectTime();

            var beats = beatmap.ControlPointInfo.TimingPoints
                               // Ignore timing points after endTime
                               .Where(timingPoint => !definitelyBigger(timingPoint.Time, endTime))
                               // Generate the beats
                               .SelectMany(timingPoint => getBeatsForTimingPoint(timingPoint, endTime))
                               // Remove beats before startTime
                               .Where(beat => almostBigger(beat, startTime))
                               // Remove beats during breaks
                               .Where(beat => !isInsideBreakPeriod(originalHitObjects, beatmap.Breaks, beat))
                               .ToList();

            // Remove beats that are too close to the next one (e.g. due to timing point changes)
            for (int i = beats.Count - 2; i >= 0; i--)
            {
                double beat = beats[i];

                if (!definitelyBigger(beats[i + 1] - beat, beatmap.ControlPointInfo.TimingPointAt(beat).BeatLength / 2))
                    beats.RemoveAt(i);
            }

            return beats;
        }

        private void addHitSamples(IEnumerable<OsuHitObject> hitObjects, List<OsuHitObject> originalHitObjects)
        {
            foreach (var obj in hitObjects)
            {
                var samples = getSamplesAtTime(originalHitObjects, obj.StartTime);

                // If samples aren't available at the exact start time of the object,
                // use samples (without additions) in the closest original hit object instead
                obj.Samples = samples ?? getClosestHitObject(originalHitObjects, obj.StartTime).Samples.Where(s => !HitSampleInfo.AllAdditions.Contains(s.Name)).ToList();
            }
        }

        private void fixComboInfo(List<OsuHitObject> hitObjects, List<OsuHitObject> originalHitObjects)
        {
            // Copy combo indices from an original object at the same time or from the closest preceding object
            // (Objects lying between two combos are assumed to belong to the preceding combo)
            hitObjects.ForEach(newObj =>
            {
                var closestOrigObj = originalHitObjects.FindLast(y => almostBigger(newObj.StartTime, y.StartTime));

                // It shouldn't be possible for closestOrigObj to be null
                // But if it is, obj should be in the first combo
                newObj.ComboIndex = closestOrigObj?.ComboIndex ?? 0;
            });

            // The copied combo indices may not be continuous if the original map starts and ends a combo in between beats
            // e.g. A stream with each object starting a new combo
            // So combo indices need to be reprocessed to ensure continuity
            // Other kinds of combo info are also added in the process
            var combos = hitObjects.GroupBy(x => x.ComboIndex).ToList();

            for (int i = 0; i < combos.Count; i++)
            {
                var group = combos[i].ToList();
                group.First().NewCombo = true;
                group.Last().LastInCombo = true;

                for (int j = 0; j < group.Count; j++)
                {
                    var x = group[j];
                    x.ComboIndex = i;
                    x.IndexInCurrentCombo = j;
                }
            }
        }

        private void randomizeCirclePos(IReadOnlyList<OsuHitObject> hitObjects, Random rng)
        {
            if (hitObjects.Count == 0) return;

            float nextSingle(float max = 1f) => (float)(rng.NextDouble() * max);

            const float two_pi = MathF.PI * 2;

            float direction = two_pi * nextSingle();
            int maxComboIndex = hitObjects.Last().ComboIndex;

            for (int i = 0; i < hitObjects.Count; i++)
            {
                var obj = hitObjects[i];
                var lastPos = i == 0
                    ? Vector2.Divide(OsuPlayfield.BASE_SIZE, 2)
                    : hitObjects[i - 1].Position;

                float distance = maxComboIndex == 0
                    ? (float)obj.Radius
                    : mapRange(obj.ComboIndex, 0, maxComboIndex, (float)obj.Radius, max_base_distance);
                if (obj.NewCombo) distance *= 1.5f;
                if (obj.Kiai) distance *= 1.2f;
                distance = Math.Min(distance_cap, distance);

                // Attempt to place the circle at a place that does not overlap with previous ones

                int tryCount = 0;

                // for checking overlap
                var precedingObjects = hitObjects.SkipLast(hitObjects.Count - i).TakeLast(overlap_check_count).ToList();

                do
                {
                    if (tryCount > 0) direction = two_pi * nextSingle();

                    var relativePos = new Vector2(
                        distance * MathF.Cos(direction),
                        distance * MathF.Sin(direction)
                    );
                    // Rotate the new circle away from playfield border
                    relativePos = OsuHitObjectGenerationUtils.RotateAwayFromEdge(lastPos, relativePos, edge_rotation_multiplier);
                    direction = MathF.Atan2(relativePos.Y, relativePos.X);

                    var newPosition = Vector2.Add(lastPos, relativePos);

                    obj.Position = newPosition;

                    clampToPlayfield(obj);

                    tryCount++;
                    if (tryCount % 10 == 0) distance *= 0.9f;
                } while (distance >= obj.Radius * 2 && checkForOverlap(precedingObjects, obj));

                if (obj.LastInCombo)
                    direction = two_pi * nextSingle();
                else
                    direction += distance / distance_cap * (nextSingle() * two_pi - MathF.PI);
            }
        }

        #endregion

        #region Metronome (IApplicableToDrawableRuleset)

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            if (Metronome.Value)
                drawableRuleset.Overlays.Add(new MetronomeBeat(drawableRuleset.Beatmap.HitObjects.First().StartTime));
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
        /// <param name="originalHitObjects">Hit objects order by time.</param>
        /// <param name="breaks">The breaks of the beatmap.</param>
        /// <param name="time">The time to be checked.</param>=
        private bool isInsideBreakPeriod(IReadOnlyCollection<OsuHitObject> originalHitObjects, IEnumerable<BreakPeriod> breaks, double time)
        {
            return breaks.Any(breakPeriod =>
            {
                OsuHitObject? firstObjAfterBreak = originalHitObjects.FirstOrDefault(obj => almostBigger(obj.StartTime, breakPeriod.EndTime));

                return almostBigger(time, breakPeriod.StartTime)
                       // There should never really be a break section with no objects after it, but we've seen crashes from users with malformed beatmaps,
                       // so it's best to guard against this.
                       && (firstObjAfterBreak == null || definitelyBigger(firstObjAfterBreak.StartTime, time));
            });
        }

        private IEnumerable<double> getBeatsForTimingPoint(TimingControlPoint timingPoint, double mapEndTime)
        {
            var beats = new List<double>();
            int i = 0;
            double currentTime = timingPoint.Time;

            while (!definitelyBigger(currentTime, mapEndTime) && ReferenceEquals(controlPointInfo.TimingPointAt(currentTime), timingPoint))
            {
                beats.Add(Math.Floor(currentTime));
                i++;
                currentTime = timingPoint.Time + i * timingPoint.BeatLength;
            }

            return beats;
        }

        private OsuHitObject getClosestHitObject(List<OsuHitObject> hitObjects, double time)
        {
            int precedingIndex = hitObjects.FindLastIndex(h => h.StartTime < time);

            if (precedingIndex == hitObjects.Count - 1) return hitObjects[precedingIndex];

            // return the closest preceding/succeeding hit object, whoever is closer in time
            return hitObjects[precedingIndex + 1].StartTime - time < time - hitObjects[precedingIndex].StartTime
                ? hitObjects[precedingIndex + 1]
                : hitObjects[precedingIndex];
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
        private IList<HitSampleInfo>? getSamplesAtTime(IEnumerable<OsuHitObject> hitObjects, double time)
        {
            // Get a hit object that
            //   either has StartTime equal to the target time
            //   or has a repeat node at the target time
            var sampleObj = hitObjects.FirstOrDefault(hitObject =>
            {
                if (almostEquals(time, hitObject.StartTime))
                    return true;

                if (!(hitObject is IHasRepeats s))
                    return false;
                // If time is outside the duration of the IHasRepeats,
                // then this hitObject isn't the one we want
                if (!almostBigger(time, hitObject.StartTime)
                    || !almostBigger(s.EndTime, time))
                    return false;

                return nodeIndexFromTime(s, time - hitObject.StartTime) != -1;
            });
            if (sampleObj == null) return null;

            IList<HitSampleInfo> samples;

            if (sampleObj is IHasRepeats slider)
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
        private int nodeIndexFromTime(IHasRepeats curve, double timeSinceStart)
        {
            double spanDuration = curve.Duration / curve.SpanCount();
            double nodeIndex = timeSinceStart / spanDuration;

            if (almostEquals(nodeIndex, Math.Round(nodeIndex)))
                return (int)Math.Round(nodeIndex);

            return -1;
        }

        private bool checkForOverlap(IEnumerable<OsuHitObject> objectsToCheck, OsuHitObject target)
        {
            return objectsToCheck.Any(h => Vector2.Distance(h.Position, target.Position) < target.Radius * 2);
        }

        /// <summary>
        /// Move the hit object into playfield, taking its radius into account.
        /// </summary>
        /// <param name="obj">The hit object to be clamped.</param>
        private void clampToPlayfield(OsuHitObject obj)
        {
            var position = obj.Position;
            float radius = (float)obj.Radius;

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

        /// <summary>
        /// Re-maps a number from one range to another.
        /// </summary>
        /// <param name="value">The number to be re-mapped.</param>
        /// <param name="fromLow">Beginning of the original range.</param>
        /// <param name="fromHigh">End of the original range.</param>
        /// <param name="toLow">Beginning of the new range.</param>
        /// <param name="toHigh">End of the new range.</param>
        /// <returns>The re-mapped number.</returns>
        private static float mapRange(float value, float fromLow, float fromHigh, float toLow, float toHigh)
        {
            return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
        }

        private static bool almostBigger(double value1, double value2)
        {
            return Precision.AlmostBigger(value1, value2, timing_precision);
        }

        private static bool definitelyBigger(double value1, double value2)
        {
            return Precision.DefinitelyBigger(value1, value2, timing_precision);
        }

        private static bool almostEquals(double value1, double value2)
        {
            return Precision.AlmostEquals(value1, value2, timing_precision);
        }

        #endregion
    }
}
