// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Skills;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuDifficultyCalculator : DifficultyCalculator
    {
        public OsuDifficultyCalculator(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        private IEnumerable<OsuHitObjectDifficulty> hitObjectDifficulties(OsuSkill aim, OsuSkill speed)
        {
            using (var timesIt = aim.Timestamps.GetEnumerator())
            using (var aimStarsIt = aim.HitObjectStars().GetEnumerator())
            using (var aimCumStarsIt = aim.CumulativeHitObjectStars().GetEnumerator())
            using (var speedStarsIt = speed.HitObjectStars().GetEnumerator())
            using (var speedCumStarsIt = speed.CumulativeHitObjectStars().GetEnumerator())
            {
                while (timesIt.MoveNext() && aimStarsIt.MoveNext() && aimCumStarsIt.MoveNext() && speedStarsIt.MoveNext() && speedCumStarsIt.MoveNext())
                {
                    yield return new OsuHitObjectDifficulty
                    {
                        Time = timesIt.Current,
                        AimStars = aimStarsIt.Current,
                        AimCumulativeStars = aimCumStarsIt.Current,
                        SpeedStars = speedStarsIt.Current,
                        SpeedCumulativeStars = speedCumStarsIt.Current
                    };
                }
            }
        }

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        {
            var aim = (OsuSkill)skills[0];
            var speed = (OsuSkill)skills[1];

            if (beatmap.HitObjects.Count == 0)
                return new OsuDifficultyAttributes { Mods = mods };

            IList<double> aimComboSr = aim.ComboStarRatings;
            IList<double> aimMissCounts = aim.MissCounts;

            IList<double> speedComboSr = speed.ComboStarRatings;
            IList<double> speedMissCounts = speed.MissCounts;

            const double miss_sr_increment = OsuSkill.MISS_STAR_RATING_INCREMENT;

            double aimRating = aimComboSr.Last();
            double speedRating = speedComboSr.Last();
            double starRating = aimRating + speedRating + Math.Abs(aimRating - speedRating) / 2;

            // Todo: These int casts are temporary to achieve 1:1 results with osu!stable, and should be removed in the future
            double hitWindowGreat = (int)(beatmap.HitObjects.First().HitWindows.Great / 2) / clockRate;
            double preempt = (int)BeatmapDifficulty.DifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.ApproachRate, 1800, 1200, 450) / clockRate;

            int maxCombo = beatmap.HitObjects.Count;
            // Add the ticks + tail of the slider. 1 is subtracted because the head circle would be counted twice (once for the slider itself in the line above)
            maxCombo += beatmap.HitObjects.OfType<Slider>().Sum(s => s.NestedHitObjects.Count - 1);

            return new OsuDifficultyAttributes
            {
                StarRating = starRating,
                Mods = mods,
                MissStarRatingIncrement = miss_sr_increment,
                AimStrain = aimRating,
                AimComboStarRatings = aimComboSr,
                AimMissCounts = aimMissCounts,
                SpeedStrain = speedRating,
                SpeedComboStarRatings = speedComboSr,
                SpeedMissCounts = speedMissCounts,
                ApproachRate = preempt > 1200 ? (1800 - preempt) / 120 : (1200 - preempt) / 150 + 5,
                OverallDifficulty = (80 - hitWindowGreat) / 6,
                MaxCombo = maxCombo,
                HitObjectDifficulties = hitObjectDifficulties(aim, speed).Where(x => x.AimStars != 0).ToList(), // only used for charts
            };
        }

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate)
        {
            // The first jump is formed by the first two hitobjects of the map.
            // If the map has less than two OsuHitObjects, the enumerator will not return anything.
            for (int i = 1; i < beatmap.HitObjects.Count; i++)
            {
                var lastLast = i > 1 ? beatmap.HitObjects[i - 2] : null;
                var last = beatmap.HitObjects[i - 1];
                var current = beatmap.HitObjects[i];

                yield return new OsuDifficultyHitObject(current, lastLast, last, clockRate);
            }
        }

        protected override Skill[] CreateSkills(IBeatmap beatmap) => new Skill[]
        {
            new Aim(),
            new Speed()
        };

        protected override Mod[] DifficultyAdjustmentMods => new Mod[]
        {
            new OsuModDoubleTime(),
            new OsuModHalfTime(),
            new OsuModEasy(),
            new OsuModHardRock(),
        };
    }
}
