// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using System.Collections.Generic;
using osu.Game.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Screens.Play
{
    public class SongProgressGraphColumn : Container
    {
        private int rows = 11;
        private Color4 empty_colour = Color4.White.Opacity(50);
        private Color4 lit_colour = SongProgress.FILL_COLOUR;
        private Color4 dimmed_colour = Color4.White.Opacity(175);

        private List<Box> drawableRows = new List<Box>();

        private int filled;
        public int Filled
        {
            get
            {
                return filled;
            }
            set
            {
                if (value == filled) return;
                filled = value;
            }
        }

        private ColumnState state;
        public ColumnState State
        {
            get
            {
                return state;
            }
            set
            {
                if (value == state) return;
                state = value;

                fillActive(value == ColumnState.Lit ? lit_colour : dimmed_colour);
            }
        }

        private void fillActive(Color4 color)
        {
            for (int i = 0; i < drawableRows.Count; i++)
            {
                drawableRows[i].Colour = i <= Filled ? color : empty_colour;
            }
        }

        public SongProgressGraphColumn()
        {
            Size = new Vector2(4, rows * 3);

            for (int row = 0; row < rows * 3; row += 3)
            {
                drawableRows.Add(new Box
                {
                    Size = new Vector2(2),
                    Position = new Vector2(0, row + 1)
                });

                Add(drawableRows[drawableRows.Count - 1]);
            }

            // Reverse drawableRows so when iterating through them they start at the bottom
            drawableRows.Reverse();
        }
    }

    public enum ColumnState
    {
        Lit, Dimmed
    }
}
