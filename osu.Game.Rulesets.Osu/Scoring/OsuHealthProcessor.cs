// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Scoring
{
    public partial class OsuHealthProcessor : DrainingHealthProcessor
    {
        public Action<string>? OnIterationFail;
        public Action<string>? OnIterationSuccess;

        private double lowestHpEver;
        private double lowestHpEnd;
        private double hpRecoveryAvailable;
        private double hpMultiplierNormal;

        public OsuHealthProcessor(double drainStartTime)
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

                for (int i = 0; i < Beatmap.HitObjects.Count; i++)
                {
                    HitObject h = Beatmap.HitObjects[i];

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

                    switch (h)
                    {
                        case Slider slider:
                        {
                            foreach (var nested in slider.NestedHitObjects)
                                increaseHp(nested);
                            break;
                        }

                        case Spinner spinner:
                        {
                            foreach (var nested in spinner.NestedHitObjects.Where(t => t is not SpinnerBonusTick))
                                increaseHp(nested);
                            break;
                        }
                    }

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

                if (!fail && currentHp < lowestHpEnd)
                {
                    fail = true;
                    testDrop *= 0.94;
                    hpMultiplierNormal *= 1.01;
                    OnIterationFail?.Invoke($"FAILED drop {testDrop}: end hp too low ({currentHp} < {lowestHpEnd})");
                }

                double recovery = (currentHpUncapped - 1) / Beatmap.HitObjects.Count;

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
                double amount = healthIncreaseFor(hitObject, hitObject.CreateJudgement().MaxResult);
                currentHpUncapped += amount;
                currentHp = Math.Max(0, Math.Min(1, currentHp + amount));
            }
        }

        protected override double GetHealthIncreaseFor(JudgementResult result) => healthIncreaseFor(result.HitObject, result.Type);

        private double healthIncreaseFor(HitObject hitObject, HitResult result)
        {
            double increase = 0;

            switch (result)
            {
                case HitResult.SmallTickMiss:
                    return IBeatmapDifficultyInfo.DifficultyRange(Beatmap.Difficulty.DrainRate, -0.02, -0.075, -0.14);

                case HitResult.LargeTickMiss:
                    return IBeatmapDifficultyInfo.DifficultyRange(Beatmap.Difficulty.DrainRate, -0.02, -0.075, -0.14);

                case HitResult.Miss:
                    return IBeatmapDifficultyInfo.DifficultyRange(Beatmap.Difficulty.DrainRate, -0.03, -0.125, -0.2);

                case HitResult.SmallTickHit:
                    // This result always comes from the slider tail, which is judged the same as a repeat.
                    increase = 0.02;
                    break;

                case HitResult.LargeTickHit:
                    // This result comes from either a slider tick or repeat.
                    increase = hitObject is SliderTick ? 0.015 : 0.02;
                    break;

                case HitResult.Meh:
                    increase = 0.002;
                    break;

                case HitResult.Ok:
                    increase = 0.011;
                    break;

                case HitResult.Great:
                    increase = 0.03;
                    break;

                case HitResult.SmallBonus:
                    increase = 0.0085;
                    break;

                case HitResult.LargeBonus:
                    increase = 0.01;
                    break;
            }

            return hpMultiplierNormal * increase;
        }
    }
}
