//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.MathUtils;
using OpenTK;

namespace osu.Game.Modes.Osu.Objects.Drawables.Pieces
{
    public class Triangles : Container<Triangle>
    {
        public override bool HandleInput => false;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            const float size = 100;
            for (int i = 0; i < 10; i++)
            {
                Add(new Triangle
                {
                    Origin = Anchor.Centre,
                    RelativePositionAxes = Axes.Both,
                    Position = new Vector2(RNG.NextSingle(), RNG.NextSingle()),
                    Scale = new Vector2(RNG.NextSingle() * 0.4f + 0.2f),
                    // Scaling height by 0.866 results in equiangular triangles (== 60° and equal side length)
                    Size = new Vector2(size, 0.866f * size),
                    Alpha = RNG.NextSingle() * 0.3f,
                });
            }
        }

        protected override void Update()
        {
            base.Update();

            foreach (Drawable d in Children)
                d.Position -= new Vector2(0, (float)(d.Scale.X * (Time.Elapsed / 2880)));
        }
    }
}