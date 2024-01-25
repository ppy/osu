// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osuTK;

namespace osu.Game.Graphics.Backgrounds
{
    public partial class TrianglesV2 : Drawable
    {
        private const float triangle_size = 100;
        private const float base_velocity = 50;

        /// <summary>
        /// sqrt(3) / 2
        /// </summary>
        private const float equilateral_triangle_ratio = 0.866f;

        public float Thickness { get; set; } = 0.02f; // No need for invalidation since it's happening in Update()

        public float ScaleAdjust { get; set; } = 1;

        /// <summary>
        /// Whether we should create new triangles as others expire.
        /// </summary>
        protected virtual bool CreateNewTriangles => true;

        /// <summary>
        /// Controls on which <see cref="Axes"/> the portion of triangles that falls within this <see cref="Drawable"/>'s
        /// shape is drawn to the screen. Default is Axes.Both.
        /// </summary>
        public Axes ClampAxes { get; set; } = Axes.Both;

        private readonly BindableFloat spawnRatio = new BindableFloat(1f);

        /// <summary>
        /// The amount of triangles we want compared to the default distribution.
        /// </summary>
        public float SpawnRatio
        {
            get => spawnRatio.Value;
            set => spawnRatio.Value = value;
        }

        /// <summary>
        /// The relative velocity of the triangles. Default is 1.
        /// </summary>
        public float Velocity = 1;

        private readonly List<TriangleParticle> parts = new List<TriangleParticle>();

        private Random? stableRandom;

        private IShader shader = null!;
        private Texture texture = null!;

