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
        private DrawableHitbox hitbox;
        internal Color4 hitboxColor { get; set; }
        public int health { get; private set; }
        public float hitboxWidth { get; set; }


        public Hitbox()
        {
            Children = new[]
            {
                hitbox = new DrawableHitbox(this)
                {
                    Origin = Anchor.Centre,
                },
            };
            Hide();
        }

        internal float GetRadius()
        {
          return (hitboxWidth * 0.5f);
        }
    }
}
