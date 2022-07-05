using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour
{
    public class CoupledColourEncoding
    {
        private const int max_repetition_interval = 16;

        public List<ColourEncoding> Payload = new List<ColourEncoding>();

        public CoupledColourEncoding? Previous { get; private set; } = null;

        /// <summary>
        /// How many notes between the current and previous identical <see cref="TaikoDifficultyHitObjectColour"/>.
        /// Negative number means that there is no repetition in range.
        /// If no repetition is found this will have a value of <see cref="max_repetition_interval"/> + 1.
        /// </summary>
        public int RepetitionInterval { get; private set; } = max_repetition_interval + 1;

        public static List<CoupledColourEncoding> Encode(List<DifficultyHitObject> data)
        {
            List<MonoEncoding> firstPass = MonoEncoding.Encode(data);
            List<ColourEncoding> secondPass = ColourEncoding.Encode(firstPass);
            List<CoupledColourEncoding> thirdPass = CoupledColourEncoding.Encode(secondPass);

            return thirdPass;
        }

        public static List<CoupledColourEncoding> Encode(List<ColourEncoding> data)
        {
            List<CoupledColourEncoding> encoded = new List<CoupledColourEncoding>();

            CoupledColourEncoding? lastEncoded = null;
            for (int i = 0; i < data.Count; i++)
            {
                lastEncoded = new CoupledColourEncoding()
                {
                    Previous = lastEncoded
                };

                bool isCoupled = i < data.Count - 2 && data[i].isIdenticalTo(data[i + 2]);
                if (!isCoupled)
                {
                    lastEncoded.Payload.Add(data[i]);
                }
                else
                {
                    while (isCoupled)
                    {
                        lastEncoded.Payload.Add(data[i]);
                        i++;

                        isCoupled = i < data.Count - 2 && data[i].isIdenticalTo(data[i + 2]);
                    }

                    // Skip over peeked data and add the rest to the payload
                    lastEncoded.Payload.Add(data[i]);
                    lastEncoded.Payload.Add(data[i + 1]);
                    i++;
                }

                encoded.Add(lastEncoded);
            }

            // Final pass to find repetition interval
            for (int i = 0; i < encoded.Count; i++)
            {
                encoded[i].FindRepetitionInterval();
            }

            return encoded;
        }

        /// <summary>
        /// Returns true if other is considered a repetition of this encoding. This is true if other's first two payload
        /// identical mono lengths.
        /// </summary>
        public bool isRepetitionOf(CoupledColourEncoding other)
        {
            if (this.Payload.Count != other.Payload.Count) return false;

            for (int i = 0; i < Math.Min(this.Payload.Count, 2); i++)
            {
                if (!this.Payload[i].hasIdenticalMonoLength(other.Payload[i])) return false;
            }

            return true;
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

            CoupledColourEncoding? other = Previous.Previous;
            int interval = 2;
            while (interval < max_repetition_interval)
            {
                if (this.isRepetitionOf(other))
                {
                    RepetitionInterval = Math.Min(interval, max_repetition_interval);
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