// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using System.Collections.Generic;
using osu.Framework.Graphics.Containers;
using System;

namespace osu.Game.Screens.Play
{
    public class SongProgressGraph : BufferedContainer
    {
        private List<SongProgressGraphColumn> columns = new List<SongProgressGraphColumn>();
        private float lastDrawWidth;

        public override bool HandleInput => false;
        public int ColumnCount => columns.Count;

        private int progress;
        public int Progress
        {
            get
            {
                return progress;
            }
            set
            {
                if (value == progress) return;
                progress = value;

                redrawProgress();
            }
        }

        private List<int> calculatedValues = new List<int>(); // values but adjusted to fit the amount of columns
        private List<int> values;
        public List<int> Values
        {
            get
            {
                return values;
            }
            set
            {
                if (value == values) return;
                values = value;
                recreateGraph();
            }
        }

        private void redrawProgress()
        {
            for (int i = 0; i < columns.Count; i++)
            {
                columns[i].State = i <= progress ? ColumnState.Lit : ColumnState.Dimmed;
            }

            ForceRedraw();
        }

        private void redrawFilled()
        {
            for (int i = 0; i < ColumnCount; i++)
            {
                columns[i].Filled = calculatedValues[i];
            }

            ForceRedraw();
        }

        private void recalculateValues()
        {
            // Resizes values to fit the amount of columns and stores it in calculatedValues
            // Defaults to all zeros if values is null

            calculatedValues.RemoveAll(delegate { return true; });

            if (values == null)
            {
                for (float i = 0; i < ColumnCount; i++)
                {
                    calculatedValues.Add(0);
                }

                return;
            }

            float step = (float)values.Count / (float)ColumnCount;

            for (float i = 0; i < values.Count; i += step) 
            {
                calculatedValues.Add(values[(int)i]);
            }
        }

        private void recreateGraph()
        {
            RemoveAll(delegate { return true; }, true);
            columns.RemoveAll(delegate { return true; });

            for (int x = 0; x < DrawWidth; x += 3)
            {
                columns.Add(new SongProgressGraphColumn
                {
                    Position = new Vector2(x + 1, 0),
                    State = ColumnState.Dimmed
                });

                Add(columns[columns.Count - 1]);
            }

            recalculateValues();
            redrawFilled();
            redrawProgress();
        }

        protected override void Update()
        {
            base.Update();

            if (DrawWidth == lastDrawWidth) return;
            recreateGraph();
            lastDrawWidth = DrawWidth;
        }

        public SongProgressGraph()
        {
            CacheDrawnFrameBuffer = true;
            PixelSnapping = true;
        }
    }
}
