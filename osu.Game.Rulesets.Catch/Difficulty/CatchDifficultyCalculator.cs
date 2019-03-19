// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Difficulty.Preprocessing;
using osu.Game.Rulesets.Catch.Difficulty.Skills;
using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Difficulty
{
    public class CatchDifficultyCalculator : DifficultyCalculator
    {
        private const double star_scaling_factor = 0.145;

        protected override int SectionLength => 750;

        public CatchDifficultyCalculator(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        {
            if (beatmap.HitObjects.Count == 0)
                return new CatchDifficultyAttributes { Mods = mods };

            // this is the same as osu!, so there's potential to share the implementation... maybe
            double preempt = BeatmapDifficulty.DifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.ApproachRate, 1800, 1200, 450) / clockRate;

            return new CatchDifficultyAttributes
            {
                StarRating = Math.Sqrt(skills[0].DifficultyValue()) * star_scaling_factor,
                Mods = mods,
                ApproachRate = preempt > 1200.0 ? -(preempt - 1800.0) / 120.0 : -(preempt - 1200.0) / 150.0 + 5.0,
                MaxCombo = beatmap.HitObjects.Count(h => h is Fruit) + beatmap.HitObjects.OfType<JuiceStream>().SelectMany(j => j.NestedHitObjects).Count(h => !(h is TinyDroplet))
            };
        }

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate)
        {
            float halfCatchWidth;

            using (var catcher = new CatcherArea.Catcher(beatmap.BeatmapInfo.BaseDifficulty))
            {
                halfCatchWidth = catcher.CatchWidth * 0.5f;
                halfCatchWidth *= 0.8f; // We're only using 80% of the catcher's width to simulate imperfect gameplay.
            }

            CatchHitObject lastObject = null;

            foreach (var hitObject in beatmap.HitObjects.OfType<CatchHitObject>())
            {
                if (lastObject == null)
                {
                    lastObject = hitObject;
                    continue;
                }

                switch (hitObject)
                {
                    // We want to only consider fruits that contribute to the combo. Droplets are addressed as accuracy and spinners are not relevant for "skill" calculations.
                    case Fruit fruit:
                        yield return new CatchDifficultyHitObject(fruit, lastObject, clockRate, halfCatchWidth);

                        lastObject = hitObject;
                        break;
                    case JuiceStream _:
                        foreach (var nested in hitObject.NestedHitObjects.OfType<CatchHitObject>().Where(o => !(o is TinyDroplet)))
                        {
                            yield return new CatchDifficultyHitObject(nested, lastObject, clockRate, halfCatchWidth);

                            lastObject = nested;
                        }

                        break;
                }
            }
        }

        protected override Skill[] CreateSkills(IBeatmap beatmap) => new Skill[]
        {
            new Movement(),
        };

        protected override Mod[] DifficultyAdjustmentMods => new Mod[]
        {
            new CatchModDoubleTime(),
            new CatchModHalfTime(),
            new CatchModHardRock(),
            new CatchModEasy(),
        };
    }
}