        /// <summary>
        /// Construct a new triangle visualisation.
        /// </summary>
        /// <param name="seed">An optional seed to stabilise random positions / attributes. Note that this does not guarantee stable playback when seeking in time.</param>
        public TrianglesV2(int? seed = null)
        {
            if (seed != null)
                stableRandom = new Random(seed.Value);
        }

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders, IRenderer renderer)
        {
            shader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "TriangleBorder");
            texture = renderer.WhitePixel;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            spawnRatio.BindValueChanged(_ => Reset(), true);
        }

        protected override void Update()
        {
            base.Update();

            Invalidate(Invalidation.DrawNode);

            if (CreateNewTriangles)
                addTriangles(false);

            float elapsedSeconds = (float)Time.Elapsed / 1000;
            // Since position is relative, the velocity needs to scale inversely with DrawHeight.
            float movedDistance = -elapsedSeconds * Velocity * base_velocity / DrawHeight;

            for (int i = 0; i < parts.Count; i++)
            {
                TriangleParticle newParticle = parts[i];

                newParticle.Position.Y += Math.Max(0.5f, parts[i].SpeedMultiplier) * movedDistance;

                parts[i] = newParticle;

                float bottomPos = parts[i].Position.Y + triangle_size * ScaleAdjust * equilateral_triangle_ratio / DrawHeight;
                if (bottomPos < 0)
                    parts.RemoveAt(i);
            }
        }

        /// <summary>
        /// Clears and re-initialises triangles according to a given seed.
        /// </summary>
        /// <param name="seed">An optional seed to stabilise random positions / attributes. Note that this does not guarantee stable playback when seeking in time.</param>
        public void Reset(int? seed = null)
        {
            if (seed != null)
                stableRandom = new Random(seed.Value);

            parts.Clear();
            addTriangles(true);
        }

        protected int AimCount { get; private set; }

        private void addTriangles(bool randomY)
        {
            // Limited by the maximum size of QuadVertexBuffer for safety.
            const int max_triangles = ushort.MaxValue / (IRenderer.VERTICES_PER_QUAD + 2);

            AimCount = (int)Math.Clamp(DrawWidth * 0.02f * SpawnRatio, 1, max_triangles);

            int currentCount = parts.Count;

            for (int i = 0; i < AimCount - currentCount; i++)
                parts.Add(createTriangle(randomY));
        }

        private TriangleParticle createTriangle(bool randomY)
        {
            TriangleParticle particle = CreateTriangle();

            float y = 1;

            if (randomY)
            {
                // since triangles are drawn from the top - allow them to be positioned a bit above the screen
                float maxOffset = triangle_size * ScaleAdjust * equilateral_triangle_ratio / DrawHeight;
                y = Interpolation.ValueAt(nextRandom(), -maxOffset, 1f, 0f, 1f);
            }

            particle.Position = new Vector2(nextRandom(), y);

            return particle;
        }

        /// <summary>
        /// Creates a triangle particle with a random speed multiplier.
        /// </summary>
        /// <returns>The triangle particle.</returns>
        protected virtual TriangleParticle CreateTriangle()
        {
            const float std_dev = 0.16f;
            const float mean = 0.5f;

            float u1 = 1 - nextRandom(); //uniform(0,1] random floats
            float u2 = 1 - nextRandom();
            float randStdNormal = (float)(Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2)); // random normal(0,1)
            float speedMultiplier = Math.Max(mean + std_dev * randStdNormal, 0.1f); // random normal(mean,stdDev^2)

            return new TriangleParticle { SpeedMultiplier = speedMultiplier };
        }

        private float nextRandom() => (float)(stableRandom?.NextDouble() ?? RNG.NextSingle());

        protected override DrawNode CreateDrawNode() => new TrianglesDrawNode(this);

        private class TrianglesDrawNode : DrawNode
        {
            protected new TrianglesV2 Source => (TrianglesV2)base.Source;

            private IShader shader = null!;
            private Texture texture = null!;

            private readonly List<TriangleParticle> parts = new List<TriangleParticle>();

            private Vector2 triangleSize;

            private Vector2 size;
            private float thickness;
            private float texelSize;
            private Axes clampAxes;

            public TrianglesDrawNode(TrianglesV2 source)
                : base(source)
            {
            }

            public override void ApplyState()
            {
                base.ApplyState();

                shader = Source.shader;
                texture = Source.texture;
                size = Source.DrawSize;
                thickness = Source.Thickness;
                clampAxes = Source.ClampAxes;
                triangleSize = new Vector2(1f, equilateral_triangle_ratio) * triangle_size * Source.ScaleAdjust;

                Quad triangleQuad = new Quad(
                    Vector2Extensions.Transform(Vector2.Zero, DrawInfo.Matrix),
                    Vector2Extensions.Transform(new Vector2(triangle_size, 0f), DrawInfo.Matrix),
                    Vector2Extensions.Transform(new Vector2(0f, triangleSize.Y), DrawInfo.Matrix),
                    Vector2Extensions.Transform(triangleSize, DrawInfo.Matrix)
                );

                texelSize = 1.5f / triangleQuad.Height;

                parts.Clear();
                parts.AddRange(Source.parts);
            }

            private IUniformBuffer<TriangleBorderData>? borderDataBuffer;

            protected override void Draw(IRenderer renderer)
            {
                base.Draw(renderer);

                if (Source.AimCount == 0 || thickness == 0)
                    return;

                borderDataBuffer ??= renderer.CreateUniformBuffer<TriangleBorderData>();
                borderDataBuffer.Data = borderDataBuffer.Data with
                {
                    Thickness = thickness,
                    TexelSize = texelSize
                };

                shader.Bind();
                shader.BindUniformBlock(@"m_BorderData", borderDataBuffer);

                Vector2 relativeSize = Vector2.Divide(triangleSize, size);

                foreach (TriangleParticle particle in parts)
                {
                    Vector2 topLeft = particle.Position - new Vector2(relativeSize.X * 0.5f, 0f);

                    Quad triangleQuad = getClampedQuad(clampAxes, topLeft, relativeSize);

                    var drawQuad = new Quad(
                        Vector2Extensions.Transform(triangleQuad.TopLeft * size, DrawInfo.Matrix),
                        Vector2Extensions.Transform(triangleQuad.TopRight * size, DrawInfo.Matrix),
                        Vector2Extensions.Transform(triangleQuad.BottomLeft * size, DrawInfo.Matrix),
                        Vector2Extensions.Transform(triangleQuad.BottomRight * size, DrawInfo.Matrix)
                    );

                    RectangleF textureCoords = new RectangleF(
                        triangleQuad.TopLeft.X - topLeft.X,
                        triangleQuad.TopLeft.Y - topLeft.Y,
                        triangleQuad.Width,
                        triangleQuad.Height
                    ) / relativeSize;

                    renderer.DrawQuad(texture, drawQuad, DrawColourInfo.Colour.Interpolate(triangleQuad), new RectangleF(0, 0, 1, 1), textureCoords: textureCoords);
                }

                shader.Unbind();
            }

            private static Quad getClampedQuad(Axes clampAxes, Vector2 topLeft, Vector2 size)
            {
                Vector2 clampedTopLeft = topLeft;

                if (clampAxes == Axes.X || clampAxes == Axes.Both)
                {
                    clampedTopLeft.X = Math.Clamp(topLeft.X, 0f, 1f);
                    size.X = Math.Clamp(topLeft.X + size.X, 0f, 1f) - clampedTopLeft.X;
                }

                if (clampAxes == Axes.Y || clampAxes == Axes.Both)
                {
                    clampedTopLeft.Y = Math.Clamp(topLeft.Y, 0f, 1f);
                    size.Y = Math.Clamp(topLeft.Y + size.Y, 0f, 1f) - clampedTopLeft.Y;
                }

                return new Quad(clampedTopLeft.X, clampedTopLeft.Y, size.X, size.Y);
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                borderDataBuffer?.Dispose();
            }
        }

        protected struct TriangleParticle
        {
            /// <summary>
            /// The position of the top vertex of the triangle.
            /// </summary>
            public Vector2 Position;

            /// <summary>
            /// The speed multiplier of the triangle.
            /// </summary>
            public float SpeedMultiplier;
        }
    }
}
