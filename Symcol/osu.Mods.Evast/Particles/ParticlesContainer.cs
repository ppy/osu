// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Framework.MathUtils;
using OpenTK;

namespace osu.Mods.Evast.Particles
{
    public class ParticlesContainer : Container
    {
        public ParticlesContainer(int xAmount, int yAmount, int size = 1, int spacing = 0)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Size = new Vector2(xAmount * spacing + xAmount * size, yAmount * spacing + yAmount * size);

            for (int j = 0; j < yAmount; j++)
            {
                for (int i = 0; i < xAmount; i++)
                {
                    Add(new Particle(size)
                    {
                        Position = new Vector2(i * spacing + i * size, j * spacing + j * size),
                    });
                }
            }
        }

        protected override bool OnClick(ClickEvent e)
        {
            foreach (var p in Children)
            {
                p.MoveTo(RNG.NextBool() ? new Vector2(0, Size.X / 2) : new Vector2(Size.Y, Size.X / 2), RNG.NextDouble(100, 5000), Easing.OutQuad);
            }

            return base.OnClick(e);
        }

        private class Particle : Container
        {
            public Particle(int size)
            {
                Anchor = Anchor.TopLeft;
                Origin = Anchor.Centre;
                Size = new Vector2(size);
                Child = new Box { RelativeSizeAxes = Axes.Both, };
            }
        }
    }
}
