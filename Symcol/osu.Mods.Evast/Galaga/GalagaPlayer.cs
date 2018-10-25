// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace osu.Mods.Evast.Galaga
{
    public class GalagaPlayer : GalagaObject
    {
        private const double base_speed = 1.0 / 512 / 2;

        public GalagaPlayer(BulletsContainer bulletsContainer) : base(bulletsContainer)
        {
            Origin = Anchor.Centre;
            RelativePositionAxes = Axes.Both;
            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(0.04f, 0.02f);
            Child = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.White,
            };
        }

        private Vector2 currentDirection = Vector2.Zero;

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    currentDirection.Y--;
                    return true;
                case Key.Down:
                    currentDirection.Y++;
                    return true;
                case Key.Left:
                    currentDirection.X--;
                    return true;
                case Key.Right:
                    currentDirection.X++;
                    return true;
            }

            return base.OnKeyDown(e);
        }

        protected override bool OnKeyUp(KeyUpEvent e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    currentDirection.Y++;
                    return true;
                case Key.Down:
                    currentDirection.Y--;
                    return true;
                case Key.Left:
                    currentDirection.X++;
                    return true;
                case Key.Right:
                    currentDirection.X--;
                    return true;
            }

            return base.OnKeyUp(e);
        }

        public void Shoot() => BulletsContainer.AddNewBullet(new Vector2(X, Y), BulletTarget.Enemy);

        protected override void Update()
        {
            base.Update();

            //Moving
            if (currentDirection.X == 0 && currentDirection.Y == 0 && !BulletsContainer.Children.Any()) return;

            X = (float)MathHelper.Clamp(X + Math.Sign(currentDirection.X) * Clock.ElapsedFrameTime * (base_speed / 2), 0.1, 0.5);
            Y = (float)MathHelper.Clamp(Y + Math.Sign(currentDirection.Y) * Clock.ElapsedFrameTime * base_speed, 0.1, 0.9);

            if (currentDirection.X < -1)
                currentDirection.X = -1;

            if (currentDirection.X > 1)
                currentDirection.X = 1;

            if (currentDirection.Y < -1)
                currentDirection.Y = -1;

            if (currentDirection.Y > 1)
                currentDirection.Y = 1;

            //Bullets
            foreach (var bullet in BulletsContainer.Children)
            {
                if (bullet.Target != BulletTarget.Player)
                    continue;

                if (bullet.Position.X > X - Size.X / 2 && bullet.Position.X < X + Size.X / 2 &&
                    bullet.Position.Y > Y - Size.Y / 2 && bullet.Position.Y < Y + Size.Y / 2)
                {
                    //TODO: interaction with the bullet

                    bullet.ClearTransforms();
                    bullet.Expire();
                }
            }
        }
    }
}
