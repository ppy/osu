// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.MathUtils;
using osu.Game.Graphics;

namespace osu.Game.Modes.Taiko.Objects.Drawable.Pieces
{
    /// <summary>
    /// A circle piece used for drumrolls.
    /// </summary>
    public class DrumRollCirclePiece : Container
    {
        private float completion;
        /// <summary>
        /// The amount of the drumroll that has been completed, as a percentage of the number
        /// of ticks in the drumroll. This determines the internal colour of the drumroll.
        /// </summary>
        public float Completion
        {
            get { return completion; }
            set
            {
                completion = MathHelper.Clamp(value, 0, 1);

                if (!IsLoaded)
                    return;

                circle.AccentColour = Interpolation.ValueAt(completion, baseColour, finalColour, 0, 1);
            }
        }

        private readonly CirclePiece circle;

        private Color4 baseColour;
        private Color4 finalColour;

        public DrumRollCirclePiece(CirclePiece piece)
        {
            RelativeSizeAxes = Axes.X;

            Add(circle = piece);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            circle.AccentColour = baseColour = colours.YellowDark;
            finalColour = colours.YellowDarker;
        }
    }
}
