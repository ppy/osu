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

        private readonly List<OsuHitObject> lastLeftObjects;
        private readonly List<OsuHitObject> lastRightObjects;

        private readonly List<DifficultyHitObject> lastLeftDifficultyObjects;
        private readonly List<DifficultyHitObject> lastRightDifficultyObjects;

        private const int maximum_objects_history = 2;
        private const int maximum_difficulty_objects_history = 3;

        protected TouchHand LastHand { get; private set; } = TouchHand.Right;

        protected RawTouchSkill(double clockRate)
        {
            ClockRate = clockRate;

            lastLeftObjects = new List<OsuHitObject>();
            lastRightObjects = new List<OsuHitObject>();

            lastLeftDifficultyObjects = new List<DifficultyHitObject>();
            lastRightDifficultyObjects = new List<DifficultyHitObject>();
        }

        protected RawTouchSkill(RawTouchSkill copy)
        {
            CurrentStrain = copy.CurrentStrain;
            ClockRate = copy.ClockRate;
            LastHand = copy.LastHand;

            lastLeftObjects = new List<OsuHitObject>(copy.lastLeftObjects);
            lastRightObjects = new List<OsuHitObject>(copy.lastRightObjects);

            lastLeftDifficultyObjects = new List<DifficultyHitObject>(copy.lastLeftDifficultyObjects);
            lastRightDifficultyObjects = new List<DifficultyHitObject>(copy.lastRightDifficultyObjects);
        }

        public void Process(OsuDifficultyHitObject current, TouchHand currentHand)
        {
            if (current.Index == 0)
            {
                // Automatically assume the first note of a beatmap is hit with
                // the left hand and the second note is hit with the right.
                lastLeftObjects.Add((OsuHitObject)current.LastObject);
                lastRightObjects.Add((OsuHitObject)current.BaseObject);

                return;
            }

            var simulated = currentHand == TouchHand.Drag || LastHand == TouchHand.Drag ? createSimulatedObject(current, LastHand) : createSimulatedObject(current, currentHand);

            updateStrainValue(current, simulated, currentHand);
            updateHistory(simulated, currentHand);
        }

        private OsuDifficultyHitObject createSimulatedObject(OsuDifficultyHitObject current, TouchHand currentHand)
        {
            // A simulated difficulty object is created for hand-specific difficulty properties.
            // Objects before the current object are derived from the same hand.
            var lastObjects = GetLastObjects(currentHand);
            var lastDifficultyObjects = GetLastDifficultyObjects(currentHand);
            var lastLast = lastObjects.Count > 1 ? lastObjects[^2] : null;

            return new OsuDifficultyHitObject(current.BaseObject, lastObjects.Last(), lastLast, ClockRate, lastDifficultyObjects, lastDifficultyObjects.Count);
        }

        private void updateStrainValue(OsuDifficultyHitObject current, OsuDifficultyHitObject simulated, TouchHand currentHand)
        {
            CurrentStrain *= strainDecay(current.StrainTime);

            CurrentStrain += StrainValueIf(simulated, currentHand);
        }

        private void updateHistory(OsuDifficultyHitObject simulated, TouchHand currentHand)
        {
            LastHand = getRelevantHand(currentHand);

            var lastObjects = GetLastObjects(LastHand);
            var lastDifficultyObjects = GetLastDifficultyObjects(LastHand);

            updateObjectHistory(lastDifficultyObjects, simulated, maximum_difficulty_objects_history);
            updateObjectHistory(lastObjects, (OsuHitObject)simulated.BaseObject, maximum_objects_history);

            static void updateObjectHistory<T>(List<T> objects, T obj, int maxLength)
            {
                objects.Add(obj);

                while (objects.Count > maxLength)
                    objects.RemoveAt(0);
            }
        }

        private TouchHand getRelevantHand(TouchHand currentHand) => currentHand == TouchHand.Drag ? LastHand : currentHand;

        protected TouchHand GetOtherHand(TouchHand currentHand)
        {
            switch (currentHand)
            {
                case TouchHand.Left:
                    return TouchHand.Right;

                case TouchHand.Right:
                    return TouchHand.Left;

                default:
                    return GetOtherHand(LastHand);
            }
        }

        protected List<OsuHitObject> GetLastObjects(TouchHand hand) => hand == TouchHand.Left ? lastLeftObjects : lastRightObjects;

        protected List<DifficultyHitObject> GetLastDifficultyObjects(TouchHand hand) => hand == TouchHand.Left ? lastLeftDifficultyObjects : lastRightDifficultyObjects;

        private double strainDecay(double ms) => Math.Pow(StrainDecayBase, ms / 1000);

        protected abstract double StrainValueIf(OsuDifficultyHitObject simulated, TouchHand currentHand);

        public abstract RawTouchSkill DeepClone();
    }
}
