// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Types;
using System;

namespace osu.Game.Rulesets.Mania.Objects
{
    internal class ManiaHitObjectDifficulty
    {
        /// <summary>
        /// Factor by how much individual / overall strain decays per second.
        /// </summary>
        /// <remarks>
        /// These values are results of tweaking a lot and taking into account general feedback.
        /// </remarks>
        internal const double INDIVIDUAL_DECAY_BASE = 0.125;
        internal const double OVERALL_DECAY_BASE = 0.30;

        internal ManiaHitObject BaseHitObject;

        private readonly int beatmapColumnCount;

        private readonly double endTime;
        private readonly double[] heldUntil;

        /// <summary>
        ///  Measures jacks or more generally: repeated presses of the same button
        /// </summary>
        private readonly double[] individualStrains;

        internal double IndividualStrain
        {
            get
            {
                return individualStrains[BaseHitObject.Column];
            }

            set
            {
                individualStrains[BaseHitObject.Column] = value;
            }
        }

        /// <summary>
        /// Measures note density in a way
        /// </summary>
        internal double OverallStrain = 1;

        public ManiaHitObjectDifficulty(ManiaHitObject baseHitObject, int columnCount)
        {
            BaseHitObject = baseHitObject;

            endTime = (baseHitObject as IHasEndTime)?.EndTime ?? baseHitObject.StartTime;

            beatmapColumnCount = columnCount;
            heldUntil = new double[beatmapColumnCount];
            individualStrains = new double[beatmapColumnCount];

            for (int i = 0; i < beatmapColumnCount; ++i)
            {
                individualStrains[i] = 0;
                heldUntil[i] = 0;
            }
        }

        internal void CalculateStrains(ManiaHitObjectDifficulty previousHitObject, double timeRate)
        {
            // TODO: Factor in holds
            double timeElapsed = (BaseHitObject.StartTime - previousHitObject.BaseHitObject.StartTime) / timeRate;
            double individualDecay = Math.Pow(INDIVIDUAL_DECAY_BASE, timeElapsed / 1000);
            double overallDecay = Math.Pow(OVERALL_DECAY_BASE, timeElapsed / 1000);

            double holdFactor = 1.0; // Factor to all additional strains in case something else is held
            double holdAddition = 0; // Addition to the current note in case it's a hold and has to be released awkwardly

            // Fill up the heldUntil array
            for (int i = 0; i < beatmapColumnCount; ++i)
            {
                heldUntil[i] = previousHitObject.heldUntil[i];

                // If there is at least one other overlapping end or note, then we get an addition, buuuuuut...
                if (BaseHitObject.StartTime < heldUntil[i] && endTime > heldUntil[i])
                {
                    holdAddition = 1.0;
                }

                // ... this addition only is valid if there is _no_ other note with the same ending. Releasing multiple notes at the same time is just as easy as releasing 1
                if (endTime == heldUntil[i])
                {
                    holdAddition = 0;
                }

                // We give a slight bonus to everything if something is held meanwhile
                if (heldUntil[i] > endTime)
                {
                    holdFactor = 1.25;
                }

                // Decay individual strains
                individualStrains[i] = previousHitObject.individualStrains[i] * individualDecay;
            }

            heldUntil[BaseHitObject.Column] = endTime;

            // Increase individual strain in own column
            IndividualStrain += 2.0 * holdFactor;

            OverallStrain = previousHitObject.OverallStrain * overallDecay + (1.0 + holdAddition) * holdFactor;
        }
    }
}
