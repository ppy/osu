// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Utils;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;

/* The calculation of strain is sequential, following rules:
 * 1) The current strain value can only depend on past notes
 * 2) Notes on the same offset may see different strain values, as column order sorting is not guaranteed.
 * 3) The first hit object is omitted as it acts as a reference for the following note.
 *
 * E.g. A:(100ms @ Col 1), B:(200ms @ Col 1), C:(200ms @ Col 2)
 * The following sequences are possible: B C, C B.
 * A is omitted because of rule 3.
 */
namespace osu.Game.Rulesets.Mania.Difficulty.Skills
{
    public class Strain : StrainDecaySkill
    {
        private const double individual_decay_base = 0.125;
        private const double overall_decay_base = 0.30;
        private const double release_threshold = 24;

        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 1;

        private readonly double[] startTimes;
        private readonly double[] previousEndTimes;
        private readonly double[] individualStrains;

        private double individualStrain;
        private double overallStrain;

        public Strain(Mod[] mods, int totalColumns)
            : base(mods)
        {
            startTimes = new double[totalColumns];
            previousEndTimes = new double[totalColumns];
            individualStrains = new double[totalColumns];
            overallStrain = 1;
        }

        /// <summary>
        /// Calculates the strain value of a <see cref="DifficultyHitObject"/>. This value is affected by previously processed objects.
        /// </summary>
        protected override double StrainValueOf(DifficultyHitObject current)
        {
            var hitObject = (ManiaDifficultyHitObject)current;

            // Given a note, startTime == endTime.
            double startTime = hitObject.StartTime;
            double endTime = hitObject.EndTime;
            int column = hitObject.BaseObject.Column;

            double holdLength = Math.Abs(endTime - startTime);
            double holdWeight = 1.0; // Factor to all additional strains in case something else is held
            double holdAddition = 0; // Addition to the current note in case it's a hold and has to be released awkwardly

            // The closest end time, currently, is the current note's end time, which is its length
            double closestEndTime = holdLength;

            bool isOverlapping = false;

            for (int i = 0; i < previousEndTimes.Length; ++i)
            {
                /* Possible scenarios if the previous note is an LN:
                 *
                 *                 +--------------+--------------+-------------+---------------+--------------+
                 *                 | Before Head  | On Head      | On Body     |  On Tail      | After Tail   |
                 *                 | (1)          | (2)          | (3)         |  (4)          | (5)          |
                 * +---------------+--------------+--------------+-------------+---------------+--------------+
                 * | Note      (A) | (A1)         | (A2)         |             |               | (A5 Inval.)  |
                 * | Before        | O            |      O       |      X      |       X       |           O  |
                 * | Note          |      O       |      O       |             |               |      O       |
                 * +---------------+--------------+--------------+-------------+---------------+--------------+
                 * | Long Note (B) | (B1)         | (B2)         |             |               | (B5)         |
                 * | Before        | [==]         | [====]       |      X      |       X       | [==========] |
                 * | Note          |      O       |      O       |             |               |      O       |
                 * +---------------+--------------+--------------+-------------+---------------+--------------+
                 * | Note      (C) | (C1)         | (C2)         | (C3)        |  (C4)         | (C5 Inval.)  |
                 * | Before        | O            |      O       |        O    |           O   |            O |
                 * | Long Note     |      [===]   |      [===]   |      [===]  |       [===]   |      [===]   |
                 * +---------------+--------------+--------------+-------------+---------------+--------------+
                 * | Long Note (D) | (D1)         | (D2)         | (D3)        |  (D4)         | (D5)         |
                 * | Before        | [==]         | [====]       | [======]    |  [========]   | [==========] |
                 * | Long Note     |      [===]   |      [===]   |      [===]  |       [===]   |      [===]   |
                 * +---------------+--------------+--------------+-------------+---------------+--------------+
                 *
                 * Invalid states:
                 * A5 and C5 are NOT possible as we sequentially, in time, process notes.
                 * Thus, no HitObject can be in previousEndTimes, if their head is AFTER the current note
                 */

                // IsOverlapping considers scenarios C3:D3:
                //      Criterion 1 accepts A3:D5
                //      Criteiron 2 accepts A1:D1, C2:D3,
                //      Thus, AND accepts C3:D3 only
                isOverlapping |= Precision.DefinitelyBigger(previousEndTimes[i], startTime, 1) &&
                                 Precision.DefinitelyBigger(endTime, previousEndTimes[i], 1);

                // We give a slight bonus to everything if something is held meanwhile
                // This considers the scenarios A5:D5
                if (Precision.DefinitelyBigger(previousEndTimes[i], endTime, 1))
                    holdWeight = 1.25;

                // Update closest end time by looking through previous LNs
                closestEndTime = Math.Min(closestEndTime, Math.Abs(endTime - previousEndTimes[i]));
            }

            // The hold addition is given if there was an overlap, however it is only valid if there are no other note with a similar ending.
            // Releasing multiple notes is just as easy as releasing 1. Nerfs the hold addition by half if the closest release is release_threshold away.
            // holdAddition
            //     ^
            // 1.0 + - - - - - -+-----------
            //     |           /
            // 0.5 + - - - - -/   Sigmoid Curve
            //     |         /|
            // 0.0 +--------+-+---------------> Release Difference / ms
            //         release_threshold
            if (isOverlapping)
                holdAddition = 1 / (1 + Math.Exp(0.5 * (release_threshold - closestEndTime)));

            // Decay and increase individualStrains in own column
            individualStrains[column] = applyDecay(individualStrains[column], startTime - startTimes[column], individual_decay_base);
            individualStrains[column] += 2.0 * holdWeight;

            // For notes at the same time (in a chord), the individualStrain should be the hardest individualStrain out of those columns
            individualStrain = hitObject.DeltaTime <= 1 ? Math.Max(individualStrain, individualStrains[column]) : individualStrains[column];

            // Decay and increase overallStrain
            overallStrain = applyDecay(overallStrain, current.DeltaTime, overall_decay_base);
            overallStrain += (1 + holdAddition) * holdWeight;

            // Update startTimes and endTimes arrays
            startTimes[column] = startTime;
            previousEndTimes[column] = endTime;

            // By subtracting CurrentStrain, this skill effectively only considers the maximum strain of any one hitobject within each strain section.
            return individualStrain + overallStrain - CurrentStrain;
        }

        protected override double CalculateInitialStrain(double offset, DifficultyHitObject current)
            => applyDecay(individualStrain, offset - current.Previous(0).StartTime, individual_decay_base)
               + applyDecay(overallStrain, offset - current.Previous(0).StartTime, overall_decay_base);

        private double applyDecay(double value, double deltaTime, double decayBase)
            => value * Math.Pow(decayBase, deltaTime / 1000);
    }
}
