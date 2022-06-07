using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Taiko.Difficulty.Evaluators
{
    public class ColourEvaluator
    {
        private static double sigmoid(double val, double center, double width)
        {
            return Math.Tanh(Math.E * -(val - center) / width);
        }

        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            TaikoDifficultyHitObject taikoCurrent = (TaikoDifficultyHitObject)current;
            TaikoDifficultyHitObjectColour colour = taikoCurrent.Colour;
            if (colour == null) return 0;
            double objectStrain = 1.6;
            if (colour.Delta)
            {
                objectStrain /= Math.Pow(colour.DeltaRunLength, 0.7);
            }
            else
            {
                objectStrain *= sigmoid(colour.DeltaRunLength, 4, 4) * 0.5 + 0.5;
            }
            objectStrain *= -sigmoid(colour.RepetitionInterval, 8, 8) * 0.5 + 0.5;
            // Console.WriteLine($"{current.StartTime},{colour.GetHashCode()},{colour.Delta},{colour.DeltaRunLength},{colour.RepetitionInterval},{objectStrain}");
            return objectStrain;
        }
    }
}