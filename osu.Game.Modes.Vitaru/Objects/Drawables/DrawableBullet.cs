using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Vitaru.Objects.Drawables.Pieces;
using osu.Game.Modes.Vitaru.Objects.Projectiles;

namespace osu.Game.Modes.Vitaru.Objects.Drawables
{
    class DrawableBullet : Container
    {
        private BulletPiece bulletPiece;
        public DrawableBullet(Bullet b)
        {
            Children = new Drawable[]
            {
                bulletPiece = new BulletPiece(b)
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                }
            };
        }


        protected override void Update()
        {
            base.Update();
        }
    }
}
