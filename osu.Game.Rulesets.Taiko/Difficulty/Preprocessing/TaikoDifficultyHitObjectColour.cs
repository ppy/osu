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
        public CoupledColourEncoding Encoding { get; private set; }

        private const int max_repetition_interval = 16;

        /// <summary>
        /// How many notes between the current and previous identical <see cref="TaikoDifficultyHitObjectColour"/>.
        /// Negative number means that there is no repetition in range.
        /// If no repetition is found this will have a value of <see cref="max_repetition_interval"/> + 1.
        /// </summary>
        public int RepetitionInterval { get; private set; } = max_repetition_interval + 1;

        /// <summary>
        ///  Evaluated colour difficulty is cached here, as difficulty don't need to be calculated per-note.
        /// </summary>
        /// TODO: Consider having all evaluated difficulty cached in TaikoDifficultyHitObject instead, since we may be
        ///       reusing evaluator results in the future.
        public double EvaluatedDifficulty = 0;

        public TaikoDifficultyHitObjectColour? Previous { get; private set; } = null;

        public TaikoDifficultyHitObjectColour? repeatedColour { get; private set; } = null;

        public TaikoDifficultyHitObjectColour(CoupledColourEncoding encoding)
        {
            Encoding = encoding;
        }

        public static List<TaikoDifficultyHitObjectColour> EncodeAndAssign(List<DifficultyHitObject> hitObjects)
        {
            List<TaikoDifficultyHitObjectColour> colours = new List<TaikoDifficultyHitObjectColour>();
            List<CoupledColourEncoding> encodings = CoupledColourEncoding.Encode(ColourEncoding.Encode(hitObjects));
            TaikoDifficultyHitObjectColour? lastColour = null;
            for (int i = 0; i < encodings.Count; i++)
            {
                lastColour = new TaikoDifficultyHitObjectColour(encodings[i])
                {
                    Previous = lastColour
                };
                colours.Add(lastColour);
            }

            foreach (TaikoDifficultyHitObjectColour colour in colours)
            {
                colour.FindRepetitionInterval();
                ((TaikoDifficultyHitObject)hitObjects[colour.Encoding.StartIndex]).Colour = colour;
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

            TaikoDifficultyHitObjectColour? other = Previous.Previous;
            int interval = this.Encoding.StartIndex - other.Encoding.EndIndex;
            while (interval < max_repetition_interval)
            {
                if (Encoding.hasIdenticalPayload(other.Encoding))
                {
                    RepetitionInterval = Math.Min(interval, max_repetition_interval);
                    repeatedColour = other;
                    return;
                }

                other = other.Previous;
                if (other == null) break;
                interval = this.Encoding.StartIndex - other.Encoding.EndIndex;
            }

            RepetitionInterval = max_repetition_interval + 1;
        }
    }
}