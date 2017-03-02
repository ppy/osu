// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using System.Collections.Generic;
using osu.Game.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Screens.Play
{
    public class SongProgressGraphColumn : Container
    {
        private int rows = 11;
        private readonly Color4 empty_colour = Color4.White.Opacity(50);
        private readonly Color4 lit_colour = SongProgress.FILL_COLOUR;
        private readonly Color4 dimmed_colour = Color4.White.Opacity(175);

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

                fillActive();
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

                fillActive();
            }
        }

        private void fillActive()
        {
            Color4 colour = State == ColumnState.Lit ? lit_colour : dimmed_colour;

            for (int i = 0; i < drawableRows.Count; i++)
            {
                if (Filled == 0) // i <= Filled doesn't work for zero fill
                {
                    drawableRows[i].Colour = empty_colour;
                }
                else
                {
                    drawableRows[i].Colour = i <= Filled ? colour : empty_colour;
                }
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
