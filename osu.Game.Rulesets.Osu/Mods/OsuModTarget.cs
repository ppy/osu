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

        public bool PerformFail() => true;

        public bool RestartOnFail => false;

        public bool DisplayResultsOnFail => true;

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            // Sudden death
            healthProcessor.FailConditions += (_, result)
                => result.Type.AffectsCombo()
                   && !result.IsHit;
        }

        // Maximum distance to jump
        private const float max_distance = 250f;

        public override void ApplyToBeatmap(IBeatmap beatmap)
        {
            Seed.Value ??= RNG.Next();

            var osuBeatmap = (OsuBeatmap)beatmap;
            var origHitObjects = osuBeatmap.HitObjects.OrderBy(x => x.StartTime).ToList();

            var hitObjects = generateBeats(osuBeatmap, origHitObjects)
                             .Select(x =>
                             {
                                 var newCircle = new HitCircle();
                                 newCircle.ApplyDefaults(osuBeatmap.ControlPointInfo, osuBeatmap.BeatmapInfo.BaseDifficulty);
                                 newCircle.StartTime = x;
                                 return (OsuHitObject)newCircle;
                             }).ToList();

            addHitSamples(hitObjects, origHitObjects);

            fixComboInfo(hitObjects, origHitObjects);

            randomizeCirclePos(hitObjects);

            osuBeatmap.HitObjects = hitObjects;

            base.ApplyToBeatmap(beatmap);
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
                                       tpBeats.Add(currentTime);
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
            for (int i = beats.Count - 2; i >= 0; i--)
            {
                var beat = beats[i];

                if (Precision.AlmostBigger(beatmap.ControlPointInfo.TimingPointAt(beat).BeatLength / 2, beats[i + 1] - beat))
                {
                    beats.RemoveAt(i);
                }
            }

            return beats;
        }

        private void addHitSamples(IReadOnlyList<OsuHitObject> hitObjects, IReadOnlyCollection<OsuHitObject> origHitObjects)
        {
            for (int i = 0; i < hitObjects.Count; i++)
            {
                var x = hitObjects[i];
                var samples = getSamplesAtTime(origHitObjects, x.StartTime);

                if (samples == null)
                {
                    if (i > 0)
                        x.Samples = hitObjects[i - 1].Samples;
                }
                else
                {
                    x.Samples = samples;
                }
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

        private void randomizeCirclePos(IReadOnlyList<OsuHitObject> hitObjects)
        {
            var rng = new Random(Seed.Value.GetValueOrDefault());

            float nextSingle(float max = 1f) => (float)(rng.NextDouble() * max);

            var direction = MathHelper.TwoPi * nextSingle();

            for (int i = 0; i < hitObjects.Count; i++)
            {
                var x = hitObjects[i];

                if (i == 0)
                {
                    x.Position = new Vector2(nextSingle(OsuPlayfield.BASE_SIZE.X), nextSingle(OsuPlayfield.BASE_SIZE.Y));
                }
                else
                {
                    var distance = Math.Min(max_distance, 40f * (float)Math.Pow(1.05, x.ComboIndex));
                    var relativePos = new Vector2(
                        distance * (float)Math.Cos(direction),
                        distance * (float)Math.Sin(direction)
                    );
                    relativePos = getRotatedVector(hitObjects[i - 1].Position, relativePos);
                    direction = (float)Math.Atan2(relativePos.Y, relativePos.X);

                    var newPosition = Vector2.Add(hitObjects[i - 1].Position, relativePos);

                    if (newPosition.Y < 0)
                        newPosition.Y = 0;
                    else if (newPosition.Y > OsuPlayfield.BASE_SIZE.Y)
                        newPosition.Y = OsuPlayfield.BASE_SIZE.Y;
                    if (newPosition.X < 0)
                        newPosition.X = 0;
                    else if (newPosition.X > OsuPlayfield.BASE_SIZE.X)
                        newPosition.X = OsuPlayfield.BASE_SIZE.X;

                    x.Position = newPosition;

                    if (x.LastInCombo)
                        direction = MathHelper.TwoPi * nextSingle();
                    else
                        direction += distance / max_distance * (nextSingle() * MathHelper.TwoPi - MathHelper.Pi);
                }
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
            {
                samples = slider.NodeSamples[(int)Math.Round((time - slider.StartTime) % slider.SpanDuration)];
            }
            else
            {
                samples = sampleObj.Samples;
            }

            return samples;
        }

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject drawable, ArmedState state)
        {
        }

        protected override void ApplyNormalVisibilityState(DrawableHitObject drawable, ArmedState state)
        {
            if (drawable is DrawableSpinner)
                return;

            var h = (OsuHitObject)drawable.HitObject;

            // apply grow and fade effect
            if (!(drawable is DrawableHitCircle circle)) return;

            using (drawable.BeginAbsoluteSequence(h.StartTime - h.TimePreempt))
            {
                // todo: this doesn't feel quite right yet
                drawable.ScaleTo(0.4f)
                        .Then().ScaleTo(1.6f, h.TimePreempt * 2);
                drawable.FadeTo(0.5f)
                        .Then().Delay(h.TimeFadeIn).FadeTo(1f);

                // remove approach circles
                circle.ApproachCircle.Hide();
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
        /// Rotates vector "initial" towards vector "destination"
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
                    sample = new PausableSkinnableSound(new SampleInfo("spinnerbonus")) // todo: use another sample?
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
