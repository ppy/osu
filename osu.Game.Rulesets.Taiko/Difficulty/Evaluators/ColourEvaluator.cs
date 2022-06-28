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

        public static double EvaluateDifficultyOf(TaikoDifficultyHitObjectColour? colour)
        {
            if (colour == null) return 0;

            double difficulty = 7.5 * Math.Log(colour.Encoding.Payload.Length + 1, 10);
            // foreach (ColourEncoding encoding in colour.Encoding.Payload)
            // {
            //     difficulty += sigmoid(encoding.MonoRunLength, 1, 1) * 0.4 + 0.6;
            // }
            difficulty *= -sigmoid(colour.RepetitionInterval, 1, 7);
            // difficulty *= -sigmoid(colour.RepetitionInterval, 2, 2) * 0.5 + 0.5;

            return difficulty;
        }

        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            return EvaluateDifficultyOf(((TaikoDifficultyHitObject)current).Colour);
        }
    }
}
