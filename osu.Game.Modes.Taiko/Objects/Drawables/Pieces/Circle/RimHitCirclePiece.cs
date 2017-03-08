// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE


using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Modes.Taiko.Objects.Drawables.Pieces.Ring;

namespace osu.Game.Modes.Taiko.Objects.Drawables.Pieces.Circle
{
    internal class RimHitCirclePiece : CirclePiece
    {
        protected override Color4 InnerColour { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InnerColour = colours.BlueDarker;
        }

        protected override RingPiece CreateRing() => new RimHitRingPiece();
    }
}
