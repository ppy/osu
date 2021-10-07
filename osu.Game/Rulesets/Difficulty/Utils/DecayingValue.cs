// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

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
            k = Math.Log(exponentialBase) / 1000;
        }

        private readonly double k;

        /// <summary>
        /// Current time in milliseconds
        /// </summary>
        public double CurrentTime { get; private set; }

        // why was this 1 before? it's making tests fail by ~0.001 sr.
        // It makes the whole system work weird, need to initialize to 1 at the time of the first hit object, but process doesn't get called for that
        public double Value { get; set; }

        public double ValueAtTime(double time)
        {
            double deltaTime = time - CurrentTime;
            return Value * Math.Exp(k * deltaTime);
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
