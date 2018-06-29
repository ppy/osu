// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Scoring
{
    public class CatchScoreProcessor : ScoreProcessor<CatchHitObject>
    {
        public CatchScoreProcessor(RulesetContainer<CatchHitObject> rulesetContainer)
            : base(rulesetContainer)
        {
        }

        private float hpDrainRate;

        protected override void SimulateAutoplay(Beatmap<CatchHitObject> beatmap)
        {
            hpDrainRate = beatmap.BeatmapInfo.BaseDifficulty.DrainRate;

            foreach (var obj in beatmap.HitObjects)
            {
                switch (obj)
                {
                    case JuiceStream stream:
                        foreach (var nestedObject in stream.NestedHitObjects)
                            switch (nestedObject)
                            {
                                case TinyDroplet _:
                                    AddJudgement(new CatchTinyDropletJudgement { Result = HitResult.Perfect });
                                    break;
                                case Droplet _:
                                    AddJudgement(new CatchDropletJudgement { Result = HitResult.Perfect });
                                    break;
                                case Fruit _:
                                    AddJudgement(new CatchJudgement { Result = HitResult.Perfect });
                                    break;
                            }
                        break;
                    case BananaShower shower:
                        foreach (var _ in shower.NestedHitObjects.Cast<CatchHitObject>())
                            AddJudgement(new CatchBananaJudgement { Result = HitResult.Perfect });
                        break;
                    case Fruit _:
                        AddJudgement(new CatchJudgement { Result = HitResult.Perfect });
                        break;
                }
            }
        }

        private const double harshness = 0.01;

        protected override void OnNewJudgement(Judgement judgement)
        {
            base.OnNewJudgement(judgement);

            if (judgement.Result == HitResult.Miss)
            {
                if (!judgement.IsBonus)
                    Health.Value -= hpDrainRate * (harshness * 2);
                return;
            }

            if (judgement is CatchJudgement catchJudgement)
                Health.Value += Math.Max(catchJudgement.HealthIncrease - hpDrainRate, 0) * harshness;
        }
    }
}
