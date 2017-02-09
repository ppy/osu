// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using System;
using System.Collections.Generic;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics;

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

        private void redrawProgress()
        {
            for (int i = 0; i < columns.Count; i++)
            {
                columns[i].State = i <= progress ? ColumnState.Lit : ColumnState.Dimmed;
            }

            ForceRedraw();
        }

        private void recreateGraph()
        {
            RemoveAll(delegate { return true; }, true);
            columns.RemoveAll(delegate { return true; });

            // Random filled values used for testing for now
            var random = new Random();
            for (int column = 0; column < DrawWidth; column += 3)
            {
                columns.Add(new SongProgressGraphColumn
                {
                    Position = new Vector2(column + 1, 0),
                    Filled = random.Next(1, 11),
                    State = ColumnState.Dimmed
                });

                Add(columns[columns.Count - 1]);
            }

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
