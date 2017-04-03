// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using osu.Game.Modes.Taiko.Objects.Drawable.Pieces;
using osu.Game.Graphics;
using osu.Framework.Allocation;

namespace osu.Game.Modes.Taiko.Objects.Drawable
{
    public class DrawableCentreHit : DrawableHit
    {
        protected override Key[] HitKeys { get; } = { Key.F, Key.J };

        public DrawableCentreHit(Hit hit)
            : base(hit)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Circle.AccentColour = colours.PinkDarker;
        }

        protected override CirclePiece CreateCirclePiece() => new CirclePiece
        {
            Children = new[] { new CentreHitSymbolPiece() }
        };
    }
}
