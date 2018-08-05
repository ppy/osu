// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework;
using osu.Framework.Caching;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Screens.Play
{
    public class SquareGraph : BufferedContainer
    {
        private Column[] columns = { };

        public int ColumnCount => columns.Length;

        public override bool HandleKeyboardInput => false;
        public override bool HandleMouseInput => false;

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

        private List<double> calculatedValues = new List<double>(); // values but adjusted to fit the amount of columns

        private List<double> values = new List<double>();
        public List<double> Values
        {
            get { return values; }
            set
            {
                if (value == values) return;
                values = value;
                layout.Invalidate();
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

        public SquareGraph()
        {
            CacheDrawnFrameBuffer = true;
        }

        private Cached layout = new Cached();

        public override bool Invalidate(Invalidation invalidation = Invalidation.All, Drawable source = null, bool shallPropagate = true)
        {
            if ((invalidation & Invalidation.DrawSize) > 0)
                layout.Invalidate();
            return base.Invalidate(invalidation, source, shallPropagate);
        }

        protected override void Update()
        {
            base.Update();

            if (!layout.IsValid)
            {
                recreateGraph();
                layout.Validate();
            }
        }

        /// <summary>
        /// Redraws all the columns to match their lit/dimmed state.
        /// </summary>
        private void redrawProgress()
        {
            for (int i = 0; i < columns.Length; i++)
                columns[i].State = i <= progress ? ColumnState.Lit : ColumnState.Dimmed;
            ForceRedraw();
        }

        /// <summary>
        /// Redraws the filled amount of all the columns.
        /// </summary>
        private void redrawFilled()
        {
            for (int i = 0; i < ColumnCount; i++)
                columns[i].Filled = (float) calculatedValues.ElementAtOrDefault(i);
            ForceRedraw();
        }

        /// <summary>
        /// Takes <see cref="Values"/> and adjusts it to fit the amount of columns.
        /// </summary>
        private void recalculateValues()
        {
            var newValues = new List<double>();

            if (values.Count == 0)
            {
                for (float i = 0; i < ColumnCount; i++)
                    newValues.Add(0);
                return;
            }

            var max = values.Max();

            float step = values.Count / (float)ColumnCount;
            for (float i = 0; i < values.Count; i += step)
            {
                double sum = 0;
                float iteration = 0;
                for (float x = i; x < i+step && x < values.Count; x++)
                {
                    sum += values[(int) x];
                    iteration = x - i + 1;
                }
                sum = sum / iteration;
                newValues.Add(sum / max);
            }

            calculatedValues = newValues;
        } 

        /// <summary>
        /// Recreates the entire graph.
        /// </summary>
        private void recreateGraph()
        {
            var newColumns = new List<Column>();

            for (float x = 0; x < DrawWidth; x += Column.WIDTH)
            {
                newColumns.Add(new Column
                {
                    LitColour = fillColour,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Height = DrawHeight,
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
            protected readonly Color4 EmptyColour = Color4.White.Opacity(20);
            public Color4 LitColour = Color4.LightBlue;
            protected readonly Color4 DimmedColour = Color4.White.Opacity(140);

            private float cubeCount => DrawHeight / WIDTH;
            private const float cube_size = 4;
            private const float padding = 2;
            public const float WIDTH = cube_size + padding;

            public event Action<ColumnState> StateChanged;

            private readonly List<Box> drawableRows = new List<Box>();

            private float filled;
            public float Filled
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

                    if (IsLoaded)
                        fillActive();

                    StateChanged?.Invoke(State);
                }
            }

            public Column()
            {
                Width = WIDTH;
            }

            protected override void LoadComplete()
            {
                for (int r = 0; r < cubeCount; r++)
                {
                    drawableRows.Add(new Box
                    {
                        Size = new Vector2(cube_size),
                        Position = new Vector2(0, r * WIDTH + padding),
                    });
                }

                Children = drawableRows;

                // Reverse drawableRows so when iterating through them they start at the bottom
                drawableRows.Reverse();

                fillActive();
            }

            private void fillActive()
            {
                Color4 colour = State == ColumnState.Lit ? LitColour : DimmedColour;

                int countFilled = (int)MathHelper.Clamp(filled * drawableRows.Count, 1, drawableRows.Count);

                if (filled<0.01)
                    countFilled = 0;

                for (int i = 0; i < drawableRows.Count; i++)
                    drawableRows[i].Colour = i < countFilled ? colour : EmptyColour;
            }
        }

        public enum ColumnState
        {
            Lit,
            Dimmed
        }
    }
}
