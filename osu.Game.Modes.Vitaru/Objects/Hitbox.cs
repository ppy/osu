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
        private DrawableHitbox hitboxSprite;
        public Color4 hitboxColor { get; set; } = Color4.Cyan;
        public int health { get; set; } = 100;
        public float hitboxWidth { get; set; } = 16;


        public Hitbox()
        {
            Children = new[]
            {
                hitboxSprite = new DrawableHitbox(this)
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                },
            };
            Hide();
        }
    }
}
