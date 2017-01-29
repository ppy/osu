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

namespace osu.Game.Graphics.Backgrounds
{
    public class Triangles : Container<Triangle>
    {
        public override bool HandleInput => false;

        public Triangles()
        {
            Alpha = 0.3f;
        }

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

        private int aimTriangleCount => (int)((DrawWidth * DrawHeight) / 800 / triangleScale);

        protected override void Update()
        {
            base.Update();

            foreach (Drawable d in Children)
            {
                d.Position -= new Vector2(0, (float)(d.Scale.X * (50 / DrawHeight) * (Time.Elapsed / 880)) / triangleScale);
                if (d.DrawPosition.Y + d.DrawSize.Y * d.Scale.Y < 0)
                    d.Expire();
            }

            bool useRandomX = Children.Count() < aimTriangleCount / 2;
            while (Children.Count() < aimTriangleCount)
                addTriangle(useRandomX);

        }

        protected virtual Triangle CreateTriangle()
        {
            var scale = triangleScale * RNG.NextSingle() * 0.4f + 0.2f;
            const float size = 100;

            return new Triangle
            {
                Origin = Anchor.TopCentre,
                RelativePositionAxes = Axes.Both,
                Scale = new Vector2(scale),
                // Scaling height by 0.866 results in equiangular triangles (== 60° and equal side length)
                Size = new Vector2(size, 0.866f * size),
                Alpha = RNG.NextSingle(),
                Depth = scale,
            };
        }

        private void addTriangle(bool randomX)
        {
            var sprite = CreateTriangle();
            sprite.Position = new Vector2(RNG.NextSingle(), randomX ? RNG.NextSingle() : 1);
            Add(sprite);
        }
    }
}
