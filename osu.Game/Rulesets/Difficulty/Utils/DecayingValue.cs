using System;
using System.Collections.Generic;
using System.Text;

namespace osu.Game.Rulesets.Difficulty.Utils
{
    public class DecayingValue
    {
        public DecayingValue(double exponentialBase)
        {
            k = Math.Log(exponentialBase)/1000;
        }

        private double k;

        public double CurrentTime { get; private set; } = 0;
        public double Value { get; set; } = 0; // why was this 1 before? it's making tests fail by ~0.001 sr

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


        public double IncrementValue(double time, double valueIncrease)
        {
            UpdateTime(time);
            Value += valueIncrease;

            return Value;
        }
    }
}
