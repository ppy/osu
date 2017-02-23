using osu.Framework.Graphics.Containers;
using OpenTK;
using OpenTK.Input;
using osu.Game.Modes.Vitaru.Objects.Drawables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Graphics.Colour;
using osu.Game.Modes.Vitaru.Objects.Drawables.Pieces;
using OpenTK.Graphics;

namespace osu.Game.Modes.Vitaru.Objects
{
    public class Hitbox : Container
    {
        private DrawableCircle hitboxSprite;
        private float hitboxWidth = 16;
        private Color4 hitboxColor = Color4.Cyan;

        public Color4 HitboxColor
        {
            get
            {
                return hitboxColor;
            }
            set
            {
                hitboxColor = value;
                hitboxSprite.CircleColor = value;
            }
        }
        public int Health { get; set; } = 100;
        public float HitboxWidth
        {
            get
            {
                return hitboxWidth;
            }
            set
            {
                hitboxWidth = value;
                hitboxSprite.CircleWidth = value;
            }
        }


        public Hitbox()
        {
            Children = new[]
            {
                hitboxSprite = new DrawableCircle
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                },
            };
            Hide();
        }
    }
}
