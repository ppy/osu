using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Vitaru.Objects;
using System;
using osu.Game.Rulesets.Vitaru.Beatmaps;
using osu.Game.Rulesets.Mods;
using System.Linq;
using osu.Game.Rulesets.Vitaru.Mods;
using osu.Game.Rulesets.Vitaru.Settings;

namespace osu.Game.Rulesets.Vitaru.Scoring
{
    public class VitaruPerformanceCalculator : PerformanceCalculator<VitaruHitObject>
    {
        private readonly ScoringMetric currentScoringMetric = VitaruSettings.VitaruConfigManager.GetBindable<ScoringMetric>(VitaruSetting.ScoringMetric);

        private const float pp_multiplier = 1f;

        public static float CurrentPPValue = 0;
        public static float MaxPPValue = 0;

        public VitaruPerformanceCalculator(Ruleset ruleset, Beatmap beatmap, Score score) : base(ruleset, beatmap, score)
        {
            CurrentPPValue = 0;
            MaxPPValue = 0;
        }

        public override double Calculate(Dictionary<string, double> categoryRatings = null)
        {
            Mod[] mods = Score.Mods;
            double accuracy = Score.Accuracy;
            int scoreMaxCombo = Score.MaxCombo;
            float difficulty = 1;

            double ar = Beatmap.BeatmapInfo.BaseDifficulty.ApproachRate;
            double cs = Beatmap.BeatmapInfo.BaseDifficulty.CircleSize;

            int count300 = Convert.ToInt32(Score.Statistics[HitResult.Great]);
            int count200 = Convert.ToInt32(Score.Statistics[HitResult.Good]);
            int count100 = Convert.ToInt32(Score.Statistics[HitResult.Meh]);
            //int count10 = Convert.ToInt32(Score.Statistics[HitResult.Tick]);
            int countMiss = Convert.ToInt32(Score.Statistics[HitResult.Miss]);

            if (currentScoringMetric == ScoringMetric.ScoreZones)
            {
                // Don't count scores made with supposedly unranked mods
                if (mods.Any(m => !m.Ranked))
                    difficulty = 0;

                if (mods.Any(m => m is VitaruModNoFail))
                    difficulty *= 0.90f;
            }

            return difficulty * Score.TotalScore * pp_multiplier;
        }

        protected override BeatmapConverter<VitaruHitObject> CreateBeatmapConverter() => new VitaruBeatmapConverter();
    }
}
