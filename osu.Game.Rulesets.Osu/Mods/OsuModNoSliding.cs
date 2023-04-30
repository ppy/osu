// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Utils;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModNoSliding : Mod, IApplicableAfterBeatmapConversion
    {
        private const int deathstream_length = 16;

        public override string Name => "No Sliding";

        public override string Acronym => "NL";

        public override double ScoreMultiplier => 1;

        public override LocalisableString Description => @"Convert all sliders to streams.";

        public override ModType Type => ModType.Conversion;

        public override Type[] IncompatibleMods => new[]
        {
            typeof(OsuModTargetPractice),
            typeof(OsuModStrictTracking),
        };

        [SettingSource("Beat divisor")]
        public BindableInt BeatDivisor { get; } = new BindableInt(4)
        {
            MinValue = 1,
            MaxValue = 8
        };

        [SettingSource("Slow down too long streams")]
        public BindableBool DeathstreamsSlowingDown { get; } = new BindableBool(true);

        [SettingSource("Slow down too fast parts")]
        public BindableBool FastPartsSlowingDown { get; } = new BindableBool(true);

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var osuBeatmap = (OsuBeatmap)beatmap;
            var newObjects = new List<OsuHitObject>();

            var points = beatmap.ControlPointInfo.TimingPoints.OrderByDescending(c => c.BeatLength);
            double minAllowedBeatLength = (points.FirstOrDefault() ?? TimingControlPoint.DEFAULT).BeatLength / 1.5d;

            foreach (var hitObject in osuBeatmap.HitObjects)
            {
                if (hitObject is not Slider s)
                {
                    newObjects.Add(hitObject);
                    continue;
                }

                var point = beatmap.ControlPointInfo.TimingPointAt(s.StartTime);
                s.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);

                if (FastPartsSlowingDown.Value && point.BeatLength < minAllowedBeatLength)
                {
                    double beatLength = point.BeatLength;
                    while (beatLength < minAllowedBeatLength)
                        beatLength *= 2;

                    point = (TimingControlPoint)point.DeepClone();
                    point.BeatLength = beatLength;
                }

                // this will be true if span is going to be converted into 1, 2, or 3 circles.
                bool spanTooShort = s.SpanDuration <= point.BeatLength * 4d / BeatDivisor.Value;
                // an attempt to fix too fast turns. I chose 250ms by eye, but if it is still going to be a problem this can be calculated based on AR.
                bool spanTooFast = s.SpanDuration <= 250d;

                double divisor = BeatDivisor.Value;
                // dur/BL*div is always lower than actual note count by 1, so using >=, not >.
                if (s.Duration / point.BeatLength * divisor >= deathstream_length && DeathstreamsSlowingDown.Value)
                    divisor /= 2d; // making stream slower twice, if it's longer than the limit.

                if ((spanTooShort || spanTooFast) && s.RepeatCount > 0)
                {
                    // regular conversion won't work well, using other methods

                    double streamSpacing = point.BeatLength / divisor;

                    if (s.RepeatCount >= 7)
                    {
                        // repeat is long, creating square pattern
                        convertSliderToSquare(s, streamSpacing, newObjects);
                    }
                    else if (s.RepeatCount % 2 == 1)
                    {
                        // slider head and tail are in the same place - creating a stack.
                        convertSliderToStack(s, streamSpacing, newObjects);
                    }
                    else
                    {
                        // slider head and tail are different - creating a compressed stream
                        newObjects.AddRange(OsuHitObjectGenerationUtils.ConvertSliderToStreamIgnoringRepeats(s, streamSpacing));
                    }
                }
                else
                {
                    // regular conversion as editor does
                    newObjects.AddRange(OsuHitObjectGenerationUtils.ConvertSliderToStream(s, point, divisor));
                }
            }

            // guard from overlapping sliders

            newObjects.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));

            for (int i = newObjects.Count - 1; i > 0; i--)
            {
                // filtering out circles closer than 1ms
                if (Precision.AlmostEquals(newObjects[i].StartTime, newObjects[i - 1].StartTime, 1d))
                    newObjects.RemoveAt(i);
            }

            osuBeatmap.HitObjects = newObjects;
        }

        private static void convertSliderToStack(Slider slider, double streamSpacing, List<OsuHitObject> list)
        {
            int i = 0;
            double time = slider.StartTime;

            while (!Precision.DefinitelyBigger(time, slider.GetEndTime(), 1))
            {
                var samplePoint = (SampleControlPoint)slider.SampleControlPoint.DeepClone();
                samplePoint.Time = time;

                list.Add(new HitCircle
                {
                    StartTime = time,
                    Position = slider.Position,
                    NewCombo = i == 0 && slider.NewCombo,
                    SampleControlPoint = samplePoint,
                    Samples = slider.HeadCircle.Samples.Select(s => s.With()).ToList()
                });

                i += 1;
                time = slider.StartTime + i * streamSpacing;
            }
        }

        private static void convertSliderToSquare(Slider slider, double streamSpacing, List<OsuHitObject> list)
        {
            int i = 0;
            double time = slider.StartTime;
            Vector2 len = slider.Path.PositionAt(1);
            Vector2 lenHalf = len / 2;
            Vector2 start = slider.Position;
            Vector2[] points =
            {
                start,
                start + lenHalf + lenHalf.PerpendicularLeft,
                start + len,
                start + lenHalf + lenHalf.PerpendicularRight
            };

            while (!Precision.DefinitelyBigger(time, slider.GetEndTime(), 1))
            {
                var samplePoint = (SampleControlPoint)slider.SampleControlPoint.DeepClone();
                samplePoint.Time = time;

                list.Add(new HitCircle
                {
                    StartTime = time,
                    Position = OsuHitObjectGenerationUtils.ClampToPlayfieldWithPadding(points[i % 4], (float)slider.Radius),
                    NewCombo = i == 0 && slider.NewCombo,
                    SampleControlPoint = samplePoint,
                    Samples = slider.HeadCircle.Samples.Select(s => s.With()).ToList()
                });

                i += 1;
                time = slider.StartTime + i * streamSpacing;
            }
        }
    }
}
