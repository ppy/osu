// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Modes.Taiko.Objects.Drawables.Pieces;
using OpenTK.Input;

namespace osu.Game.Modes.Taiko.Objects.Drawables
{
    public class DrawableStrongCentreHit : DrawableStrongHit
    {
        protected override Key[] HitKeys { get; } = { Key.F, Key.J };

        public DrawableStrongCentreHit(Hit hit)
            : base(hit)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Circle.AccentColour = colours.PinkDarker;
        }

        protected override CirclePiece CreateCirclePiece() => new StrongCirclePiece
        {
            Children = new[] { new CentreHitSymbolPiece() }
        };
    }
}
