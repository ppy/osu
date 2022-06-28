using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing
{
    /// <summary>
    /// Stores colour compression information for a <see cref="TaikoDifficultyHitObject"/>. This is only present for the
    /// first <see cref="TaikoDifficultyHitObject"/> in a <see cref="CoupledColourEncoding"/> chunk.
    /// </summary>
    public class TaikoDifficultyHitObjectColour
    {
        private const int max_repetition_interval = 16;

        /// <summary>
        /// How many notes between the current and previous identical <see cref="TaikoDifficultyHitObjectColour"/>.
        /// Negative number means that there is no repetition in range.
        /// If no repetition is found this will have a value of <see cref="max_repetition_interval"/> + 1.
        /// </summary>
        public int RepetitionInterval { get; private set; } = max_repetition_interval + 1;

        /// <summary>
        /// Encoding information of <see cref="TaikoDifficultyHitObjectColour"/>.
        /// </summary>
        public CoupledColourEncoding Encoding { get; private set; }

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
        /// Finds the closest previous <see cref="TaikoDifficultyHitObjectColour"/> that has the identical <see cref="CoupledColourEncoding.Payload"/>.
        /// Interval is defined as the amount of <see cref="CoupledColourEncoding"/> chunks between the current and repeated encoding.
        /// </summary>
        public void FindRepetitionInterval()
        {
            if (Previous?.Previous == null)
            {
                RepetitionInterval = max_repetition_interval + 1;
                return;
            }

            TaikoDifficultyHitObjectColour? other = Previous.Previous;
            int interval = 2;
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
                ++interval;
            }

            RepetitionInterval = max_repetition_interval + 1;
        }
    }
}