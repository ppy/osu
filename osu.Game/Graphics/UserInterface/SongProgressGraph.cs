using OpenTK;
using System;
using System.Collections.Generic;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;

namespace osu.Game.Graphics.UserInterface
{
    public class SongProgressGraph : BufferedContainer
    {
        private List<SongProgressGraphColumn> columns = new List<SongProgressGraphColumn>();

        public override bool HandleInput => false;

        private float progress;
        public float Progress
        {
            get
            {
                return progress;
            }
            set
            {
                if (value == progress) return;
                progress = value;

                for (int i = 0; i < columns.Count; i++)
                {
                    columns[i].State = i <= (columns.Count * progress) ? SongProgressGraphColumnState.Lit : SongProgressGraphColumnState.Dimmed;
                }

                ForceRedraw();
            }
        }

        public SongProgressGraph()
        {
            CacheDrawnFrameBuffer = true;
            PixelSnapping = true;

            Margin = new MarginPadding
            {
                Left = 1,
                Right = 1
            };

            var random = new Random();
            for (int column = 0; column < 1200; column += 3)
            {
                columns.Add(new SongProgressGraphColumn
                {
                    Position = new Vector2(column, 0),
                    Filled = random.Next(1, 11),
                    State = SongProgressGraphColumnState.Dimmed
                });

                Add(columns[columns.Count - 1]);
            }
        }
    }
}
