// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using System.Collections.Generic;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Extensions.Color4Extensions;

namespace osu.Game.Screens.Play
{
    public class SongProgressGraphColumn : Container
    {
        private int rows = 11;
        private readonly Color4 emptyColour = Color4.White.Opacity(50);
        private readonly Color4 litColour = SongProgress.FILL_COLOUR;
        private readonly Color4 dimmedColour = Color4.White.Opacity(175);

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
            Color4 colour = State == ColumnState.Lit ? litColour : dimmedColour;

            for (int i = 0; i < drawableRows.Count; i++)
            {
                if (Filled == 0) // i <= Filled doesn't work for zero fill
                {
                    drawableRows[i].Colour = emptyColour;
                }
                else
                {
                    drawableRows[i].Colour = i <= Filled ? colour : emptyColour;
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
