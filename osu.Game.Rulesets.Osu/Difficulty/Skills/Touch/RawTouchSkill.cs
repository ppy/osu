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

        private readonly double clockRate;

        private readonly List<OsuHitObject> lastLeftObjects;
        private readonly List<OsuHitObject> lastRightObjects;

        private readonly List<DifficultyHitObject> lastLeftDifficultyObjects;
        private readonly List<DifficultyHitObject> lastRightDifficultyObjects;

        private const int maximum_objects_history = 2;
        private const int maximum_difficulty_objects_history = 3;

        protected TouchHand LastHand { get; private set; } = TouchHand.Right;
        protected TouchHand LastNonDragHand { get; private set; } = TouchHand.Right;

        protected RawTouchSkill(double clockRate)
        {
            this.clockRate = clockRate;

            lastLeftObjects = new List<OsuHitObject>();
            lastRightObjects = new List<OsuHitObject>();

            lastLeftDifficultyObjects = new List<DifficultyHitObject>();
            lastRightDifficultyObjects = new List<DifficultyHitObject>();
        }

        protected RawTouchSkill(RawTouchSkill copy)
        {
            CurrentStrain = copy.CurrentStrain;
            LastHand = copy.LastHand;
            LastNonDragHand = copy.LastNonDragHand;
            clockRate = copy.clockRate;

            lastLeftObjects = new List<OsuHitObject>(copy.lastLeftObjects);
            lastRightObjects = new List<OsuHitObject>(copy.lastRightObjects);

            lastLeftDifficultyObjects = new List<DifficultyHitObject>(copy.lastLeftDifficultyObjects);
            lastRightDifficultyObjects = new List<DifficultyHitObject>(copy.lastRightDifficultyObjects);
        }

        public void Process(OsuDifficultyHitObject current, TouchHand currentHand)
        {
            var osuCurrentObject = (OsuHitObject)current.BaseObject;
            var osuLastObject = (OsuHitObject)current.LastObject;

            if (current.Index == 0)
            {
                // Automatically assume the first note of a beatmap is hit with
                // the left hand and the second note is hit with the right.
                lastLeftObjects.Add(osuLastObject);
                lastRightObjects.Add(osuCurrentObject);

                return;
            }

            var simulated = currentHand == TouchHand.Drag || LastHand == TouchHand.Drag ? CreateSimulatedObject(osuCurrentObject, LastNonDragHand) : CreateSimulatedObject(osuCurrentObject, currentHand);

            updateStrainValue(current, simulated, currentHand);
            updateHistory(simulated, currentHand);
        }

        protected OsuDifficultyHitObject CreateSimulatedObject(OsuHitObject current, TouchHand hand)
        {
            var lastObjects = GetLastObjects(hand);
            var lastDifficultyObjects = GetLastDifficultyObjects(hand);
            var lastLast = lastObjects.Count > 1 ? lastObjects[^2] : null;

            return new OsuDifficultyHitObject(current, lastObjects.Last(), lastLast, clockRate, lastDifficultyObjects, lastDifficultyObjects.Count);
        }

        protected OsuDifficultyHitObject CreateSimulatedSwapObject(OsuHitObject current, TouchHand hand)
        {
            var otherHand = GetOtherHand(hand);

            var last = GetLastObjects(otherHand).Last();
            var lastLast = GetLastObjects(hand).Last();

            var lastDifficultyObjects = GetLastDifficultyObjects(hand);

            return new OsuDifficultyHitObject(current, last, lastLast, clockRate, lastDifficultyObjects, lastDifficultyObjects.Count);
        }

        private void updateStrainValue(OsuDifficultyHitObject current, OsuDifficultyHitObject simulated, TouchHand currentHand)
        {
            CurrentStrain *= strainDecay(current.StrainTime);

            CurrentStrain += StrainValueIf(simulated, currentHand);
        }

        private void updateHistory(OsuDifficultyHitObject simulated, TouchHand currentHand)
        {
            LastNonDragHand = getRelevantHand(currentHand);
            LastHand = currentHand;

            var lastObjects = GetLastObjects(LastNonDragHand);
            var lastDifficultyObjects = GetLastDifficultyObjects(LastNonDragHand);

            updateObjectHistory(lastDifficultyObjects, simulated, maximum_difficulty_objects_history);
            updateObjectHistory(lastObjects, (OsuHitObject)simulated.BaseObject, maximum_objects_history);

            static void updateObjectHistory<T>(List<T> objects, T obj, int maxLength)
            {
                objects.Add(obj);

                while (objects.Count > maxLength)
                    objects.RemoveAt(0);
            }
        }

        private TouchHand getRelevantHand(TouchHand currentHand) => currentHand == TouchHand.Drag ? LastNonDragHand : currentHand;

        protected TouchHand GetOtherHand(TouchHand currentHand)
        {
            switch (currentHand)
            {
                case TouchHand.Left:
                    return TouchHand.Right;

                case TouchHand.Right:
                    return TouchHand.Left;

                default:
                    // When dragging, the last non-drag hand is the current hand.
                    return GetOtherHand(LastNonDragHand);
            }
        }

        protected List<OsuHitObject> GetLastObjects(TouchHand hand) => hand == TouchHand.Left ? lastLeftObjects : lastRightObjects;

        protected List<DifficultyHitObject> GetLastDifficultyObjects(TouchHand hand) => hand == TouchHand.Left ? lastLeftDifficultyObjects : lastRightDifficultyObjects;

        private double strainDecay(double ms) => Math.Pow(StrainDecayBase, ms / 1000);

        protected abstract double StrainValueIf(OsuDifficultyHitObject simulated, TouchHand currentHand);

        public abstract RawTouchSkill DeepClone();
    }
}
