// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public class DrawableRimHit : DrawableHit
    {
        public override TaikoAction[] HitActions { get; } = { TaikoAction.LeftRim, TaikoAction.RightRim };

        public DrawableRimHit(Hit hit)
            : base(hit)
        {
        }

        protected override SkinnableDrawable CreateMainPiece() => new SkinnableDrawable(new TaikoSkinComponent(TaikoSkinComponents.RimHit),
            _ => new RimHitCirclePiece(), confineMode: ConfineMode.ScaleToFit);
    }
}
