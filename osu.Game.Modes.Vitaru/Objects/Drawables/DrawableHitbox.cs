using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Vitaru.Objects.Drawables.Pieces;

namespace osu.Game.Modes.Vitaru.Objects.Drawables
{
    public class DrawableHitbox : Container
    {
        private HitboxPiece hitboxPiece;
        public DrawableHitbox(Hitbox hitbox)
        {
            Children = new Drawable[]
            {
                hitboxPiece = new HitboxPiece(hitbox)
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                }
            };
        }

        public void revealHitbox(bool visible)
        {
            hitboxPiece.Alpha = visible ? 1 : 0;
        }

        protected override void Update()
        {
            base.Update();
        }
    }
}
