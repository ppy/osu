﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Scoring
{
    internal class CatchScoreProcessor : ScoreProcessor<CatchBaseHit>
    {
        public CatchScoreProcessor(RulesetContainer<CatchBaseHit> rulesetContainer)
            : base(rulesetContainer)
        {
        }

        protected override void SimulateAutoplay(Beatmap<CatchBaseHit> beatmap)
        {
            foreach (var obj in beatmap.HitObjects)
            {
                var fruit = obj as Fruit;

                if (fruit != null)
                    AddJudgement(new CatchJudgement { Result = HitResult.Perfect });
            }

            base.SimulateAutoplay(beatmap);
        }
    }
}
