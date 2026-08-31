// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Audio
{
    /// <summary>
    /// A rolling estimate of tap timing relative to the preview. Includes the user's input delay.
    /// </summary>
    public class AudioOffsetTapEstimator
    {
        public const int REQUIRED_TAPS = 8;
        public const int MAX_TAPS = 16;

        private readonly Queue<double> errors = new Queue<double>();
        private double lastTapTime = double.NegativeInfinity;

        public int Count => errors.Count;
        public bool HasEstimate => Count >= REQUIRED_TAPS;

        public double MedianError
        {
            get
            {
                if (Count == 0)
                    return 0;

                double[] ordered = errors.Order().ToArray();
                return (ordered[(Count - 1) / 2] + ordered[Count / 2]) / 2;
            }
        }

        public bool AddTap(double referenceTime, double inputTime, double beatLength, out double error)
        {
            double phase = (referenceTime - AudioOffsetCalibrationTrackStore.FIRST_BEAT) / beatLength;
            error = (phase - Math.Floor(phase + 0.5)) * beatLength;

            // Reject double presses, including two taiko keys pressed for the same beat.
            // inputTime is monotonic even when the reference track loops.
            if (inputTime - lastTapTime < beatLength / 2)
                return false;

            lastTapTime = inputTime;
            errors.Enqueue(error);
            if (Count > MAX_TAPS)
                errors.Dequeue();
            return true;
        }

        public void Reset()
        {
            errors.Clear();
            lastTapTime = double.NegativeInfinity;
        }
    }
}
