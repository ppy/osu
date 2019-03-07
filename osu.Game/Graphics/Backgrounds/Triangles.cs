// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osuTK;
using osuTK.Graphics;
using System;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osuTK.Graphics.ES30;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Allocation;
using System.Collections.Generic;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Lists;

namespace osu.Game.Graphics.Backgrounds
{
    public class Triangles : Drawable
    {
        private const float triangle_size = 100;
        private const float base_velocity = 50;

        /// <summary>
        /// How many screen-space pixels are smoothed over.
        /// Same behavior as Sprite's EdgeSmoothness.
        /// </summary>
        private const float edge_smoothness = 1;

        public Color4 ColourLight = Color4.White;
        public Color4 ColourDark = Color4.Black;

        /// <summary>
        /// Whether we want to expire triangles as they exit our draw area completely.
        /// </summary>
        protected virtual bool ExpireOffScreenTriangles => true;

        /// <summary>
        /// Whether we should create new triangles as others expire.
        /// </summary>
        protected virtual bool CreateNewTriangles => true;

        /// <summary>
        /// The amount of triangles we want compared to the default distribution.
        /// </summary>
        protected virtual float SpawnRatio => 1;

        private float triangleScale = 1;

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

        private IShader shader;
        private readonly Texture texture;

        public Triangles()
        {
            texture = Texture.WhitePixel;
        }

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders)
        {
            shader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE_ROUNDED);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            addTriangles(true);
        }

        public float TriangleScale
        {
            get => triangleScale;
            set
            {
                float change = value / triangleScale;
                triangleScale = value;

                for (int i = 0; i < parts.Count; i++)
                {
                    TriangleParticle newParticle = parts[i];
                    newParticle.Scale *= change;
                    parts[i] = newParticle;
                }
            }
        }

        protected override void Update()
        {
            base.Update();

            Invalidate(Invalidation.DrawNode, shallPropagate: false);

            if (CreateNewTriangles)
                addTriangles(false);

            float adjustedAlpha = HideAlphaDiscrepancies
                // Cubically scale alpha to make it drop off more sharply.
                ? (float)Math.Pow(DrawColourInfo.Colour.AverageColour.Linear.A, 3)
                : 1;

            float elapsedSeconds = (float)Time.Elapsed / 1000;
            // Since position is relative, the velocity needs to scale inversely with DrawHeight.
            // Since we will later multiply by the scale of individual triangles we normalize by
            // dividing by triangleScale.
            float movedDistance = -elapsedSeconds * Velocity * base_velocity / (DrawHeight * triangleScale);

            for (int i = 0; i < parts.Count; i++)
            {
                TriangleParticle newParticle = parts[i];

                // Scale moved distance by the size of the triangle. Smaller triangles should move more slowly.
                newParticle.Position.Y += parts[i].Scale * movedDistance;
                newParticle.Colour.A = adjustedAlpha;

                parts[i] = newParticle;

                float bottomPos = parts[i].Position.Y + triangle_size * parts[i].Scale * 0.866f / DrawHeight;
                if (bottomPos < 0)
                    parts.RemoveAt(i);
            }
        }

        private void addTriangles(bool randomY)
        {
            int aimTriangleCount = (int)(DrawWidth * DrawHeight * 0.002f / (triangleScale * triangleScale) * SpawnRatio);

            for (int i = 0; i < aimTriangleCount - parts.Count; i++)
                parts.Add(createTriangle(randomY));
        }

        private TriangleParticle createTriangle(bool randomY)
        {
            TriangleParticle particle = CreateTriangle();

            particle.Position = new Vector2(RNG.NextSingle(), randomY ? RNG.NextSingle() : 1);
            particle.Colour = CreateTriangleShade();

            return particle;
        }

        /// <summary>
        /// Creates a triangle particle with a random scale.
        /// </summary>
        /// <returns>The triangle particle.</returns>
        protected virtual TriangleParticle CreateTriangle()
        {
            const float std_dev = 0.16f;
            const float mean = 0.5f;

            float u1 = 1 - RNG.NextSingle(); //uniform(0,1] random floats
            float u2 = 1 - RNG.NextSingle();
            float randStdNormal = (float)(Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2)); //random normal(0,1)
            var scale = Math.Max(triangleScale * (mean + std_dev * randStdNormal), 0.1f); //random normal(mean,stdDev^2)

            return new TriangleParticle { Scale = scale };
        }

        /// <summary>
        /// Creates a shade of colour for the triangles.
        /// </summary>
        /// <returns>The colour.</returns>
        protected virtual Color4 CreateTriangleShade() => Interpolation.ValueAt(RNG.NextSingle(), ColourDark, ColourLight, 0, 1);

        protected override DrawNode CreateDrawNode() => new TrianglesDrawNode();

        protected override void ApplyDrawNode(DrawNode node)
        {
            base.ApplyDrawNode(node);

            var trianglesNode = (TrianglesDrawNode)node;

            trianglesNode.Shader = shader;
            trianglesNode.Texture = texture;
            trianglesNode.Size = DrawSize;

            trianglesNode.Parts.Clear();
            trianglesNode.Parts.AddRange(parts);
        }

        private class TrianglesDrawNode : DrawNode
        {
            public IShader Shader;
            public Texture Texture;

            public readonly List<TriangleParticle> Parts = new List<TriangleParticle>();
            public Vector2 Size;

            private readonly LinearBatch<TexturedVertex2D> vertexBatch = new LinearBatch<TexturedVertex2D>(100 * 3, 10, PrimitiveType.Triangles);

            public override void Draw(Action<TexturedVertex2D> vertexAction)
            {
                base.Draw(vertexAction);

                Shader.Bind();
                Texture.TextureGL.Bind();

                Vector2 localInflationAmount = edge_smoothness * DrawInfo.MatrixInverse.ExtractScale().Xy;

                foreach (TriangleParticle particle in Parts)
                {
                    var offset = triangle_size * new Vector2(particle.Scale * 0.5f, particle.Scale * 0.866f);
                    var size = new Vector2(2 * offset.X, offset.Y);

                    var triangle = new Triangle(
                        Vector2Extensions.Transform(particle.Position * Size, DrawInfo.Matrix),
                        Vector2Extensions.Transform(particle.Position * Size + offset, DrawInfo.Matrix),
                        Vector2Extensions.Transform(particle.Position * Size + new Vector2(-offset.X, offset.Y), DrawInfo.Matrix)
                    );

                    ColourInfo colourInfo = DrawColourInfo.Colour;
                    colourInfo.ApplyChild(particle.Colour);

                    Texture.DrawTriangle(
                        triangle,
                        colourInfo,
                        null,
                        vertexBatch.AddAction,
                        Vector2.Divide(localInflationAmount, size));
                }

                Shader.Unbind();
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                vertexBatch.Dispose();
            }
        }

        protected struct TriangleParticle : IComparable<TriangleParticle>
        {
            /// <summary>
            /// The position of the top vertex of the triangle.
            /// </summary>
            public Vector2 Position;

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
            /// <returns></returns>
            public int CompareTo(TriangleParticle other) => other.Scale.CompareTo(Scale);
        }
    }
}
