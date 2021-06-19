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
using osu.Game.Configuration;
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
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModTarget : ModWithVisibilityAdjustment, IApplicableToDrawableRuleset<OsuHitObject>,
                                IApplicableToHealthProcessor, IApplicableToDifficulty, IApplicableFailOverride, IHasSeed
    {
        public override string Name => "Target";
        public override string Acronym => "TP";
        public override ModType Type => ModType.Conversion;
        public override IconUsage? Icon => OsuIcon.ModTarget;
        public override string Description => @"Practice keeping up with the beat of the song.";
        public override double ScoreMultiplier => 1;

        [SettingSource("Seed", "Use a custom seed instead of a random one", SettingControlType = typeof(SeedSettingsControl))]
        public Bindable<int?> Seed { get; } = new Bindable<int?>
        {
            Default = null,
            Value = null
        };

        public bool RestartOnFail => false;

        public bool DisplayResultsOnFail => true;

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

        private ControlPointInfo controlPointInfo;

        public bool PerformFail()
        {
            return true;
        }

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            // Sudden death
            healthProcessor.FailConditions += (_, result)
                => result.Type.AffectsCombo()
                   && !result.IsHit;
        }

        public override void ApplyToBeatmap(IBeatmap beatmap)
        {
            Seed.Value ??= RNG.Next();

            var osuBeatmap = (OsuBeatmap)beatmap;

            if (osuBeatmap.HitObjects.Count == 0) return;

            controlPointInfo = osuBeatmap.ControlPointInfo;
            var origHitObjects = osuBeatmap.HitObjects.OrderBy(x => x.StartTime).ToList();

            var hitObjects = generateBeats(osuBeatmap, origHitObjects)
                             .Select(x =>
                             {
                                 var newCircle = new HitCircle();
                                 newCircle.ApplyDefaults(controlPointInfo, osuBeatmap.BeatmapInfo.BaseDifficulty);
                                 newCircle.StartTime = x;
                                 return (OsuHitObject)newCircle;
                             }).ToList();

            addHitSamples(hitObjects, origHitObjects);

            fixComboInfo(hitObjects, origHitObjects);

            randomizeCirclePos(hitObjects);

            osuBeatmap.HitObjects = hitObjects;

            base.ApplyToBeatmap(beatmap);
        }

        public void ReadFromDifficulty(BeatmapDifficulty difficulty)
        {
        }

        public void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            // Decrease AR to increase preempt time
            difficulty.ApproachRate *= 0.5f;
        }

        // Background metronome

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            drawableRuleset.Overlays.Add(new TargetBeatContainer());
        }

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

        private static float map(float value, float fromLow, float fromHigh, float toLow, float toHigh)
        {
            return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
        }

        private IEnumerable<double> generateBeats(IBeatmap beatmap, IReadOnlyCollection<OsuHitObject> origHitObjects)
        {
            var startTime = origHitObjects.First().StartTime;
            var endObj = origHitObjects.Last();
            var endTime = endObj switch
            {
                Slider slider => slider.EndTime,
                Spinner spinner => spinner.EndTime,
                _ => endObj.StartTime
            };

            var beats = beatmap.ControlPointInfo.TimingPoints
                               .Where(x => Precision.AlmostBigger(endTime, x.Time))
                               .SelectMany(tp =>
                               {
                                   var tpBeats = new List<double>();
                                   var currentTime = tp.Time;

                                   while (Precision.AlmostBigger(endTime, currentTime) && beatmap.ControlPointInfo.TimingPointAt(currentTime) == tp)
                                   {
                                       tpBeats.Add(Math.Floor(currentTime));
                                       currentTime += tp.BeatLength;
                                   }

                                   return tpBeats;
                               })
                               .Where(x => Precision.AlmostBigger(x, startTime))
                               // Remove beats during breaks
                               .Where(x => !beatmap.Breaks.Any(b =>
                                   Precision.AlmostBigger(x, b.StartTime)
                                   && Precision.AlmostBigger(origHitObjects.First(y => Precision.AlmostBigger(y.StartTime, b.EndTime)).StartTime, x)
                               ))
                               .ToList();

            // Remove beats that are too close to the next one (e.g. due to timing point changes)
            for (var i = beats.Count - 2; i >= 0; i--)
            {
                var beat = beats[i];

                if (Precision.AlmostBigger(beatmap.ControlPointInfo.TimingPointAt(beat).BeatLength / 2, beats[i + 1] - beat)) beats.RemoveAt(i);
            }

            return beats;
        }

        private void addHitSamples(IEnumerable<OsuHitObject> hitObjects, IReadOnlyList<OsuHitObject> origHitObjects)
        {
            var lastSampleIdx = 0;

            foreach (var x in hitObjects)
            {
                var samples = getSamplesAtTime(origHitObjects, x.StartTime);

                if (samples == null)
                {
                    while (lastSampleIdx < origHitObjects.Count && origHitObjects[lastSampleIdx].StartTime <= x.StartTime)
                        lastSampleIdx++;
                    lastSampleIdx--;

                    if (lastSampleIdx < 0 && lastSampleIdx >= origHitObjects.Count) continue;

                    if (lastSampleIdx < origHitObjects.Count - 1)
                    {
                        // get samples from the next hit object if it is closer in time
                        if (origHitObjects[lastSampleIdx + 1].StartTime - x.StartTime < x.StartTime - origHitObjects[lastSampleIdx].StartTime)
                            lastSampleIdx++;
                    }

                    x.Samples = origHitObjects[lastSampleIdx].Samples.Where(s => !HitSampleInfo.AllAdditions.Contains(s.Name)).ToList();
                }
                else
                    x.Samples = samples;
            }
        }

        private void fixComboInfo(List<OsuHitObject> hitObjects, List<OsuHitObject> origHitObjects)
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

                    var radius = (float)obj.Radius;

                    if (newPosition.Y < radius)
                        newPosition.Y = radius;
                    else if (newPosition.Y > OsuPlayfield.BASE_SIZE.Y - radius)
                        newPosition.Y = OsuPlayfield.BASE_SIZE.Y - radius;
                    if (newPosition.X < radius)
                        newPosition.X = radius;
                    else if (newPosition.X > OsuPlayfield.BASE_SIZE.X - radius)
                        newPosition.X = OsuPlayfield.BASE_SIZE.X - radius;

                    obj.Position = newPosition;

                    tryCount++;
                    if (tryCount % 10 == 0) distance *= 0.9f;
                } while (distance >= obj.Radius * 2 && isOverlappingWithRecent(hitObjects, i));

                if (obj.LastInCombo)
                    direction = MathHelper.TwoPi * nextSingle();
                else
                    direction += distance / distance_cap * (nextSingle() * MathHelper.TwoPi - MathHelper.Pi);
            }
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
            var sampleObj = hitObjects.FirstOrDefault(x =>
            {
                if (Precision.AlmostEquals(time, x.StartTime)) return true;

                if (!(x is Slider s))
                    return false;
                if (!Precision.AlmostBigger(time, s.StartTime)
                    || !Precision.AlmostBigger(s.EndTime, time))
                    return false;

                return Precision.AlmostEquals((time - s.StartTime) % s.SpanDuration, 0);
            });
            if (sampleObj == null) return null;

            IList<HitSampleInfo> samples;

            if (sampleObj is Slider slider)
                samples = slider.NodeSamples[(int)Math.Round((time - slider.StartTime) % slider.SpanDuration)];
            else
                samples = sampleObj.Samples;

            return samples;
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
                    sample = new PausableSkinnableSound(new SampleInfo("spinnerbonus")) // todo: use another sample
                };
            }
        }
    }
}
