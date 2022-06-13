// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
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
        private readonly IReadOnlyList<TaikoDifficultyHitObject> monoDifficultyHitObjects;
        public readonly int MonoIndex;
        private readonly IReadOnlyList<TaikoDifficultyHitObject> noteObjects;
        public readonly int NoteIndex;

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
        /// Creates a list of <see cref="TaikoDifficultyHitObject"/>s from a <see cref="IBeatmap"/>s.
        /// TODO: Review this - this is moved here from TaikoDifficultyCalculator so that TaikoDifficultyCalculator can
        ///       have less knowledge of implementation details (i.e. creating all the different hitObject lists, and
        ///       calling FindRepetitionInterval for the final object). The down side of this is
        ///       TaikoDifficultyHitObejct.CreateDifficultyHitObjects is now pretty much a proxy for this.
        /// </summary>
        /// <param name="beatmap">The beatmap from which the list of <see cref="TaikoDifficultyHitObject"/> is created.</param>
        /// <param name="clockRate">The rate at which the gameplay clock is run at.</param>
        public static List<DifficultyHitObject> Create(IBeatmap beatmap, double clockRate)
        {
            List<DifficultyHitObject> difficultyHitObject = new List<DifficultyHitObject>();
            List<TaikoDifficultyHitObject> centreObjects = new List<TaikoDifficultyHitObject>();
            List<TaikoDifficultyHitObject> rimObjects = new List<TaikoDifficultyHitObject>();
            List<TaikoDifficultyHitObject> noteObjects = new List<TaikoDifficultyHitObject>();

            for (int i = 2; i < beatmap.HitObjects.Count; i++)
            {
                difficultyHitObject.Add(
                    new TaikoDifficultyHitObject(
                        beatmap.HitObjects[i], beatmap.HitObjects[i - 1], beatmap.HitObjects[i - 2], clockRate, difficultyHitObject,
                        centreObjects, rimObjects, noteObjects, difficultyHitObject.Count)
                );
            }

            // Find repetition interval for the final TaikoDifficultyHitObjectColour
            ((TaikoDifficultyHitObject)difficultyHitObject.Last()).Colour?.FindRepetitionInterval();
            return difficultyHitObject;
        }

        /// <summary>
        /// Creates a new difficulty hit object.
        /// </summary>
        /// <param name="hitObject">The gameplay <see cref="HitObject"/> associated with this difficulty object.</param>
        /// <param name="lastObject">The gameplay <see cref="HitObject"/> preceding <paramref name="hitObject"/>.</param>
        /// <param name="lastLastObject">The gameplay <see cref="HitObject"/> preceding <paramref name="lastObject"/>.</param>
        /// <param name="clockRate">The rate of the gameplay clock. Modified by speed-changing mods.</param>
        /// <param name="objects">The list of all <see cref="DifficultyHitObject"/>s in the current beatmap.</param>
        /// <param name="centreHitObjects">The list of centre (don) <see cref="DifficultyHitObject"/>s in the current beatmap.</param>
        /// <param name="rimHitObjects">The list of rim (kat) <see cref="DifficultyHitObject"/>s in the current beatmap.</param>
        /// <param name="noteObjects">The list of <see cref="DifficultyHitObject"/>s that is a hit (i.e. not a slider or spinner) in the current beatmap.</param>
        /// <param name="index">The position of this <see cref="DifficultyHitObject"/> in the <paramref name="objects"/> list.</param>
        ///
        /// TODO: This argument list is getting long, we might want to refactor this into a static method that create
        ///       all <see cref="DifficultyHitObject"/>s from a <see cref="IBeatmap"/>.
        private TaikoDifficultyHitObject(HitObject hitObject, HitObject lastObject, HitObject lastLastObject, double clockRate,
            List<DifficultyHitObject> objects,
            List<TaikoDifficultyHitObject> centreHitObjects,
            List<TaikoDifficultyHitObject> rimHitObjects,
            List<TaikoDifficultyHitObject> noteObjects, int index)
            : base(hitObject, lastObject, clockRate, objects, index)
        {
            var currentHit = hitObject as Hit;
            this.noteObjects = noteObjects;

            Rhythm = getClosestRhythm(lastObject, lastLastObject, clockRate);
            HitType = currentHit?.Type;

            if (HitType == Objects.HitType.Centre)
            {
                MonoIndex = centreHitObjects.Count;
                centreHitObjects.Add(this);
                monoDifficultyHitObjects = centreHitObjects;
            }
            else if (HitType == Objects.HitType.Rim)
            {
                MonoIndex = rimHitObjects.Count;
                rimHitObjects.Add(this);
                monoDifficultyHitObjects = rimHitObjects;
            }

            // Need to be done after HitType is set.
            if (HitType == null) return;

            NoteIndex = noteObjects.Count;
            noteObjects.Add(this);

            // Need to be done after NoteIndex is set.
            Colour = TaikoDifficultyHitObjectColour.GetInstanceFor(this);
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

        public TaikoDifficultyHitObject PreviousMono(int backwardsIndex) => monoDifficultyHitObjects.ElementAtOrDefault(MonoIndex - (backwardsIndex + 1));

        public TaikoDifficultyHitObject NextMono(int forwardsIndex) => monoDifficultyHitObjects.ElementAtOrDefault(MonoIndex + (forwardsIndex + 1));

        public TaikoDifficultyHitObject PreviousNote(int backwardsIndex) => noteObjects.ElementAtOrDefault(NoteIndex - (backwardsIndex + 1));

        public TaikoDifficultyHitObject NextNote(int forwardsIndex) => noteObjects.ElementAtOrDefault(NoteIndex + (forwardsIndex + 1));
    }
}
