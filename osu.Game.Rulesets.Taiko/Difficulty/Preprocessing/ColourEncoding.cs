using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing
{
    public class ColourEncoding
    {
        /// <summary>
        /// Amount consecutive notes of the same colour
        /// </summary>
        public int MonoRunLength = 1;

        /// <summary>
        /// Amount of consecutive encoding with the same <see cref="MonoRunLength" />
        /// </summary>
        public int EncodingRunLength = 1;

        /// <summary>
        /// How many notes are encoded with this encoding
        /// </summary>
        public int NoteLength => MonoRunLength + EncodingRunLength;

        /// <summary>
        /// Beginning index in the data that this encodes
        /// </summary>
        public int StartIndex = 0;

        public bool isIdenticalTo(ColourEncoding other)
        {
            return other.MonoRunLength == MonoRunLength && other.EncodingRunLength == EncodingRunLength;
        }

        public static List<ColourEncoding> Encode(List<DifficultyHitObject> data)
        {
            // Compute mono lengths
            List<ColourEncoding> firstPass = new List<ColourEncoding>();
            ColourEncoding? lastEncoded = null;
            for (int i = 0; i < data.Count; i++)
            {
                TaikoDifficultyHitObject taikoObject = (TaikoDifficultyHitObject)data[i];
                // This ignores all non-note objects, which may or may not be the desired behaviour
                TaikoDifficultyHitObject previousObject = (TaikoDifficultyHitObject)taikoObject.PreviousNote(0);

                if (
                    previousObject == null ||
                    lastEncoded == null ||
                    taikoObject.HitType != previousObject.HitType ||
                    taikoObject.Rhythm.Ratio > 1.9) // Reset colour after a slow down of 2x (set as 1.9x for margin of error)
                {
                    lastEncoded = new ColourEncoding();
                    lastEncoded.StartIndex = i;
                    firstPass.Add(lastEncoded);
                    continue;
                }

                lastEncoded.MonoRunLength += 1;
            }

            // Compute encoding lengths
            List<ColourEncoding> secondPass = new List<ColourEncoding>();
            lastEncoded = null;
            for (int i = 0; i < firstPass.Count; i++)
            {
                if (i == 0 || lastEncoded == null || firstPass[i].MonoRunLength != firstPass[i - 1].MonoRunLength)
                {
                    lastEncoded = firstPass[i];
                    secondPass.Add(firstPass[i]);
                    continue;
                }

                lastEncoded.EncodingRunLength += 1;
            }

            return secondPass;
        }
    }
}