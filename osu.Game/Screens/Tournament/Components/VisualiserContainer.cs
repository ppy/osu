// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Transforms;
using osu.Framework.MathUtils;
using osu.Framework.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Screens.Tournament.Components
{
    class VisualiserContainer : Container
    {
        /// <summary>
        /// Number of lines in the visualiser.
        /// </summary>
        public int Lines
        {
            get { return allLines.Count; }
            set
            {
                while (value > allLines.Count)
                    addLine();

                while (value < allLines.Count)
                    removeLine();
            }
        }

        private List<VisualiserLine> allLines = new List<VisualiserLine>();

        private float offset;

        private void addLine()
        {
            VisualiserLine newLine = new VisualiserLine()
            {
                RelativeSizeAxes = Axes.Both,

                Offset = offset,
                CycleTime = RNG.Next(10000, 12000),
            };

            allLines.Add(newLine);
            Add(newLine);

            offset += RNG.Next(100, 5000);
        }

        private void removeLine()
        {
            if (allLines.Count == 0)
                return;

            Remove(allLines.First());
            allLines.Remove(allLines.First());
        }

        class VisualiserLine : Container
        {
            /// <summary>
            /// Time offset.
            /// </summary>
            public float Offset;

            public double CycleTime;

            private float leftPos => -(float)((Time.Current + Offset) / CycleTime) + expiredCount;

            private Texture texture;

            private int expiredCount = 0;

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                texture = textures.Get("Drawings/visualiser-line");
            }

            protected override void UpdateAfterChildren()
            {
                base.UpdateAfterChildren();

                while (Children.Count() < 3)
                    addLine();

                float pos = leftPos;

                foreach (var c in Children)
                {
                    if (c.Position.X < -1)
                    {
                        c.ClearTransforms();
                        c.Expire();
                        expiredCount++;
                    }
                    else
                        c.MoveToX(pos, 100);

                    pos += 1;
                }
            }

            private void addLine()
            {
                Add(new Sprite()
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,

                    RelativePositionAxes = Axes.Both,
                    RelativeSizeAxes = Axes.Both,

                    Texture = texture,

                    X = leftPos + 1
                });
            }
        }
    }
}
