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

/* Previous Note States
 * When calculating strain by its history, there's these possible states.
 *
 * The last row of each cell shows the current note
 * the 2nd last              shows the previous note
 *
 * The column describes where the previous note's end time is.
 * E.g. E3 states that the previous note's end time is on the body (of the current note)
 *
 * Invalid/Impossible states are marked with X
 * These states are not possible as their head is AFTER our current offset.
 *
 * E.g. D2 implies that our current note is a Hold. The previous note is a note, on the same offset.
 *
 *                 +-------------+-------------+-------------+-------------+--------------+
 *                 | Before Head | On Head     | On Body     |  On Tail    | After Tail   |
 *                 | (1)         | (2)         | (3)         |  (4)        | (5)          |
 * +---------------+-------------+-------------+-------------+-------------+--------------+
 * | Note      (A) | (A1)        | (A2)        |             |             |              |
 * | Before        | O           |      O      |      X      |      X      |      X       |
 * | Note          |      O      |      O      |             |             |              |
 * +---------------+-------------+-------------+-------------+-------------+--------------+
 * | Long Note (B) | (B1)        | (B2)        |             |             | (B5)         |
 * | Before        | [==]        | [====]      |      X      |      X      | [==========] |
 * | Note          |      O      |      O      |             |             |      O       |
 * +---------------+-------------+-------------+-------------+-------------+--------------+
 * | Long Note (C) |             |             |             |             | (C5)         |
 * | Before        |      X      |      x      |      X      |      X      |      [===]   |
 * | Note          |             |             |             |             |      O       |
 * +---------------+-------------+-------------+-------------+-------------+--------------+
 * | Note      (D) | (D1)        | (D2)        |             |             |              |
 * | Before        | O           |      O      |      X      |      X      |      X       |
 * | Long Note     |      [===]  |      [===]  |             |             |              |
 * +---------------+-------------+-------------+-------------+-------------+--------------+
 * | Long Note (E) | (E1)        | (E2)        | (E3)        | (E4)        | (E5)         |
 * | Before        | [==]        | [====]      | [======]    | [=========] | [==========] |
 * | Long Note     |      [===]  |      [===]  |      [===]  |       [===] |      [===]   |
 * +---------------+-------------+-------------+-------------+-------------+--------------+
 * | Long Note (F) |             |             | (F3)        | (F4)        | (F5)         |
 * | Before        |      X      |      X      |      [=]    |       [===] |      [=====] |
 * | Long Note     |             |             |      [===]  |       [===] |      [===]   |
 * +---------------+-------------+-------------+-------------+-------------+--------------+
 *
 */

namespace osu.Game.Rulesets.Mania.Difficulty.Skills
{
    public class Strain : StrainDecaySkill
    {
        private const double decay_base = 0.125;
        private const double global_decay_base = 0.30;
        private const double release_threshold = 24;

        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 1;

        private readonly double[] prevStartTimes;
        private readonly double[] prevEndTimes;
        private readonly double[] prevStrains;

        private double prevStrain;
        private double globalStrain;

        public Strain(Mod[] mods, int totalColumns)
            : base(mods)
        {
            prevStartTimes = new double[totalColumns];
            prevEndTimes = new double[totalColumns];
            prevStrains = new double[totalColumns];
            globalStrain = 1;
        }

        /// <summary>
        /// Calculates the strain value of a <see cref="DifficultyHitObject"/>. This value is affected by previously processed objects.
        ///
        /// Also note that the first hitObject is not considered in the calculation:
        /// <see cref="ManiaDifficultyCalculator.CreateDifficultyHitObjects"/>
        ///
        /// </summary>
        protected override double StrainValueOf(DifficultyHitObject current)
        {
            var hitObject = (ManiaDifficultyHitObject)current;

            // Given a note, startTime == endTime.
            double startTime = hitObject.StartTime;
            double endTime = hitObject.EndTime;
            int column = hitObject.BaseObject.Column;

            double holdLength = Math.Abs(endTime - startTime);
            double endOnBodyBias = 0; // Addition to the current note in case it's a hold and has to be released awkwardly
            double endAfterTailWeight = 1.0; // Factor to all additional strains in case something else is held

            // The closest end time, currently, is the current note's end time, which is its length
            double closestEndTime = holdLength;

            bool isEndOnBody = false;
            bool isEndAfterTail = false;

            for (int i = 0; i < prevEndTimes.Length; ++i)
            {
                /* True for Column 3 Scenarios:
                 *      Criterion 1 accepts Col 3-5
                 *      Criterion 2 accepts Col 1, D2:F3,
                 *      Thus, AND accepts Col 3 Only.
                 */
                isEndOnBody |= Precision.DefinitelyBigger(prevEndTimes[i], startTime, 1) &&
                               Precision.DefinitelyBigger(endTime, prevEndTimes[i], 1);

                // True for Column 5 Scenarios
                isEndAfterTail |= Precision.DefinitelyBigger(prevEndTimes[i], endTime, 1);

                // Update closest end time by looking through previous LNs
                closestEndTime = Math.Min(closestEndTime, Math.Abs(endTime - prevEndTimes[i]));
            }

            /* Give Hold Addition for Scenario Column 3.
             * Releasing multiple notes is as easy as releasing one.
             * Halves hold addition if closest release is release_threshold away.
             *
             * End on Body Bias
             *     ^
             * 1.0 + - - - - - -+-----------
             *     |           /
             * 0.5 + - - - - -/   Sigmoid Curve
             *     |         /|
             * 0.0 +--------+-+---------------> Release Difference / ms
             *         release_threshold
             */
            if (isEndOnBody)
                endOnBodyBias = 1 / (1 + Math.Exp(0.5 * (release_threshold - closestEndTime)));

            // Bonus for Holds that end after our tail.
            // We give a slight bonus to everything if something is held meanwhile
            if (isEndAfterTail)
                endAfterTailWeight = 1.25;

            // Decay previous column strain by the column timeDelta
            prevStrains[column] = applyDecay(prevStrains[column], startTime - prevStartTimes[column], decay_base);
            prevStrains[column] += 2.0 * endAfterTailWeight;

            // For notes at the same time (in a chord), the strain should be the hardest strain out of those columns
            // This works by checking if
            double strain = hitObject.DeltaTime <= 1 ? Math.Max(prevStrain, prevStrains[column]) : prevStrains[column];

            // Decay and increase overallStrain
            globalStrain = applyDecay(globalStrain, current.DeltaTime, global_decay_base);
            globalStrain += (1 + endOnBodyBias) * endAfterTailWeight;

            // Update startTimes and endTimes arrays
            prevStartTimes[column] = startTime;
            prevEndTimes[column] = endTime;
            prevStrain = strain;

            // By subtracting CurrentStrain, this skill effectively only considers the maximum strain of any one hitobject within each strain section.
            return strain + globalStrain - CurrentStrain;
        }

        protected override double CalculateInitialStrain(double offset, DifficultyHitObject current)
            => applyDecay(prevStrain, offset - current.Previous(0).StartTime, decay_base)
               + applyDecay(globalStrain, offset - current.Previous(0).StartTime, global_decay_base);

        private double applyDecay(double value, double deltaTime, double decayBase)
            => value * Math.Pow(decayBase, deltaTime / 1000);
    }
}
