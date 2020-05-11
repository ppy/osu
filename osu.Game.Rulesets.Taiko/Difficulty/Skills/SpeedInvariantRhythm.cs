// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    public class Rhythm : Skill
    {
        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 0;
        private const double strain_decay = 0.96;
        private double currentStrain;

        private readonly List<TaikoDifficultyHitObject> ratioObjectHistory = new List<TaikoDifficultyHitObject>();
        private int ratioHistoryLength;
        private const int ratio_history_max_length = 8;

        private int rhythmLength;

        // Penalty for repeated sequences of rhythm changes
        private double repititionPenalty(double timeSinceRepititionMS)
        {
            double t = Math.Atan(timeSinceRepititionMS / 3000) / (Math.PI / 2);
            return t;
        }

        private double repititionPenalty(int notesSince)
        {
            double t = notesSince * 150;
            t = Math.Atan(t / 3000) / (Math.PI / 2);
            return t;
        }

        // Penalty for short patterns
        // Must be low to buff maps like wizodmiot
        // Must not be too low for maps like inverse world
        private double patternLengthPenalty(int patternLength)
        {
            double shortPatternPenalty = Math.Min(0.15 * patternLength, 1.0);
            double longPatternPenalty = Math.Max(Math.Min(2.5 - 0.15 * patternLength, 1.0), 0.0);
            return Math.Min(shortPatternPenalty, longPatternPenalty);
        }

        // Penalty for notes so slow that alting is not necessary.
        private double speedPenalty(double noteLengthMS)
        {
            if (noteLengthMS < 80) return 1;
            if (noteLengthMS < 160) return Math.Max(0, 1.4 - 0.005 * noteLengthMS);
            if (noteLengthMS < 300) return 0.6;
            return 0.0;
        }

        // Penalty for the first rhythm change in a pattern
        private const double first_burst_penalty = 0.1;
        private bool prevIsSpeedup = true;

        protected override double StrainValueOf(DifficultyHitObject dho)
        {
            currentStrain *= strain_decay;

            TaikoDifficultyHitObject currentHO = (TaikoDifficultyHitObject)dho;
            rhythmLength += 1;

            if (!currentHO.HasTimingChange)
            {
                return 0.0;
            }

            double objectDifficulty = currentHO.Rhythm.Difficulty;

            // find repeated ratios

            ratioObjectHistory.Add(currentHO);
            ratioHistoryLength += 1;

            if (ratioHistoryLength > ratio_history_max_length)
            {
                ratioObjectHistory.RemoveAt(0);
                ratioHistoryLength -= 1;
            }

            for (int l = 2; l <= ratio_history_max_length / 2; l++)
            {
                for (int start = ratioHistoryLength - l - 1; start >= 0; start--)
                {
                    bool samePattern = true;

                    for (int i = 0; i < l; i++)
                    {
                        if (ratioObjectHistory[start + i].RhythmID != ratioObjectHistory[ratioHistoryLength - l + i].RhythmID)
                        {
                            samePattern = false;
                        }
                    }

                    if (samePattern) // Repitition found!
                    {
                        int notesSince = currentHO.n - ratioObjectHistory[start].n;
                        objectDifficulty *= repititionPenalty(notesSince);
                        break;
                    }
                }
            }

            if (currentHO.Rhythm.IsSpeedup())
            {
                objectDifficulty *= 1;
                if (currentHO.Rhythm.IsLargeSpeedup()) objectDifficulty *= 1;
                if (prevIsSpeedup) objectDifficulty *= 1;

                prevIsSpeedup = true;
            }
            else
            {
                prevIsSpeedup = false;
            }

            objectDifficulty *= patternLengthPenalty(rhythmLength);
            objectDifficulty *= speedPenalty(currentHO.NoteLength);

            rhythmLength = 0;

            currentStrain += objectDifficulty;
            return currentStrain;
        }
    }
}
