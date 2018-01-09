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
    internal class CatchScoreProcessor : ScoreProcessor<CatchHitObject>
    {
        public CatchScoreProcessor(RulesetContainer<CatchHitObject> rulesetContainer)
            : base(rulesetContainer)
        {
        }

        protected override void SimulateAutoplay(Beatmap<CatchHitObject> beatmap)
        {
            foreach (var obj in beatmap.HitObjects)
            {
                var stream = obj as JuiceStream;

                if (stream != null)
                {
                    AddJudgement(new CatchJudgement { Result = HitResult.Perfect });
                    AddJudgement(new CatchJudgement { Result = HitResult.Perfect });

                    foreach (var unused in stream.NestedHitObjects.OfType<CatchHitObject>())
                        AddJudgement(new CatchJudgement { Result = HitResult.Perfect });

                    continue;
                }

                var fruit = obj as Fruit;

                if (fruit != null)
                    AddJudgement(new CatchJudgement { Result = HitResult.Perfect });
            }

            base.SimulateAutoplay(beatmap);
        }
    }
}
