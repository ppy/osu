using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Vitaru.Objects.Characters;
using osu.Game.Modes.Vitaru.Objects.Drawables.Pieces;
using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Modes.Vitaru.Objects.Drawables
{
    public class DrawablePlayer : Container
    {

        private PlayerSprite playerSprite;
        private SpellPiece playerSpell;
        //private Glow playerGlow;

        public DrawablePlayer()
        {
            Children = new Drawable[]
            {
                playerSprite = new PlayerSprite()
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                },
                playerSpell = new SpellPiece()
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Alpha = 0,
                },
                /*
                playerGlow = new Glow()
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Alpha = 0,
                }*/

                //Death Sprite, Not concern right now

                /*DSprite = new DeathSprite
                {
                    Scale = new Vector2(s.Scale),
                    Colour = p.Colour,
                },*/
            };
        }

        public override bool Contains(Vector2 screenSpacePos) => true;
      

        protected override void Update()
        {
            base.Update();
        }

        public void setKiai(bool visible)
        {
            playerSpell.Alpha = visible ? 1: 0;
            //playerGlow.Alpha = visible ? 1 : 0;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            
        }
    }
}