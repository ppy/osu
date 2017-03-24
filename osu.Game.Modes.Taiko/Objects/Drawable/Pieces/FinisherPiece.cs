// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;

namespace osu.Game.Modes.Taiko.Objects.Drawable.Pieces
{
    /// <summary>
    /// A type of circle piece that encapsulates a circle piece to visualise it as a "finisher" hitobject.
    /// <para>
    /// Finisher hitobjects are 1.5x larger, while maintaining the same length.
    /// </para>
    /// </summary>
    public class FinisherPiece : ScrollingPiece
    {
        public FinisherPiece(ScrollingPiece original)
        {
            // First we scale the note up
            Scale = new Vector2(1.5f);

            Children = new[]
            {
                original
            };

            // Next we reduce the draw width for drum rolls to keep the width
            // equal to that of a non-finisher drum roll
            original.Width /= 1.5f;
        }
    }
}
