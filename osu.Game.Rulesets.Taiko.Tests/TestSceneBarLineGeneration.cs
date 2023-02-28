// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public partial class TestSceneBarLineGeneration : OsuTestScene
    {
        [Test]
        public void TestCloseBarLineGeneration()
        {
            const double start_time = 1000;

            var beatmap = new Beatmap<TaikoHitObject>
            {
                HitObjects =
                {
                    new Hit
                    {
                        Type = HitType.Centre,
                        StartTime = start_time
                    }
                },
                BeatmapInfo =
                {
                    Difficulty = new BeatmapDifficulty { SliderTickRate = 4 },
                    Ruleset = new TaikoRuleset().RulesetInfo
                },
            };

            beatmap.ControlPointInfo.Add(start_time, new TimingControlPoint());
            beatmap.ControlPointInfo.Add(start_time + 1, new TimingControlPoint());

            var barlines = new BarLineGenerator<BarLine>(beatmap).BarLines;

            AddAssert("first barline generated", () => barlines.Any(b => b.StartTime == start_time));
            AddAssert("second barline generated", () => barlines.Any(b => b.StartTime == start_time + 1));
        }

        [Test]
        public void TestOmitBarLineEffectPoint()
        {
            const double start_time = 1000;
            const double beat_length = 500;

            const int time_signature_numerator = 4;

            var beatmap = new Beatmap<TaikoHitObject>
            {
                HitObjects =
                {
                    new Hit
                    {
                        Type = HitType.Centre,
                        StartTime = start_time
                    }
                },
                BeatmapInfo =
                {
                    Difficulty = new BeatmapDifficulty { SliderTickRate = 4 },
                    Ruleset = new TaikoRuleset().RulesetInfo
                },
            };

            beatmap.ControlPointInfo.Add(start_time, new TimingControlPoint
            {
                BeatLength = beat_length,
                TimeSignature = new TimeSignature(time_signature_numerator),
                OmitFirstBarLine = true
            });

            var barlines = new BarLineGenerator<BarLine>(beatmap).BarLines;

            AddAssert("first barline ommited", () => barlines.All(b => b.StartTime != start_time));
            AddAssert("second barline generated", () => barlines.Any(b => b.StartTime == start_time + (beat_length * time_signature_numerator)));
        }

        [Test]
        public void TestNegativeStartTimeTimingPoint()
        {
            const double beat_length = 250;

            const int time_signature_numerator = 4;

            var beatmap = new Beatmap<TaikoHitObject>
            {
                HitObjects =
                {
                    new Hit
                    {
                        Type = HitType.Centre,
                        StartTime = 1000
                    }
                },
                BeatmapInfo =
                {
                    Difficulty = new BeatmapDifficulty { SliderTickRate = 4 },
                    Ruleset = new TaikoRuleset().RulesetInfo
                },
            };

            beatmap.ControlPointInfo.Add(-100, new TimingControlPoint
            {
                BeatLength = beat_length,
                TimeSignature = new TimeSignature(time_signature_numerator)
            });

            var barlines = new BarLineGenerator<BarLine>(beatmap).BarLines;

            AddAssert("bar line generated at t=900", () => barlines.Any(line => line.StartTime == 900));
            AddAssert("bar line generated at t=1900", () => barlines.Any(line => line.StartTime == 1900));
        }
    }
}
