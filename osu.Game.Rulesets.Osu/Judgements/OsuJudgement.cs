// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Judgements
{
    public class OsuJudgement : Judgement
    {
        public override HitResult MaxResult => HitResult.Great;

        /// <summary>
        /// The positional hit offset.
        /// </summary>
        public Vector2 PositionOffset;

        protected override int NumericResultFor(HitResult result)
        {
            switch (result)
            {
                default:
                    return 0;
                case HitResult.Meh:
                    return 50;
                case HitResult.Good:
                    return 100;
                case HitResult.Great:
                    return 300;
            }
        }

        public ComboResult Combo;
    }
}