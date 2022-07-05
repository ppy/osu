using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Evaluators;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour
{
    /// <summary>
    /// Stores colour compression information for a <see cref="TaikoDifficultyHitObject"/>. This is only present for the
    /// first <see cref="TaikoDifficultyHitObject"/> in a <see cref="CoupledColourEncoding"/> chunk.
    /// </summary>
    public class TaikoDifficultyHitObjectColour
    {
        public CoupledColourEncoding Encoding { get; private set; }

        public double EvaluatedDifficulty = 0;

        private TaikoDifficultyHitObjectColour(CoupledColourEncoding encoding)
        {
            Encoding = encoding;
        }

        // TODO: Might wanna move this somewhere else as it is introducing circular references
        public static List<TaikoDifficultyHitObjectColour> EncodeAndAssign(List<DifficultyHitObject> hitObjects)
        {
            List<TaikoDifficultyHitObjectColour> colours = new List<TaikoDifficultyHitObjectColour>();
            List<CoupledColourEncoding> encodings = CoupledColourEncoding.Encode(hitObjects);

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
    }
}