// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Judgements;

namespace osu.Game.Rulesets.Mania.Objects
{
    /// <summary>
    /// Represents a hit object which has a single hit press.
    /// </summary>
    public class Note : ManiaHitObject
    {
        public override Judgement CreateJudgement() => new ManiaJudgement();

        private IBeatmap Beatmap;

        public readonly Bindable<int> SnapBindable = new Bindable<int>();

        public int Snap
        {
            get => SnapBindable.Value;
            set => SnapBindable.Value = value;
        }

        public Note()
        {
            this.StartTimeBindable.BindValueChanged(_ => SnapToBeatmap(), true);
        }

        private void SnapToBeatmap()
        {
            if (Beatmap != null)
            {
                TimingControlPoint currentTimingPoint = Beatmap.ControlPointInfo.TimingPointAt(StartTime);
                int timeSignature = (int)currentTimingPoint.TimeSignature;
                double startTime = currentTimingPoint.Time;
                double secondsPerFourCounts = currentTimingPoint.BeatLength * 4;

                double offset = startTime % secondsPerFourCounts;
                double snapResult = StartTime % secondsPerFourCounts - offset;

                if (AlmostDivisibleBy(snapResult, secondsPerFourCounts / 4.0))
                {
                    Snap = 1;
                }
                else if (AlmostDivisibleBy(snapResult, secondsPerFourCounts / 8.0))
                {
                    Snap = 2;
                }
                else if (AlmostDivisibleBy(snapResult, secondsPerFourCounts / 12.0))
                {
                    Snap = 3;
                }
                else if (AlmostDivisibleBy(snapResult, secondsPerFourCounts / 16.0))
                {
                    Snap = 4;
                }
                else if (AlmostDivisibleBy(snapResult, secondsPerFourCounts / 24.0))
                {
                    Snap = 6;
                }
                else if (AlmostDivisibleBy(snapResult, secondsPerFourCounts / 32.0))
                {
                    Snap = 8;
                }
                else if (AlmostDivisibleBy(snapResult, secondsPerFourCounts / 48.0))
                {
                    Snap = 12;
                }
                else if (AlmostDivisibleBy(snapResult, secondsPerFourCounts / 64.0))
                {
                    Snap = 16;
                }
                else
                {
                    Snap = 0;
                }
            }
        }

        private const double LENIENCY_MS = 1.0;
        private static bool AlmostDivisibleBy(double dividend, double divisor)
        {
            double remainder = Math.Abs(dividend) % divisor;
            return Precision.AlmostEquals(remainder, 0, LENIENCY_MS) || Precision.AlmostEquals(remainder - divisor, 0, LENIENCY_MS);
        }
    }
}
