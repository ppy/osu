// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using OpenTK;

namespace osu.Mods.Evast.Galaga
{
    public class BulletsContainer : Container<Bullet>
    {
        public BulletsContainer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        public void AddNewBullet(Vector2 position, BulletTarget target)
        {
            Bullet newBullet;

            Add(newBullet = new Bullet
            {
                Position = position,
                Target = target,
            });

            newBullet.MoveToX(target == BulletTarget.Player ? 0 : 1, 1000 * (target == BulletTarget.Player ? position.X : 1 - position.X));
            newBullet.Expire();
        }
    }
}
