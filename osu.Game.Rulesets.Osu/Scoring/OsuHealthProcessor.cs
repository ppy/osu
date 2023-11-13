// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
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

            do
            {
                currentHp = 1;
                currentHpUncapped = 1;

                double lowestHp = currentHp;
                double lastTime = DrainStartTime;
                int currentBreak = 0;
                bool fail = false;
                string failReason = string.Empty;

                for (int i = 0; i < Beatmap.HitObjects.Count; i++)
                {
                    HitObject h = Beatmap.HitObjects[i];

                    // Find active break (between current and lastTime)
                    double localLastTime = lastTime;
                    double breakTime = 0;

                    // TODO: This doesn't handle overlapping/sequential breaks correctly (/b/614).
                    // Subtract any break time from the duration since the last object
                    if (Beatmap.Breaks.Count > 0 && currentBreak < Beatmap.Breaks.Count)
                    {
                        BreakPeriod e = Beatmap.Breaks[currentBreak];

                        if (e.StartTime >= localLastTime && e.EndTime <= h.StartTime)
                        {
                            // consider break start equal to object end time for version 8+ since drain stops during this time
                            breakTime = (Beatmap.BeatmapInfo.BeatmapVersion < 8) ? (e.EndTime - e.StartTime) : e.EndTime - localLastTime;
                            currentBreak++;
                        }
                    }

                    reduceHp(testDrop * (h.StartTime - lastTime - breakTime));

                    lastTime = h.GetEndTime();

                    if (currentHp < lowestHp)
                        lowestHp = currentHp;

                    if (currentHp <= lowestHpEver)
                    {
                        fail = true;
                        testDrop *= 0.96;
                        failReason = $"hp too low ({currentHp} < {lowestHpEver})";
                        break;
                    }

                    double hpReduction = testDrop * (h.GetEndTime() - h.StartTime);
                    double hpOverkill = Math.Max(0, hpReduction - currentHp);
                    reduceHp(hpReduction);

                    if (h is Slider slider)
                    {
                        foreach (var nested in slider.NestedHitObjects)
                            increaseHp(nested);
                    }
                    else if (h is Spinner spinner)
                    {
                        foreach (var nested in spinner.NestedHitObjects.Where(t => t is not SpinnerBonusTick))
                            increaseHp(nested);
                    }

                    // Note: Because HP is capped during the above increases, long sliders (with many ticks) or spinners
                    // will appear to overkill at lower drain levels than they should. However, it is also not correct to simply use the uncapped version.
                    if (hpOverkill > 0 && currentHp - hpOverkill <= lowestHpEver)
                    {
                        fail = true;
                        testDrop *= 0.96;
                        failReason = $"overkill ({currentHp} - {hpOverkill} <= {lowestHpEver})";
                        break;
                    }

                    increaseHp(h);
                }

                if (!fail && currentHp < lowestHpEnd)
                {
                    fail = true;
                    testDrop *= 0.94;
                    hpMultiplierNormal *= 1.01;
                    failReason = $"end hp too low ({currentHp} < {lowestHpEnd})";
                }

                double recovery = (currentHpUncapped - 1) / Beatmap.HitObjects.Count;

                if (!fail && recovery < hpRecoveryAvailable)
                {
                    fail = true;
                    testDrop *= 0.96;
                    hpMultiplierNormal *= 1.01;
                    failReason = $"recovery too low ({recovery} < {hpRecoveryAvailable})";
                }

                if (fail)
                {
                    OnIterationFail?.Invoke($"FAILED drop {testDrop}: {failReason}");
                    continue;
                }

                OnIterationSuccess?.Invoke($"PASSED drop {testDrop}");
                return testDrop;
            } while (true);

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
            double increase;

            switch (result)
            {
                case HitResult.SmallTickMiss:
                    return IBeatmapDifficultyInfo.DifficultyRange(Beatmap.Difficulty.DrainRate, -0.02, -0.075, -0.14);

                case HitResult.LargeTickMiss:
                    return IBeatmapDifficultyInfo.DifficultyRange(Beatmap.Difficulty.DrainRate, -0.02, -0.075, -0.14);

                case HitResult.Miss:
                    return IBeatmapDifficultyInfo.DifficultyRange(Beatmap.Difficulty.DrainRate, -0.03, -0.125, -0.2);

                case HitResult.SmallTickHit:
                    // This result is always as a result of the slider tail.
                    increase = 0.02;
                    break;

                case HitResult.LargeTickHit:
                    // This result is either a result of a slider tick or a repeat.
                    increase = hitObject is SliderTick ? 0.015 : 0.02;
                    break;

                case HitResult.Meh:
                    increase = 0.002;
                    break;

                case HitResult.Ok:
                    increase = 0.011;
                    break;

                case HitResult.Good:
                    increase = 0.024;
                    break;

                case HitResult.Great:
                    increase = 0.03;
                    break;

                case HitResult.Perfect:
                    // 1.1 * Great. Unused.
                    increase = 0.033;
                    break;

                case HitResult.SmallBonus:
                    increase = 0.0085;
                    break;

                case HitResult.LargeBonus:
                    increase = 0.01;
                    break;

                default:
                    increase = 0;
                    break;
            }

            return hpMultiplierNormal * increase;
        }
    }
}
