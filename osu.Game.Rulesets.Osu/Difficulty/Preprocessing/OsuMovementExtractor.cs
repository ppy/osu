// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
using MathNet.Numerics.LinearAlgebra;
using osu.Game.Rulesets.Osu.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Preprocessing
{
    internal static class OsuMovementExtractor
    {
        private static readonly LinearSpline correction_neg2_moving_spline = LinearSpline.InterpolateSorted(
            new[] { -1.0, 1.0 },
            new[] { 1.1, 0 });

        private const double t_ratio_threshold = 1.4;
        private const double correction_neg2_still = 0;

        /// <summary>
        /// Extracts movement (only for the first object in a beatmap).
        /// </summary>
        public static List<OsuMovement> ExtractFirst(OsuHitObject obj)
        {
            var movement = OsuMovement.Empty(obj.StartTime / 1000.0);

            var movementWithNested = new List<OsuMovement> { movement };
            // add zero difficulty movements corresponding to slider ticks/slider ends so combo is reflected properly
            int extraNestedCount = obj.NestedHitObjects.Count - 1;

            for (int i = 0; i < extraNestedCount; i++)
            {
                movementWithNested.Add(OsuMovement.Empty(movement.StartTime));
            }

            return movementWithNested;
        }

        /// <summary>
        /// Calculates the movement time, effective distance and other details for the movement from objPrev to objCurr.
        /// </summary>
        /// <param name="fourthLastObject">Hit object four objects ago, relative to <paramref name="currentObject"/>.</param>
        /// <param name="secondLastObject">Hit object immediately preceding <paramref name="lastObject"/></param>
        /// <param name="lastObject">Hit object immediately preceding <paramref name="currentObject"/>.</param>
        /// <param name="currentObject">The hit object being currently considered.</param>
        /// <param name="nextObject">Hit object immediately succeeding <paramref name="currentObject"/>.</param>
        /// <param name="tapStrain">The tap strain of the current object.</param> TODO: does this have to be passed down?
        /// <param name="noteDensity">The visual note density of the current object.</param> TODO: above
        /// <param name="gameplayRate">The current rate of the gameplay clock.</param>
        /// <param name="hidden">Whether the hidden mod is active.</param>
        /// <returns>List of movements performed in attempt to hit the current object.</returns>
        public static List<OsuMovement> Extract(
            [CanBeNull] OsuHitObject secondLastObject,
            OsuHitObject lastObject,
            OsuHitObject currentObject,
            [CanBeNull] OsuHitObject nextObject,
            Vector<double> tapStrain,
            double gameplayRate,
            bool hidden = false,
            double noteDensity = 0,
            [CanBeNull] OsuHitObject fourthLastObject = null)
        {
            var movement = new OsuMovement();
            var parameters = new MovementExtractionParameters(fourthLastObject, secondLastObject, lastObject, currentObject, nextObject, gameplayRate);

            movement.RawMovementTime = parameters.LastToCurrent.TimeDelta;
            movement.StartTime = currentObject.StartTime / 1000.0;

            if (currentObject is Spinner || lastObject is Spinner)
            {
                movement.Throughput = 0;
                movement.Distance = 0;
                movement.MovementTime = 1;
                movement.Cheesablility = 0;
                movement.CheeseWindow = 0;
                return new List<OsuMovement> { movement };
            }

            movement.EndsOnSlider = currentObject is Slider;

            double movementThroughput = FittsLaw.Throughput(parameters.LastToCurrent.RelativeLength, parameters.LastToCurrent.TimeDelta);

            movement.Throughput = movementThroughput;

            double correctionNeg2 = calculatePreviousObjectCorrection(parameters);
            double correctionNext = calculateNextObjectCorrection(parameters);
            double patternCorrection = calculateFourObjectPatternCorrection(parameters);

            double tapCorrection = calculateTapStrainBuff(tapStrain, parameters.LastToCurrent, movementThroughput);
            calculateCheeseWindow(parameters, movementThroughput);

            double highBpmJumpBuff = calculateHighBPMJumpBuff(parameters);
            double smallCircleBonus = calculateSmallCircleBuff(parameters);
            double dPrevCurrStackedNerf = calculateStackedNoteNerf(parameters.LastToCurrent);
            double smallJumpNerfFactor = calculateSmallJumpNerf(parameters);
            double bigJumpBuffFactor = calculateBigJumpBuff(parameters);
            double correctionHidden = calculateHiddenCorrection(hidden, noteDensity);

            if (isStackedWiggle(parameters))
            {
                correctionNeg2 = 0;
                correctionNext = 0;
                patternCorrection = 0;
                tapCorrection = 0;
            }

            double jumpOverlapCorrection = calculateJumpOverlapCorrection(parameters);
            double distanceIncreaseBuff = calculateDistanceIncreaseBuff(parameters);

            // Apply the corrections
            double dPrevCurrWithCorrection = dPrevCurrStackedNerf * (1 + smallCircleBonus) * (1 + correctionNeg2 + correctionNext + patternCorrection) *
                                             (1 + highBpmJumpBuff) * (1 + tapCorrection) * smallJumpNerfFactor * bigJumpBuffFactor * (1 + correctionHidden) *
                                             jumpOverlapCorrection * distanceIncreaseBuff;

            movement.Distance = dPrevCurrWithCorrection;
            movement.MovementTime = parameters.LastToCurrent.TimeDelta;
            movement.Cheesablility = parameters.Cheesability;
            movement.CheeseWindow = parameters.CheeseWindow;

            var movementWithNested = new List<OsuMovement> { movement };

            // add zero difficulty movements corresponding to slider ticks/slider ends so combo is reflected properly
            int extraNestedCount = currentObject.NestedHitObjects.Count - 1;

            for (int i = 0; i < extraNestedCount; i++)
                movementWithNested.Add(OsuMovement.Empty(movement.StartTime));

            return movementWithNested;
        }

        private static double calculatePreviousObjectCorrection(MovementExtractionParameters p)
        {
            // Correction #1 - The Previous Object
            // Estimate how objNeg2 affects the difficulty of hitting objCurr
            if (p.SecondLastObject == null || p.LastToCurrent.RelativeLength == 0)
                return 0;

            Debug.Assert(p.SecondLastToLast != null);

            double movementLengthRatio = p.LastToCurrent.TimeDelta / p.SecondLastToLast.Value.TimeDelta;
            double movementAngleCosine = cosineOfAngleBetweenPairs(p.SecondLastToLast.Value, p.LastToCurrent);

            if (movementLengthRatio > t_ratio_threshold)
            {
                if (p.SecondLastToLast.Value.RelativeLength == 0)
                    return 0;

                double angleCorrection = correction_neg2_moving_spline.Interpolate(movementAngleCosine);

                double movingness = SpecialFunctions.Logistic(p.SecondLastToLast.Value.RelativeLength * 6 - 5) - SpecialFunctions.Logistic(-5);
                return movingness * angleCorrection * 1.5;
            }

            if (movementLengthRatio < 1 / t_ratio_threshold)
            {
                if (p.SecondLastToLast.Value.RelativeLength == 0)
                    return 0;

                return (1 - movementAngleCosine) * SpecialFunctions.Logistic((p.SecondLastToLast.Value.RelativeLength * movementLengthRatio - 1.5) * 4) * 0.3;
            }

            p.LastObjectCenteredBetweenNeighbours = true;

            // rescale SecondLastToLast so that it's comparable timescale-wise to LastToCurrent
            var timeNormalisedSecondLastToLast = -p.SecondLastToLast.Value.RelativeVector / p.SecondLastToLast.Value.TimeDelta * p.LastToCurrent.TimeDelta;
            // transform secondLastObject to coordinates anchored in lastObject
            double secondLastTransformedX = timeNormalisedSecondLastToLast.DotProduct(p.LastToCurrent.RelativeVector) / p.LastToCurrent.RelativeLength;
            double secondLastTransformedY = (timeNormalisedSecondLastToLast - secondLastTransformedX * p.LastToCurrent.RelativeVector / p.LastToCurrent.RelativeLength).L2Norm();

            double correctionNeg2Flow = AngleCorrection.FLOW_NEG2.Evaluate(p.LastToCurrent.RelativeLength, secondLastTransformedX, secondLastTransformedY);
            double correctionNeg2Snap = AngleCorrection.SNAP_NEG2.Evaluate(p.LastToCurrent.RelativeLength, secondLastTransformedX, secondLastTransformedY);
            double correctionNeg2Stop = SpecialFunctions.Logistic(10 * Math.Sqrt(secondLastTransformedX * secondLastTransformedX + secondLastTransformedY * secondLastTransformedY + 1) - 12);

            p.SecondLastToCurrentFlowiness = SpecialFunctions.Logistic((correctionNeg2Snap - correctionNeg2Flow - 0.05) * 20);

            return PowerMean.Of(new[] { correctionNeg2Flow, correctionNeg2Snap, correctionNeg2Stop }, -10) * 1.3;
        }

        private static double calculateNextObjectCorrection(MovementExtractionParameters p)
        {
            // Correction #2 - The Next Object
            // Estimate how objNext affects the difficulty of hitting objCurr
            if (p.NextObject == null || p.LastToCurrent.RelativeLength == 0)
                return 0;

            Debug.Assert(p.CurrentToNext != null);

            double tRatioNext = p.LastToCurrent.TimeDelta / p.CurrentToNext.Value.TimeDelta;
            double cosPrevCurrNext =
                Math.Min(Math.Max(-p.LastToCurrent.RelativeVector.DotProduct(p.CurrentToNext.Value.RelativeVector) / p.LastToCurrent.RelativeLength / p.CurrentToNext.Value.RelativeLength, -1), 1);

            if (tRatioNext > t_ratio_threshold)
            {
                if (p.CurrentToNext.Value.RelativeLength == 0)
                    return 0;

                double correctionNextMoving = correction_neg2_moving_spline.Interpolate(cosPrevCurrNext);

                double movingness = SpecialFunctions.Logistic(p.CurrentToNext.Value.RelativeLength * 6 - 5) - SpecialFunctions.Logistic(-5);
                return movingness * correctionNextMoving * 0.5;
            }

            if (tRatioNext < 1 / t_ratio_threshold)
            {
                if (p.CurrentToNext.Value.RelativeLength == 0)
                    return 0;

                return (1 - cosPrevCurrNext) * SpecialFunctions.Logistic((p.CurrentToNext.Value.RelativeLength * tRatioNext - 1.5) * 4) * 0.15;
            }

            p.CurrentObjectCenteredBetweenNeighbours = true;

            var normalizedPosNext = p.CurrentToNext.Value.RelativeVector / p.CurrentToNext.Value.TimeDelta * p.LastToCurrent.TimeDelta;
            double xNext = normalizedPosNext.DotProduct(p.LastToCurrent.RelativeVector) / p.LastToCurrent.RelativeLength;
            double yNext = (normalizedPosNext - xNext * p.LastToCurrent.RelativeVector / p.LastToCurrent.RelativeLength).L2Norm();

            double correctionNextFlow = AngleCorrection.FLOW_NEXT.Evaluate(p.LastToCurrent.RelativeLength, xNext, yNext);
            double correctionNextSnap = AngleCorrection.SNAP_NEXT.Evaluate(p.LastToCurrent.RelativeLength, xNext, yNext);

            p.LastToNextFlowiness = SpecialFunctions.Logistic((correctionNextSnap - correctionNextFlow - 0.05) * 20);

            return Math.Max(PowerMean.Of(correctionNextFlow, correctionNextSnap, -10) - 0.1, 0) * 0.5;
        }

        private static double calculateFourObjectPatternCorrection(MovementExtractionParameters p)
        {
            // Correction #3 - 4-object pattern
            // Estimate how the whole pattern consisting of objNeg2 to objNext affects
            // the difficulty of hitting objCurr. This only takes effect when the pattern
            // is not so spaced (i.e. does not contain jumps)
            if (!p.LastObjectCenteredBetweenNeighbours || !p.CurrentObjectCenteredBetweenNeighbours)
                return 0;

            Debug.Assert(p.CurrentToNext != null);
            Debug.Assert(p.SecondLastToLast != null);

            double gap = (p.LastToCurrent.RelativeVector - p.CurrentToNext.Value.RelativeVector / 2 - p.SecondLastToLast.Value.RelativeVector / 2).L2Norm() / (p.LastToCurrent.RelativeLength + 0.1);
            return (SpecialFunctions.Logistic((gap - 1) * 8) - SpecialFunctions.Logistic(-6)) *
                   SpecialFunctions.Logistic((p.SecondLastToLast.Value.RelativeLength - 0.7) * 10) * SpecialFunctions.Logistic((p.CurrentToNext.Value.RelativeLength - 0.7) * 10) *
                   PowerMean.Of(p.SecondLastToCurrentFlowiness, p.LastToNextFlowiness, 2) * 0.6;
        }

        private static double calculateTapStrainBuff(Vector<double> tapStrain, OsuObjectPair lastToCurrent, double movementThroughput)
        {
            // Correction #4 - Tap Strain
            // Estimate how tap strain affects difficulty
            if (!(lastToCurrent.RelativeLength > 0) || tapStrain == null)
                return 0;

            return SpecialFunctions.Logistic((PowerMean.Of(tapStrain, 2) / movementThroughput - 1.34) / 0.1) * 0.15;
        }

        private static void calculateCheeseWindow(MovementExtractionParameters p, double movementThroughput)
        {
            // Correction #5 - Cheesing
            // The player might make the movement of objPrev -> objCurr easier by
            // hitting objPrev early and objCurr late. Here we estimate the amount of
            // cheesing and update MT accordingly.
            double timeEarly = 0;
            double timeLate = 0;
            double cheesabilityEarly = 0;
            double cheesabilityLate = 0;

            if (p.LastToCurrent.RelativeLength > 0)
            {
                double tNeg2PrevReciprocal;
                double ipNeg2Prev;

                if (p.SecondLastObject != null)
                {
                    Debug.Assert(p.SecondLastToLast != null);

                    tNeg2PrevReciprocal = 1 / (p.SecondLastToLast.Value.TimeDelta + 1e-10);
                    ipNeg2Prev = FittsLaw.Throughput(p.SecondLastToLast.Value.RelativeLength, p.SecondLastToLast.Value.TimeDelta);
                }
                else
                {
                    tNeg2PrevReciprocal = 0;
                    ipNeg2Prev = 0;
                }

                cheesabilityEarly = SpecialFunctions.Logistic((ipNeg2Prev / movementThroughput - 0.6) * (-15)) * 0.5;
                timeEarly = cheesabilityEarly * (1 / (1 / (p.LastToCurrent.TimeDelta + 0.07) + tNeg2PrevReciprocal));

                double tCurrNextReciprocal;
                double ipCurrNext;

                if (p.NextObject != null)
                {
                    Debug.Assert(p.CurrentToNext != null);

                    tCurrNextReciprocal = 1 / (p.CurrentToNext.Value.TimeDelta + 1e-10);
                    ipCurrNext = FittsLaw.Throughput(p.CurrentToNext.Value.RelativeLength, p.CurrentToNext.Value.TimeDelta);
                }
                else
                {
                    tCurrNextReciprocal = 0;
                    ipCurrNext = 0;
                }

                cheesabilityLate = SpecialFunctions.Logistic((ipCurrNext / movementThroughput - 0.6) * (-15)) * 0.5;
                timeLate = cheesabilityLate * (1 / (1 / (p.LastToCurrent.TimeDelta + 0.07) + tCurrNextReciprocal));
            }

            p.Cheesability = cheesabilityEarly + cheesabilityLate;
            p.CheeseWindow = (timeEarly + timeLate) / (p.LastToCurrent.TimeDelta + 1e-10);
        }

        private static double calculateHighBPMJumpBuff(MovementExtractionParameters p)
        {
            // Correction #6 - High bpm jump buff (alt buff)
            return SpecialFunctions.Logistic((p.EffectiveBPM - 354) / 16) *
                   SpecialFunctions.Logistic((p.LastToCurrent.RelativeLength - 1.9) / 0.15) * 0.23;
        }

        private static double calculateSmallCircleBuff(MovementExtractionParameters p)
        {
            // Correction #7 - Small circle bonus
            return ((SpecialFunctions.Logistic((55 - 2 * p.CurrentObject.Radius) / 3.0) * 0.3) +
                    (Math.Pow(24.5 - Math.Min(p.CurrentObject.Radius, 24.5), 1.4) * 0.01315)) *
                   Math.Max(SpecialFunctions.Logistic((p.LastToCurrent.RelativeLength - 0.5) / 0.1), 0.25);
        }

        private static double calculateStackedNoteNerf(OsuObjectPair lastToCurrent)
        {
            // Correction #8 - Stacked notes nerf
            return Math.Max(0, Math.Min(lastToCurrent.RelativeLength, Math.Min(1.2 * lastToCurrent.RelativeLength - 0.185, 1.4 * lastToCurrent.RelativeLength - 0.32)));
        }

        private static double calculateSmallJumpNerf(MovementExtractionParameters p)
        {
            // Correction #9 - Slow small jump nerf
            return 1 - 0.17 * Math.Exp(-Math.Pow((p.LastToCurrent.RelativeLength - 2.2) / 0.7, 2)) *
                SpecialFunctions.Logistic((255 - p.EffectiveBPM) / 10);
        }

        private static double calculateBigJumpBuff(MovementExtractionParameters p)
        {
            // Correction #10 - Slow big jump buff
            return 1 + 0.15 * SpecialFunctions.Logistic((p.LastToCurrent.RelativeLength - 6) / 0.5) *
                SpecialFunctions.Logistic((210 - p.EffectiveBPM) / 8);
        }

        private static double calculateHiddenCorrection(bool hidden, double noteDensity)
        {
            // Correction #11 - Hidden Mod
            if (!hidden)
                return 0;

            return 0.05 + 0.008 * noteDensity;
        }

        private static bool isStackedWiggle(MovementExtractionParameters p)
        {
            // Correction #12 - Stacked wiggle fix
            if (p.SecondLastObject == null || p.NextObject == null)
                return false;

            return p.SecondLastToLast?.RelativeLength < 1
                   && p.SecondLastToCurrent?.RelativeLength < 1
                   && p.SecondLastToNext?.RelativeLength < 1
                   && p.LastToCurrent.RelativeLength < 1
                   && p.LastToNext?.RelativeLength < 1
                   && p.CurrentToNext?.RelativeLength < 1;
        }

        private static double calculateJumpOverlapCorrection(MovementExtractionParameters p)
        {
            // Correction #13 - Repetitive jump nerf
            // Nerf big jumps where objNeg2 and objCurr are close or where objNeg4 and objCurr are close
            return 1 - (Math.Max(0.15 - 0.1 * (p.SecondLastToCurrent?.RelativeLength ?? 0), 0) + Math.Max(0.1125 - 0.075 * (p.FourthLastToCurrent?.RelativeLength ?? 0), 0)) *
                SpecialFunctions.Logistic((p.LastToCurrent.RelativeLength - 3.3) / 0.25);
        }

        private static double calculateDistanceIncreaseBuff(MovementExtractionParameters p)
        {
            // Correction #14 - Sudden distance increase buff
            if (p.SecondLastObject == null)
                return 1;

            double dNeg2PrevOverlapNerf = Math.Min(1, Math.Pow(p.SecondLastToLast?.RelativeLength ?? 0, 3));
            double timeDifferenceNerf = Math.Exp(-4
                                                 * Math.Pow(
                                                     1 - Math.Max(p.LastToCurrent.TimeDelta / ((p.SecondLastToLast?.TimeDelta ?? 0) + 1e-10),
                                                         (p.SecondLastToLast?.TimeDelta ?? 0) / (p.LastToCurrent.TimeDelta + 1e-10)), 2));
            double distanceRatio = p.LastToCurrent.RelativeLength / Math.Max(1, p.SecondLastToLast?.RelativeLength ?? 0);
            double bpmScaling = Math.Max(1, -16 * p.LastToCurrent.TimeDelta + 3.4);
            return 1 + 0.225 * bpmScaling * timeDifferenceNerf * dNeg2PrevOverlapNerf * Math.Max(0, distanceRatio - 2);
        }

        private static double cosineOfAngleBetweenPairs(OsuObjectPair first, OsuObjectPair second)
        {
            // it is assumed that first points from object A to B, and second points from B to C
            // therefore to adhere to the dot product formula for cosine, the first vector has to be reversed
            double cosine = -first.RelativeVector.DotProduct(second.RelativeVector) / first.RelativeLength / second.RelativeLength;
            // clamp mostly for sanity.
            return Math.Clamp(cosine, -1, 1);
        }
    }
}
