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
        private const double star_scaling_factor = 0.137;

        protected override int SectionLength => 750;

        private float halfCatcherWidth;

        public CatchDifficultyCalculator(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        {
            if (beatmap.HitObjects.Count == 0)
                return new CatchDifficultyAttributes { Mods = mods, Skills = skills };

            // this is the same as osu!, so there's potential to share the implementation... maybe
            double preempt = BeatmapDifficulty.DifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.ApproachRate, 1800, 1200, 450) / clockRate;

            return new CatchDifficultyAttributes
            {
                DirectionChangeCount = ((Movement)skills[0]).DirectionChangeCount,
                StarRating = Math.Sqrt(skills[0].DifficultyValue()) * star_scaling_factor,
                Mods = mods,
                ApproachRate = preempt > 1200.0 ? -(preempt - 1800.0) / 120.0 : -(preempt - 1200.0) / 150.0 + 5.0,
                MaxCombo = beatmap.HitObjects.Count(h => h is Fruit) + beatmap.HitObjects.OfType<JuiceStream>().SelectMany(j => j.NestedHitObjects).Count(h => !(h is TinyDroplet)),
                Skills = skills
            };
        }

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate)
        {
            CatchHitObject lastObject = null;

            // In 2B beatmaps, it is possible that a normal Fruit is placed in the middle of a JuiceStream.
            foreach (var hitObject in beatmap.HitObjects
                                             .SelectMany(obj => obj is JuiceStream stream ? stream.NestedHitObjects : new[] { obj })
                                             .Cast<CatchHitObject>()
                                             .OrderBy(x => x.StartTime))
            {
                // We want to only consider fruits that contribute to the combo.
                if (hitObject is BananaShower || hitObject is TinyDroplet)
                    continue;

                if (lastObject != null)
                    yield return new CatchDifficultyHitObject(hitObject, lastObject, clockRate, halfCatcherWidth);

                lastObject = hitObject;
            }
        }

        protected override Skill[] CreateSkills(IBeatmap beatmap)
        {
            halfCatcherWidth = Catcher.CalculateCatchWidth(beatmap.BeatmapInfo.BaseDifficulty) * 0.5f;

            // For circle sizes above 5.5, reduce the catcher width further to simulate imperfect gameplay.
            halfCatcherWidth *= 1 - (Math.Max(0, beatmap.BeatmapInfo.BaseDifficulty.CircleSize - 5.5f) * 0.0625f);

            return new Skill[]
            {
                new Movement(halfCatcherWidth),
            };
        }

        protected override Mod[] DifficultyAdjustmentMods => new Mod[]
        {
            new CatchModDoubleTime(),
            new CatchModHalfTime(),
            new CatchModHardRock(),
            new CatchModEasy(),
        };
    }
}
