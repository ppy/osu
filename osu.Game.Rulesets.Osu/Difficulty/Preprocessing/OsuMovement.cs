// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Interpolation;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Difficulty.Utils;

namespace osu.Game.Rulesets.Osu.Difficulty.Preprocessing
{
    public class OsuMovement
    {
        private static readonly LinearSpline correction_neg2_moving_spline = LinearSpline.InterpolateSorted(
            new[] { -1.0, 1.0 },
            new[] { 1.1, 0 });

        private const double t_ratio_threshold = 1.4;
        private const double correction_neg2_still = 0;

        /// <summary>
        /// Uncorrected time taken to execute the movement.
        /// </summary>
        public double RawMovementTime { get; private set; }

        /// <summary>
        /// Corrected distance between objects.
        /// </summary>
        public double Distance { get; private set; }

        /// <summary>
        /// Corrected movement time.
        /// </summary>
        public double MovementTime { get; private set; }

        /// <summary>
        /// The calculated throughput of the player for this movement.
        /// </summary>
        public double Throughput { get; private set; }

        /// <summary>
        /// Estimated cheesablility of the movement.
        /// </summary>
        public double Cheesablility { get; private set; }

        /// <summary>
        /// The "cheese window" of the movement
        /// (how much allowance is gained by hitting first note early and the second late).
        /// </summary>
        public double CheeseWindow { get; private set; }

        /// <summary>
        /// The start time of the movement, in seconds.
        /// </summary>
        public double StartTime { get; private set; }

        /// <summary>
        /// Whether the movement ends on a slider.
        /// </summary>
        public bool EndsOnSlider { get; private set; }

