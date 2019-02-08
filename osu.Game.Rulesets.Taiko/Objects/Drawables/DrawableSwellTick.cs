// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public class DrawableSwellTick : DrawableTaikoHitObject<SwellTick>
    {
        public override bool DisplayResult => false;

        public DrawableSwellTick(SwellTick hitObject)
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
