// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Catch.Scoring
{
    public partial class CatchHealthProcessor : DrainingHealthProcessor
    {
        public Action<string>? OnIterationFail;
        public Action<string>? OnIterationSuccess;

        private double lowestHpEver;
        private double lowestHpEnd;
        private double hpRecoveryAvailable;
        private double hpMultiplierNormal;

        public CatchHealthProcessor(double drainStartTime)
            : base(drainStartTime)
        {
        }

        public override void ApplyBeatmap(IBeatmap beatmap)
        {
            lowestHpEver = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.DrainRate, 0.975, 0.8, 0.3);
            lowestHpEnd = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.DrainRate, 0.99, 0.9, 0.4);
            hpRecoveryAvailable = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.DrainRate, 0.04, 0.02, 0);

            base.ApplyBeatmap(beatmap);
        }

        protected override void Reset(bool storeResults)
        {
            hpMultiplierNormal = 1;
            base.Reset(storeResults);
        }

        protected override double ComputeDrainRate()
        {
            double testDrop = 0.00025;
            double currentHp;
            double currentHpUncapped;

            while (true)
            {
                currentHp = 1;
                currentHpUncapped = 1;

                double lowestHp = currentHp;
                double lastTime = DrainStartTime;
                int currentBreak = 0;
                bool fail = false;

                List<HitObject> allObjects = EnumerateHitObjects(Beatmap).Where(h => h is Fruit || h is Droplet || h is Banana).ToList();

                for (int i = 0; i < allObjects.Count; i++)
                {
                    HitObject h = allObjects[i];

                    while (currentBreak < Beatmap.Breaks.Count && Beatmap.Breaks[currentBreak].EndTime <= h.StartTime)
                    {
                        // If two hitobjects are separated by a break period, there is no drain for the full duration between the hitobjects.
                        // This differs from legacy (version < 8) beatmaps which continue draining until the break section is entered,
                        // but this shouldn't have a noticeable impact in practice.
                        lastTime = h.StartTime;
                        currentBreak++;
                    }

                    reduceHp(testDrop * (h.StartTime - lastTime));

                    lastTime = h.GetEndTime();

                    if (currentHp < lowestHp)
                        lowestHp = currentHp;

                    if (currentHp <= lowestHpEver)
                    {
                        fail = true;
                        testDrop *= 0.96;
                        OnIterationFail?.Invoke($"FAILED drop {testDrop}: hp too low ({currentHp} < {lowestHpEver})");
                        break;
                    }

                    increaseHp(h);
                }

                if (!fail && currentHp < lowestHpEnd)
                {
                    fail = true;
                    testDrop *= 0.94;
                    hpMultiplierNormal *= 1.01;
                    OnIterationFail?.Invoke($"FAILED drop {testDrop}: end hp too low ({currentHp} < {lowestHpEnd})");
                }

                double recovery = (currentHpUncapped - 1) / allObjects.Count;

                if (!fail && recovery < hpRecoveryAvailable)
                {
                    fail = true;
                    testDrop *= 0.96;
                    hpMultiplierNormal *= 1.01;
                    OnIterationFail?.Invoke($"FAILED drop {testDrop}: recovery too low ({recovery} < {hpRecoveryAvailable})");
                }

                if (!fail)
                {
                    OnIterationSuccess?.Invoke($"PASSED drop {testDrop}");
                    return testDrop;
                }
            }

            void reduceHp(double amount)
            {
                currentHpUncapped = Math.Max(0, currentHpUncapped - amount);
                currentHp = Math.Max(0, currentHp - amount);
            }

            void increaseHp(HitObject hitObject)
            {
                double amount = healthIncreaseFor(hitObject.CreateJudgement().MaxResult);
                currentHpUncapped += amount;
                currentHp = Math.Max(0, Math.Min(1, currentHp + amount));
            }
        }

        protected override double GetHealthIncreaseFor(JudgementResult result) => healthIncreaseFor(result.Type);

        private double healthIncreaseFor(HitResult result)
        {
            double increase = 0;

            switch (result)
            {
                case HitResult.SmallTickMiss:
                    return 0;

                case HitResult.LargeTickMiss:
                case HitResult.Miss:
                    return IBeatmapDifficultyInfo.DifficultyRange(Beatmap.Difficulty.DrainRate, -0.03, -0.125, -0.2);

                case HitResult.SmallTickHit:
                    increase = 0.0015;
                    break;

                case HitResult.LargeTickHit:
                    increase = 0.015;
                    break;

                case HitResult.Great:
                    increase = 0.03;
                    break;

                case HitResult.LargeBonus:
                    increase = 0.0025;
                    break;
            }

            return hpMultiplierNormal * increase;
        }
    }
}
