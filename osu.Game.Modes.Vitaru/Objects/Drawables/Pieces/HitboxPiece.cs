using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Modes.Vitaru.Objects.Drawables.Pieces
{
    class HitboxPiece : Container
    {
        private CircularContainer hitboxContainer;
        private object hitbox;

        public HitboxPiece(Hitbox hitbox)
        {
            this.hitbox = hitbox;
            Children = new Drawable[]
            {
                new Container
                {
                    Masking = true,
                    AutoSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    BorderThickness = hitbox.hitboxWidth / 4,
                    Depth = 1,
                    BorderColour = hitbox.hitboxColor,
                    Alpha = 1f,
                    CornerRadius = hitbox.hitboxWidth / 2,
                    Children = new[]
                    {
                        new Box
                        {
                            Colour = Color4.White,
                            Alpha = 1,
                            Width = hitbox.hitboxWidth,
                            Height = hitbox.hitboxWidth,
                        },
                    },
                },
                hitboxContainer = new CircularContainer
                {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Scale = new Vector2(hitbox.hitboxWidth),
                        Depth = 2,
                        Masking = true,
                        EdgeEffect = new EdgeEffect
                        {
                            Type = EdgeEffectType.Shadow,
                            Colour = (hitbox.hitboxColor).Opacity(0.4f),
                            Radius = hitbox.hitboxWidth / 8,
                        }
                }
            };
        }
    }
}