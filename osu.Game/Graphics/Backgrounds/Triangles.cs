// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;
using osu.Framework.Utils;
using osuTK;
using osuTK.Graphics;
using System;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Allocation;
using System.Collections.Generic;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Lists;
using osu.Framework.Bindables;

namespace osu.Game.Graphics.Backgrounds
{
    public partial class Triangles : Drawable
    {
        private const float triangle_size = 100;
        private const float base_velocity = 50;

        /// <summary>
        /// sqrt(3) / 2
        /// </summary>
        private const float equilateral_triangle_ratio = 0.866f;

        private Color4 colourLight = Color4.White;

        public Color4 ColourLight
        {
            get => colourLight;
            set
            {
                if (colourLight == value) return;

                colourLight = value;
                updateColours();
            }
        }

        private Color4 colourDark = Color4.Black;

        public Color4 ColourDark
        {
            get => colourDark;
            set
            {
                if (colourDark == value) return;

                colourDark = value;
                updateColours();
            }
        }

        /// <summary>
        /// Whether we should create new triangles as others expire.
        /// </summary>
        protected virtual bool CreateNewTriangles => true;

        /// <summary>
        /// The amount of triangles we want compared to the default distribution.
        /// </summary>
        protected virtual float SpawnRatio => 1;

        private readonly BindableFloat triangleScale = new BindableFloat(1f);

        public float TriangleScale
        {
            get => triangleScale.Value;
            set => triangleScale.Value = value;
        }

        /// <summary>
        /// If enabled, only the portion of triangles that falls within this <see cref="Drawable"/>'s
        /// shape is drawn to the screen. Default is true.
        /// </summary>
        public bool ClampToDrawable { get; set; } = true;

        /// <summary>
        /// Whether we should drop-off alpha values of triangles more quickly to improve
        /// the visual appearance of fading. This defaults to on as it is generally more
        /// aesthetically pleasing, but should be turned off in buffered containers.
        /// </summary>
        public bool HideAlphaDiscrepancies = true;

        /// <summary>
        /// The relative velocity of the triangles. Default is 1.
        /// </summary>
        public float Velocity = 1;

        private readonly SortedList<TriangleParticle> parts = new SortedList<TriangleParticle>(Comparer<TriangleParticle>.Default);

        private Random stableRandom;
        private IShader shader;
        private Texture texture;

        /// <summary>
        /// Construct a new triangle visualisation.
        /// </summary>
        /// <param name="seed">An optional seed to stabilise random positions / attributes. Note that this does not guarantee stable playback when seeking in time.</param>
        public Triangles(int? seed = null)
        {
            if (seed != null)
                stableRandom = new Random(seed.Value);
        }

        [BackgroundDependencyLoader]
        private void load(IRenderer renderer, ShaderManager shaders)
        {
            texture = renderer.WhitePixel;
            shader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "TriangleBorder");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            triangleScale.BindValueChanged(_ => Reset(), true);
        }

        protected override void Update()
        {
            base.Update();

            Invalidate(Invalidation.DrawNode);

            if (CreateNewTriangles)
                addTriangles(false);

            float adjustedAlpha = HideAlphaDiscrepancies
                // Cubically scale alpha to make it drop off more sharply.
                ? MathF.Pow(DrawColourInfo.Colour.AverageColour.Linear.A, 3)
                : 1;

            float elapsedSeconds = (float)Time.Elapsed / 1000;
            // Since position is relative, the velocity needs to scale inversely with DrawHeight.
            // Since we will later multiply by the scale of individual triangles we normalize by
            // dividing by triangleScale.
            float movedDistance = -elapsedSeconds * Velocity * base_velocity / (DrawHeight * TriangleScale);

            for (int i = 0; i < parts.Count; i++)
            {
                TriangleParticle newParticle = parts[i];

                // Scale moved distance by the size of the triangle. Smaller triangles should move more slowly.
                newParticle.Position.Y += Math.Max(0.5f, parts[i].Scale) * movedDistance;
                newParticle.Colour.A = adjustedAlpha;

                parts[i] = newParticle;

                float bottomPos = parts[i].Position.Y + triangle_size * parts[i].Scale * equilateral_triangle_ratio / DrawHeight;
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

            AimCount = (int)Math.Min(max_triangles, DrawWidth * DrawHeight * 0.002f / (TriangleScale * TriangleScale) * SpawnRatio);

            int currentCount = parts.Count;

            for (int i = 0; i < AimCount - currentCount; i++)
                parts.Add(createTriangle(randomY));
        }

        private TriangleParticle createTriangle(bool randomY)
        {
            TriangleParticle particle = CreateTriangle();

            particle.Position = getRandomPosition(randomY, particle.Scale);
            particle.ColourShade = nextRandom();
            particle.Colour = CreateTriangleShade(particle.ColourShade);

            return particle;
        }

        private Vector2 getRandomPosition(bool randomY, float scale)
        {
            float y = 1;

            if (randomY)
            {
                // since triangles are drawn from the top - allow them to be positioned a bit above the screen
                float maxOffset = triangle_size * scale * equilateral_triangle_ratio / DrawHeight;
                y = Interpolation.ValueAt(nextRandom(), -maxOffset, 1f, 0f, 1f);
            }

            return new Vector2(nextRandom(), y);
        }

        /// <summary>
        /// Creates a triangle particle with a random scale.
        /// </summary>
        /// <returns>The triangle particle.</returns>
        protected virtual TriangleParticle CreateTriangle()
        {
            const float std_dev = 0.16f;
            const float mean = 0.5f;

            float u1 = 1 - nextRandom(); //uniform(0,1] random floats
            float u2 = 1 - nextRandom();
            float randStdNormal = (float)(Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2)); // random normal(0,1)
            float scale = Math.Max(TriangleScale * (mean + std_dev * randStdNormal), 0.1f); // random normal(mean,stdDev^2)

            return new TriangleParticle { Scale = scale };
        }

