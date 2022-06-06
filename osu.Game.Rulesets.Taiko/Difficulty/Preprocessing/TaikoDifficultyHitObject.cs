// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing
{
    /// <summary>
    /// Represents a single hit object in taiko difficulty calculation.
    /// </summary>
    public class TaikoDifficultyHitObject : DifficultyHitObject
    {
        // TODO: Review this - these was originally handled in TaikodifficultyCalculator.CreateDifficultyHitObjects, but
        //       it might be a good idea to encapsulate as much detail within the class as possible.
        private static List<TaikoDifficultyHitObject> centreHitObjects = new List<TaikoDifficultyHitObject>();
        private static List<TaikoDifficultyHitObject> rimHitObjects = new List<TaikoDifficultyHitObject>();

        private readonly IReadOnlyList<TaikoDifficultyHitObject> monoDifficultyHitObjects;
        public readonly int MonoPosition;

        /// <summary>
        /// The rhythm required to hit this hit object.
        /// </summary>
        public readonly TaikoDifficultyHitObjectRhythm Rhythm;

        /// <summary>
        /// Colour data for this hit object. This is used by colour evaluator to calculate colour, but can be used
        /// differently by other skills in the future.
        /// </summary>
        public readonly TaikoDifficultyHitObjectColour Colour;

        /// <summary>
        /// The hit type of this hit object.
        /// </summary>
        public readonly HitType? HitType;

        /// <summary>
        /// Creates a new difficulty hit object.
        /// </summary>
        /// <param name="hitObject">The gameplay <see cref="HitObject"/> associated with this difficulty object.</param>
        /// <param name="lastObject">The gameplay <see cref="HitObject"/> preceding <paramref name="hitObject"/>.</param>
        /// <param name="lastLastObject">The gameplay <see cref="HitObject"/> preceding <paramref name="lastObject"/>.</param>
        /// <param name="clockRate">The rate of the gameplay clock. Modified by speed-changing mods.</param>
        /// <param name="objects">The list of <see cref="DifficultyHitObject"/>s in the current beatmap.</param>
        /// /// <param name="position">The position of this <see cref="DifficultyHitObject"/> in the <paramref name="objects"/> list.</param>
        public TaikoDifficultyHitObject(HitObject hitObject, HitObject lastObject, HitObject lastLastObject, double clockRate, List<DifficultyHitObject> objects, int position)
            : base(hitObject, lastObject, clockRate, objects, position)
        {
            var currentHit = hitObject as Hit;

            Rhythm = getClosestRhythm(lastObject, lastLastObject, clockRate);
            HitType = currentHit?.Type;

            // Need to be done after HitType is set.
            if (HitType != null)
            {
                // Get previous hit object, while skipping one that does not have defined colour (sliders and spinners).
                // Without skipping through these, sliders and spinners would have contributed to a colour change for the next note.
                TaikoDifficultyHitObject previousHitObject = (TaikoDifficultyHitObject)objects.LastOrDefault();
                while (previousHitObject != null && previousHitObject.Colour == null)
                {
                    previousHitObject = (TaikoDifficultyHitObject)previousHitObject.Previous(0);
                }

                Colour = TaikoDifficultyHitObjectColour.GetInstanceFor(this);
            }

            if (HitType == Objects.HitType.Centre)
            {
                MonoPosition = centreHitObjects.Count();
                centreHitObjects.Add(this);
                monoDifficultyHitObjects = centreHitObjects;
            }
            else if (HitType == Objects.HitType.Rim)
            {
                MonoPosition = rimHitObjects.Count();
                rimHitObjects.Add(this);
                monoDifficultyHitObjects = rimHitObjects;
            }
        }

        /// <summary>
        /// List of most common rhythm changes in taiko maps.
        /// </summary>
        /// <remarks>
        /// The general guidelines for the values are:
        /// <list type="bullet">
        /// <item>rhythm changes with ratio closer to 1 (that are <i>not</i> 1) are harder to play,</item>
        /// <item>speeding up is <i>generally</i> harder than slowing down (with exceptions of rhythm changes requiring a hand switch).</item>
        /// </list>
        /// </remarks>
        private static readonly TaikoDifficultyHitObjectRhythm[] common_rhythms =
        {
            new TaikoDifficultyHitObjectRhythm(1, 1, 0.0),
            new TaikoDifficultyHitObjectRhythm(2, 1, 0.3),
            new TaikoDifficultyHitObjectRhythm(1, 2, 0.5),
            new TaikoDifficultyHitObjectRhythm(3, 1, 0.3),
            new TaikoDifficultyHitObjectRhythm(1, 3, 0.35),
            new TaikoDifficultyHitObjectRhythm(3, 2, 0.6), // purposefully higher (requires hand switch in full alternating gameplay style)
            new TaikoDifficultyHitObjectRhythm(2, 3, 0.4),
            new TaikoDifficultyHitObjectRhythm(5, 4, 0.5),
            new TaikoDifficultyHitObjectRhythm(4, 5, 0.7)
        };

        /// <summary>
        /// Returns the closest rhythm change from <see cref="common_rhythms"/> required to hit this object.
        /// </summary>
        /// <param name="lastObject">The gameplay <see cref="HitObject"/> preceding this one.</param>
        /// <param name="lastLastObject">The gameplay <see cref="HitObject"/> preceding <paramref name="lastObject"/>.</param>
        /// <param name="clockRate">The rate of the gameplay clock.</param>
        private TaikoDifficultyHitObjectRhythm getClosestRhythm(HitObject lastObject, HitObject lastLastObject, double clockRate)
        {
            double prevLength = (lastObject.StartTime - lastLastObject.StartTime) / clockRate;
            double ratio = DeltaTime / prevLength;

            return common_rhythms.OrderBy(x => Math.Abs(x.Ratio - ratio)).First();
        }

        public TaikoDifficultyHitObject PreviousMono(int backwardsIndex) => monoDifficultyHitObjects.ElementAtOrDefault(MonoPosition - (backwardsIndex + 1));

        public TaikoDifficultyHitObject NextMono(int forwardsIndex) => monoDifficultyHitObjects.ElementAtOrDefault(MonoPosition + (forwardsIndex + 1));
    }
}
