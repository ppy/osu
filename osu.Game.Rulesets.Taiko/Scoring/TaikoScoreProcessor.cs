// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko.Scoring
{
    internal class TaikoScoreProcessor : ScoreProcessor<TaikoHitObject>
    {
        /// <summary>
        /// The HP awarded by a <see cref="HitResult.Great"/> hit.
        /// </summary>
        private const double hp_hit_great = 0.03;

        /// <summary>
        /// Taiko fails at the end of the map if the player has not half-filled their HP bar.
        /// </summary>
        protected override bool DefaultFailCondition => JudgedHits == MaxHits && Health.Value <= 0.5;

        private double hpMultiplier;
        private double hpMissMultiplier;

        public TaikoScoreProcessor(RulesetContainer<TaikoHitObject> rulesetContainer)
            : base(rulesetContainer)
        {
        }

        protected override void ApplyBeatmap(Beatmap<TaikoHitObject> beatmap)
        {
            base.ApplyBeatmap(beatmap);

            hpMultiplier = 0.01 / (hp_hit_great * beatmap.HitObjects.FindAll(o => o is Hit).Count * BeatmapDifficulty.DifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.DrainRate, 0.5, 0.75, 0.98));

            hpMissMultiplier = BeatmapDifficulty.DifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.DrainRate, 0.0018, 0.0075, 0.0120);
        }

        protected override void ApplyResult(JudgementResult result)
        {
            base.ApplyResult(result);

            double hpIncrease = result.Judgement.HealthIncreaseFor(result);

            if (result.Type == HitResult.Miss)
                hpIncrease *= hpMissMultiplier;
            else
                hpIncrease *= hpMultiplier;

            Health.Value += hpIncrease;
        }

        protected override void Reset(bool storeResults)
        {
            base.Reset(storeResults);

            Health.Value = 0;
        }
    }
}
