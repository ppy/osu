// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    public class Stamina : Skill
    {

        private int hand;
        private int noteNumber = 0;

        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 0.4;
        // i only add strain every second note so its kind of like using 0.16

        private readonly int maxHistoryLength = 2;
        private List<double> noteDurationHistory = new List<double>();

        private List<TaikoDifficultyHitObject> lastHitObjects = new List<TaikoDifficultyHitObject>();

        private double offhandObjectDuration = double.MaxValue;

        // Penalty for tl tap or roll
        private double cheesePenalty(double last2NoteDuration)
        {
            if (last2NoteDuration > 125) return 1;
            if (last2NoteDuration < 100) return 0.6;

            return 0.6 + (last2NoteDuration - 100) * 0.016;
        }

        private double speedBonus(double last2NoteDuration)
        {
            // note that we are only looking at every 2nd note, so a 300bpm stream has a note duration of 100ms.
            if (last2NoteDuration >= 200) return 0;
            double bonus = 200 - last2NoteDuration;
            bonus *= bonus;
            return bonus / 100000;
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            noteNumber += 1;

            TaikoDifficultyHitObject currentHO = (TaikoDifficultyHitObject) current;

            if (noteNumber % 2 == hand)
            {
                lastHitObjects.Add(currentHO);
                noteDurationHistory.Add(currentHO.NoteLength + offhandObjectDuration);

                if (noteNumber == 1)
                    return 1;

                if (noteDurationHistory.Count > maxHistoryLength)
                    noteDurationHistory.RemoveAt(0);

                double shortestRecentNote = min(noteDurationHistory);
                double bonus = 0;
                bonus += speedBonus(shortestRecentNote);

                double objectStaminaStrain = 1 + bonus;
                if (currentHO.StaminaCheese) objectStaminaStrain *= cheesePenalty(currentHO.NoteLength + offhandObjectDuration);

                return objectStaminaStrain;
            }

            offhandObjectDuration = currentHO.NoteLength;
            return 0;
        }

        private static double min(List<double> l)
        {
            double minimum = double.MaxValue;

            foreach (double d in l)
            {
                if (d < minimum)
                    minimum = d;
            }
            return minimum;
        }

        public Stamina(bool rightHand)
        {
            hand = 0;
            if (rightHand)
            {
                hand = 1;
            }
        }

    }
}
