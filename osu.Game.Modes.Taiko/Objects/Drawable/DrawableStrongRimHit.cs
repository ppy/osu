// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Graphics;
using OpenTK.Input;
using osu.Game.Modes.Taiko.Objects.Drawable.Pieces;

namespace osu.Game.Modes.Taiko.Objects.Drawable
{
    public class DrawableStrongRimHit : DrawableStrongHit
    {
        protected override Key[] HitKeys { get; } = { Key.D, Key.K };

        private readonly CirclePiece circlePiece;

        public DrawableStrongRimHit(Hit hit)
            : base(hit)
        {
            Add(circlePiece = new StrongCirclePiece
            {
                Children = new[]
                {
                    new RimHitSymbolPiece()
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            circlePiece.AccentColour = colours.BlueDarker;
        }
    }
}
