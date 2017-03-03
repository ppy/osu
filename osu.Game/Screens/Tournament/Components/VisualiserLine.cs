using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shaders;
using System;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Timing;
using osu.Framework.Graphics.Transforms;

namespace osu.Game.Screens.Tournament.Components
{
    class VisualiserLine : Drawable
    {
        private float strokeWidth = 1;
        public float StrokeWidth
        {
            get { return strokeWidth; }
            set
            {
                if (strokeWidth == value)
                    return;

                strokeWidth = value;
                Invalidate(Invalidation.DrawNode, shallPropagate: false);
            }
        }

        private float strokeHeight = 1;
        public float StrokeHeight
        {
            get { return strokeHeight; }
            set
            {
                if (strokeHeight == value)
                    return;

                strokeHeight = value;
                Invalidate(Invalidation.DrawNode, shallPropagate: false);
            }
        }

        private float separation = 0;
        public float Separation
        {
            get { return separation; }
            set
            {
                if (separation == value)
                    return;

                separation = value;
                Invalidate(Invalidation.DrawNode, shallPropagate: false);
            }
        }

        private Shader shader;

        private VisualiserLineDrawNodeSharedData visualiserLineDrawNodeSharedData => new VisualiserLineDrawNodeSharedData();

        /// <summary>
        /// The period of this visualiser line, in radians.
        /// </summary>
        private float period;

        /// <summary>
        /// The period offset this line was constructed with, in radians.
        /// </summary>
        private float initialPeriodOffset;
        /// <summary>
        /// The rolling period offset (by transformation), in radians.
        /// </summary>
        private float internalPeriodOffset;
        /// <summary>
        /// The final period offset, in radians.
        /// </summary>
        private float periodOffset
        {
            get { return initialPeriodOffset + internalPeriodOffset; }
            set
            {
                if (internalPeriodOffset == value)
                    return;

                internalPeriodOffset = value;
                Invalidate(Invalidation.DrawNode, shallPropagate: false);
            }
        }

        /// <summary>
        /// Constructs a new Visualiser Line.
        /// </summary>
        /// <param name="period">The period of the line, in radians.</param>
        /// <param name="periodOffset">The offset to the period of the line, in radians.</param>
        /// <param name="cycleTime">The time to cycle the line.</param>
        public VisualiserLine(float period, float periodOffset = 0, int cycleTime = 0)
        {
            this.period = period;
            this.initialPeriodOffset = periodOffset;

            Clock = new FramedClock();

            if (cycleTime > 0)
                TransformFloatTo(0, period, cycleTime, EasingTypes.None, new TransformVisualiserOffset());

            Loop();
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

            vNode.Period = period;
            vNode.PeriodOffset = periodOffset;
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

        class TransformVisualiserOffset : TransformFloat
        {
            public override void Apply(Drawable d)
            {
                base.Apply(d);
                (d as VisualiserLine).periodOffset = CurrentValue;
            }
        }
    }
}
