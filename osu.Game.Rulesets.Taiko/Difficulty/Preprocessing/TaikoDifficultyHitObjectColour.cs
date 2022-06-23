#nullable disable

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing
{
    /// <summary>
    /// Stores colour compression information for a <see cref="TaikoDifficultyHitObject"/>.
    /// </summary>
    public class TaikoDifficultyHitObjectColour
    {
        private const int max_repetition_interval = 16;

        public TaikoDifficultyHitObjectColour Previous { get; private set; }

        /// <summary>
        /// True if the current colour is different from the previous colour.
        /// </summary>
        public bool Delta { get; private set; }

        /// <summary>
        /// How many notes are Delta repeated
        /// </summary>
        public int DeltaRunLength { get; private set; }

        /// <summary>
        /// How many notes between the current and previous identical <see cref="TaikoDifficultyHitObjectColour"/>.
        /// Negative number means that there is no repetition in range.
        /// If no repetition is found this will have a value of <see cref="max_repetition_interval"/> + 1.
        /// </summary>
        public int RepetitionInterval { get; private set; }

        /// <summary>
        ///  Evaluated colour difficulty is cached here, as difficulty don't need to be calculated per-note.
        /// </summary>
        /// TODO: Consider having all evaluated difficulty cached in TaikoDifficultyHitObject instead, since we may be
        ///       reusing evaluator results in the future.
        public double EvaluatedDifficulty;

        public TaikoDifficultyHitObjectColour repeatedColour { get; private set; }

        /// <summary>
        /// Get the <see cref="TaikoDifficultyHitObjectColour"/> instance for the given hitObject. This is implemented
        /// as a static function instead of constructor to allow for reusing existing instances.
        /// </summary>
        public static List<TaikoDifficultyHitObjectColour> CreateColoursFor(List<DifficultyHitObject> hitObjects)
        {
            List<TaikoDifficultyHitObjectColour> colours = new List<TaikoDifficultyHitObjectColour>();

            for (int i = 0; i < hitObjects.Count; i++)
            {
                TaikoDifficultyHitObject hitObject = (TaikoDifficultyHitObject)hitObjects[i];
                TaikoDifficultyHitObject lastObject = hitObject.PreviousNote(0);
                TaikoDifficultyHitObjectColour previous = lastObject?.Colour;
                bool delta = lastObject == null || hitObject.HitType != lastObject.HitType;

                if (previous != null && delta == previous.Delta)
                {
                    previous.DeltaRunLength += 1;
                    hitObject.Colour = previous;
                    continue;
                }

                TaikoDifficultyHitObjectColour colour = new TaikoDifficultyHitObjectColour()
                {
                    Delta = delta,
                    DeltaRunLength = 1,
                    RepetitionInterval = max_repetition_interval + 1,
                    Previous = previous
                };
                hitObject.Colour = colour;
                colours.Add(colour);
            }

            for (int i = 0; i < colours.Count; i++)
            {
                colours[i].FindRepetitionInterval();
            }

            return colours;
        }

        /// <summary>
        /// Finds the closest previous <see cref="TaikoDifficultyHitObjectColour"/> that has the identical delta value
        /// and run length with the current instance, and returns the amount of notes between them.
        /// </summary>
        public void FindRepetitionInterval()
        {
            if (Previous?.Previous == null)
            {
                RepetitionInterval = max_repetition_interval + 1;
                return;
            }

            int interval = Previous.DeltaRunLength;
            TaikoDifficultyHitObjectColour other = Previous.Previous;

            while (other != null && interval < max_repetition_interval)
            {
                if (other.Delta == Delta && other.DeltaRunLength == DeltaRunLength)
                {
                    RepetitionInterval = Math.Min(interval, max_repetition_interval);
                    repeatedColour = other;
                    return;
                }

                interval += other.DeltaRunLength;
                other = other.Previous;
            }

            RepetitionInterval = max_repetition_interval + 1;
        }
    }
}
