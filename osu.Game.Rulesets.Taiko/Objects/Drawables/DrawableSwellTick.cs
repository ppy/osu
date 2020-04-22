// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public class DrawableSwellTick : DrawableTaikoHitObject<SwellTick>
    {
        public override bool DisplayResult => false;

        public DrawableSwellTick(SwellTick hitObject)
            : base(hitObject)
        {
        }

        protected override void UpdateInitialTransforms() => this.FadeOut();

        public void TriggerResult(HitResult type)
        {
            HitObject.StartTime = Time.Current;
            ApplyResult(r => r.Type = type);
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
        }

        public override bool OnPressed(TaikoAction action) => false;

        protected override SkinnableDrawable CreateMainPiece() => new SkinnableDrawable(new TaikoSkinComponent(TaikoSkinComponents.DrumRollTick),
            _ => new TickPiece());
    }
}
