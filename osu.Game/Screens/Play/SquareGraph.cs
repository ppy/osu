// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Allocation;
using osu.Framework.Threading;
using osu.Framework.Layout;

namespace osu.Game.Screens.Play
{
    public partial class SquareGraph : Container
    {
        private BufferedContainer<Column> columns;

        private readonly LayoutValue layout = new LayoutValue(Invalidation.DrawSize | Invalidation.DrawInfo);

        public int ColumnCount => columns?.Children.Count ?? 0;

        private int progress;

        public int Progress
        {
            get => progress;
            set
            {
                if (value == progress) return;

                progress = value;
                redrawProgress();
            }
        }

        private float[] calculatedValues = Array.Empty<float>(); // values but adjusted to fit the amount of columns

        private int[] values;

        public int[] Values
        {
            get => values;
            set
            {
                if (value == values) return;

                values = value;
                haveValuesChanged = true;
                layout.Invalidate();
            }
        }

        bool haveValuesChanged;

        private Color4 fillColour;

        public Color4 FillColour
        {
            get => fillColour;
            set
            {
                if (value == fillColour) return;

                fillColour = value;
                redrawFilled();
            }
        }

        public SquareGraph()
        {
            AddLayout(layout);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = columns = new BufferedContainer<Column>(cachedFrameBuffer: true)
            {
                RedrawOnScale = false,
                RelativeSizeAxes = Axes.Both
            };
        }


        // private Vector2 parentScale;

        protected override void Update()
        {
            base.Update();
            
            if (!layout.IsValid)
            {
                UpdateGraph();
                layout.Validate();
            }
        }

        /// <summary>
        /// Updates the graph by either adding or removing columns based on DrawWidth.
        /// Does nothing if correct number of columns already exists and/or if <see cref="SquareGraph.values"/> haven't changed.
        /// </summary>
        protected virtual void UpdateGraph()
        {
            int targetColumnCount = values == null ? 0 : (int)(DrawWidth / Column.WIDTH);

            // early exit the most frequent case
            if (!haveValuesChanged && targetColumnCount == ColumnCount)
            {
                columns.ForceRedraw();
                return;
            }

            ensureColumnCount(targetColumnCount);

            // fill graph data
            recalculateValues();
            redrawFilled();
            redrawProgress();

            haveValuesChanged = false;
        }

        private void ensureColumnCount(int targetColumnCount)
        {
            // remove excess columns
            while (targetColumnCount < ColumnCount)
            {
                columns.Remove(columns.Children[ColumnCount - 1], true);
            }

            // update height of existing columns
            foreach (var column in columns)
            {
                column.Height = DrawHeight;
            }

            // add missing columns
            float x = ColumnCount * Column.WIDTH;
            while (targetColumnCount > ColumnCount)
            {
                var column = new Column()
                {
                    Height = DrawHeight,
                    LitColour = fillColour,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Position = new Vector2(x, 0),
                    State = ColumnState.Dimmed,
                };

                LoadComponentAsync(column);
                columns.Add(column);

                x += Column.WIDTH;
            }
        }

        /// <summary>
        /// Redraws all the columns to match their lit/dimmed state.
        /// </summary>
        private void redrawProgress()
        {
            for (int i = 0; i < ColumnCount; i++)
                columns[i].State = i <= progress ? ColumnState.Lit : ColumnState.Dimmed;
            columns?.ForceRedraw();
        }

        /// <summary>
        /// Redraws the filled amount of all the columns.
        /// </summary>
        private void redrawFilled()
        {
            for (int i = 0; i < ColumnCount; i++)
                columns[i].Filled = calculatedValues.ElementAtOrDefault(i);
            columns?.ForceRedraw();
        }

        /// <summary>
        /// Takes <see cref="Values"/> and adjusts it to fit the amount of columns.
        /// </summary>
        private void recalculateValues()
        {
            var newValues = new List<float>();

            if (values == null)
            {
                for (float i = 0; i < ColumnCount; i++)
                {
                    newValues.Add(0);
                }
            }
            else
            {
                int max = values.Max();
                float step = values.Length / (float)ColumnCount;

                for (float i = 0; i < values.Length; i += step)
                {
                    newValues.Add((float)values[(int)i] / max);
                }
            }

            calculatedValues = newValues.ToArray();
        }

        public partial class Column : Container, IStateful<ColumnState>
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
                get => filled;
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
                get => state;
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

            [BackgroundDependencyLoader]
            private void load()
            {
                drawableRows.AddRange(Enumerable.Range(0, (int)cubeCount).Select(r => new Box
                {
                    Size = new Vector2(cube_size),
                    Position = new Vector2(0, r * WIDTH + padding),
                }));

                Children = drawableRows;

                // Reverse drawableRows so when iterating through them they start at the bottom
                drawableRows.Reverse();
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                fillActive();
            }

            private void fillActive()
            {
                Color4 colour = State == ColumnState.Lit ? LitColour : DimmedColour;

                int countFilled = (int)Math.Clamp(filled * drawableRows.Count, 0, drawableRows.Count);

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
