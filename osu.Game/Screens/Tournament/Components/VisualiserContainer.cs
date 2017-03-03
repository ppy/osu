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

                PeriodOffset = offset,
                Period = 2 * (float)Math.PI,
                CycleTime = RNG.Next(10000, 12000)
            };

            allLines.Add(newLine);
            Add(newLine);

            offset += (float)Math.PI / 6f;
        }

        private void removeLine()
        {
            if (allLines.Count == 0)
                return;

            Remove(allLines.First());
            allLines.Remove(allLines.First());
        }

        class VisualiserLine : Drawable
        {
            /// <summary>
            /// Width of the line strokes.
            /// </summary>
            public float StrokeWidth = 1f;

            /// <summary>
            /// Height of the line strokes.
            /// </summary>
            public float StrokeHeight = 1f;

            /// <summary>
            /// Separation between strokes in the line.
            /// </summary>
            public float Separation = 0;

            /// <summary>
            /// Period offset of the line.
            /// </summary>
            public float PeriodOffset;

            /// <summary>
            /// Period of the line.
            /// </summary>
            public float Period;

            /// <summary>
            /// The time to cycle one period of the line in milliseconds.
            /// </summary>
            public double CycleTime;

            private Shader shader;

            private VisualiserLineDrawNodeSharedData visualiserLineDrawNodeSharedData => new VisualiserLineDrawNodeSharedData();

            private float runningPeriodOffset;

            protected override void Update()
            {
                base.Update();

                if (CycleTime != 0)
                {
                    runningPeriodOffset += (float)(Time.Elapsed / CycleTime) * Period;
                    Invalidate(Invalidation.DrawNode, shallPropagate: false);
                }
            }

            protected override DrawNode CreateDrawNode() => new VisualiserLineDrawNode();

            [BackgroundDependencyLoader]
            private void load(ShaderManager shaders)
            {
                shader = shaders?.Load(VertexShaderDescriptor.Colour, @"DottedLine");
            }

            protected override void ApplyDrawNode(DrawNode node)
            {
                base.ApplyDrawNode(node);

                VisualiserLineDrawNode vNode = node as VisualiserLineDrawNode;
                vNode.Shader = shader;
                vNode.Shared = visualiserLineDrawNodeSharedData;
                vNode.ScreenSpaceDrawQuad = ScreenSpaceDrawQuad;

                vNode.Period = Period;
                vNode.PeriodOffset = PeriodOffset + runningPeriodOffset;
                vNode.StrokeWidth = StrokeWidth;
                vNode.StrokeHeight = StrokeHeight;
                vNode.Separation = Separation;
            }

            class VisualiserLineDrawNodeSharedData
            {
                public QuadBatch<Vertex2D> QuadBatch = new QuadBatch<Vertex2D>(1, 1);
            }

            class VisualiserLineDrawNode : DrawNode
            {
                public Shader Shader;
                public VisualiserLineDrawNodeSharedData Shared;

                public Quad ScreenSpaceDrawQuad;

                public float Period;
                public float PeriodOffset;

                public float StrokeWidth;
                public float StrokeHeight;
                public float Separation;

                public override void Draw(Action<TexturedVertex2D> vertexAction)
                {
                    base.Draw(vertexAction);

                    Shader.Bind();

                    Shader.GetUniform<Vector2>(@"g_Position").Value = ScreenSpaceDrawQuad.TopLeft;
                    Shader.GetUniform<Vector2>(@"g_Size").Value = ScreenSpaceDrawQuad.Size;

                    Shader.GetUniform<float>(@"g_Period").Value = Period;
                    Shader.GetUniform<float>(@"g_PeriodOffset").Value = PeriodOffset;

                    Shader.GetUniform<float>(@"g_StrokeWidth").Value = StrokeWidth;
                    Shader.GetUniform<float>(@"g_StrokeHeight").Value = StrokeHeight;
                    Shader.GetUniform<float>(@"g_Separation").Value = Separation;

                    Shared.QuadBatch.Add(new Vertex2D()
                    {
                        Position = ScreenSpaceDrawQuad.BottomLeft,
                        Colour = DrawInfo.Colour.BottomLeft.Linear
                    });

                    Shared.QuadBatch.Add(new Vertex2D()
                    {
                        Position = ScreenSpaceDrawQuad.BottomRight,
                        Colour = DrawInfo.Colour.BottomRight.Linear
                    });

                    Shared.QuadBatch.Add(new Vertex2D()
                    {
                        Position = ScreenSpaceDrawQuad.TopRight,
                        Colour = DrawInfo.Colour.TopRight.Linear
                    });

                    Shared.QuadBatch.Add(new Vertex2D()
                    {
                        Position = ScreenSpaceDrawQuad.TopLeft,
                        Colour = DrawInfo.Colour.TopLeft.Linear
                    });

                    Shader.Unbind();
                }
            }
        }
    }
}