        /// <summary>
        /// Extracts movement (only for the first object in a beatmap).
        /// </summary>
        public static List<OsuMovement> ExtractMovement(OsuHitObject obj)
        {
            var movement = GetEmptyMovement(obj.StartTime / 1000.0);

            var movementWithNested = new List<OsuMovement> { movement };
            // add zero difficulty movements corresponding to slider ticks/slider ends so combo is reflected properly
            int extraNestedCount = obj.NestedHitObjects.Count - 1;

            for (int i = 0; i < extraNestedCount; i++)
            {
                movementWithNested.Add(GetEmptyMovement(movement.StartTime));
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
        public static List<OsuMovement> ExtractMovement(
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

            double lastToCurrentTimeDelta = (currentObject.StartTime - lastObject.StartTime) / gameplayRate / 1000.0;
            movement.RawMovementTime = lastToCurrentTimeDelta;
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

            var lastObjectPosition = Vector<double>.Build.Dense(new[] { lastObject.StackedPosition.X, (double)lastObject.StackedPosition.Y });
            var currentObjectPosition = Vector<double>.Build.Dense(new[] { currentObject.StackedPosition.X, (double)currentObject.StackedPosition.Y });

            // movement vector relative to the object diameter, as per Fitts's law
            var relativeLastToCurrentVector = (currentObjectPosition - lastObjectPosition) / (2 * currentObject.Radius);
            double relativeLastToCurrentLength = relativeLastToCurrentVector.L2Norm();
            double movementThroughput = FittsLaw.Throughput(relativeLastToCurrentLength, lastToCurrentTimeDelta);

            movement.Throughput = movementThroughput;

            var secondLastObjectPosition = Vector<double>.Build.Dense(2);
            var nextObjectPosition = Vector<double>.Build.Dense(2);

            var relativeSecondLastToLastVector = Vector<double>.Build.Dense(2);
            var relativeCurrentToNextVector = Vector<double>.Build.Dense(2);

            // all lengths relative to current object diameter
            double relativeFourthLastToCurrentLength = 0;
            double relativeSecondLastToLastLength = 0;
            double relativeSecondLastToCurrentLength = 0;
            double relativeCurrentToNextLength = 0;

            double secondLastToLastTimeDelta = 0;
            double currentToNextTimeDelta = 0;

            double flowinessNeg2PrevCurr = 0;
            double flowinessPrevCurrNext = 0;

            bool previousTimeCenteredRelativeToNeighbours = false;
            bool currentTimeCenteredRelativeToNeighbours = false;

            if (fourthLastObject != null)
            {
                var posNeg4 = Vector<double>.Build.Dense(new[] { fourthLastObject.StackedPosition.X, (double)fourthLastObject.StackedPosition.Y });
                relativeFourthLastToCurrentLength = ((currentObjectPosition - posNeg4) / (2 * currentObject.Radius)).L2Norm();
            }

            if (secondLastObject != null)
            {
                secondLastObjectPosition = Vector<double>.Build.Dense(new[] { secondLastObject.StackedPosition.X, (double)secondLastObject.StackedPosition.Y });
                relativeSecondLastToLastVector = (lastObjectPosition - secondLastObjectPosition) / (2 * currentObject.Radius);
                relativeSecondLastToLastLength = relativeSecondLastToLastVector.L2Norm();
                secondLastToLastTimeDelta = (lastObject.StartTime - secondLastObject.StartTime) / gameplayRate / 1000.0;
                relativeSecondLastToCurrentLength = ((currentObjectPosition - secondLastObjectPosition) / (2 * currentObject.Radius)).L2Norm();
            }

            if (nextObject != null)
            {
                nextObjectPosition = Vector<double>.Build.Dense(new[] { nextObject.StackedPosition.X, (double)nextObject.StackedPosition.Y });
                relativeCurrentToNextVector = (nextObjectPosition - currentObjectPosition) / (2 * currentObject.Radius);
                relativeCurrentToNextLength = relativeCurrentToNextVector.L2Norm();
                currentToNextTimeDelta = (nextObject.StartTime - currentObject.StartTime) / gameplayRate / 1000.0;
            }

            // Correction #1 - The Previous Object
            // Estimate how objNeg2 affects the difficulty of hitting objCurr
            double correctionNeg2 = 0;

            if (secondLastObject != null && relativeLastToCurrentLength != 0)
            {
                double tRatioNeg2 = lastToCurrentTimeDelta / secondLastToLastTimeDelta;
                double cosNeg2PrevCurr = Math.Min(Math.Max(-relativeSecondLastToLastVector.DotProduct(relativeLastToCurrentVector) / relativeSecondLastToLastLength / relativeLastToCurrentLength, -1), 1);

                if (tRatioNeg2 > t_ratio_threshold)
                {
                    if (relativeSecondLastToLastLength == 0)
                    {
                        correctionNeg2 = correction_neg2_still;
                    }
                    else
                    {
                        double correctionNeg2Moving = correction_neg2_moving_spline.Interpolate(cosNeg2PrevCurr);

                        double movingness = SpecialFunctions.Logistic(relativeSecondLastToLastLength * 6 - 5) - SpecialFunctions.Logistic(-5);
                        correctionNeg2 = (movingness * correctionNeg2Moving + (1 - movingness) * correction_neg2_still) * 1.5;
                    }
                }
                else if (tRatioNeg2 < 1 / t_ratio_threshold)
                {
                    if (relativeSecondLastToLastLength == 0)
                    {
                        correctionNeg2 = 0;
                    }
                    else
                    {
                        correctionNeg2 = (1 - cosNeg2PrevCurr) * SpecialFunctions.Logistic((relativeSecondLastToLastLength * tRatioNeg2 - 1.5) * 4) * 0.3;
                    }
                }
                else
                {
                    previousTimeCenteredRelativeToNeighbours = true;

                    var normalizedPosNeg2 = -relativeSecondLastToLastVector / secondLastToLastTimeDelta * lastToCurrentTimeDelta;
                    double xNeg2 = normalizedPosNeg2.DotProduct(relativeLastToCurrentVector) / relativeLastToCurrentLength;
                    double yNeg2 = (normalizedPosNeg2 - xNeg2 * relativeLastToCurrentVector / relativeLastToCurrentLength).L2Norm();

                    double correctionNeg2Flow = AngleCorrection.FLOW_NEG2.Evaluate(relativeLastToCurrentLength, xNeg2, yNeg2);
                    double correctionNeg2Snap = AngleCorrection.SNAP_NEG2.Evaluate(relativeLastToCurrentLength, xNeg2, yNeg2);
                    double correctionNeg2Stop = calcCorrection0Stop(relativeLastToCurrentLength, xNeg2, yNeg2);

                    flowinessNeg2PrevCurr = SpecialFunctions.Logistic((correctionNeg2Snap - correctionNeg2Flow - 0.05) * 20);

                    correctionNeg2 = PowerMean.Of(new[] { correctionNeg2Flow, correctionNeg2Snap, correctionNeg2Stop }, -10) * 1.3;
                }
            }

            // Correction #2 - The Next Object
            // Estimate how objNext affects the difficulty of hitting objCurr
            double correctionNext = 0;

            if (nextObject != null && relativeLastToCurrentLength != 0)
            {
                double tRatioNext = lastToCurrentTimeDelta / currentToNextTimeDelta;
                double cosPrevCurrNext = Math.Min(Math.Max(-relativeLastToCurrentVector.DotProduct(relativeCurrentToNextVector) / relativeLastToCurrentLength / relativeCurrentToNextLength, -1), 1);

                if (tRatioNext > t_ratio_threshold)
                {
                    if (relativeCurrentToNextLength == 0)
                    {
                        correctionNext = 0;
                    }
                    else
                    {
                        double correctionNextMoving = correction_neg2_moving_spline.Interpolate(cosPrevCurrNext);

                        double movingness = SpecialFunctions.Logistic(relativeCurrentToNextLength * 6 - 5) - SpecialFunctions.Logistic(-5);
                        correctionNext = movingness * correctionNextMoving * 0.5;
                    }
                }
                else if (tRatioNext < 1 / t_ratio_threshold)
                {
                    if (relativeCurrentToNextLength == 0)
                    {
                        correctionNext = 0;
                    }
                    else
                    {
                        correctionNext = (1 - cosPrevCurrNext) * SpecialFunctions.Logistic((relativeCurrentToNextLength * tRatioNext - 1.5) * 4) * 0.15;
                    }
                }
                else
                {
                    currentTimeCenteredRelativeToNeighbours = true;

                    var normalizedPosNext = relativeCurrentToNextVector / currentToNextTimeDelta * lastToCurrentTimeDelta;
                    double xNext = normalizedPosNext.DotProduct(relativeLastToCurrentVector) / relativeLastToCurrentLength;
                    double yNext = (normalizedPosNext - xNext * relativeLastToCurrentVector / relativeLastToCurrentLength).L2Norm();

                    double correctionNextFlow = AngleCorrection.FLOW_NEXT.Evaluate(relativeLastToCurrentLength, xNext, yNext);
                    double correctionNextSnap = AngleCorrection.SNAP_NEXT.Evaluate(relativeLastToCurrentLength, xNext, yNext);

                    flowinessPrevCurrNext = SpecialFunctions.Logistic((correctionNextSnap - correctionNextFlow - 0.05) * 20);

                    correctionNext = Math.Max(PowerMean.Of(correctionNextFlow, correctionNextSnap, -10) - 0.1, 0) * 0.5;
                }
            }

            // Correction #3 - 4-object pattern
            // Estimate how the whole pattern consisting of objNeg2 to objNext affects
            // the difficulty of hitting objCurr. This only takes effect when the pattern
            // is not so spaced (i.e. does not contain jumps)
            double patternCorrection = 0;

            if (previousTimeCenteredRelativeToNeighbours && currentTimeCenteredRelativeToNeighbours)
            {
                double gap = (relativeLastToCurrentVector - relativeCurrentToNextVector / 2 - relativeSecondLastToLastVector / 2).L2Norm() / (relativeLastToCurrentLength + 0.1);
                patternCorrection = (SpecialFunctions.Logistic((gap - 1) * 8) - SpecialFunctions.Logistic(-6)) *
                                    SpecialFunctions.Logistic((relativeSecondLastToLastLength - 0.7) * 10) * SpecialFunctions.Logistic((relativeCurrentToNextLength - 0.7) * 10) *
                                    PowerMean.Of(flowinessNeg2PrevCurr, flowinessPrevCurrNext, 2) * 0.6;
            }

            // Correction #4 - Tap Strain
            // Estimate how tap strain affects difficulty
            double tapCorrection = 0;

            if (relativeLastToCurrentLength > 0 && tapStrain != null)
            {
                tapCorrection = SpecialFunctions.Logistic((PowerMean.Of(tapStrain, 2) / movementThroughput - 1.34) / 0.1) * 0.15;
            }

            // Correction #5 - Cheesing
            // The player might make the movement of objPrev -> objCurr easier by
            // hitting objPrev early and objCurr late. Here we estimate the amount of
            // cheesing and update MT accordingly.
            double timeEarly = 0;
            double timeLate = 0;
            double cheesabilityEarly = 0;
            double cheesabilityLate = 0;

            if (relativeLastToCurrentLength > 0)
            {
                double tNeg2PrevReciprocal;
                double ipNeg2Prev;

                if (secondLastObject != null)
                {
                    tNeg2PrevReciprocal = 1 / (secondLastToLastTimeDelta + 1e-10);
                    ipNeg2Prev = FittsLaw.Throughput(relativeSecondLastToLastLength, secondLastToLastTimeDelta);
                }
                else
                {
                    tNeg2PrevReciprocal = 0;
                    ipNeg2Prev = 0;
                }

                cheesabilityEarly = SpecialFunctions.Logistic((ipNeg2Prev / movementThroughput - 0.6) * (-15)) * 0.5;
                timeEarly = cheesabilityEarly * (1 / (1 / (lastToCurrentTimeDelta + 0.07) + tNeg2PrevReciprocal));

                double tCurrNextReciprocal;
                double ipCurrNext;

                if (nextObject != null)
                {
                    tCurrNextReciprocal = 1 / (currentToNextTimeDelta + 1e-10);
                    ipCurrNext = FittsLaw.Throughput(relativeCurrentToNextLength, currentToNextTimeDelta);
                }
                else
                {
                    tCurrNextReciprocal = 0;
                    ipCurrNext = 0;
                }

                cheesabilityLate = SpecialFunctions.Logistic((ipCurrNext / movementThroughput - 0.6) * (-15)) * 0.5;
                timeLate = cheesabilityLate * (1 / (1 / (lastToCurrentTimeDelta + 0.07) + tCurrNextReciprocal));
            }

            // Correction #6 - High bpm jump buff (alt buff)
            double effectiveBpm = 30 / (lastToCurrentTimeDelta + 1e-10);
            double highBpmJumpBuff = SpecialFunctions.Logistic((effectiveBpm - 354) / 16) *
                                     SpecialFunctions.Logistic((relativeLastToCurrentLength - 1.9) / 0.15) * 0.23;

            // Correction #7 - Small circle bonus
            double smallCircleBonus = ((SpecialFunctions.Logistic((55 - 2 * currentObject.Radius) / 3.0) * 0.3) +
                                       (Math.Pow(24.5 - Math.Min(currentObject.Radius, 24.5), 1.4) * 0.01315)) *
                                      Math.Max(SpecialFunctions.Logistic((relativeLastToCurrentLength - 0.5) / 0.1), 0.25);

            // Correction #8 - Stacked notes nerf
            double dPrevCurrStackedNerf = Math.Max(0, Math.Min(relativeLastToCurrentLength, Math.Min(1.2 * relativeLastToCurrentLength - 0.185, 1.4 * relativeLastToCurrentLength - 0.32)));

            // Correction #9 - Slow small jump nerf
            double smallJumpNerfFactor = 1 - 0.17 * Math.Exp(-Math.Pow((relativeLastToCurrentLength - 2.2) / 0.7, 2)) *
                SpecialFunctions.Logistic((255 - effectiveBpm) / 10);

            // Correction #10 - Slow big jump buff
            double bigJumpBuffFactor = 1 + 0.15 * SpecialFunctions.Logistic((relativeLastToCurrentLength - 6) / 0.5) *
                SpecialFunctions.Logistic((210 - effectiveBpm) / 8);

            // Correction #11 - Hidden Mod
            double correctionHidden = 0;

            if (hidden)
            {
                correctionHidden = 0.05 + 0.008 * noteDensity;
            }

            // Correction #12 - Stacked wiggle fix
            if (secondLastObject != null && nextObject != null)
            {
                var dPrevNext = ((nextObjectPosition - lastObjectPosition) / (2 * currentObject.Radius)).L2Norm();
                var dNeg2Next = ((nextObjectPosition - secondLastObjectPosition) / (2 * currentObject.Radius)).L2Norm();

                if (relativeSecondLastToLastLength < 1 && relativeSecondLastToCurrentLength < 1 && dNeg2Next < 1 && relativeLastToCurrentLength < 1 && dPrevNext < 1 && relativeCurrentToNextLength < 1)
                {
                    correctionNeg2 = 0;
                    correctionNext = 0;
                    patternCorrection = 0;
                    tapCorrection = 0;
                }
            }

            // Correction #13 - Repetitive jump nerf
            // Nerf big jumps where objNeg2 and objCurr are close or where objNeg4 and objCurr are close
            double jumpOverlapCorrection = 1 - (Math.Max(0.15 - 0.1 * relativeSecondLastToCurrentLength, 0) + Math.Max(0.1125 - 0.075 * relativeFourthLastToCurrentLength, 0)) *
                SpecialFunctions.Logistic((relativeLastToCurrentLength - 3.3) / 0.25);

            // Correction #14 - Sudden distance increase buff
            double distanceIncreaseBuff = 1;

            if (secondLastObject != null)
            {
                double dNeg2PrevOverlapNerf = Math.Min(1, Math.Pow(relativeSecondLastToLastLength, 3));
                double timeDifferenceNerf = Math.Exp(-4 * Math.Pow(1 - Math.Max(lastToCurrentTimeDelta / (secondLastToLastTimeDelta + 1e-10), secondLastToLastTimeDelta / (lastToCurrentTimeDelta + 1e-10)), 2));
                double distanceRatio = relativeLastToCurrentLength / Math.Max(1, relativeSecondLastToLastLength);
                double bpmScaling = Math.Max(1, -16 * lastToCurrentTimeDelta + 3.4);
                distanceIncreaseBuff = 1 + 0.225 * bpmScaling * timeDifferenceNerf * dNeg2PrevOverlapNerf * Math.Max(0, distanceRatio - 2);
            }

            // Apply the corrections
            double dPrevCurrWithCorrection = dPrevCurrStackedNerf * (1 + smallCircleBonus) * (1 + correctionNeg2 + correctionNext + patternCorrection) *
                                             (1 + highBpmJumpBuff) * (1 + tapCorrection) * smallJumpNerfFactor * bigJumpBuffFactor * (1 + correctionHidden) *
                                             jumpOverlapCorrection * distanceIncreaseBuff;

            movement.Distance = dPrevCurrWithCorrection;
            movement.MovementTime = lastToCurrentTimeDelta;
            movement.Cheesablility = cheesabilityEarly + cheesabilityLate;
            movement.CheeseWindow = (timeEarly + timeLate) / (lastToCurrentTimeDelta + 1e-10);

            var movementWithNested = new List<OsuMovement> { movement };

            // add zero difficulty movements corresponding to slider ticks/slider ends so combo is reflected properly
            int extraNestedCount = currentObject.NestedHitObjects.Count - 1;

            for (int i = 0; i < extraNestedCount; i++)
            {
                movementWithNested.Add(GetEmptyMovement(movement.StartTime));
            }

            return movementWithNested;
        }

        public static OsuMovement GetEmptyMovement(double time)
        {
            return new OsuMovement
            {
                Distance = 0,
                MovementTime = 1,
                CheeseWindow = 0,
                Cheesablility = 0,
                RawMovementTime = 0,
                Throughput = 0,
                StartTime = time
            };
        }

        private static double calcCorrection0Stop(double d, double x, double y)
        {
            return SpecialFunctions.Logistic(10 * Math.Sqrt(x * x + y * y + 1) - 12);
        }
    }
}
