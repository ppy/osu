//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.MathUtils;
using OpenTK;

namespace osu.Game.Graphics.Backgrounds
{
    public class Triangles : Container
    {
        private Texture triangle;

        public Triangles()
        {
            Masking = true;
            Alpha = 0.3f;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            triangle = textures.Get(@"Play/osu/triangle@2x");
        }

        private int aimTriangleCount => (int)((DrawWidth * DrawHeight) / 800);

        protected override void Update()
        {
            base.Update();

            foreach (Drawable d in Children)
            {
                d.Position -= new Vector2(0, (float)(d.Scale.X * (Time.Elapsed / 880)));
                if (d.DrawPosition.Y + d.DrawSize.Y * d.Scale.Y < 0)
                    d.Expire();
            }

            bool useRandomX = Children.Count() < aimTriangleCount / 2;
            while (Children.Count() < aimTriangleCount)
                addTriangle(useRandomX);

        }

        private void addTriangle(bool randomX)
        {
            Add(new Sprite
            {
                Texture = triangle,
                Origin = Anchor.TopCentre,
                RelativePositionAxes = Axes.Both,
                Position = new Vector2(RNG.NextSingle(), randomX ? RNG.NextSingle() : 1),
                Scale = new Vector2(RNG.NextSingle() * 0.4f + 0.2f),
                Alpha = RNG.NextSingle()
            });
        }
    }
}