        /// <summary>
        /// Creates a shade of colour for the triangles.
        /// </summary>
        /// <returns>The colour.</returns>
        protected virtual Color4 CreateTriangleShade(float shade) => Interpolation.ValueAt(shade, colourDark, colourLight, 0, 1);

        private void updateColours()
        {
            for (int i = 0; i < parts.Count; i++)
            {
                TriangleParticle newParticle = parts[i];
                newParticle.Colour = CreateTriangleShade(newParticle.ColourShade);
                parts[i] = newParticle;
            }
        }

        private float nextRandom() => (float)(stableRandom?.NextDouble() ?? RNG.NextSingle());

        protected override DrawNode CreateDrawNode() => new TrianglesDrawNode(this);

        private class TrianglesDrawNode : DrawNode
        {
            private const float fill = 1f;

            protected new Triangles Source => (Triangles)base.Source;

            private IShader shader;
            private Texture texture;
            private bool clamp;

            private readonly List<TriangleParticle> parts = new List<TriangleParticle>();
            private readonly Vector2 triangleSize = new Vector2(1f, equilateral_triangle_ratio) * triangle_size;

            private Vector2 size;

            public TrianglesDrawNode(Triangles source)
                : base(source)
            {
            }

            public override void ApplyState()
            {
                base.ApplyState();

                shader = Source.shader;
                texture = Source.texture;
                size = Source.DrawSize;
                clamp = Source.ClampToDrawable;

                parts.Clear();
                parts.AddRange(Source.parts);
            }

            private IUniformBuffer<TriangleBorderData> borderDataBuffer;

            protected override void Draw(IRenderer renderer)
            {
                base.Draw(renderer);

                borderDataBuffer ??= renderer.CreateUniformBuffer<TriangleBorderData>();
                borderDataBuffer.Data = borderDataBuffer.Data with
                {
                    Thickness = fill,
                    // Due to triangles having various sizes we would need to set a different "TexelSize" value for each of them, which is insanely expensive, thus we should use one single value.
                    // TexelSize computed for an average triangle (size 100) will result in big triangles becoming blurry, so we may just use 0 for all of them.
                    TexelSize = 0
                };

                shader.Bind();
                shader.BindUniformBlock(@"m_BorderData", borderDataBuffer);

                foreach (TriangleParticle particle in parts)
                {
                    Vector2 relativeSize = Vector2.Divide(triangleSize * particle.Scale, size);

                    Vector2 topLeft = particle.Position - new Vector2(relativeSize.X * 0.5f, 0f);

                    Quad triangleQuad = clamp ? clampToDrawable(topLeft, relativeSize) : new Quad(topLeft.X, topLeft.Y, relativeSize.X, relativeSize.Y);

                    var drawQuad = new Quad(
                        Vector2Extensions.Transform(triangleQuad.TopLeft * size, DrawInfo.Matrix),
                        Vector2Extensions.Transform(triangleQuad.TopRight * size, DrawInfo.Matrix),
                        Vector2Extensions.Transform(triangleQuad.BottomLeft * size, DrawInfo.Matrix),
                        Vector2Extensions.Transform(triangleQuad.BottomRight * size, DrawInfo.Matrix)
                    );

                    ColourInfo colourInfo = DrawColourInfo.Colour;
                    colourInfo.ApplyChild(particle.Colour);

                    RectangleF textureCoords = new RectangleF(
                        triangleQuad.TopLeft.X - topLeft.X,
                        triangleQuad.TopLeft.Y - topLeft.Y,
                        triangleQuad.Width,
                        triangleQuad.Height
                    ) / relativeSize;

                    renderer.DrawQuad(texture, drawQuad, colourInfo, new RectangleF(0, 0, 1, 1), textureCoords: textureCoords);
                }

                shader.Unbind();
            }

            private static Quad clampToDrawable(Vector2 topLeft, Vector2 size)
            {
                float leftClamped = Math.Clamp(topLeft.X, 0f, 1f);
                float topClamped = Math.Clamp(topLeft.Y, 0f, 1f);

                return new Quad(
                    leftClamped,
                    topClamped,
                    Math.Clamp(topLeft.X + size.X, 0f, 1f) - leftClamped,
                    Math.Clamp(topLeft.Y + size.Y, 0f, 1f) - topClamped
                );
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                borderDataBuffer?.Dispose();
            }
        }

        protected struct TriangleParticle : IComparable<TriangleParticle>
        {
            /// <summary>
            /// The position of the top vertex of the triangle.
            /// </summary>
            public Vector2 Position;

            /// <summary>
            /// The colour shade of the triangle.
            /// This is needed for colour recalculation of visible triangles when <see cref="ColourDark"/> or <see cref="ColourLight"/> is changed.
            /// </summary>
            public float ColourShade;

            /// <summary>
            /// The colour of the triangle.
            /// </summary>
            public Color4 Colour;

            /// <summary>
            /// The scale of the triangle.
            /// </summary>
            public float Scale;

            /// <summary>
            /// Compares two <see cref="TriangleParticle"/>s. This is a reverse comparer because when the
            /// triangles are added to the particles list, they should be drawn from largest to smallest
            /// such that the smaller triangles appear on top.
            /// </summary>
            /// <param name="other"></param>
            public int CompareTo(TriangleParticle other) => other.Scale.CompareTo(Scale);
        }
    }
}
