// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE


using OpenTK;

namespace osu.Game.Modes.Taiko.Objects.Drawables.Pieces.DrumRoll
{
    /// <summary>
    /// The internal coloured "bar" of a finisher drum roll.
    /// This overshoots the expected length by corner radius on both sides.
    /// </summary>
    public class DrumRollFinisherBodyPiece : DrumRollBodyPiece
    {
        public override float CornerRadius => base.CornerRadius * 1.5f;

        public DrumRollFinisherBodyPiece(float baseLength)
            : base(baseLength)
        {
            Size *= new Vector2(1, 1.5f);
        }
    }
}
