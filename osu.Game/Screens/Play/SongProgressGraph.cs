// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using System.Linq;
using System.Collections.Generic;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Containers;
using osu.Framework.Extensions.Color4Extensions;

namespace osu.Game.Screens.Play
{
    public class SongProgressGraph : BufferedContainer
    {
        private Column[] columns = { };

        public int ColumnCount => columns.Length;

        public override bool HandleInput => false;

        private int progress;
        public int Progress
        {
            get { return progress; }
            set
            {
                if (value == progress) return;
                progress = value;

                redrawProgress();
            }
        }

        private int[] calculatedValues = { }; // values but adjusted to fit the amount of columns
        private int[] values;
        public int[] Values
        {
            get { return values; }
            set
            {
                if (value == values) return;
                values = value;
                recreateGraph();
            }
        }

        private Color4 fillColour;
        public Color4 FillColour
        {
            get { return fillColour; }
            set
            {
                if (value == fillColour) return;
                fillColour = value;

                redrawFilled();
            }
        }

        public SongProgressGraph()
        {
            CacheDrawnFrameBuffer = true;
            PixelSnapping = true;
        }

        private float lastDrawWidth;
        protected override void Update()
        {
            base.Update();

            // todo: Recreating in update is probably not the best idea
            if (DrawWidth == lastDrawWidth) return;
            recreateGraph();
            lastDrawWidth = DrawWidth;
        }

        /// <summary>
        /// Redraws all the columns to match their lit/dimmed state.
        /// </summary>
        private void redrawProgress()
        {
            for (int i = 0; i < columns.Length; i++)
            {
                columns[i].State = i <= progress ? ColumnState.Lit : ColumnState.Dimmed;
            }

            ForceRedraw();
        }

        /// <summary>
        /// Redraws the filled amount of all the columns.
        /// </summary>
        private void redrawFilled()
        {
            for (int i = 0; i < ColumnCount; i++)
            {
                columns[i].Filled = calculatedValues.ElementAtOrDefault(i);
            }
        }

        /// <summary>
        /// Takes <see cref="Values"/> and adjusts it to fit the amount of columns.
        /// </summary>
        private void recalculateValues()
        {
            var newValues = new List<int>();

            if (values == null)
            {
                for (float i = 0; i < ColumnCount; i++)
                    newValues.Add(0);

                return;
            }

            float step = values.Length / (float)ColumnCount;
            for (float i = 0; i < values.Length; i += step)
            {
                newValues.Add(values[(int)i]);
            }

            calculatedValues = newValues.ToArray();
        }

        /// <summary>
        /// Recreates the entire graph.
        /// </summary>
        private void recreateGraph()
        {
            var newColumns = new List<Column>();

            for (float x = 0; x < DrawWidth; x += Column.WIDTH)
            {
                newColumns.Add(new Column(fillColour)
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Position = new Vector2(x, 0),
                    State = ColumnState.Dimmed,
                });
            }

            columns = newColumns.ToArray();
            Children = columns;

            recalculateValues();
            redrawFilled();
            redrawProgress();
        }

        public class Column : Container, IStateful<ColumnState>
        {
            private readonly Color4 emptyColour = Color4.White.Opacity(100);
            private readonly Color4 litColour;
            private readonly Color4 dimmedColour = Color4.White.Opacity(175);

            private const float cube_count = 6;
            private const float cube_size = 4;
            private const float padding = 2;
            public const float WIDTH = cube_size + padding;
            public const float HEIGHT = cube_count * WIDTH + padding;

            private readonly List<Box> drawableRows = new List<Box>();

            private int filled;
            public int Filled
            {
                get { return filled; }
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
                get { return state; }
                set
                {
                    if (value == state) return;
                    state = value;

                    fillActive();
                }
            }

            public Column(Color4 litColour)
            {
                Size = new Vector2(WIDTH, HEIGHT);
                this.litColour = litColour;

                for (int r = 0; r < cube_count; r++)
                {
                    drawableRows.Add(new Box
                    {
                        EdgeSmoothness = new Vector2(padding / 4),
                        Size = new Vector2(cube_size),
                        Position = new Vector2(0, r * WIDTH + padding),
                    });
                }

                Children = drawableRows;

                // Reverse drawableRows so when iterating through them they start at the bottom
                drawableRows.Reverse();
            }

            private void fillActive()
            {
                Color4 colour = State == ColumnState.Lit ? litColour : dimmedColour;

                for (int i = 0; i < drawableRows.Count; i++)
                {
                    if (Filled == 0) // i <= Filled doesn't work for zero fill
                        drawableRows[i].Colour = emptyColour;
                    else
                        drawableRows[i].Colour = i <= Filled ? colour : emptyColour;
                }
            }
        }

        public enum ColumnState
        {
            Lit,
            Dimmed
        }
    }
}
