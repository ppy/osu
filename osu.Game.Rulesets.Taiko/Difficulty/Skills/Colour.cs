// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    /// <summary>
    /// Calculates the colour coefficient of taiko difficulty.
    /// </summary>
    public class Colour : StrainDecaySkill
    {
        protected override double SkillMultiplier => 0.12;

        // This is set to decay slower than other skills, due to the fact that only the first note of each Mono/Colour/Coupled
        // encoding having any difficulty values, and we want to allow colour difficulty to be able to build up even on
        // slower maps.
        protected override double StrainDecayBase => 0.8;

        public Colour(Mod[] mods)
            : base(mods)
        {
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            double difficulty = ((TaikoDifficultyHitObject)current).Colour?.EvaluatedDifficulty ?? 0;
            return difficulty;
        }

        // TODO: Remove before pr
        public static String GetDebugHeaderLabels()
        {
            return "StartTime,Raw,Decayed,CoupledRunLength,RepetitionInterval,EncodingRunLength,Payload(MonoRunLength|MonoCount)";
        }

        // TODO: Remove before pr
        public string GetDebugString(DifficultyHitObject current)
        {
            double difficulty = ((TaikoDifficultyHitObject)current).Colour?.EvaluatedDifficulty ?? 0;
            TaikoDifficultyHitObject? taikoCurrent = (TaikoDifficultyHitObject)current;
            TaikoDifficultyHitObjectColour? colour = taikoCurrent?.Colour;
            if (taikoCurrent != null && colour != null)
            {
                List<ColourEncoding> payload = colour.Encoding.Payload;
                string payloadDisplay = "";
                for (int i = 0; i < payload.Count; ++i)
                {
                    payloadDisplay += $"({payload[i].Payload[0].RunLength}|{payload[i].Payload.Count})";
                }

                return $"{current.StartTime},{difficulty},{CurrentStrain},{colour.Encoding.Payload[0].Payload.Count},{colour.Encoding.RepetitionInterval},{colour.Encoding.Payload.Count},{payloadDisplay}";
            }
            else
            {
                return $"{current.StartTime},{difficulty},{CurrentStrain},0,0,0,0,0";
            }
        }
    }
}
