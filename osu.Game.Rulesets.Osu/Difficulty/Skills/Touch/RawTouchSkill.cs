// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills.Touch
{
    public abstract class RawTouchSkill : IDeepCloneable<RawTouchSkill>
    {
        protected virtual double StrainDecayBase => 0.15;

        public double CurrentStrain { get; private set; }

        protected readonly double ClockRate;

        protected readonly List<OsuHitObject> LastLeftObjects;
        protected readonly List<OsuHitObject> LastRightObjects;

        protected readonly List<DifficultyHitObject> LastLeftDifficultyObjects;
        protected readonly List<DifficultyHitObject> LastRightDifficultyObjects;

        private static readonly int maximum_objects_history = 2;
        private static readonly int maximum_difficulty_objects_history = 3;

        private TouchHand lastHand = TouchHand.Right;

        protected RawTouchSkill(double clockRate)
        {
            ClockRate = clockRate;

            LastLeftObjects = new List<OsuHitObject>();
            LastRightObjects = new List<OsuHitObject>();

            LastLeftDifficultyObjects = new List<DifficultyHitObject>();
            LastRightDifficultyObjects = new List<DifficultyHitObject>();
        }

        protected RawTouchSkill(RawTouchSkill copy)
        {
            CurrentStrain = copy.CurrentStrain;
            ClockRate = copy.ClockRate;
            lastHand = copy.lastHand;

            LastLeftObjects = new List<OsuHitObject>(copy.LastLeftObjects);
            LastRightObjects = new List<OsuHitObject>(copy.LastRightObjects);

            LastLeftDifficultyObjects = new List<DifficultyHitObject>(copy.LastLeftDifficultyObjects);
            LastRightDifficultyObjects = new List<DifficultyHitObject>(copy.LastRightDifficultyObjects);
        }

        public void Process(OsuDifficultyHitObject current, TouchHand currentHand)
        {
            if (current.Index == 0)
            {
                // Automatically assume the first note of a beatmap is hit with
                // the left hand and the second note is hit with the right.
                LastLeftObjects.Add((OsuHitObject)current.LastObject);
                LastRightObjects.Add((OsuHitObject)current.BaseObject);

                return;
            }

            var simulated = currentHand == TouchHand.Drag ? current : createSimulatedObject(current, currentHand);

            updateStrainValue(current, simulated, currentHand);
            updateHistory(simulated, currentHand);
        }

        private OsuDifficultyHitObject createSimulatedObject(OsuDifficultyHitObject current, TouchHand currentHand)
        {
            // A simulated difficulty object is created for hand-specific difficulty properties.
            // Objects before the current object are derived from the same hand.
            var lastObjects = currentHand == TouchHand.Left ? LastLeftObjects : LastRightObjects;
            var lastDifficultyObjects = currentHand == TouchHand.Left ? LastLeftDifficultyObjects : LastRightDifficultyObjects;
            var lastLast = lastObjects.Count > 1 ? lastObjects[^2] : null;

            return new OsuDifficultyHitObject(current.BaseObject, lastObjects.Last(), lastLast, ClockRate, lastDifficultyObjects, lastDifficultyObjects.Count);
        }

        private void updateStrainValue(OsuDifficultyHitObject current, OsuDifficultyHitObject simulated, TouchHand currentHand)
        {
            CurrentStrain *= strainDecay(current.StrainTime);

            // For drag, treat the object in the same way as non-touchscreen gameplay.
            if (currentHand == TouchHand.Drag)
                CurrentStrain += StrainValueOf(simulated);
            else
                CurrentStrain += StrainValueIf(simulated, currentHand, lastHand);
        }

        private void updateHistory(OsuDifficultyHitObject current, TouchHand currentHand)
        {
            var relevantHand = getRelevantHand(currentHand);

            var lastObjects = relevantHand == TouchHand.Left ? LastLeftObjects : LastRightObjects;
            var lastDifficultyObjects = relevantHand == TouchHand.Left ? LastLeftDifficultyObjects : LastRightDifficultyObjects;

            updateHistory(lastDifficultyObjects, current, maximum_difficulty_objects_history);
            updateHistory(lastObjects, (OsuHitObject)current.BaseObject, maximum_objects_history);

            static void updateHistory<T>(List<T> objects, T obj, int maxLength)
            {
                objects.Add(obj);

                while (objects.Count > maxLength)
                    objects.RemoveAt(0);
            }
        }

        private TouchHand getRelevantHand(TouchHand currentHand) => currentHand == TouchHand.Drag ? lastHand : currentHand;

        protected TouchHand GetOtherHand(TouchHand currentHand)
        {
            switch (currentHand)
            {
                case TouchHand.Left:
                    return TouchHand.Right;
                case TouchHand.Right:
                    return TouchHand.Left;
                default:
                    return GetOtherHand(lastHand);
            }
        }

        private double strainDecay(double ms) => Math.Pow(StrainDecayBase, ms / 1000);

        protected abstract double StrainValueOf(OsuDifficultyHitObject current);

        protected abstract double StrainValueIf(OsuDifficultyHitObject simulated, TouchHand currentHand, TouchHand lastHand);

        public abstract RawTouchSkill DeepClone();
    }
}
