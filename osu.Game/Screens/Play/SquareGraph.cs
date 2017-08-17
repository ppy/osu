// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Caching;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Screens.Play
{
    public class SquareGraph : BufferedContainer
    {
        private Column[] columns = { };

        public int ColumnCount => columns.Length;

        public override bool HandleInput => false;

        private float progress;
        public float Progress
        {
            get { return progress; }
            set
            {
                if (value == progress) return;
                progress = value;
                redrawProgress();
            }
        }

        private float[] calculatedValues = { }; // values but adjusted to fit the amount of columns

        private int[] values;
        public int[] Values
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

        public virtual bool FillWholeSquares => true;

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
            float progressWidth = DrawWidth * progress;

            foreach(Column column in columns)
            {
                float columnProgress = (progressWidth - column.X) / column.DrawWidth;
                column.Progress = FillWholeSquares ? Convert.ToSingle(columnProgress >= 1.0f) : MathHelper.Clamp(columnProgress, 0.0f, 1.0f);
            }

            ForceRedraw();
        }

        /// <summary>
        /// Redraws the filled amount of all the columns.
        /// </summary>
        private void redrawFilled()
        {
            for (int i = 0; i < ColumnCount; i++)
                columns[i].Filled = calculatedValues.ElementAtOrDefault(i);
            ForceRedraw();
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
                    newValues.Add(0);

                return;
            }

            var max = values.Max();

            float step = values.Length / (float)ColumnCount;
            for (float i = 0; i < values.Length; i += step)
            {
                newValues.Add((float)values[(int)i] / max);
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
                newColumns.Add(new Column
                {
                    LitColour = fillColour,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Height = DrawHeight,
                    Position = new Vector2(x, 0),
                });
            }

            columns = newColumns.ToArray();
            Children = columns;

            recalculateValues();
            redrawFilled();
            redrawProgress();
        }

        public class Column : Container
        {
            protected readonly Color4 EmptyColour = Color4.White.Opacity(20);
            public Color4 LitColour = Color4.LightBlue;
            protected readonly Color4 DimmedColour = Color4.White.Opacity(140);

            private float cubeCount => DrawHeight / WIDTH;
            private const float cube_size = 4;
            private const float padding = 2;
            public const float WIDTH = cube_size + padding;

            private readonly List<Box> drawableLitRows = new List<Box>();
            private readonly List<Box> drawableDimmedRows = new List<Box>();

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

            private float progress;
            public float Progress
            {
                get { return progress; }
                set
                {
                    if (value == progress) return;
                    progress = value;

                    if (IsLoaded)
                        fillActive();
                }
            }

            public Column()
            {
                Width = cube_size;
            }

            protected override void LoadComplete()
            {
                // Create the boxes in reversed order to start iterations at the bottom
                for (int r = (int) cubeCount - 1; r >= 0; r--)
                {
                    drawableLitRows.Add(new Box
                    {
                        Height = cube_size,
                        Position = new Vector2(0, r * WIDTH + padding),
                        Colour = LitColour
                    });

                    drawableDimmedRows.Add(new Box
                    {
                        Height = cube_size,
                        Y = r * WIDTH + padding
                    });
                }

                Children = drawableLitRows.Concat(drawableDimmedRows).ToList();

                fillActive();
            }

            private void fillActive()
            {
                int countFilled = (int)MathHelper.Clamp(filled * cubeCount, 0, cubeCount);

                for (int i = 0; i < drawableLitRows.Count; i++)
                {
                    float litProgress = i < countFilled ? progress : 0.0f;

                    // Change the size and position of both boxes of this row
                    drawableLitRows[i].Width = litProgress * cube_size;

                    drawableDimmedRows[i].Width = (1.0f - litProgress) * cube_size;
                    drawableDimmedRows[i].X = litProgress * cube_size;
                    drawableDimmedRows[i].Colour = i < countFilled ? DimmedColour : EmptyColour;
                }
            }
        }
    }
}
