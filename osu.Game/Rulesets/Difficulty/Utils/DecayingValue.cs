// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;

namespace osu.Game.Rulesets.Difficulty.Utils
{
    /// <summary>
    /// Represents a value that exponentially decays over time
    /// </summary>
    public class DecayingValue
    {
        /// <param name="exponentialBase">The value will decay by this multiplier in one second</param>
        public DecayingValue(double exponentialBase)
        {
            decayRate = Math.Log(exponentialBase) / 1000;
        }

        private readonly double decayRate;

        /// <summary>
        /// Current time in milliseconds
        /// </summary>
        public double CurrentTime { get; private set; }

        public double Value { get; private set; }

        public double ValueAtTime(double time)
        {
            Debug.Assert(time >= CurrentTime);

            double deltaTime = time - CurrentTime;
            return Value * Math.Exp(decayRate * deltaTime);
        }

        public double UpdateTime(double time)
        {
            Value = ValueAtTime(time);
            CurrentTime = time;
            return Value;
        }

        public double IncrementValueAtTime(double time, double valueIncrease)
        {
            UpdateTime(time);
            Value += valueIncrease;

            return Value;
        }
    }
}
