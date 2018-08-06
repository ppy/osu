// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public class DrawableSwellTick : DrawableTaikoHitObject
    {
        public override bool DisplayResult => false;

        public DrawableSwellTick(TaikoHitObject hitObject)
            : base(hitObject)
        {
        }

        public void TriggerResult(HitResult type) => ApplyResult(r => r.Type = type);

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
        }

        protected override void UpdateState(ArmedState state)
        {
        }

        public override bool OnPressed(TaikoAction action) => false;
    }
}
