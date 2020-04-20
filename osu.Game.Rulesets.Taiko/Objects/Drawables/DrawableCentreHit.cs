// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public class DrawableCentreHit : DrawableHit
    {
        public override TaikoAction[] HitActions { get; } = { TaikoAction.LeftCentre, TaikoAction.RightCentre };

        public DrawableCentreHit(Hit hit)
            : base(hit)
        {
            MainPiece.Add(new CentreHitSymbolPiece());
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            MainPiece.AccentColour = colours.PinkDarker;
        }
    }
}
