// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    /// <summary>
    /// Calculates the colour coefficient of taiko difficulty.
    /// </summary>
    public class Colour : StrainDecaySkill
    {
        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 0.4;

        public Colour(Mod[] mods)
            : base(mods)
        {
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            TaikoDifficultyHitObjectColour colour = ((TaikoDifficultyHitObject)current).Colour;
            double difficulty = colour == null ? 0 : colour.EvaluatedDifficulty;
            if (current != null && colour != null)
            {
                ColourEncoding[] payload = colour.Encoding.Payload;
                string payloadDisplay = "";
                for (int i = 0; i < payload.Length; ++i)
                {
                    payloadDisplay += $",({payload[i].MonoRunLength},{payload[i].EncodingRunLength})";
                }

                System.Console.WriteLine($"{current.StartTime},{colour.RepetitionInterval},{colour.Encoding.RunLength}{payloadDisplay}");
            }

            return difficulty;
        }
    }
}
