// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Evaluators;
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
        /// Process and encode a list of <see cref="TaikoDifficultyHitObject"/>s into a list of <see cref="TaikoDifficultyHitObjectColour"/>s,
        /// assign the appropriate <see cref="TaikoDifficultyHitObjectColour"/>s to each <see cref="TaikoDifficultyHitObject"/>,
        /// and preevaluate colour difficulty of each <see cref="TaikoDifficultyHitObject"/>.
        /// </summary>
        public static List<TaikoDifficultyHitObjectColour> ProcessAndAssign(List<DifficultyHitObject> hitObjects)
        {
            List<TaikoDifficultyHitObjectColour> colours = new List<TaikoDifficultyHitObjectColour>();
            List<CoupledColourEncoding> encodings = Encode(hitObjects);

            // Assign colour to objects
            encodings.ForEach(coupledEncoding =>
            {
                coupledEncoding.Payload.ForEach(encoding =>
                {
                    encoding.Payload.ForEach(mono =>
                    {
                        mono.EncodedData.ForEach(hitObject =>
                        {
                            hitObject.Colour = new TaikoDifficultyHitObjectColour(coupledEncoding);
                        });
                    });
                });

                // Preevaluate and assign difficulty values
                ColourEvaluator.PreEvaluateDifficulties(coupledEncoding);
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

                // If the colour changed, or if this is the first object in the run, create a new mono encoding
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

                // If we're here, we're in the same encoding as the previous object, thus lastEncoded is not null. Add
                // the current object to the encoded payload.
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
        /// Encodes a list of <see cref="TaikoDifficultyHitObject"/>s into a list of <see cref="CoupledColourEncoding"/>s.
        /// </summary>
        public static List<CoupledColourEncoding> Encode(List<DifficultyHitObject> data)
        {
            List<MonoEncoding> firstPass = EncodeMono(data);
            List<ColourEncoding> secondPass = EncodeColour(firstPass);
            List<CoupledColourEncoding> thirdPass = EncodeCoupledColour(secondPass);

            return thirdPass;
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

                    // Skip over peeked data and add the rest to the payload
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
    }
}
