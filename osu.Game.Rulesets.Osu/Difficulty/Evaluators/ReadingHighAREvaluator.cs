// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    // Main class with some util functions
    public static class ReadingHighAREvaluator
    {
        public static double EvaluateDifficultyOf(DifficultyHitObject current, bool applyAdjust = false)
        {
            var currObj = (OsuDifficultyHitObject)current;

            double result = GetDifficulty(currObj.Preempt);

            if (applyAdjust)
            {
                double inpredictability = ReadingEvaluator.EvaluateInpredictabilityOf(current);

                // follow lines make high AR easier, so apply nerf if object isn't new combo
                inpredictability *= 1 + 0.1 * (800 - currObj.FollowLineTime) / 800;

                result *= 0.9 + 1 * inpredictability;
                result *= 1.05 - 0.4 * EvaluateFieryAnglePunishmentOf(current);
            }

            return result;
        }

        // Explicitely nerfs edgecased fiery-type jumps for high AR. The difference from Inpredictability is that this is not used in HD calc
        public static double EvaluateFieryAnglePunishmentOf(DifficultyHitObject current)
        {
            if (current.Index <= 2)
                return 0;

            var currObj = (OsuDifficultyHitObject)current;
            var lastObj0 = (OsuDifficultyHitObject)current.Previous(0);
            var lastObj1 = (OsuDifficultyHitObject)current.Previous(1);
            var lastObj2 = (OsuDifficultyHitObject)current.Previous(2);

            if (currObj.Angle.IsNull() || lastObj0.Angle.IsNull() || lastObj1.Angle.IsNull() || lastObj2.Angle.IsNull())
                return 0;

            // Punishment will be reduced if velocity is changing
            double velocityChangeFactor = getVelocityChangeFactor(currObj, lastObj0);
            velocityChangeFactor = 1 - Math.Pow(velocityChangeFactor, 2);

            double a1 = currObj.Angle.Value / Math.PI;
            double a2 = lastObj0.Angle.Value / Math.PI;
            double a3 = lastObj1.Angle.Value / Math.PI;
            double a4 = lastObj2.Angle.Value / Math.PI;

            // - 4 same sharp angles in a row: (0.3 0.3 0.3 0.3) -> max punishment

            // Normalized difference
            double angleDifference1 = Math.Abs(a1 - a2);
            double angleDifference2 = Math.Abs(a1 - a3);
            double angleDifference3 = Math.Abs(a1 - a4);

            // Will be close to 1 if angleDifference1 and angleDifference2 was both close to 0
            double sameAnglePunishment = Math.Pow((1 - angleDifference1) * (1 - angleDifference2) * (1 - angleDifference3), 3);

            // Starting from 60 degrees - reduce same angle punishment
            double angleSharpnessFactor = Math.Max(0, a1 - 1.0 / 3);
            angleSharpnessFactor = 1 - angleSharpnessFactor;

            sameAnglePunishment *= angleSharpnessFactor;
            sameAnglePunishment *= velocityChangeFactor;
            sameAnglePunishment *= 0.75;

            // - Alternating angles with 0: (0.3 0 0.3 0) or (0 0.3 0 0.3) -> max punishment, (0.3 0 0.1 0) -> some punishment

            double alternateWithZeroAnglePunishment = Math.Max(
                getAlternateWithZeroAnglePunishment(a1, a2, a3, a4),
                getAlternateWithZeroAnglePunishment(a2, a1, a4, a3));
            alternateWithZeroAnglePunishment *= velocityChangeFactor;

            return Math.Min(1, sameAnglePunishment + alternateWithZeroAnglePunishment);
        }

        private static double getVelocityChangeFactor(OsuDifficultyHitObject osuCurrObj, OsuDifficultyHitObject osuLastObj)
        {
            double currVelocity = osuCurrObj.LazyJumpDistance / osuCurrObj.StrainTime;
            double prevVelocity = osuLastObj.LazyJumpDistance / osuLastObj.StrainTime;

            double velocityChangeFactor = 0;

            // https://www.desmos.com/calculator/kqxmqc8pkg
            if (currVelocity > 0 || prevVelocity > 0)
            {
                double velocityChange = Math.Max(0,
                Math.Min(
                    Math.Abs(prevVelocity - currVelocity) - 0.5 * Math.Min(currVelocity, prevVelocity),
                    Math.Max(((OsuHitObject)osuCurrObj.BaseObject).Radius / Math.Max(osuCurrObj.StrainTime, osuLastObj.StrainTime), Math.Min(currVelocity, prevVelocity))
                    )); // Stealed from xexxar
                velocityChangeFactor = velocityChange / Math.Max(currVelocity, prevVelocity); // maxiumum is 0.4
                velocityChangeFactor /= 0.4;
            }

            return velocityChangeFactor;
        }

        private static double getAlternateWithZeroAnglePunishment(double a1, double a2, double a3, double a4)
        {
            // We assume that a1 and a3 are 0
            double zeroFactor = Math.Pow((1 - a1) * (1 - a3), 8);
            zeroFactor *= Math.Pow(1 - Math.Abs(a1 - a3), 2);

            double angleSimilarityFactor = 1 - Math.Abs(a2 - a4);
            double angleSharpnessFactor = Math.Min(1 - Math.Max(0, a2 - 1.0 / 3), 1 - Math.Max(0, a4 - 1.0 / 3));

            return zeroFactor * angleSimilarityFactor * angleSharpnessFactor;
        }

        // High AR curve
        // https://www.desmos.com/calculator/hbj7swzlth
        public static double GetDifficulty(double preempt)
        {
            double value = Math.Pow(3.5, 3 - 0.01 * preempt); // 1 for 300ms, 0.25 for 400ms, 0.0625 for 500ms
            value = softmin(value, 2, 1.7); // use softmin to achieve full-memory cap, 2 times more than AR11 (300ms)
            return value;
        }

        // We are using mutiply and divide instead of add and subtract, so values won't be negative
        // https://www.desmos.com/calculator/fv5xerwpd2
        private static double softmin(double a, double b, double power = Math.E) => a * b / Math.Log(Math.Pow(power, a) + Math.Pow(power, b), power);
    }
}
