// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            OsuHitObject secondLastObject,
            OsuHitObject lastObject,
            OsuHitObject currentObject,
            OsuHitObject nextObject,
            Vector<double> tapStrain,
            double gameplayRate,
            bool hidden = false,
            double noteDensity = 0,
            OsuHitObject fourthLastObject = null)
        {
            var movement = new OsuMovement();

            OsuObjectPair? fourthLastToCurrent = OsuObjectPair.Nullable(fourthLastObject, currentObject, gameplayRate);

            OsuObjectPair? secondLastToLast = OsuObjectPair.Nullable(secondLastObject, lastObject, gameplayRate);
            OsuObjectPair? secondLastToCurrent = OsuObjectPair.Nullable(secondLastObject, currentObject, gameplayRate);
            OsuObjectPair? secondLastToNext = OsuObjectPair.Nullable(secondLastObject, nextObject, gameplayRate);

            OsuObjectPair lastToCurrent = new OsuObjectPair(lastObject, currentObject, gameplayRate);
            OsuObjectPair? lastToNext = OsuObjectPair.Nullable(lastObject, nextObject, gameplayRate);

            OsuObjectPair? currentToNext = OsuObjectPair.Nullable(currentObject, nextObject, gameplayRate);

            movement.RawMovementTime = lastToCurrent.TimeDelta;
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

            if (secondLastObject is Spinner)
                secondLastObject = null;

            if (nextObject is Spinner)
                nextObject = null;

            movement.EndsOnSlider = currentObject is Slider;

            double movementThroughput = FittsLaw.Throughput(lastToCurrent.RelativeLength, lastToCurrent.TimeDelta);

            movement.Throughput = movementThroughput;

            double flowinessNeg2PrevCurr = 0;
            double flowinessPrevCurrNext = 0;

            bool previousTimeCenteredRelativeToNeighbours = false;
            bool currentTimeCenteredRelativeToNeighbours = false;

            var correctionNeg2 = calculatePreviousObjectCorrection(secondLastToLast, lastToCurrent, ref previousTimeCenteredRelativeToNeighbours, ref flowinessNeg2PrevCurr);
            var correctionNext = calculateNextObjectCorrection(currentToNext, lastToCurrent, ref currentTimeCenteredRelativeToNeighbours, ref flowinessPrevCurrNext);
            double patternCorrection = calculateFourObjectPatternCorrection(previousTimeCenteredRelativeToNeighbours, currentTimeCenteredRelativeToNeighbours, lastToCurrent, currentToNext, secondLastToLast, flowinessNeg2PrevCurr, flowinessPrevCurrNext);

            var tapCorrection = calculateTapStrainBuff(tapStrain, lastToCurrent, movementThroughput);
            double timeEarly = calculateCheeseWindow(lastToCurrent, secondLastToLast, movementThroughput, currentToNext, out var timeLate, out var cheesabilityEarly, out var cheesabilityLate);

            double effectiveBpm = 30 / (lastToCurrent.TimeDelta + 1e-10);

            var highBpmJumpBuff = calculateHighBPMJumpBuff(effectiveBpm, lastToCurrent);
            double smallCircleBonus = calculateSmallCircleBuff(currentObject, lastToCurrent);
            double dPrevCurrStackedNerf = calculateStackedNoteNerf(lastToCurrent);
            double smallJumpNerfFactor = calculateSmallJumpNerf(lastToCurrent, effectiveBpm);
            double bigJumpBuffFactor = calculateBigJumpBuff(lastToCurrent, effectiveBpm);
            double correctionHidden = calculateHiddenCorrection(hidden, noteDensity);

            // Correction #12 - Stacked wiggle fix
            if (secondLastObject != null && nextObject != null)
            {
                var dPrevNext = lastToNext.Value.RelativeLength;
                var dNeg2Next = secondLastToNext.Value.RelativeLength;

                if (secondLastToLast.Value.RelativeLength < 1
                    && secondLastToCurrent.Value.RelativeLength < 1
                    && dNeg2Next < 1
                    && lastToCurrent.RelativeLength < 1
                    && dPrevNext < 1
                    && currentToNext.Value.RelativeLength < 1)
                {
                    correctionNeg2 = 0;
                    correctionNext = 0;
                    patternCorrection = 0;
                    tapCorrection = 0;
                }
            }

            double jumpOverlapCorrection = calculateJumpOverlapCorrection(secondLastToCurrent, fourthLastToCurrent, lastToCurrent);
            double distanceIncreaseBuff = calculateDistanceIncreaseBuff(secondLastObject, secondLastToLast, lastToCurrent);

            // Apply the corrections
            double dPrevCurrWithCorrection = dPrevCurrStackedNerf * (1 + smallCircleBonus) * (1 + correctionNeg2 + correctionNext + patternCorrection) *
                                             (1 + highBpmJumpBuff) * (1 + tapCorrection) * smallJumpNerfFactor * bigJumpBuffFactor * (1 + correctionHidden) *
                                             jumpOverlapCorrection * distanceIncreaseBuff;

            movement.Distance = dPrevCurrWithCorrection;
            movement.MovementTime = lastToCurrent.TimeDelta;
            movement.Cheesablility = cheesabilityEarly + cheesabilityLate;
            movement.CheeseWindow = (timeEarly + timeLate) / (lastToCurrent.TimeDelta + 1e-10);

            var movementWithNested = new List<OsuMovement> { movement };

            // add zero difficulty movements corresponding to slider ticks/slider ends so combo is reflected properly
            int extraNestedCount = currentObject.NestedHitObjects.Count - 1;

            for (int i = 0; i < extraNestedCount; i++)
            {
                movementWithNested.Add(OsuMovement.Empty(movement.StartTime));
            }

            return movementWithNested;
        }

        private static double calculatePreviousObjectCorrection(OsuObjectPair? secondLastToLast, OsuObjectPair lastToCurrent, ref bool previousTimeCenteredRelativeToNeighbours,
                                                                ref double flowinessNeg2PrevCurr)
        {
            // Correction #1 - The Previous Object
            // Estimate how objNeg2 affects the difficulty of hitting objCurr
            double correctionNeg2 = 0;

            if (secondLastToLast != null && lastToCurrent.RelativeLength != 0)
            {
                double tRatioNeg2 = lastToCurrent.TimeDelta / secondLastToLast.Value.TimeDelta;
                double cosNeg2PrevCurr =
                    Math.Min(Math.Max(-secondLastToLast.Value.RelativeVector.DotProduct(lastToCurrent.RelativeVector) / secondLastToLast.Value.RelativeLength / lastToCurrent.RelativeLength, -1), 1);

                if (tRatioNeg2 > t_ratio_threshold)
                {
                    if (secondLastToLast.Value.RelativeLength == 0)
                    {
                        correctionNeg2 = correction_neg2_still;
                    }
                    else
                    {
                        double correctionNeg2Moving = correction_neg2_moving_spline.Interpolate(cosNeg2PrevCurr);

                        double movingness = SpecialFunctions.Logistic(secondLastToLast.Value.RelativeLength * 6 - 5) - SpecialFunctions.Logistic(-5);
                        correctionNeg2 = (movingness * correctionNeg2Moving + (1 - movingness) * correction_neg2_still) * 1.5;
                    }
                }
                else if (tRatioNeg2 < 1 / t_ratio_threshold)
                {
                    if (secondLastToLast.Value.RelativeLength == 0)
                    {
                        correctionNeg2 = 0;
                    }
                    else
                    {
                        correctionNeg2 = (1 - cosNeg2PrevCurr) * SpecialFunctions.Logistic((secondLastToLast.Value.RelativeLength * tRatioNeg2 - 1.5) * 4) * 0.3;
                    }
                }
                else
                {
                    previousTimeCenteredRelativeToNeighbours = true;

                    var normalizedPosNeg2 = -secondLastToLast.Value.RelativeVector / secondLastToLast.Value.TimeDelta * lastToCurrent.TimeDelta;
                    double xNeg2 = normalizedPosNeg2.DotProduct(lastToCurrent.RelativeVector) / lastToCurrent.RelativeLength;
                    double yNeg2 = (normalizedPosNeg2 - xNeg2 * lastToCurrent.RelativeVector / lastToCurrent.RelativeLength).L2Norm();

                    double correctionNeg2Flow = AngleCorrection.FLOW_NEG2.Evaluate(lastToCurrent.RelativeLength, xNeg2, yNeg2);
                    double correctionNeg2Snap = AngleCorrection.SNAP_NEG2.Evaluate(lastToCurrent.RelativeLength, xNeg2, yNeg2);
                    double correctionNeg2Stop = calcCorrection0Stop(lastToCurrent.RelativeLength, xNeg2, yNeg2);

                    flowinessNeg2PrevCurr = SpecialFunctions.Logistic((correctionNeg2Snap - correctionNeg2Flow - 0.05) * 20);

                    correctionNeg2 = PowerMean.Of(new[] { correctionNeg2Flow, correctionNeg2Snap, correctionNeg2Stop }, -10) * 1.3;
                }
            }

            return correctionNeg2;
        }

        private static double calculateNextObjectCorrection(OsuObjectPair? currentToNext, OsuObjectPair lastToCurrent, ref bool currentTimeCenteredRelativeToNeighbours, ref double flowinessPrevCurrNext)
        {
            // Correction #2 - The Next Object
            // Estimate how objNext affects the difficulty of hitting objCurr
            double correctionNext = 0;

            if (currentToNext != null && lastToCurrent.RelativeLength != 0)
            {
                double tRatioNext = lastToCurrent.TimeDelta / currentToNext.Value.TimeDelta;
                double cosPrevCurrNext =
                    Math.Min(Math.Max(-lastToCurrent.RelativeVector.DotProduct(currentToNext.Value.RelativeVector) / lastToCurrent.RelativeLength / currentToNext.Value.RelativeLength, -1), 1);

                if (tRatioNext > t_ratio_threshold)
                {
                    if (currentToNext.Value.RelativeLength == 0)
                    {
                        correctionNext = 0;
                    }
                    else
                    {
                        double correctionNextMoving = correction_neg2_moving_spline.Interpolate(cosPrevCurrNext);

                        double movingness = SpecialFunctions.Logistic(currentToNext.Value.RelativeLength * 6 - 5) - SpecialFunctions.Logistic(-5);
                        correctionNext = movingness * correctionNextMoving * 0.5;
                    }
                }
                else if (tRatioNext < 1 / t_ratio_threshold)
                {
                    if (currentToNext.Value.RelativeLength == 0)
                    {
                        correctionNext = 0;
                    }
                    else
                    {
                        correctionNext = (1 - cosPrevCurrNext) * SpecialFunctions.Logistic((currentToNext.Value.RelativeLength * tRatioNext - 1.5) * 4) * 0.15;
                    }
                }
                else
                {
                    currentTimeCenteredRelativeToNeighbours = true;

                    var normalizedPosNext = currentToNext.Value.RelativeVector / currentToNext.Value.TimeDelta * lastToCurrent.TimeDelta;
                    double xNext = normalizedPosNext.DotProduct(lastToCurrent.RelativeVector) / lastToCurrent.RelativeLength;
                    double yNext = (normalizedPosNext - xNext * lastToCurrent.RelativeVector / lastToCurrent.RelativeLength).L2Norm();

                    double correctionNextFlow = AngleCorrection.FLOW_NEXT.Evaluate(lastToCurrent.RelativeLength, xNext, yNext);
                    double correctionNextSnap = AngleCorrection.SNAP_NEXT.Evaluate(lastToCurrent.RelativeLength, xNext, yNext);

                    flowinessPrevCurrNext = SpecialFunctions.Logistic((correctionNextSnap - correctionNextFlow - 0.05) * 20);

                    correctionNext = Math.Max(PowerMean.Of(correctionNextFlow, correctionNextSnap, -10) - 0.1, 0) * 0.5;
                }
            }

            return correctionNext;
        }

        private static double calculateFourObjectPatternCorrection(bool previousTimeCenteredRelativeToNeighbours, bool currentTimeCenteredRelativeToNeighbours, OsuObjectPair lastToCurrent,
                                                                   OsuObjectPair? currentToNext, OsuObjectPair? secondLastToLast, double flowinessNeg2PrevCurr, double flowinessPrevCurrNext)
        {
            // Correction #3 - 4-object pattern
            // Estimate how the whole pattern consisting of objNeg2 to objNext affects
            // the difficulty of hitting objCurr. This only takes effect when the pattern
            // is not so spaced (i.e. does not contain jumps)
            double patternCorrection = 0;

            if (previousTimeCenteredRelativeToNeighbours && currentTimeCenteredRelativeToNeighbours)
            {
                Debug.Assert(currentToNext != null);
                Debug.Assert(secondLastToLast != null);

                double gap = (lastToCurrent.RelativeVector - currentToNext.Value.RelativeVector / 2 - secondLastToLast.Value.RelativeVector / 2).L2Norm() / (lastToCurrent.RelativeLength + 0.1);
                patternCorrection = (SpecialFunctions.Logistic((gap - 1) * 8) - SpecialFunctions.Logistic(-6)) *
                                    SpecialFunctions.Logistic((secondLastToLast.Value.RelativeLength - 0.7) * 10) * SpecialFunctions.Logistic((currentToNext.Value.RelativeLength - 0.7) * 10) *
                                    PowerMean.Of(flowinessNeg2PrevCurr, flowinessPrevCurrNext, 2) * 0.6;
            }

            return patternCorrection;
        }

        private static double calculateTapStrainBuff(Vector<double> tapStrain, OsuObjectPair lastToCurrent, double movementThroughput)
        {
            // Correction #4 - Tap Strain
            // Estimate how tap strain affects difficulty
            double tapCorrection = 0;

            if (lastToCurrent.RelativeLength > 0 && tapStrain != null)
            {
                tapCorrection = SpecialFunctions.Logistic((PowerMean.Of(tapStrain, 2) / movementThroughput - 1.34) / 0.1) * 0.15;
            }

            return tapCorrection;
        }

        private static double calculateCheeseWindow(OsuObjectPair lastToCurrent, OsuObjectPair? secondLastToLast, double movementThroughput, OsuObjectPair? currentToNext, out double timeLate,
                                                    out double cheesabilityEarly, out double cheesabilityLate)
        {
            // Correction #5 - Cheesing
            // The player might make the movement of objPrev -> objCurr easier by
            // hitting objPrev early and objCurr late. Here we estimate the amount of
            // cheesing and update MT accordingly.
            double timeEarly = 0;
            timeLate = 0;
            cheesabilityEarly = 0;
            cheesabilityLate = 0;

            if (lastToCurrent.RelativeLength > 0)
            {
                double tNeg2PrevReciprocal;
                double ipNeg2Prev;

                if (secondLastToLast != null)
                {
                    tNeg2PrevReciprocal = 1 / (secondLastToLast.Value.TimeDelta + 1e-10);
                    ipNeg2Prev = FittsLaw.Throughput(secondLastToLast.Value.RelativeLength, secondLastToLast.Value.TimeDelta);
                }
                else
                {
                    tNeg2PrevReciprocal = 0;
                    ipNeg2Prev = 0;
                }

                cheesabilityEarly = SpecialFunctions.Logistic((ipNeg2Prev / movementThroughput - 0.6) * (-15)) * 0.5;
                timeEarly = cheesabilityEarly * (1 / (1 / (lastToCurrent.TimeDelta + 0.07) + tNeg2PrevReciprocal));

                double tCurrNextReciprocal;
                double ipCurrNext;

                if (currentToNext != null)
                {
                    tCurrNextReciprocal = 1 / (currentToNext.Value.TimeDelta + 1e-10);
                    ipCurrNext = FittsLaw.Throughput(currentToNext.Value.RelativeLength, currentToNext.Value.TimeDelta);
                }
                else
                {
                    tCurrNextReciprocal = 0;
                    ipCurrNext = 0;
                }

                cheesabilityLate = SpecialFunctions.Logistic((ipCurrNext / movementThroughput - 0.6) * (-15)) * 0.5;
                timeLate = cheesabilityLate * (1 / (1 / (lastToCurrent.TimeDelta + 0.07) + tCurrNextReciprocal));
            }

            return timeEarly;
        }

        private static double calculateHighBPMJumpBuff(double effectiveBpm, OsuObjectPair lastToCurrent)
        {
            // Correction #6 - High bpm jump buff (alt buff)
            double highBpmJumpBuff = SpecialFunctions.Logistic((effectiveBpm - 354) / 16) *
                                     SpecialFunctions.Logistic((lastToCurrent.RelativeLength - 1.9) / 0.15) * 0.23;
            return highBpmJumpBuff;
        }

        private static double calculateSmallCircleBuff(OsuHitObject currentObject, OsuObjectPair lastToCurrent)
        {
            // Correction #7 - Small circle bonus
            double smallCircleBonus = ((SpecialFunctions.Logistic((55 - 2 * currentObject.Radius) / 3.0) * 0.3) +
                                       (Math.Pow(24.5 - Math.Min(currentObject.Radius, 24.5), 1.4) * 0.01315)) *
                                      Math.Max(SpecialFunctions.Logistic((lastToCurrent.RelativeLength - 0.5) / 0.1), 0.25);
            return smallCircleBonus;
        }

        private static double calculateStackedNoteNerf(OsuObjectPair lastToCurrent)
        {
            // Correction #8 - Stacked notes nerf
            double dPrevCurrStackedNerf = Math.Max(0, Math.Min(lastToCurrent.RelativeLength, Math.Min(1.2 * lastToCurrent.RelativeLength - 0.185, 1.4 * lastToCurrent.RelativeLength - 0.32)));
            return dPrevCurrStackedNerf;
        }

        private static double calculateSmallJumpNerf(OsuObjectPair lastToCurrent, double effectiveBpm)
        {
            // Correction #9 - Slow small jump nerf
            double smallJumpNerfFactor = 1 - 0.17 * Math.Exp(-Math.Pow((lastToCurrent.RelativeLength - 2.2) / 0.7, 2)) *
                SpecialFunctions.Logistic((255 - effectiveBpm) / 10);
            return smallJumpNerfFactor;
        }

        private static double calculateBigJumpBuff(OsuObjectPair lastToCurrent, double effectiveBpm)
        {
            // Correction #10 - Slow big jump buff
            double bigJumpBuffFactor = 1 + 0.15 * SpecialFunctions.Logistic((lastToCurrent.RelativeLength - 6) / 0.5) *
                SpecialFunctions.Logistic((210 - effectiveBpm) / 8);
            return bigJumpBuffFactor;
        }

        private static double calculateHiddenCorrection(bool hidden, double noteDensity)
        {
            // Correction #11 - Hidden Mod
            double correctionHidden = 0;

            if (hidden)
            {
                correctionHidden = 0.05 + 0.008 * noteDensity;
            }

            return correctionHidden;
        }

        private static double calculateJumpOverlapCorrection(OsuObjectPair? secondLastToCurrent, OsuObjectPair? fourthLastToCurrent, OsuObjectPair lastToCurrent)
        {
            // Correction #13 - Repetitive jump nerf
            // Nerf big jumps where objNeg2 and objCurr are close or where objNeg4 and objCurr are close
            double jumpOverlapCorrection = 1 - (Math.Max(0.15 - 0.1 * (secondLastToCurrent?.RelativeLength ?? 0), 0) + Math.Max(0.1125 - 0.075 * (fourthLastToCurrent?.RelativeLength ?? 0), 0)) *
                SpecialFunctions.Logistic((lastToCurrent.RelativeLength - 3.3) / 0.25);
            return jumpOverlapCorrection;
        }

        private static double calculateDistanceIncreaseBuff(OsuHitObject secondLastObject, OsuObjectPair? secondLastToLast, OsuObjectPair lastToCurrent)
        {
            // Correction #14 - Sudden distance increase buff
            double distanceIncreaseBuff = 1;

            if (secondLastObject != null)
            {
                double dNeg2PrevOverlapNerf = Math.Min(1, Math.Pow(secondLastToLast?.RelativeLength ?? 0, 3));
                double timeDifferenceNerf = Math.Exp(-4
                                                     * Math.Pow(
                                                         1 - Math.Max(lastToCurrent.TimeDelta / ((secondLastToLast?.TimeDelta ?? 0) + 1e-10),
                                                             (secondLastToLast?.TimeDelta ?? 0) / (lastToCurrent.TimeDelta + 1e-10)), 2));
                double distanceRatio = lastToCurrent.RelativeLength / Math.Max(1, secondLastToLast?.RelativeLength ?? 0);
                double bpmScaling = Math.Max(1, -16 * lastToCurrent.TimeDelta + 3.4);
                distanceIncreaseBuff = 1 + 0.225 * bpmScaling * timeDifferenceNerf * dNeg2PrevOverlapNerf * Math.Max(0, distanceRatio - 2);
            }

            return distanceIncreaseBuff;
        }

        private static double calcCorrection0Stop(double d, double x, double y)
        {
            return SpecialFunctions.Logistic(10 * Math.Sqrt(x * x + y * y + 1) - 12);
        }
    }
}
