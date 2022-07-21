// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour.Data;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour
{
    /// <summary>
    /// Utility class to perform various encodings. This is separated out from the encoding classes to prevent circular
    /// dependencies.
    /// </summary>
    public class TaikoColourDifficultyPreprocessor
    {
        /// <summary>
        /// Processes and encodes a list of <see cref="TaikoDifficultyHitObject"/>s into a list of <see cref="TaikoDifficultyHitObjectColour"/>s,
        /// assigning the appropriate <see cref="TaikoDifficultyHitObjectColour"/>s to each <see cref="TaikoDifficultyHitObject"/>,
        /// and pre-evaluating colour difficulty of each <see cref="TaikoDifficultyHitObject"/>.
        /// </summary>
        public static List<TaikoDifficultyHitObjectColour> ProcessAndAssign(List<DifficultyHitObject> hitObjects)
        {
            List<TaikoDifficultyHitObjectColour> colours = new List<TaikoDifficultyHitObjectColour>();
            List<CoupledColourEncoding> encodings = Encode(hitObjects);

            // Assign indexing and encoding data to all relevant objects. Only the first note of each encoding type is
            // assigned with the relevant encodings.
            encodings.ForEach(coupledEncoding =>
            {
                coupledEncoding.Payload[0].Payload[0].EncodedData[0].Colour.CoupledColourEncoding = coupledEncoding;

                // TODO: Review this -
                // The outermost loop is kept a ForEach loop since it doesn't need index information, and we want to
                // keep i and j for ColourEncoding's and MonoEncoding's index respectively, to keep it in line with
                // documentation.
                // If we want uniformity for the outermost loop, it can be switched to a for loop with h or something
                // else as an index
                //
                // While parent and index should be part of the encoding process, they are assigned here instead due to
                // this being a simple one location to assign them.
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
            });

            return colours;
        }

        /// <summary>
        /// Encodes a list of <see cref="TaikoDifficultyHitObject"/>s into a list of <see cref="MonoEncoding"/>s.
        /// </summary>
        public static List<MonoEncoding> EncodeMono(List<DifficultyHitObject> data)
        {
            List<MonoEncoding> encoded = new List<MonoEncoding>();

            MonoEncoding? lastEncoded = null;

            for (int i = 0; i < data.Count; i++)
            {
                TaikoDifficultyHitObject taikoObject = (TaikoDifficultyHitObject)data[i];
                // This ignores all non-note objects, which may or may not be the desired behaviour
                TaikoDifficultyHitObject? previousObject = taikoObject.PreviousNote(0);

                // If the colour changed or if this is the first object in the run, create a new mono encoding
                if
                (
                    previousObject == null || // First object in the list
                    (taikoObject.BaseObject as Hit)?.Type != (previousObject.BaseObject as Hit)?.Type
                )
                {
                    lastEncoded = new MonoEncoding();
                    lastEncoded.EncodedData.Add(taikoObject);
                    encoded.Add(lastEncoded);
                    continue;
                }

                // If we're here, we're in the same encoding as the previous object, thus lastEncoded is not null.
                // Add the current object to the encoded payload.
                lastEncoded!.EncodedData.Add(taikoObject);
            }

            return encoded;
        }

        /// <summary>
        /// Encodes a list of <see cref="MonoEncoding"/>s into a list of <see cref="ColourEncoding"/>s.
        /// </summary>
        public static List<ColourEncoding> EncodeColour(List<MonoEncoding> data)
        {
            List<ColourEncoding> encoded = new List<ColourEncoding>();
            ColourEncoding? lastEncoded = null;

            for (int i = 0; i < data.Count; i++)
            {
                // Starts a new ColourEncoding if the previous MonoEncoding has a different mono length, or if this is
                // the first MonoEncoding in the list.
                if (lastEncoded == null || data[i].RunLength != data[i - 1].RunLength)
                {
                    lastEncoded = new ColourEncoding();
                    lastEncoded.Payload.Add(data[i]);
                    encoded.Add(lastEncoded);
                    continue;
                }

                // If we're here, we're in the same encoding as the previous object. Add the current MonoEncoding to the
                // encoded payload.
                lastEncoded.Payload.Add(data[i]);
            }

            return encoded;
        }

        /// <summary>
        /// Encodes a list of <see cref="ColourEncoding"/>s into a list of <see cref="CoupledColourEncoding"/>s.
        /// </summary>
        public static List<CoupledColourEncoding> EncodeCoupledColour(List<ColourEncoding> data)
        {
            List<CoupledColourEncoding> encoded = new List<CoupledColourEncoding>();
            CoupledColourEncoding? lastEncoded = null;

            for (int i = 0; i < data.Count; i++)
            {
                // Starts a new CoupledColourEncoding. ColourEncodings that should be grouped together will be handled
                // later within this loop.
                lastEncoded = new CoupledColourEncoding
                {
                    Previous = lastEncoded
                };

                // Determine if future ColourEncodings should be grouped.
                bool isCoupled = i < data.Count - 2 && data[i].IsRepetitionOf(data[i + 2]);

                if (!isCoupled)
                {
                    // If not, add the current ColourEncoding to the encoded payload and continue.
                    lastEncoded.Payload.Add(data[i]);
                }
                else
                {
                    // If so, add the current ColourEncoding to the encoded payload and start repeatedly checking if the
                    // subsequent ColourEncodings should be grouped by increasing i and doing the appropriate isCoupled check.
                    while (isCoupled)
                    {
                        lastEncoded.Payload.Add(data[i]);
                        i++;
                        isCoupled = i < data.Count - 2 && data[i].IsRepetitionOf(data[i + 2]);
                    }

                    // Skip over viewed data and add the rest to the payload
                    lastEncoded.Payload.Add(data[i]);
                    lastEncoded.Payload.Add(data[i + 1]);
                    i++;
                }

                encoded.Add(lastEncoded);
            }

            // Final pass to find repetition intervals
            for (int i = 0; i < encoded.Count; i++)
            {
                encoded[i].FindRepetitionInterval();
            }

            return encoded;
        }

        /// <summary>
        /// Encodes a list of <see cref="TaikoDifficultyHitObject"/>s into a list of <see cref="CoupledColourEncoding"/>s.
        /// </summary>
        public static List<CoupledColourEncoding> Encode(List<DifficultyHitObject> data)
        {
            List<MonoEncoding> firstPass = EncodeMono(data);
            List<ColourEncoding> secondPass = EncodeColour(firstPass);
            List<CoupledColourEncoding> thirdPass = EncodeCoupledColour(secondPass);

            return thirdPass;
        }
    }
}
