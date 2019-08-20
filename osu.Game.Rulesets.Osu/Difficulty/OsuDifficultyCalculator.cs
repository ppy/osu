// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;

using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.RootFinding;

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Skills;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Difficulty.MathUtil;


namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuDifficultyCalculator : DifficultyCalculator
    {
        private const double probabilityThreshold = 0.02;
        private const double tpMin = 0.1;
        private const double tpMax = 100;
        private const double tpPrecision = 1e-8;

        private const double aimMultiplier = 0.585;
        private const double tapMultiplier = 0.778;
        private const double srExponent = 0.85;


        public OsuDifficultyCalculator(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }


        protected override DifficultyAttributes Calculate(IBeatmap beatmap, Mod[] mods, double clockRate)
        {
            var hitObjectsNoSpinner = beatmap.HitObjects.Where(obj => !(obj is Spinner))
                                                        .Select(obj => (OsuHitObject)obj).ToList();
            if (beatmap.HitObjects.Count == 0)
                return new OsuDifficultyAttributes { Mods = mods};

            (var strainHistory, var maxTapStrain) = calculateTapStrain(hitObjectsNoSpinner, clockRate);
            double tapDiff = maxTapStrain.Average();

            IEnumerable<OsuMovement> movements = createMovements(hitObjectsNoSpinner, clockRate, strainHistory);
            double aimDiff = calculateThroughput(movements);

            double tapSR = tapMultiplier * Math.Pow(tapDiff, srExponent);
            double aimSR = aimMultiplier * Math.Pow(aimDiff, srExponent);
            double sr = Mean.PowerMean(tapSR, aimSR, 7) * 1.069;

            // Todo: These int casts are temporary to achieve 1:1 results with osu!stable, and should be removed in the future
            double hitWindowGreat = (int)(beatmap.HitObjects.First().HitWindows.Great / 2) / clockRate;
            double preempt = (int)BeatmapDifficulty.DifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.ApproachRate, 1800, 1200, 450) / clockRate;

            int maxCombo = beatmap.HitObjects.Count;
            // Add the ticks + tail of the slider. 1 is subtracted because the head circle would be counted twice (once for the slider itself in the line above)
            maxCombo += beatmap.HitObjects.OfType<Slider>().Sum(s => s.NestedHitObjects.Count - 1);

            return new OsuDifficultyAttributes
            {
                StarRating = sr,
                Mods = mods,
                AimStrain = aimSR,
                SpeedStrain = tapSR,
                ApproachRate = preempt > 1200 ? (1800 - preempt) / 120 : (1200 - preempt) / 150 + 5,
                OverallDifficulty = (80 - hitWindowGreat) / 6,
                MaxCombo = maxCombo
            };
        }

        private (List<Vector<double>>, Vector<double>) calculateTapStrain(List<OsuHitObject> hitObjects, double clockRate)
        {
            var decayCoeffs = Vector<double>.Build.Dense(Generate.LinearSpaced(4, 1.7, -1.6)).PointwiseExp();
            double prevTime = 0;
            var currStrain = decayCoeffs * 0;
            var maxStrain = decayCoeffs * 0;
            var strainHistory = new List<Vector<double>>();

            foreach (var obj in hitObjects)
            {
                double currTime = obj.StartTime / 1000.0;
                currStrain = currStrain.PointwiseMultiply((-decayCoeffs * (currTime - prevTime) / clockRate).PointwiseExp());
                maxStrain = maxStrain.PointwiseMaximum(currStrain);
                strainHistory.Add(currStrain);

                currStrain += decayCoeffs;
                prevTime = currTime;
            }

            return (strainHistory, maxStrain);
        }


        private List<OsuMovement> createMovements(List<OsuHitObject> hitObjects, double clockRate, List<Vector<double>> strainHistory)
        {
            OsuMovement.Initialize();
            var movements = new List<OsuMovement>();

            for (int i = 1; i < hitObjects.Count; i++)
            {
                var obj0 = i > 1 ? hitObjects[i-2] : null;
                var obj1 = hitObjects[i-1];
                var obj2 = hitObjects[i];
                var obj3 = i < hitObjects.Count - 1 ? hitObjects[i+1] : null;
                var tapStrain = strainHistory[i];

                movements.Add(new OsuMovement(obj0, obj1, obj2, obj3, tapStrain, clockRate));
            }

            return movements;
        }

        private double calculateThroughput(IEnumerable<OsuMovement> movements)
        {
            double fcProbabilityTPMin = calculateFCProbability(movements, tpMin);

            if (fcProbabilityTPMin >= probabilityThreshold)
                return tpMin;

            double fcProbabilityTPMax = calculateFCProbability(movements, tpMax);

            if (fcProbabilityTPMax <= probabilityThreshold)
                return tpMax;

            Func<double, double> fcProbMinusThreshold = tp => calculateFCProbability(movements, tp) - probabilityThreshold;

            return Brent.FindRoot(fcProbMinusThreshold, tpMin, tpMax, tpPrecision);


        }

        private double calculateFCProbability(IEnumerable<OsuMovement> movements, double tp)
        {
            double fcProbability = 1;

            foreach (OsuMovement movement in movements)
            {
                double hitProbability = FittsLaw.CalculateHitProbability(movement.D, movement.MT, tp);
                fcProbability *= hitProbability;
            }
            return fcProbability;
        }

        
        //protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        //{
        //    if (beatmap.HitObjects.Count == 0)
        //        return new OsuDifficultyAttributes { Mods = mods };

        //    double aimRating = Math.Sqrt(skills[0].DifficultyValue()) * difficulty_multiplier;
        //    double speedRating = Math.Sqrt(skills[1].DifficultyValue()) * difficulty_multiplier;
        //    double starRating = aimRating + speedRating + Math.Abs(aimRating - speedRating) / 2;

        //    // Todo: These int casts are temporary to achieve 1:1 results with osu!stable, and should be removed in the future
        //    double hitWindowGreat = (int)(beatmap.HitObjects.First().HitWindows.Great / 2) / clockRate;
        //    double preempt = (int)BeatmapDifficulty.DifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.ApproachRate, 1800, 1200, 450) / clockRate;

        //    int maxCombo = beatmap.HitObjects.Count;
        //    // Add the ticks + tail of the slider. 1 is subtracted because the head circle would be counted twice (once for the slider itself in the line above)
        //    maxCombo += beatmap.HitObjects.OfType<Slider>().Sum(s => s.NestedHitObjects.Count - 1);

        //    return new OsuDifficultyAttributes
        //    {
        //        StarRating = starRating,
        //        Mods = mods,
        //        AimStrain = aimRating,
        //        SpeedStrain = speedRating,
        //        ApproachRate = preempt > 1200 ? (1800 - preempt) / 120 : (1200 - preempt) / 150 + 5,
        //        OverallDifficulty = (80 - hitWindowGreat) / 6,
        //        MaxCombo = maxCombo
        //    };
        //}

        

        protected override Skill[] CreateSkills(IBeatmap beatmap) => new Skill[]
        {
            new Aim(),
            new Speed()
        };

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate)
        {
            throw new NotImplementedException();
        }

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        {
            throw new NotImplementedException();
        }

        protected override Mod[] DifficultyAdjustmentMods => new Mod[]
        {
            new OsuModDoubleTime(),
            new OsuModHalfTime(),
            new OsuModEasy(),
            new OsuModHardRock(),
        };
    }
}
