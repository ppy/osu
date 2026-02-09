// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Difficulty.Preprocessing
{
    /// <summary>
    /// Wraps a <see cref="HitObject"/> and provides additional information to be used for difficulty calculation.
    /// </summary>
    public class DifficultyHitObject
    {
        private readonly IReadOnlyList<DifficultyHitObject> difficultyHitObjects;

        /// <summary>
        /// The index of this <see cref="DifficultyHitObject"/> in the list of all <see cref="DifficultyHitObject"/>s.
        /// </summary>
        public int Index;

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
        /// Beatmap playback rate.
        /// </summary>
        public readonly double ClockRate;

        /// <summary>
        /// Creates a new <see cref="DifficultyHitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> which this <see cref="DifficultyHitObject"/> wraps.</param>
        /// <param name="lastObject">The last <see cref="HitObject"/> which occurs before <paramref name="hitObject"/> in the beatmap.</param>
        /// <param name="clockRate">The rate at which the gameplay clock is run at.</param>
        /// <param name="objects">The list of <see cref="DifficultyHitObject"/>s in the current beatmap.</param>
        /// <param name="index">The index of this <see cref="DifficultyHitObject"/> in <paramref name="objects"/> list.</param>
        public DifficultyHitObject(HitObject hitObject, HitObject lastObject, double clockRate, List<DifficultyHitObject> objects, int index)
        {
            difficultyHitObjects = objects;
            Index = index;
            BaseObject = hitObject;
            LastObject = lastObject;
            DeltaTime = (hitObject.StartTime - lastObject.StartTime) / clockRate;
            StartTime = hitObject.StartTime / clockRate;
            EndTime = hitObject.GetEndTime() / clockRate;
            ClockRate = clockRate;
        }

        public DifficultyHitObject Previous(int backwardsIndex)
        {
            int index = Index - (backwardsIndex + 1);
            return index >= 0 && index < difficultyHitObjects.Count ? difficultyHitObjects[index] : default;
        }

        public DifficultyHitObject Next(int forwardsIndex)
        {
            int index = Index + (forwardsIndex + 1);
            return index >= 0 && index < difficultyHitObjects.Count ? difficultyHitObjects[index] : default;
        }

        /// <summary>
        /// Retrieves the full hit window for a <see cref="HitResult"/>.
        /// </summary>
        public virtual double HitWindow(HitResult hitResult)
        {
            // Try to get HitWindows from nested hit objects
            // This is important for objects such as Slider in osu! where the object itself has HitWindows set to Empty, but the nested SliderHead has proper hit windows
            if (BaseObject.HitWindows == HitWindows.Empty)
            {
                foreach (var nestedHitObject in BaseObject.NestedHitObjects)
                {
                    if (nestedHitObject.HitWindows == HitWindows.Empty)
                        continue;

                    return 2 * nestedHitObject.HitWindows.WindowFor(hitResult) / ClockRate;
                }
            }

            return 2 * BaseObject.HitWindows.WindowFor(hitResult) / ClockRate;
        }
    }
}
