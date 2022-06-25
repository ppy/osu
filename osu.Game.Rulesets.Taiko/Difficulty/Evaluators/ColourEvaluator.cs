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

        public static double EvaluateDifficultyOf(TaikoDifficultyHitObjectColour colour)
        {
            return 1;
        }

        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            return EvaluateDifficultyOf(((TaikoDifficultyHitObject)current).Colour);
        }
    }
}
