// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Judgements;

namespace osu.Game.Rulesets.Catch.Objects
{
    public class Banana : Fruit
    {
        public override FruitVisualRepresentation VisualRepresentation => FruitVisualRepresentation.Banana;

        public override Judgement CreateJudgement() => new CatchBananaJudgement();
    }
}
