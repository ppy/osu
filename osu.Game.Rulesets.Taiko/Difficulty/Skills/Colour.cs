// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    public class Colour : Skill
    {
        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 0.4;

        private bool prevIsKat = false;

        private int currentMonoLength = 1;
        private List<int> monoHistory = new List<int>();
        private readonly int mono_history_max_length = 5;
        private int monoHistoryLength = 0;

        private double sameParityPenalty()
        {
            return 0.0;
        }

        private double repititionPenalty(int notesSince)
        {
            double d = notesSince;
            return Math.Atan(d / 30) / (Math.PI / 2);
        }

        private double patternLengthPenalty(int patternLength)
        {
            double shortPatternPenalty = Math.Min(0.25 * patternLength, 1.0);
            double longPatternPenalty = Math.Max(Math.Min(2.5 - 0.15 * patternLength, 1.0), 0.0);
            return Math.Min(shortPatternPenalty, longPatternPenalty);
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            double objectDifficulty = 0.0;

            if (current.LastObject is Hit && current.BaseObject is Hit && current.DeltaTime < 1000)
            {

                TaikoDifficultyHitObject currentHO = (TaikoDifficultyHitObject)current;

                if (currentHO.IsKat == prevIsKat)
                {
                    currentMonoLength += 1;
                }

                else
                {

                    objectDifficulty = 1.0;

                    if (monoHistoryLength > 0 && (monoHistory[monoHistoryLength - 1] + currentMonoLength) % 2 == 0)
                    {
                        objectDifficulty *= sameParityPenalty();
                    }

                    monoHistory.Add(currentMonoLength);
                    monoHistoryLength += 1;

                    if (monoHistoryLength > mono_history_max_length)
                    {
                        monoHistory.RemoveAt(0);
                        monoHistoryLength -= 1;
                    }

                    for (int l = 2; l <= mono_history_max_length / 2; l++)
                    {
                        for (int start = monoHistoryLength - l - 1; start >= 0; start--)
                        {
                            bool samePattern = true;

                            for (int i = 0; i < l; i++)
                            {
                                if (monoHistory[start + i] != monoHistory[monoHistoryLength - l + i])
                                {
                                    samePattern = false;
                                }
                            }

                            if (samePattern) // Repitition found!
                            {
                                int notesSince = 0;
                                for (int i = start; i < monoHistoryLength; i++) notesSince += monoHistory[i];
                                objectDifficulty *= repititionPenalty(notesSince);
                                break;
                            }
                        }
                    }

                    currentMonoLength = 1;
                    prevIsKat = currentHO.IsKat;

                }

            }

            /*
            string path = @"out.txt";
            using (StreamWriter sw = File.AppendText(path))
            {
                if (((TaikoDifficultyHitObject)current).IsKat) sw.WriteLine("k    " + Math.Min(1.25, returnVal) * returnMultiplier);
                else sw.WriteLine("d    " + Math.Min(1.25, returnVal) * returnMultiplier);
            }
            */

            return objectDifficulty;
        }

    }
}
