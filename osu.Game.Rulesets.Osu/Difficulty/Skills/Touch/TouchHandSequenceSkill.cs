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
    /// <summary>
    /// Represents a sequence of hand movement that hit <see cref="OsuHitObject"/>s.
    /// <para>
    /// In touch device gameplay, a player may hit <see cref="OsuHitObject"/>s with two hands, which can be
    /// treated as having two cursors instead of one. As such, a hand may not hit every <see cref="OsuHitObject"/>s
    /// in a beatmap. This means that treating all <see cref="OsuHitObject"/>s to be hit sequentially by one cursor,
    /// similar to non-touch device gameplay difficulty assessment, is inaccurate.
    /// </para>
    /// <para>
    /// This class keeps track of previous <see cref="OsuHitObject"/>s that were hit by a hand and use them to
    /// simulate an <see cref="OsuDifficultyHitObject"/> if it is hit by a specific hand. The simulated
    /// <see cref="OsuDifficultyHitObject"/> uses the previous <see cref="OsuHitObject"/>s that were hit by the
    /// hand to obtain the correct difficulty properties (e.g. distance, angle) of the <see cref="OsuHitObject"/>,
    /// which improves the accuracy of the assessment.
    /// </para>
    /// </summary>
    public abstract class TouchHandSequenceSkill : IDeepCloneable<TouchHandSequenceSkill>
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

        protected TouchHandSequenceSkill(double clockRate)
        {
            this.clockRate = clockRate;

            lastLeftObjects = new List<OsuHitObject>();
            lastRightObjects = new List<OsuHitObject>();

            lastLeftDifficultyObjects = new List<DifficultyHitObject>();
            lastRightDifficultyObjects = new List<DifficultyHitObject>();
        }

        protected TouchHandSequenceSkill(TouchHandSequenceSkill copy)
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

        /// <summary>
        /// Creates an <see cref="OsuDifficultyHitObject"/> that simulates an <see cref="OsuHitObject"/>
        /// if it and <see cref="OsuHitObject"/>s before it were hit with a specific <see cref="TouchHand"/>.
        /// </summary>
        /// <param name="current">The <see cref="OsuHitObject"/> to simulate.</param>
        /// <param name="hand">The <see cref="TouchHand"/> that hit the <see cref="OsuHitObject"/>.</param>
        /// <returns>The <see cref="OsuDifficultyHitObject"/> that simulates the <see cref="OsuHitObject"/>.</returns>
        protected OsuDifficultyHitObject CreateSimulatedObject(OsuHitObject current, TouchHand hand)
        {
            var lastObjects = GetLastObjects(hand);
            var lastDifficultyObjects = GetLastDifficultyObjects(hand);

            var last = lastObjects.Last();
            var lastLast = lastObjects.Count > 1 ? lastObjects[^2] : null;

            return new OsuDifficultyHitObject(current, last, lastLast, clockRate, lastDifficultyObjects, lastDifficultyObjects.Count);
        }

        private void updateStrainValue(OsuDifficultyHitObject current, OsuDifficultyHitObject simulated, TouchHand currentHand)
        {
            CurrentStrain *= strainDecay(current.StrainTime);

            CurrentStrain += StrainValueIf(simulated, currentHand);
        }

        private void updateHistory(OsuDifficultyHitObject simulated, TouchHand currentHand)
        {
            if (currentHand != TouchHand.Drag)
                LastNonDragHand = currentHand;

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

        /// <summary>
        /// Obtains the <see cref="TouchHand"/> that is the opposite of <paramref name="hand"/>.
        /// </summary>
        /// <remarks>
        /// For <see cref="TouchHand.Drag"/>, the opposite <see cref="TouchHand"/> will be the opposite of <see cref="LastNonDragHand"/>.
        /// </remarks>
        /// <param name="hand">The <see cref="TouchHand"/>.</param>
        /// <returns>The <see cref="TouchHand"/> that is the opposite of <paramref name="hand"/>.</returns>
        protected TouchHand GetOtherHand(TouchHand hand)
        {
            switch (hand)
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

        /// <summary>
        /// Obtains a list of previous <see cref="OsuHitObject"/>s that were hit with a specific <see cref="TouchHand"/>.
        /// </summary>
        /// <remarks>
        /// For <see cref="TouchHand.Drag"/>, the previous <see cref="OsuHitObject"/>s are those hit with <see cref="LastNonDragHand"/>.
        /// </remarks>
        /// <param name="hand">The <see cref="TouchHand"/>.</param>
        /// <returns>A list of previous <see cref="OsuHitObject"/>s that were hit by <paramref name="hand"/>.</returns>
        protected List<OsuHitObject> GetLastObjects(TouchHand hand)
        {
            switch (hand)
            {
                case TouchHand.Left:
                    return lastLeftObjects;

                case TouchHand.Right:
                    return lastRightObjects;

                default:
                    return GetLastObjects(LastNonDragHand);
            }
        }

        /// <summary>
        /// Obtains a list of previous <see cref="DifficultyHitObject"/>s that were hit with a specific <see cref="TouchHand"/>.
        /// </summary>
        /// <remarks>
        /// For <see cref="TouchHand.Drag"/>, the previous <see cref="DifficultyHitObject"/>s are those hit with <see cref="LastNonDragHand"/>.
        /// </remarks>
        /// <param name="hand">The <see cref="TouchHand"/>.</param>
        /// <returns>A list of previous <see cref="DifficultyHitObject"/>s that were hit by <paramref name="hand"/>.</returns>
        protected List<DifficultyHitObject> GetLastDifficultyObjects(TouchHand hand)
        {
            switch (hand)
            {
                case TouchHand.Left:
                    return lastLeftDifficultyObjects;

                case TouchHand.Right:
                    return lastRightDifficultyObjects;

                default:
                    return GetLastDifficultyObjects(LastNonDragHand);
            }
        }

        private double strainDecay(double ms) => Math.Pow(StrainDecayBase, ms / 1000);

        /// <summary>
        /// Computes the strain value of a simulated <see cref="OsuDifficultyHitObject"/> if it is hit with a specific <see cref="TouchHand"/>.
        /// </summary>
        /// <param name="simulated">The simulated <see cref="OsuDifficultyHitObject"/>.</param>
        /// <param name="currentHand">The <see cref="TouchHand"/> that hit the <see cref="OsuDifficultyHitObject"/>.</param>
        /// <returns>The strain value of the <see cref="OsuDifficultyHitObject"/>.</returns>
        protected abstract double StrainValueIf(OsuDifficultyHitObject simulated, TouchHand currentHand);

        public abstract TouchHandSequenceSkill DeepClone();
    }
}
