//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
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

        private float triangleScale = 1;

        public float TriangleScale
        {
            get { return triangleScale; }
            set
            {
                triangleScale = value;

                Children.ForEach(t => t.ScaleTo(triangleScale));
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            for (int i = 0; i < aimTriangleCount; i++)
                addTriangle(true);
        }

        private int aimTriangleCount => (int)(DrawWidth * DrawHeight * 0.002f / (triangleScale * triangleScale));

        protected override void Update()
        {
            base.Update();

            foreach (Drawable d in Children)
            {
                d.Position -= new Vector2(0, (float)(d.Scale.X * (50 / DrawHeight) * (Time.Elapsed / 950)) / triangleScale);
                if (d.DrawPosition.Y + d.DrawSize.Y * d.Scale.Y < 0)
                    d.Expire();
            }

            while (Children.Count() < aimTriangleCount)
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
                // Scaling height by 0.866 results in equiangular triangles (== 60° and equal side length)
                Colour = GetTriangleShade(),
                Size = new Vector2(size, 0.866f * size),
                Depth = scale,
            };
        }

        protected virtual Color4 GetTriangleShade() => Interpolation.ValueAt(RNG.NextSingle(), ColourDark, ColourLight, 0, 1);

        private void addTriangle(bool randomY)
        {
            var sprite = CreateTriangle();
            var triangleHeight = sprite.DrawHeight / DrawHeight;
            sprite.Position = new Vector2(RNG.NextSingle(), randomY ? (RNG.NextSingle() * (1 + triangleHeight) - triangleHeight) : 1);
            Add(sprite);
        }
    }
}
