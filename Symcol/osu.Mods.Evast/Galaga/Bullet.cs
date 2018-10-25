// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Mods.Evast.Galaga
{
    public class Bullet : CircularContainer
    {
        private BulletTarget target = BulletTarget.Enemy;
        public BulletTarget Target
        {
            set { target = value; }
            get { return target; }
        }

        public Bullet()
        {
            Origin = Anchor.Centre;
            RelativePositionAxes = Axes.Both;
            Size = new Vector2(10);
            Masking = true;
            Child = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.White,
            };
        }
    }
}
