// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.MathUtils;
using OpenTK;
using OpenTK.Graphics;
using System;

namespace osu.Game.Graphics.Backgrounds
{
    public class Triangles : Container<Triangle>
    {
        public override bool HandleInput => false;

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
        /// aesthetically pleasing, but should be turned off in <see cref="BufferedContainer{T}"/>s.
        /// </summary>
        public bool HideAlphaDiscrepancies = true;

        public float TriangleScale
        {
            get { return triangleScale; }
            set
            {
                float change = value / triangleScale;
                triangleScale = value;

                if (change != 1)
                    Children.ForEach(t => t.Scale *= change);
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            for (int i = 0; i < aimTriangleCount; i++)
                addTriangle(true);
        }

        private int aimTriangleCount => (int)(DrawWidth * DrawHeight * 0.002f / (triangleScale * triangleScale) * SpawnRatio);

        protected override void Update()
        {
            base.Update();

            float adjustedAlpha = HideAlphaDiscrepancies ?
                // Cubically scale alpha to make it drop off more sharply.
                (float)Math.Pow(DrawInfo.Colour.AverageColour.Linear.A, 3) :
                1;

            foreach (var t in Children)
            {
                t.Alpha = adjustedAlpha;
                t.Position -= new Vector2(0, (float)(t.Scale.X * (50 / DrawHeight) * (Time.Elapsed / 950)) / triangleScale);
                if (ExpireOffScreenTriangles && t.DrawPosition.Y + t.DrawSize.Y * t.Scale.Y < 0)
                    t.Expire();
            }

            while (CreateNewTriangles && Children.Count() < aimTriangleCount)
                addTriangle(false);
        }

        protected virtual Triangle CreateTriangle()
        {
            float stdDev = 0.16f;
            float mean = 0.5f;

            float u1 = 1 - RNG.NextSingle(); //uniform(0,1] random floats
            float u2 = 1 - RNG.NextSingle();
            float randStdNormal = (float)(Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2)); //random normal(0,1)
            var scale = Math.Max(triangleScale * (mean + stdDev * randStdNormal), 0.1f); //random normal(mean,stdDev^2)

            const float size = 100;

            return new Triangle
            {
                Origin = Anchor.TopCentre,
                RelativePositionAxes = Axes.Both,
                Scale = new Vector2(scale),
                EdgeSmoothness = new Vector2(1),
                Colour = GetTriangleShade(),
                // Scaling height by 0.866 results in equiangular triangles (== 60° and equal side length)
                Size = new Vector2(size, 0.866f * size),
                Depth = scale,
            };
        }

        protected virtual Color4 GetTriangleShade() => Interpolation.ValueAt(RNG.NextSingle(), ColourDark, ColourLight, 0, 1);

        private void addTriangle(bool randomY)
        {
            var sprite = CreateTriangle();
            float triangleHeight = (sprite.DrawHeight / DrawHeight);
            sprite.Position = new Vector2(RNG.NextSingle(), randomY ? RNG.NextSingle() * (1 + triangleHeight) - triangleHeight : 1);
            Add(sprite);
        }
    }
}
