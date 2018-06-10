// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Catch.Objects.Drawable
{
    public class DrawableTinyDroplet : DrawableDroplet
    {
        public DrawableTinyDroplet(Droplet h)
            : base(h)
        {
            Size = new Vector2((float)CatchHitObject.OBJECT_RADIUS) / 8;
        }

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (CheckPosition == null) return;

            if (timeOffset >= 0)
                AddJudgement(new CatchTinyDropletJudgement { Result = CheckPosition.Invoke(HitObject) ? HitResult.Perfect : HitResult.Miss });
        }
    }
}
