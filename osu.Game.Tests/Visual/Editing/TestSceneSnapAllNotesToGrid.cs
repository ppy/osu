// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Tests.Beatmaps;
using osuTK;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneSnapAllNotesToGrid : EditorTestScene
    {
        private const double beat_length = 60_000 / 180.0;
        private const double timing_point_time = 1500;
        private const int divisor = 4;

        private double circleRawTime;
        private double spinnerRawStart;
        private double spinnerRawEnd;
        private double sliderRawStart;
        private double reverseSliderRawStart;
        private int reverseSliderRepeatCount;

        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var controlPointInfo = new ControlPointInfo();
            controlPointInfo.Add(timing_point_time, new TimingControlPoint { BeatLength = beat_length });
            return new TestBeatmap(ruleset, false)
            {
                ControlPointInfo = controlPointInfo
            };
        }

        [Test]
        public void TestSnapAllToCurrentGrid()
        {
            AddStep("set raw times", () =>
            {
                circleRawTime = timing_point_time + 1271;
                spinnerRawStart = timing_point_time + 4104;
                spinnerRawEnd = spinnerRawStart + 333.33;
                sliderRawStart = timing_point_time + 1271 + 5000;
                reverseSliderRawStart = timing_point_time + 1271 + 9200;
                reverseSliderRepeatCount = 2;
            });

            AddStep("add hitobjects", () =>
            {
                EditorBeatmap.AddRange(new HitObject[]
                {
                    new HitCircle { Position = new Vector2(256, 192), StartTime = circleRawTime },
                    new Spinner
                    {
                        Position = new Vector2(256, 192),
                        StartTime = spinnerRawStart,
                        Duration = spinnerRawEnd - spinnerRawStart,
                    },
                    new Slider
                    {
                        Position = new Vector2(256, 256),
                        StartTime = sliderRawStart,
                        Path = new SliderPath(new[] { new PathControlPoint(Vector2.Zero), new PathControlPoint(new Vector2(250, 0)) }),
                    },
                    new Slider
                    {
                        Position = new Vector2(384, 256),
                        StartTime = reverseSliderRawStart,
                        RepeatCount = reverseSliderRepeatCount,
                        Path = new SliderPath(new[] { new PathControlPoint(Vector2.Zero), new PathControlPoint(new Vector2(220, 0)) }),
                        Samples = { new HitSampleInfo(HitSampleInfo.HIT_NORMAL) },
                        NodeSamples =
                        {
                            new List<HitSampleInfo> { new HitSampleInfo(HitSampleInfo.HIT_NORMAL) },
                            new List<HitSampleInfo> { new HitSampleInfo(HitSampleInfo.HIT_NORMAL) },
                        },
                    },
                });
            });

            // Timing change partway through the plain slider: head and tail can fall under different snap grids.
            AddStep("add timing point between slider start and end", () =>
            {
                var slider = EditorBeatmap.HitObjects.OfType<Slider>().First(s => s.RepeatCount == 0);
                // Deliberately not centred so the split is asymmetric.
                double offsetTime = slider.StartTime + (slider.EndTime - slider.StartTime) * 0.38;
                EditorBeatmap.ControlPointInfo.Add(offsetTime, new TimingControlPoint { BeatLength = beat_length * 0.67 });
                EditorBeatmap.UpdateAllHitObjects();
            });

            AddStep($"set beat divisor to 1/{divisor}", () =>
            {
                var beatDivisor = (BindableBeatDivisor)Editor.Dependencies.Get(typeof(BindableBeatDivisor))!;
                beatDivisor.SetArbitraryDivisor(divisor);
            });

            AddStep("snap all to grid", () => EditorBeatmap.SnapAllHitObjectsToCurrentGrid());

            AddAssert("circle snapped", () =>
            {
                var c = EditorBeatmap.HitObjects.OfType<HitCircle>().Single();
                return Precision.AlmostEquals(c.StartTime, EditorBeatmap.SnapTime(circleRawTime, null));
            });

            AddAssert("spinner snapped", () =>
            {
                var s = EditorBeatmap.HitObjects.OfType<Spinner>().Single();
                double expStart = EditorBeatmap.SnapTime(spinnerRawStart, null);
                double expEnd = EditorBeatmap.SnapTime(spinnerRawEnd, null);
                double minEnd = expStart + EditorBeatmap.GetBeatLengthAtTime(expStart);
                if (expEnd < minEnd)
                    expEnd = minEnd;

                return Precision.AlmostEquals(s.StartTime, expStart) && Precision.AlmostEquals(s.EndTime, expEnd);
            });

            AddAssert("timing point between plain slider head and tail after snap", () =>
            {
                var s = EditorBeatmap.HitObjects.OfType<Slider>().First(sl => sl.RepeatCount == 0);
                return EditorBeatmap.ControlPointInfo.TimingPoints.Any(tp => tp.Time > s.StartTime && tp.Time < s.EndTime);
            });

            AddAssert("plain slider head snapped", () =>
            {
                var s = EditorBeatmap.HitObjects.OfType<Slider>().First(sl => sl.RepeatCount == 0);
                return Precision.AlmostEquals(s.StartTime, EditorBeatmap.SnapTime(sliderRawStart, null));
            });

            AddAssert("plain slider tail near grid", () =>
            {
                var s = EditorBeatmap.HitObjects.OfType<Slider>().First(sl => sl.RepeatCount == 0);
                double snappedEnd = EditorBeatmap.SnapTime(s.EndTime, null);
                return Math.Abs(s.EndTime - snappedEnd) < 2.0 && !double.IsNaN(s.SliderVelocityMultiplier);
            });

            AddAssert("reverse slider repeat count preserved", () =>
            {
                var s = EditorBeatmap.HitObjects.OfType<Slider>().First(sl => sl.RepeatCount > 0);
                return s.RepeatCount == reverseSliderRepeatCount;
            });

            AddAssert("reverse slider head snapped", () =>
            {
                var s = EditorBeatmap.HitObjects.OfType<Slider>().First(sl => sl.RepeatCount > 0);
                return Precision.AlmostEquals(s.StartTime, EditorBeatmap.SnapTime(reverseSliderRawStart, null));
            });

            AddAssert("reverse slider tail near grid", () =>
            {
                var s = EditorBeatmap.HitObjects.OfType<Slider>().First(sl => sl.RepeatCount > 0);
                double snappedEnd = EditorBeatmap.SnapTime(s.EndTime, null);
                return Math.Abs(s.EndTime - snappedEnd) < 2.0 && !double.IsNaN(s.SliderVelocityMultiplier);
            });
        }
    }
}
