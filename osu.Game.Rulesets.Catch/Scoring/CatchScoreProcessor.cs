// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Catch.Objects;
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

        protected override void SimulateAutoplay(Beatmap<CatchHitObject> beatmap)
        {
            foreach (var obj in beatmap.HitObjects)
            {
                switch (obj)
                {
                    case JuiceStream stream:
                        foreach (var _ in stream.NestedHitObjects.Cast<CatchHitObject>())
                            AddJudgement(new CatchJudgement { Result = HitResult.Perfect });
                        break;
                    case BananaShower shower:
                        foreach (var _ in shower.NestedHitObjects.Cast<CatchHitObject>())
                            AddJudgement(new CatchJudgement { Result = HitResult.Perfect });
                        AddJudgement(new CatchJudgement { Result = HitResult.Perfect });
                        break;
                    case Fruit _:
                        AddJudgement(new CatchJudgement { Result = HitResult.Perfect });
                        break;
                }
            }

            base.SimulateAutoplay(beatmap);
        }
    }
}
