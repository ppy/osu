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
        private DecayingValue(double decayConstant)
        {
            this.decayConstant = decayConstant;
        }

        /// <param name="decayMultiplier">The value will decay by this multiplier in one second</param>
        public static DecayingValue FromDecayMultiplierPerSecond(double decayMultiplier)
        {
            return new DecayingValue(Math.Log(decayMultiplier) / 1000);
        }

        /// <param name="halfLife">The value will reduced by a factor of 0.5 after this many milliseconds</param>
        public static DecayingValue FromHalfLifeMilliseconds(double halfLife)
        {
            return new DecayingValue(-Math.Log(2) / halfLife);
        }

        /// <param name="decayConstant">The value will change by a factor of Math.Exp(decayConstand * elapsedTime)</param>
        public static DecayingValue FromDecayConstant(double decayConstant)
        {
            return new DecayingValue(decayConstant);
        }

        private readonly double decayConstant;

        /// <summary>
        /// Current time in milliseconds
        /// </summary>
        public double CurrentTime { get; private set; }

        public double Value { get; set; }

        public double ValueAtTime(double time)
        {
            Debug.Assert(time >= CurrentTime);

            double deltaTime = time - CurrentTime;
            return Value * Math.Exp(decayConstant * deltaTime);
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
