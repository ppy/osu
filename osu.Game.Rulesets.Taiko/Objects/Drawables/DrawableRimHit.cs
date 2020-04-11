// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public class DrawableRimHit : DrawableHit
    {
        public override TaikoAction[] HitActions { get; } = { TaikoAction.LeftRim, TaikoAction.RightRim };

        public DrawableRimHit(Hit hit)
            : base(hit)
        {
        }

        protected override CompositeDrawable CreateMainPiece() => new RimHitCirclePiece();
    }
}
