using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Modes.Vitaru.Objects.Projectiles;

namespace osu.Game.Modes.Vitaru.Objects.Drawables.Pieces
{
    class BulletPiece : Container
    {
        private CircularContainer bulletContainer;
        private object bullet;

        public BulletPiece(Bullet bullet)
        {
            this.bullet = bullet;
            Children = new Drawable[]
            {
                new Container
                {
                    Masking = true,
                    AutoSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    BorderThickness = bullet.bulletWidth / 4,
                    Depth = 1,
                    BorderColour = bullet.bulletColor,
                    Alpha = 1f,
                    CornerRadius = bullet.bulletWidth / 2,
                    Children = new[]
                    {
                        new Box
                        {
                            Colour = Color4.White,
                            Alpha = 1,
                            Width = bullet.bulletWidth,
                            Height = bullet.bulletWidth,
                        },
                    },
                },
                bulletContainer = new CircularContainer
                {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Scale = new Vector2(bullet.bulletWidth),
                        Depth = 2,
                        Masking = true,
                        EdgeEffect = new EdgeEffect
                        {
                            Type = EdgeEffectType.Shadow,
                            Colour = (bullet.bulletColor).Opacity(0.75f),
                            Radius = bullet.bulletWidth / 8,
                        }
                }
            };
        }
    }
}