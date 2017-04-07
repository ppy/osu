// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.MathUtils;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Overlays;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseSongProgress : TestCase
    {
        public override string Description => @"With fake data";

        private SongProgress progress;

        public override void Reset()
        {
            base.Reset();

            Add(progress = new SongProgress
            {
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                RelativeSizeAxes = Axes.X,
            });

            AddStep("Toggle Bar", progress.ToggleBar);
            AddWaitStep(5);
            //AddStep("Toggle Bar", progress.ToggleVisibility);
            //AddStep("New Values", displayNewValues);

            displayNewValues();
        }

        private void displayNewValues()
        {
            List<int> newValues = new List<int>();
            for (int i = 0; i < 1000; i++)
            {
                newValues.Add(RNG.Next(0, 6));
            }

            progress.Values = newValues.ToArray();
            progress.Progress = RNG.NextDouble();
        }
    }

    public class SongProgress : OverlayContainer
    {
        private const int progress_height = 5;

        private static readonly Vector2 handle_size = new Vector2(14, 25);

        private const float transition_duration = 200;

        private readonly SongProgressBar bar;
        private readonly SongProgressGraph graph;

        public Action<double> OnSeek;

        private double progress;
        public double Progress
        {
            get { return progress; }
            set
            {
                progress = value;
                updateProgress();
            }
        }

        public int[] Values
        {
            get { return graph.Values; }
            set { graph.Values = value; }
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            graph.FillColour = bar.FillColour = colours.BlueLighter;
        }

        public SongProgress()
        {
            RelativeSizeAxes = Axes.X;
            Height = progress_height + SongProgressGraph.Column.HEIGHT + handle_size.Y;

            Children = new Drawable[]
            {
                graph = new SongProgressGraph
                {
                    RelativeSizeAxes = Axes.X,
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    Height = SongProgressGraph.Column.HEIGHT,
                    Margin = new MarginPadding { Bottom = progress_height },
                },
                bar = new SongProgressBar(progress_height, SongProgressGraph.Column.HEIGHT, handle_size)
                {
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    Alpha = 0,
                    SeekRequested = delegate (float position)
                    {
                        OnSeek?.Invoke(position);
                    },
                },
            };
        }

        private void updateProgress()
        {
            bar.UpdatePosition((float)progress);
            graph.Progress = (int)(graph.ColumnCount * progress);
        }

        private bool barVisible;

        public void ToggleBar()
        {
            barVisible = !barVisible;

            updateBarVisibility();
        }

        private void updateBarVisibility()
        {
            bar.FadeTo(barVisible ? 1 : 0, transition_duration, EasingTypes.In);
            MoveTo(new Vector2(0, barVisible ? 0 : progress_height), transition_duration, EasingTypes.In);
        }

        protected override void PopIn()
        {
            updateBarVisibility();
        }

        protected override void PopOut()
        {
        }

        protected override void Update()
        {
            base.Update();

            updateProgress();
        }
    }

    public class SongProgressGraph : BufferedContainer
    {
        private Game.Screens.Play.SongProgressGraph.Column[] columns = { };

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
                columns[i].State = i <= progress ? Game.Screens.Play.SongProgressGraph.ColumnState.Lit : Game.Screens.Play.SongProgressGraph.ColumnState.Dimmed;
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
            var newColumns = new List<Game.Screens.Play.SongProgressGraph.Column>();

            for (float x = 0; x < DrawWidth; x += Game.Screens.Play.SongProgressGraph.Column.WIDTH)
            {
                newColumns.Add(new Game.Screens.Play.SongProgressGraph.Column(fillColour)
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Position = new Vector2(x, 0),
                    State = Game.Screens.Play.SongProgressGraph.ColumnState.Dimmed,
                });
            }

            columns = newColumns.ToArray();
            Children = columns;

            recalculateValues();
            redrawFilled();
            redrawProgress();
        }

        public class Column : Container, IStateful<Game.Screens.Play.SongProgressGraph.ColumnState>
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

            private Game.Screens.Play.SongProgressGraph.ColumnState state;
            public Game.Screens.Play.SongProgressGraph.ColumnState State
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
                Color4 colour = State == Game.Screens.Play.SongProgressGraph.ColumnState.Lit ? litColour : dimmedColour;

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

    public class SongProgressBar : DragBar
    {
        public Color4 FillColour
        {
            get { return FillContainer.Colour; }
            set { FillContainer.Colour = value; }
        }

        public SongProgressBar(float barHeight, float handleBarHeight, Vector2 handleSize)
        {
            Height = barHeight + handleBarHeight + handleSize.Y;
            FillContainer.RelativeSizeAxes = Axes.X;
            FillContainer.Height = barHeight;

            Add(new Box
            {
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                RelativeSizeAxes = Axes.X,
                Height = barHeight,
                Colour = Color4.Black,
                Alpha = 0.5f,
                Depth = 1
            });
            FillContainer.Add(new Container
            {
                Origin = Anchor.BottomRight,
                Anchor = Anchor.BottomRight,
                Width = 2,
                Height = barHeight + handleBarHeight,
                Colour = Color4.White,
                Position = new Vector2(2, 0),
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    new Container
                    {
                        Origin = Anchor.BottomCentre,
                        Anchor = Anchor.TopCentre,
                        Size = handleSize,
                        CornerRadius = 5,
                        Masking = true,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.White
                            }
                        }
                    }
                }
            });
        }
    }
}
