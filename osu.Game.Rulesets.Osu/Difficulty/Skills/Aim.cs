// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to correctly aim at every object in the map with a uniform CircleSize and normalized distances.
    /// </summary>
    public class Aim : OsuStrainSkill
    {
        private const double angle_bonus_begin = Math.PI / 3;
        private const double timing_threshold = 107;

        private double skillMultiplier => 26.25;
        private double strainDecayBase => 0.15;

        private IBeatmap beatmap;
        public Aim(IBeatmap beatmap, Mod[] mods)
            : base(mods)
        {
            this.beatmap = beatmap;
        }

        private double currentStrain = 1;

        protected double strainValueOf(PrePerNoteStrainSkill[] preSkills, int index, DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner)
                return 0;

            var osuCurrent = (OsuDifficultyHitObject)current;

            // The values 0.4, 0.005, and 1.0 are from sense fit to ideal pp result 
            double angleBonus = preSkills[0].GetAllStrainPeaks()[index] * 0.4;

            // When you put this multiplier higher, this will act like speed bonus.
            double fingerControlBonus = preSkills[2].GetAllStrainPeaks()[index] * 0.005;

            // When two values are both higher, you could get a lot of bonus.
            // Remark things you want to check other pp result.
            double totalBonus = Math.Pow(
                (Math.Pow(0.99 + angleBonus, 1.1)) *
                (Math.Pow(0.99 + fingerControlBonus, 1.1))
                , 1.0 / 1.1);


            double sliderBonus = 1 + preSkills[1].GetAllStrainPeaks()[index] * 1;

            // Remarked because it is replaced to NoteVarianceAngle
            //if (Previous.Count > 0)
            //{
            //    var osuPrevious = (OsuDifficultyHitObject)Previous[0];

            //    if (osuCurrent.Angle != null && osuCurrent.Angle.Value > angle_bonus_begin)
            //    {
            //        const double scale = 90;

            //        var angleBonus = Math.Sqrt(
            //            Math.Max(osuPrevious.JumpDistance - scale, 0)
            //            * Math.Pow(Math.Sin(osuCurrent.Angle.Value - angle_bonus_begin), 2)
            //            * Math.Max(osuCurrent.JumpDistance - scale, 0));
            //        result = 1.4 * applyDiminishingExp(Math.Max(0, angleBonus)) / Math.Max(timing_threshold, osuPrevious.StrainTime);
            //    }
            //}

            double jumpDistanceExp = applyDiminishingExp(osuCurrent.JumpDistance);
            // multiplying sliderBonus
            double travelDistanceExp = applyDiminishingExp(osuCurrent.TravelDistance) * sliderBonus;

            double distanceExp = jumpDistanceExp + travelDistanceExp + Math.Sqrt(jumpDistanceExp * travelDistanceExp);

            // 320ms means almost 180bpm(90bpm) jump.
            // As a result, Slower jumps are buffed slightly.
            return
                totalBonus *
                (calculateAimValue(0, distanceExp, osuCurrent.StrainTime) * 0.9 +
                calculateAimValue(0, distanceExp, 160) * 0.05);
        }

        private double calculateAimValue(double result, double distanceExp, double strainTime)
        {
            return Math.Max(
                result + distanceExp / Math.Max(strainTime, timing_threshold),
                distanceExp / strainTime
            );
        }

        /// Testing newer travel distance calculation
        /// It is not used
        private float calculateTravelDistanceNewer(Slider OsuSlider)
        {
            var currentTimingPoint = beatmap.ControlPointInfo.TimingPointAt(OsuSlider.StartTime);

            double radius = OsuSlider.Radius;
            double sliderTickRate = beatmap.Difficulty.SliderTickRate;
            double bpmToWhiteTick = 60000 / currentTimingPoint.BPM;

            double spanDuration = OsuSlider.EndTime - OsuSlider.StartTime;
            double timesOfTick = Math.Round(spanDuration / bpmToWhiteTick * sliderTickRate);
            double spanPerTick = spanDuration / timesOfTick;

            //Console.WriteLine(
            //    "duration: " + spanDuration +
            //    ", result: " + timesOfTick
            //    );

            float totalDistance = 0f;
            bool plus = false; // ignoring first calculation
            var pastPos = OsuSlider.StackedPosition;
            double offset = OsuSlider.StartTime; 

            double endTimeMinusOffset = spanDuration >= 72 ? 36 : spanDuration / 2;

            while (offset + spanPerTick <= OsuSlider.EndTime + 1)
            {
                offset += spanPerTick;

                var currentPos = OsuSlider.StackedPositionAt(offset);
                var adaptedPos = calculateAdaptedPosition(pastPos, currentPos, radius);

                float perDistance = (adaptedPos - pastPos).Length;

                if (plus)
                    totalDistance += perDistance;
                else
                    plus = true;
                pastPos = adaptedPos;
            }

            if (plus) // adding last tick
            {
                var finishPos = OsuSlider.StackedPositionAt(OsuSlider.EndTime - endTimeMinusOffset);
                var adaptedPos = calculateAdaptedPosition(pastPos, finishPos, radius);

                float perDistance = (adaptedPos - pastPos).Length;

                totalDistance += perDistance;
            }

            return totalDistance;
        }

        private Vector2 calculateAdaptedPosition(Vector2 pos, Vector2 pos2, double radius)
        {
            Vector2 distanceVec = (pos2 - pos);

            double angle = Math.Atan2(distanceVec.Y, distanceVec.X);

            Vector2 calculatedPos = pos2 + new Vector2(
                (float)(-radius * Math.Cos(angle)),
                (float)(-radius * Math.Sin(angle))
            ) * Math.Min((float) radius / 2 * distanceVec.Length, 1);

            return calculatedPos;
        }

        private double applyDiminishingExp(double val) => Math.Pow(val, 0.99);

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        protected override double CalculateInitialStrain(double time) => currentStrain * strainDecay(time - Previous[0].StartTime);

        protected override double StrainValueAt(PrePerNoteStrainSkill[] preSkills, int index, DifficultyHitObject current)
        {
            currentStrain *= strainDecay(current.DeltaTime);
            currentStrain += strainValueOf(preSkills, index, current) * skillMultiplier;

            return currentStrain;
        }
    }
}
