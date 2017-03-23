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
        private const float cube_count = 6;
        private const float cube_size = 4;
        private const float padding = 2;

        public const float WIDTH = cube_size + padding;

        public const float HEIGHT = cube_count * WIDTH;

        private readonly Color4 emptyColour = Color4.White.Opacity(100);
        private readonly Color4 litColour = SongProgress.FILL_COLOUR;
        private readonly Color4 dimmedColour = Color4.White.Opacity(175);

        private readonly List<Box> drawableRows = new List<Box>();

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

        public SongProgressGraphColumn()
        {
            Size = new Vector2(WIDTH, HEIGHT);

            for (int r = 0; r < cube_count; r++)
            {
                drawableRows.Add(new Box
                {
                    EdgeSmoothness = new Vector2(padding / 4),
                    Size = new Vector2(cube_size),
                    Position = new Vector2(0, r * WIDTH + padding)
                });

                Add(drawableRows[drawableRows.Count - 1]);
            }

            // Reverse drawableRows so when iterating through them they start at the bottom
            drawableRows.Reverse();
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
    }

    public enum ColumnState
    {
        Lit,
        Dimmed
    }
}
