// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
using osu.Game.Rulesets.Osu.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Preprocessing
{
    internal static class OsuMovementExtractor
    {
        private static readonly LinearSpline correction_moving_spline = LinearSpline.InterpolateSorted(
            new[] { -1.0, 1.0 },
            new[] { 1.1, 0 });

        private const double movement_length_ratio_threshold = 1.4;

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
        /// <param name="tapStrain">The tap strain of the current object.</param> TODO: does this have to be passed down? maybe store in the object?
        /// <param name="noteDensity">The visual note density of the current object.</param> TODO: above
        /// <param name="gameplayRate">The current rate of the gameplay clock.</param>
        /// <param name="hidden">Whether the hidden mod is active.</param>
        /// <returns>List of movements performed in attempt to hit the current object.</returns>
        public static List<OsuMovement> Extract(
            [CanBeNull] OsuHitObject secondLastObject,
            OsuHitObject lastObject,
            OsuHitObject currentObject,
            [CanBeNull] OsuHitObject nextObject,
            double tapStrain,
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
            movement.Distance = correctMovementDistance(parameters, movementThroughput, tapStrain, hidden, noteDensity);
            calculateCheeseWindow(parameters, movementThroughput);
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

        /// <summary>
        /// Distance is main measure of difficulty in Fitt's Law so we correct it when we need to adjust difficulty of certain aspects/patterns.
        /// </summary>
        private static double correctMovementDistance(MovementExtractionParameters parameters, double movementThroughput, double tapStrain, bool hidden, double noteDensity)
        {
            double previousObjectPlacementCorrection = calculatePreviousObjectPlacementCorrection(parameters);
            double nextObjectPlacementCorrection = calculateNextObjectPlacementCorrection(parameters);
            double fourObjectPatternCorrection = calculateFourObjectPatternCorrection(parameters);
            double placementCorrection = 1.0 + previousObjectPlacementCorrection + nextObjectPlacementCorrection + fourObjectPatternCorrection;

            double tapCorrection = calculateTapStrainBuff(tapStrain, parameters.LastToCurrent, movementThroughput);

            if (isStackedWiggle(parameters))
            {
                placementCorrection = 1.0;
                tapCorrection = 1.0;
            }

            return placementCorrection *
                   calculateStackedNoteNerf(parameters.LastToCurrent) *
                   tapCorrection *
                   calculateSmallCircleBuff(parameters) *
                   calculateHighBPMJumpBuff(parameters) *
                   calculateSmallJumpNerf(parameters) *
                   calculateBigJumpBuff(parameters) *
                   calculateHiddenCorrection(hidden, noteDensity) *
                   calculateJumpOverlapCorrection(parameters);
        }

        /// <summary>
        /// Correction #1 - The Previous Object
        /// Estimate how second-last object placement affects the difficulty of hitting current object.
        /// </summary>
        private static double calculatePreviousObjectPlacementCorrection(MovementExtractionParameters p)
        {
            if (p.SecondLastObject == null || p.LastToCurrent.RelativeLength == 0)
                return 0;

            Debug.Assert(p.SecondLastToLast != null);

            double movementLengthRatio = p.LastToCurrent.TimeDelta / p.SecondLastToLast.Value.TimeDelta;
            double movementAngleCosine = cosineOfAngleBetweenPairs(p.SecondLastToLast.Value, p.LastToCurrent);

            if (movementLengthRatio > movement_length_ratio_threshold)
            {
                if (p.SecondLastToLast.Value.RelativeLength == 0)
                    return 0;

                double angleCorrection = correction_moving_spline.Interpolate(movementAngleCosine);

                double movingness = SpecialFunctions.Logistic(p.SecondLastToLast.Value.RelativeLength * 6 - 5) - SpecialFunctions.Logistic(-5);
                return movingness * angleCorrection * 1.5;
            }

            if (movementLengthRatio < 1 / movement_length_ratio_threshold)
            {
                if (p.SecondLastToLast.Value.RelativeLength == 0)
                    return 0;

                return (1 - movementAngleCosine) * SpecialFunctions.Logistic((p.SecondLastToLast.Value.RelativeLength * movementLengthRatio - 1.5) * 4) * 0.3;
            }

            p.LastObjectTemporallyCenteredBetweenNeighbours = true;

            // rescale SecondLastToLast so that it's comparable timescale-wise to LastToCurrent
            var timeNormalisedSecondLastToLast = -p.SecondLastToLast.Value.RelativeVector / p.SecondLastToLast.Value.TimeDelta * p.LastToCurrent.TimeDelta;
            // transform secondLastObject to coordinates anchored in lastObject
            double secondLastTransformedX = timeNormalisedSecondLastToLast.DotProduct(p.LastToCurrent.RelativeVector) / p.LastToCurrent.RelativeLength;
            double secondLastTransformedY = (timeNormalisedSecondLastToLast - secondLastTransformedX * p.LastToCurrent.RelativeVector / p.LastToCurrent.RelativeLength).L2Norm();

            double correctionSecondLastFlow = AngleCorrection.FLOW_SECONDLAST.Evaluate(p.LastToCurrent.RelativeLength, secondLastTransformedX, secondLastTransformedY);
            double correctionSecondLastSnap = AngleCorrection.SNAP_SECONDLAST.Evaluate(p.LastToCurrent.RelativeLength, secondLastTransformedX, secondLastTransformedY);
            double correctionSecondLastStop = SpecialFunctions.Logistic(10 * Math.Sqrt(secondLastTransformedX * secondLastTransformedX + secondLastTransformedY * secondLastTransformedY + 1) - 12);

            p.SecondLastToCurrentFlowiness = SpecialFunctions.Logistic((correctionSecondLastSnap - correctionSecondLastFlow - 0.05) * 20);

            return PowerMean.Of(new[] { correctionSecondLastFlow, correctionSecondLastSnap, correctionSecondLastStop }, -10) * 1.3;
        }

        /// <summary>
        /// Correction #2 - The Next Object
        /// Estimate how next object placement affects the difficulty of hitting current object.
        /// </summary>
        private static double calculateNextObjectPlacementCorrection(MovementExtractionParameters p)
        {
            if (p.NextObject == null || p.LastToCurrent.RelativeLength == 0)
                return 0;

            Debug.Assert(p.CurrentToNext != null);

            double movementLengthRatio = p.LastToCurrent.TimeDelta / p.CurrentToNext.Value.TimeDelta;
            double movementAngleCosine = cosineOfAngleBetweenPairs(p.LastToCurrent, p.CurrentToNext.Value);

            if (movementLengthRatio > movement_length_ratio_threshold)
            {
                if (p.CurrentToNext.Value.RelativeLength == 0)
                    return 0;

                double correctionNextMoving = correction_moving_spline.Interpolate(movementAngleCosine);

                double movingness = SpecialFunctions.Logistic(p.CurrentToNext.Value.RelativeLength * 6 - 5) - SpecialFunctions.Logistic(-5);
                return movingness * correctionNextMoving * 0.5;
            }

            if (movementLengthRatio < 1 / movement_length_ratio_threshold)
            {
                if (p.CurrentToNext.Value.RelativeLength == 0)
                    return 0;

                return (1 - movementAngleCosine) * SpecialFunctions.Logistic((p.CurrentToNext.Value.RelativeLength * movementLengthRatio - 1.5) * 4) * 0.15;
            }

            p.CurrentObjectTemporallyCenteredBetweenNeighbours = true;

            // rescale CurrentToNext so that it's comparable timescale-wise to LastToCurrent
            var timeNormalizedNext = p.CurrentToNext.Value.RelativeVector / p.CurrentToNext.Value.TimeDelta * p.LastToCurrent.TimeDelta;

            // transform nextObject to coordinates anchored in lastObject
            double nextTransformedX = timeNormalizedNext.DotProduct(p.LastToCurrent.RelativeVector) / p.LastToCurrent.RelativeLength;
            double nextTransformedY = (timeNormalizedNext - nextTransformedX * p.LastToCurrent.RelativeVector / p.LastToCurrent.RelativeLength).L2Norm();

            double correctionNextFlow = AngleCorrection.FLOW_NEXT.Evaluate(p.LastToCurrent.RelativeLength, nextTransformedX, nextTransformedY);
            double correctionNextSnap = AngleCorrection.SNAP_NEXT.Evaluate(p.LastToCurrent.RelativeLength, nextTransformedX, nextTransformedY);

            p.LastToNextFlowiness = SpecialFunctions.Logistic((correctionNextSnap - correctionNextFlow - 0.05) * 20);

            return Math.Max(PowerMean.Of(correctionNextFlow, correctionNextSnap, -10) - 0.1, 0) * 0.5;
        }

        /// <summary>
        /// Correction #3 - 4-object pattern
        /// Estimate how the whole pattern consisting of second-last to next objects affects the difficulty of hitting current object.
        /// This only takes effect when the pattern is not so spaced (i.e. does not contain jumps)
        /// </summary>
        private static double calculateFourObjectPatternCorrection(MovementExtractionParameters p)
        {
            if (!p.LastObjectTemporallyCenteredBetweenNeighbours ||
                !p.CurrentObjectTemporallyCenteredBetweenNeighbours)
                return 0;

            Debug.Assert(p.CurrentToNext != null);
            Debug.Assert(p.SecondLastToLast != null);

            double gap = (p.LastToCurrent.RelativeVector - p.CurrentToNext.Value.RelativeVector / 2 - p.SecondLastToLast.Value.RelativeVector / 2).L2Norm() / (p.LastToCurrent.RelativeLength + 0.1);
            return (SpecialFunctions.Logistic((gap - 1) * 8) - SpecialFunctions.Logistic(-6)) *
                   SpecialFunctions.Logistic((p.SecondLastToLast.Value.RelativeLength - 0.7) * 10) * SpecialFunctions.Logistic((p.CurrentToNext.Value.RelativeLength - 0.7) * 10) *
                   PowerMean.Of(p.SecondLastToCurrentFlowiness, p.LastToNextFlowiness, 2) * 0.6;
        }

        /// <summary>
        /// Correction #4 - Tap Strain
        /// This buffs current object's aim difficulty rating by tap difficulty when distance is bigger than 0.
        /// </summary>
        private static double calculateTapStrainBuff(double tapStrain, OsuObjectPair lastToCurrent, double movementThroughput)
        {
            if (!(lastToCurrent.RelativeLength > 0))
                return 1.0;

            var tapBonus = SpecialFunctions.Logistic((tapStrain / movementThroughput - 1.34) / 0.1);

            return 1.0 + tapBonus * 0.15;
        }

        /// <summary>
        /// Correction #5 - Cheesing
        /// The player might make the movement from previous object to current easier by hitting former early and latter late.
        /// Here we estimate the amount of such cheesing to update MovementTime accordingly.
        /// </summary>
        private static void calculateCheeseWindow(MovementExtractionParameters p, double movementThroughput)
        {
            double timeEarly = 0;
            double timeLate = 0;
            double cheesabilityEarly = 0;
            double cheesabilityLate = 0;

            if (p.LastToCurrent.RelativeLength > 0)
            {
                double secondLastToLastReciprocalMovementLength;
                double secondLastToLastThroughput;

                if (p.SecondLastObject != null)
                {
                    Debug.Assert(p.SecondLastToLast != null);

                    secondLastToLastReciprocalMovementLength = 1 / (p.SecondLastToLast.Value.TimeDelta + 1e-10);
                    secondLastToLastThroughput = FittsLaw.Throughput(p.SecondLastToLast.Value.RelativeLength, p.SecondLastToLast.Value.TimeDelta);
                }
                else
                {
                    secondLastToLastReciprocalMovementLength = 0;
                    secondLastToLastThroughput = 0;
                }

                cheesabilityEarly = SpecialFunctions.Logistic((secondLastToLastThroughput / movementThroughput - 0.6) * (-15)) * 0.5;
                timeEarly = cheesabilityEarly * (1 / (1 / (p.LastToCurrent.TimeDelta + 0.07) + secondLastToLastReciprocalMovementLength));

                double currentToNextReciprocalMovementLength;
                double currentToNextThroughput;

                if (p.NextObject != null)
                {
                    Debug.Assert(p.CurrentToNext != null);

                    currentToNextReciprocalMovementLength = 1 / (p.CurrentToNext.Value.TimeDelta + 1e-10);
                    currentToNextThroughput = FittsLaw.Throughput(p.CurrentToNext.Value.RelativeLength, p.CurrentToNext.Value.TimeDelta);
                }
                else
                {
                    currentToNextReciprocalMovementLength = 0;
                    currentToNextThroughput = 0;
                }

                cheesabilityLate = SpecialFunctions.Logistic((currentToNextThroughput / movementThroughput - 0.6) * (-15)) * 0.5;
                timeLate = cheesabilityLate * (1 / (1 / (p.LastToCurrent.TimeDelta + 0.07) + currentToNextReciprocalMovementLength));
            }

            p.Cheesability = cheesabilityEarly + cheesabilityLate;
            p.CheeseWindow = (timeEarly + timeLate) / (p.LastToCurrent.TimeDelta + 1e-10);
        }

        /// <summary>
        /// Correction #6 - High bpm jump buff (alt buff)
        /// High speed (300 bpm+) jumps are underweighted by fitt's law so we're correting for it here.
        /// </summary>
        private static double calculateHighBPMJumpBuff(MovementExtractionParameters p)
        {
            var bpmCutoff = SpecialFunctions.Logistic((p.EffectiveBPM - 354) / 16.0);

            var distanceBuff = SpecialFunctions.Logistic((p.LastToCurrent.RelativeLength - 1.9) / 0.15);

            return 1.0 + bpmCutoff * distanceBuff * 0.23;
        }

        /// <summary>
        /// Correction #7 - Small circle bonus
        /// Small circles (CS 6.5+) are underweighted by fitt's law so we're correting for it here.
        /// Graphs: https://www.desmos.com/calculator/u6rjndtklb
        /// </summary>
        private static double calculateSmallCircleBuff(MovementExtractionParameters p)
        {
            // we only want to buff radiuses starting from about 35 (CS 4 radius is 36.48)
            var radiusCutoff = SpecialFunctions.Logistic((55 - 2 * p.CurrentObject.Radius) / 2.9) * 0.275;

            // we want to reduce bonus for small distances
            var distanceCutoff = Math.Max(SpecialFunctions.Logistic((p.LastToCurrent.RelativeLength - 0.5) / 0.1), 0.25);

            var bonusCurve = Math.Min(SpecialFunctions.Logistic((-p.CurrentObject.Radius + 10.0) / 4.0) * 0.8, 24.5);

            return 1.0 + (radiusCutoff + bonusCurve) * distanceCutoff;
        }

        /// <summary>
        /// Correction #8 - Stacked notes nerf
        /// We apply a nerf to the difficulty depending on how much objects overlap.
        /// </summary>
        private static double calculateStackedNoteNerf(OsuObjectPair lastToCurrent)
        {
            return Math.Max(0,
                Math.Min(lastToCurrent.RelativeLength,
                    Math.Min(1.2 * lastToCurrent.RelativeLength - 0.185, 1.4 * lastToCurrent.RelativeLength - 0.32)
                    )
                );
        }

        /// <summary>
        /// Correction #9 - Slow small jump nerf
        /// We apply nerf to jumps within ~1-3.5 distance (with peak at 2.2) depending on BPM.
        /// Graphs: https://www.desmos.com/calculator/lbwtkv1qom
        /// </summary>
        private static double calculateSmallJumpNerf(MovementExtractionParameters p)
        {
            // this applies nerf up to 300 bpm and starts deminishing it at ~200 bpm
            var bpmCutoff = SpecialFunctions.Logistic((255 - p.EffectiveBPM) / 10.0);

            var distanceNerf = Math.Exp(-Math.Pow((p.LastToCurrent.RelativeLength - 2.2) / 0.7, 2.0));

            return 1.0 - distanceNerf * bpmCutoff * 0.17;
        }

        /// <summary>
        /// Correction #10 - Slow big jump buff
        /// We apply buff to jumps with distance starting from ~4 on low BPMs.
        /// Graphs: https://www.desmos.com/calculator/fmewz0foql
        /// </summary>
        private static double calculateBigJumpBuff(MovementExtractionParameters p)
        {
            // this applies buff up until ~250 bpm
            var bpmCutoff = SpecialFunctions.Logistic((210 - p.EffectiveBPM) / 8.0);

            var distanceBuff = SpecialFunctions.Logistic((p.LastToCurrent.RelativeLength - 6.0) / 0.5);

            return 1.0 + distanceBuff * bpmCutoff * 0.15;
        }

        /// <summary>
        /// Correction #11 - Hidden Mod
        /// We apply slight bonus when using HD depending on current visual note density.
        /// </summary>
        private static double calculateHiddenCorrection(bool hidden, double noteDensity)
        {
            // Correction #11 - Hidden Mod
            if (!hidden)
                return 1.0;

            return 1.05 + 0.008 * noteDensity;
        }

        /// <summary>
        /// Correction #12 - Stacked wiggle fix
        /// If distance between each 4 objects is less than 1 (meaning they overlap) reset all angle corrections as well as tap correction.
        /// This fixes "wiggles" (usually a stream of objects that are placed in a zig-zag pattern that can be aimed in a straight line by going through overlapped places)
        /// </summary>
        private static bool isStackedWiggle(MovementExtractionParameters p)
        {
            if (p.SecondLastObject == null || p.NextObject == null)
                return false;

            return p.SecondLastToLast?.RelativeLength < 1
                   && p.SecondLastToCurrent?.RelativeLength < 1
                   && p.SecondLastToNext?.RelativeLength < 1
                   && p.LastToCurrent.RelativeLength < 1
                   && p.LastToNext?.RelativeLength < 1
                   && p.CurrentToNext?.RelativeLength < 1;
        }

        /// <summary>
        /// Correction #13 - Repetitive jump nerf
        /// We apply a nerf to big jumps where second-last or fourth-last and current objects are close.
        /// This mainly targets repeating jumps such as
        /// 1  3
        ///  \/
        ///  /\
        /// 4  2
        /// </summary>
        private static double calculateJumpOverlapCorrection(MovementExtractionParameters p)
        {
            var secondLastToCurrentNerf = Math.Max(0.15 - 0.1 * p.SecondLastToCurrent?.RelativeLength ?? 0.0, 0.0);
            var fourthLastToCurrentNerf = Math.Max(0.1125 - 0.075 * p.FourthLastToCurrent?.RelativeLength ?? 0.0, 0.0);

            var distanceCutoff = SpecialFunctions.Logistic((p.LastToCurrent.RelativeLength - 3.3) / 0.25);

            return 1.0 - (secondLastToCurrentNerf + fourthLastToCurrentNerf) * distanceCutoff;
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
