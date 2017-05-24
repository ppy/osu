// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces;
using OpenTK.Input;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public class DrawableRimHitStrong : DrawableHitStrong
    {
        protected override Key[] HitKeys { get; } = { Key.D, Key.K };

        public DrawableRimHitStrong(Hit hit)
            : base(hit)
        {
            MainPiece.Add(new RimHitSymbolPiece());
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            MainPiece.AccentColour = colours.BlueDarker;
        }
    }
}
