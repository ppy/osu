// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Difficulty.Preprocessing
{
    /// <summary>
    /// Wraps a <see cref="HitObject"/> and provides additional information to be used for difficulty calculation.
    /// </summary>
    public class DifficultyHitObject
    {
        /// <summary>
        /// The <see cref="HitObject"/> this <see cref="DifficultyHitObject"/> wraps.
        /// </summary>
        public readonly HitObject BaseObject;

        /// <summary>
        /// The last <see cref="HitObject"/> which occurs before <see cref="BaseObject"/>.
        /// </summary>
        public readonly HitObject LastObject;

        /// <summary>
        /// Amount of time elapsed between <see cref="BaseObject"/> and <see cref="LastObject"/>, adjusted by clockrate.
        /// </summary>
        public readonly double DeltaTime;

        /// <summary>
        /// Clockrate adjusted start time of <see cref="BaseObject"/>.
        /// </summary>
        public readonly double StartTime;

        /// <summary>
        /// Clockrate adjusted end time of <see cref="BaseObject"/>.
        /// </summary>
        public readonly double EndTime;

        /// <summary>
        /// A list of previous <see cref="DifficultyHitObject"/>s, indexed such that the most recent previous object is at index 0.
        /// </summary>
        public IReadOnlyList<DifficultyHitObject> Previous => (IReadOnlyList<DifficultyHitObject>)PreviousBacking ?? Array.Empty<DifficultyHitObject>();

        /// <summary>
        /// A linked node, linking to the previous <see cref="DifficultyHitObject"/>. This is set by <see cref="DifficultyCalculator"/>.
        /// </summary>
        internal ObjectLink<DifficultyHitObject> PreviousBacking;

        /// <summary>
        /// Creates a new <see cref="DifficultyHitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> which this <see cref="DifficultyHitObject"/> wraps.</param>
        /// <param name="lastObject">The last <see cref="HitObject"/> which occurs before <paramref name="hitObject"/> in the beatmap.</param>
        /// <param name="clockRate">The rate at which the gameplay clock is run at.</param>
        public DifficultyHitObject(HitObject hitObject, HitObject lastObject, double clockRate)
        {
            BaseObject = hitObject;
            LastObject = lastObject;
            DeltaTime = (hitObject.StartTime - lastObject.StartTime) / clockRate;
            StartTime = hitObject.StartTime / clockRate;
            EndTime = hitObject.GetEndTime() / clockRate;

            PreviousBacking = new ObjectLink<DifficultyHitObject>(this, null);
        }
    }
}
