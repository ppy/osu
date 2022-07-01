using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Taiko.Difficulty.Evaluators
{
    public class ColourEvaluator
    {
        // TODO - Share this sigmoid
        private static double sigmoid(double val, double center, double width)
        {
            return Math.Tanh(Math.E * -(val - center) / width);
        }

        public static double EvaluateDifficultyOf(ColourEncoding encoding)
        {
            return 1 / Math.Pow(encoding.MonoRunLength, 0.5);
        }

        public static double EvaluateDifficultyOf(CoupledColourEncoding coupledEncoding)
        {
            double difficulty = 0;
            for (int i = 0; i < coupledEncoding.Payload.Length; i++)
            {
                difficulty += EvaluateDifficultyOf(coupledEncoding.Payload[i]);
            }
            return difficulty;
        }

        public static double EvaluateDifficultyOf(TaikoDifficultyHitObjectColour? colour)
        {
            if (colour == null) return 0;

            // double difficulty = 9.5 * Math.Log(colour.Encoding.Payload.Length + 1, 10);
            double difficulty = 3 * EvaluateDifficultyOf(colour.Encoding);
            // foreach (ColourEncoding encoding in colour.Encoding.Payload)
            // {
            //     difficulty += sigmoid(encoding.MonoRunLength, 1, 1) * 0.4 + 0.6;
            // }
            // difficulty *= -sigmoid(colour.RepetitionInterval, 1, 7);
            difficulty *= -sigmoid(colour.RepetitionInterval, 6, 5) * 0.5 + 0.5;

            return difficulty;
        }

        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            return EvaluateDifficultyOf(((TaikoDifficultyHitObject)current).Colour);
        }
    }
}
