// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour.Data;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour
{
    /// <summary>
    /// Utility class to perform various encodings.
    /// </summary>
    public static class TaikoColourDifficultyPreprocessor
    {
        /// <summary>
        /// Processes and encodes a list of <see cref="TaikoDifficultyHitObject"/>s into a list of <see cref="TaikoDifficultyHitObjectColour"/>s,
        /// assigning the appropriate <see cref="TaikoDifficultyHitObjectColour"/>s to each <see cref="TaikoDifficultyHitObject"/>,
        /// and pre-evaluating colour difficulty of each <see cref="TaikoDifficultyHitObject"/>.
        /// </summary>
        public static void ProcessAndAssign(List<DifficultyHitObject> hitObjects)
        {
            List<CoupledColourEncoding> encodings = encode(hitObjects);

            // Assign indexing and encoding data to all relevant objects. Only the first note of each encoding type is
            // assigned with the relevant encodings.
            foreach (var coupledEncoding in encodings)
            {
                coupledEncoding.Payload[0].Payload[0].EncodedData[0].Colour.CoupledColourEncoding = coupledEncoding;

                // The outermost loop is kept a ForEach loop since it doesn't need index information, and we want to
                // keep i and j for ColourEncoding's and MonoEncoding's index respectively, to keep it in line with
                // documentation.
                for (int i = 0; i < coupledEncoding.Payload.Count; ++i)
                {
                    ColourEncoding colourEncoding = coupledEncoding.Payload[i];
                    colourEncoding.Parent = coupledEncoding;
                    colourEncoding.Index = i;
                    colourEncoding.Payload[0].EncodedData[0].Colour.ColourEncoding = colourEncoding;

                    for (int j = 0; j < colourEncoding.Payload.Count; ++j)
                    {
                        MonoEncoding monoEncoding = colourEncoding.Payload[j];
                        monoEncoding.Parent = colourEncoding;
                        monoEncoding.Index = j;
                        monoEncoding.EncodedData[0].Colour.MonoEncoding = monoEncoding;
                    }
                }
            }
        }

        /// <summary>
        /// Encodes a list of <see cref="TaikoDifficultyHitObject"/>s into a list of <see cref="CoupledColourEncoding"/>s.
        /// </summary>
        private static List<CoupledColourEncoding> encode(List<DifficultyHitObject> data)
        {
            List<MonoEncoding> firstPass = encodeMono(data);
            List<ColourEncoding> secondPass = encodeColour(firstPass);
            List<CoupledColourEncoding> thirdPass = encodeCoupledColour(secondPass);

            return thirdPass;
        }

        /// <summary>
        /// Encodes a list of <see cref="TaikoDifficultyHitObject"/>s into a list of <see cref="MonoEncoding"/>s.
        /// </summary>
        private static List<MonoEncoding> encodeMono(List<DifficultyHitObject> data)
        {
            List<MonoEncoding> encodings = new List<MonoEncoding>();
            MonoEncoding? currentEncoding = null;

            for (int i = 0; i < data.Count; i++)
            {
                TaikoDifficultyHitObject taikoObject = (TaikoDifficultyHitObject)data[i];

                // This ignores all non-note objects, which may or may not be the desired behaviour
                TaikoDifficultyHitObject? previousObject = taikoObject.PreviousNote(0);

                // If this is the first object in the list or the colour changed, create a new mono encoding
                if (currentEncoding == null || previousObject == null || (taikoObject.BaseObject as Hit)?.Type != (previousObject.BaseObject as Hit)?.Type)
                {
                    currentEncoding = new MonoEncoding();
                    encodings.Add(currentEncoding);
                }

                // Add the current object to the encoded payload.
                currentEncoding.EncodedData.Add(taikoObject);
            }

            return encodings;
        }

        /// <summary>
        /// Encodes a list of <see cref="MonoEncoding"/>s into a list of <see cref="ColourEncoding"/>s.
        /// </summary>
        private static List<ColourEncoding> encodeColour(List<MonoEncoding> data)
        {
            List<ColourEncoding> encodings = new List<ColourEncoding>();
            ColourEncoding? currentEncoding = null;

            for (int i = 0; i < data.Count; i++)
            {
                // Start a new ColourEncoding if the previous MonoEncoding has a different mono length, or if this is the first MonoEncoding in the list.
                if (currentEncoding == null || data[i].RunLength != data[i - 1].RunLength)
                {
                    currentEncoding = new ColourEncoding();
                    encodings.Add(currentEncoding);
                }

                // Add the current MonoEncoding to the encoded payload.
                currentEncoding.Payload.Add(data[i]);
            }

            return encodings;
        }

        /// <summary>
        /// Encodes a list of <see cref="ColourEncoding"/>s into a list of <see cref="CoupledColourEncoding"/>s.
        /// </summary>
        private static List<CoupledColourEncoding> encodeCoupledColour(List<ColourEncoding> data)
        {
            List<CoupledColourEncoding> encodings = new List<CoupledColourEncoding>();
            CoupledColourEncoding? currentEncoding = null;

            for (int i = 0; i < data.Count; i++)
            {
                // Start a new CoupledColourEncoding. ColourEncodings that should be grouped together will be handled later within this loop.
                currentEncoding = new CoupledColourEncoding(currentEncoding);

                // Determine if future ColourEncodings should be grouped.
                bool isCoupled = i < data.Count - 2 && data[i].IsRepetitionOf(data[i + 2]);

                if (!isCoupled)
                {
                    // If not, add the current ColourEncoding to the encoded payload and continue.
                    currentEncoding.Payload.Add(data[i]);
                }
                else
                {
                    // If so, add the current ColourEncoding to the encoded payload and start repeatedly checking if the
                    // subsequent ColourEncodings should be grouped by increasing i and doing the appropriate isCoupled check.
                    while (isCoupled)
                    {
                        currentEncoding.Payload.Add(data[i]);
                        i++;
                        isCoupled = i < data.Count - 2 && data[i].IsRepetitionOf(data[i + 2]);
                    }

                    // Skip over viewed data and add the rest to the payload
                    currentEncoding.Payload.Add(data[i]);
                    currentEncoding.Payload.Add(data[i + 1]);
                    i++;
                }

                encodings.Add(currentEncoding);
            }

            // Final pass to find repetition intervals
            for (int i = 0; i < encodings.Count; i++)
            {
                encodings[i].FindRepetitionInterval();
            }

            return encodings;
        }
    }
}
