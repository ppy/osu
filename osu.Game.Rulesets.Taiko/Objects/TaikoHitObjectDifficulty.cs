// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;

namespace osu.Game.Rulesets.Taiko.Objects
{
    internal class TaikoHitObjectDifficulty
    {
        /// <summary>
        /// Factor by how much individual / overall strain decays per second.
        /// </summary>
        /// <remarks>
        /// These values are results of tweaking a lot and taking into account general feedback.
        /// </remarks>
        internal const double DECAY_BASE = 0.30;

        private const double type_change_bonus = 0.75;
        private const double rhythm_change_bonus = 1.0;
        private const double rhythm_change_base_threshold = 0.2;
        private const double rhythm_change_base = 2.0;

        internal TaikoHitObject BaseHitObject;

        /// <summary>
        /// Measures note density in a way
        /// </summary>
        internal double Strain = 1;

        private double timeElapsed;
        private int sameTypeSince = 1;

        private bool isRim => BaseHitObject is RimHit;

        public TaikoHitObjectDifficulty(TaikoHitObject baseHitObject)
        {
            BaseHitObject = baseHitObject;
        }

        internal void CalculateStrains(TaikoHitObjectDifficulty previousHitObject, double timeRate)
        {
            // Rather simple, but more specialized things are inherently inaccurate due to the big difference playstyles and opinions make.
            // See Taiko feedback thread.
            timeElapsed = (BaseHitObject.StartTime - previousHitObject.BaseHitObject.StartTime) / timeRate;
            double decay = Math.Pow(DECAY_BASE, timeElapsed / 1000);

            double addition = 1;

            // Only if we are no slider or spinner we get an extra addition
            if (previousHitObject.BaseHitObject is Hit && BaseHitObject is Hit
                && BaseHitObject.StartTime - previousHitObject.BaseHitObject.StartTime < 1000) // And we only want to check out hitobjects which aren't so far in the past
            {
                addition += typeChangeAddition(previousHitObject);
                addition += rhythmChangeAddition(previousHitObject);
            }

            double additionFactor = 1.0;
            // Scale AdditionFactor linearly from 0.4 to 1 for TimeElapsed from 0 to 50
            if (timeElapsed < 50.0)
                additionFactor = 0.4 + 0.6 * timeElapsed / 50.0;

            Strain = previousHitObject.Strain * decay + addition * additionFactor;
        }

        private TypeSwitch lastTypeSwitchEven = TypeSwitch.None;
        private double typeChangeAddition(TaikoHitObjectDifficulty previousHitObject)
        {
            // If we don't have the same hit type, trigger a type change!
            if (previousHitObject.isRim != isRim)
            {
                lastTypeSwitchEven = previousHitObject.sameTypeSince % 2 == 0 ? TypeSwitch.Even : TypeSwitch.Odd;

                // We only want a bonus if the parity of the type switch changes!
                switch (previousHitObject.lastTypeSwitchEven)
                {
                    case TypeSwitch.Even:
                        if (lastTypeSwitchEven == TypeSwitch.Odd)
                            return type_change_bonus;
                        break;
                    case TypeSwitch.Odd:
                        if (lastTypeSwitchEven == TypeSwitch.Even)
                            return type_change_bonus;
                        break;
                }
            }
            // No type change? Increment counter and keep track of last type switch
            else
            {
                lastTypeSwitchEven = previousHitObject.lastTypeSwitchEven;
                sameTypeSince = previousHitObject.sameTypeSince + 1;
            }

            return 0;
        }

        private double rhythmChangeAddition(TaikoHitObjectDifficulty previousHitObject)
        {
            // We don't want a division by zero if some random mapper decides to put 2 HitObjects at the same time.
            if (timeElapsed == 0 || previousHitObject.timeElapsed == 0)
                return 0;

            double timeElapsedRatio = Math.Max(previousHitObject.timeElapsed / timeElapsed, timeElapsed / previousHitObject.timeElapsed);

            if (timeElapsedRatio >= 8)
                return 0;

            double difference = Math.Log(timeElapsedRatio, rhythm_change_base) % 1.0;

            if (isWithinChangeThreshold(difference))
                return rhythm_change_bonus;

            return 0;
        }

        private bool isWithinChangeThreshold(double value)
        {
            return value > rhythm_change_base_threshold && value < 1 - rhythm_change_base_threshold;
        }

        private enum TypeSwitch
        {
            None,
            Even,
            Odd
        }
    }
}
