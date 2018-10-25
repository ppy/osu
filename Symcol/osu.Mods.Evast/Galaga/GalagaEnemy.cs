// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Mods.Evast.Galaga
{
    public class GalagaEnemy : GalagaObject
    {
        public GalagaEnemy(BulletsContainer bulletsContainer) : base(bulletsContainer)
        {
            Origin = Anchor.Centre;
            RelativePositionAxes = Axes.Both;
            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(0.02f, 0.04f);
            Child = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.White,
            };
        }

        public void Shoot()
        {
            if (IsAlive)
                BulletsContainer.AddNewBullet(new Vector2(X, Y), BulletTarget.Player);
        }

        protected override void Update()
        {
            base.Update();

            if (!BulletsContainer.Children.Any())
                return;

            foreach (var bullet in BulletsContainer.Children)
            {
                if (bullet.Target != BulletTarget.Enemy)
                    continue;

                if (bullet.Position.X > X - Size.X / 2 && bullet.Position.X < X + Size.X / 2 &&
                    bullet.Position.Y > Y - Size.Y / 2 && bullet.Position.Y < Y + Size.Y / 2)
                {
                    Expire();

                    bullet.ClearTransforms();
                    bullet.Expire();
                }
            }
        }
    }
}
