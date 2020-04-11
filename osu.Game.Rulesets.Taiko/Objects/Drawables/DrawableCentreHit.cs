// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public class DrawableCentreHit : DrawableHit
    {
        public override TaikoAction[] HitActions { get; } = { TaikoAction.LeftCentre, TaikoAction.RightCentre };

        public DrawableCentreHit(Hit hit)
            : base(hit)
        {
        }

        protected override CompositeDrawable CreateMainPiece() => new CentreHitCirclePiece();
    }
}
