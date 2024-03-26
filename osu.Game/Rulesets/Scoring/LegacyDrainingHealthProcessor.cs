// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Scoring
{
    /// <summary>
    /// A <see cref="DrainingHealthProcessor"/> that matches legacy drain rate calculations as best as possible.
    /// </summary>
    public abstract partial class LegacyDrainingHealthProcessor : DrainingHealthProcessor
    {
        public Action<string>? OnIterationFail;
        public Action<string>? OnIterationSuccess;

        protected double HpMultiplierNormal { get; private set; }

        private double lowestHpEver;
        private double lowestHpEnd;
        private double hpRecoveryAvailable;

        protected LegacyDrainingHealthProcessor(double drainStartTime)
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
            HpMultiplierNormal = 1;
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
                int topLevelObjectCount = 0;

                foreach (var h in EnumerateTopLevelHitObjects())
                {
                    topLevelObjectCount++;

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

                    double hpReduction = testDrop * (h.GetEndTime() - h.StartTime);
                    double hpOverkill = Math.Max(0, hpReduction - currentHp);
                    reduceHp(hpReduction);

                    foreach (var nested in EnumerateNestedHitObjects(h))
                        increaseHp(nested);

                    // Note: Because HP is capped during the above increases, long sliders (with many ticks) or spinners
                    // will appear to overkill at lower drain levels than they should. However, it is also not correct to simply use the uncapped version.
                    if (hpOverkill > 0 && currentHp - hpOverkill <= lowestHpEver)
                    {
                        fail = true;
                        testDrop *= 0.96;
                        OnIterationFail?.Invoke($"FAILED drop {testDrop}: overkill ({currentHp} - {hpOverkill} <= {lowestHpEver})");
                        break;
                    }

                    increaseHp(h);
                }

                if (topLevelObjectCount == 0)
                    return testDrop;

                if (!fail && currentHp < lowestHpEnd)
                {
                    fail = true;
                    testDrop *= 0.94;
                    HpMultiplierNormal *= 1.01;
                    OnIterationFail?.Invoke($"FAILED drop {testDrop}: end hp too low ({currentHp} < {lowestHpEnd})");
                }

                double recovery = (currentHpUncapped - 1) / Math.Max(1, topLevelObjectCount);

                if (!fail && recovery < hpRecoveryAvailable)
                {
                    fail = true;
                    testDrop *= 0.96;
                    HpMultiplierNormal *= 1.01;
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
                double amount = GetHealthIncreaseFor(hitObject, hitObject.Judgement.MaxResult);
                currentHpUncapped += amount;
                currentHp = Math.Max(0, Math.Min(1, currentHp + amount));
            }
        }

        protected sealed override double GetHealthIncreaseFor(JudgementResult result) => GetHealthIncreaseFor(result.HitObject, result.Type);

        protected abstract IEnumerable<HitObject> EnumerateTopLevelHitObjects();

        protected abstract IEnumerable<HitObject> EnumerateNestedHitObjects(HitObject hitObject);

        protected abstract double GetHealthIncreaseFor(HitObject hitObject, HitResult result);
    }
}
