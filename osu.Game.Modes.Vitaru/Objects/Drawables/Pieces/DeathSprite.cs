using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Modes.Touhosu.UI;
using osu.Game.Modes.Touhosu.Objects.Characters;
using osu.Framework.Graphics;

namespace osu.Game.Modes.Touhosu.Objects.Drawables.Pieces
{
    class DeathSprite : Container
    {
        public DeathSprite DSprite;
        private TouhosuPlayer player;

        public DeathSprite()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Children = new[]
            {
                sprite = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    BlendingMode = BlendingMode.Additive,
                }
            };
        }

        public PlayerSprite(TouhosuPlayer player)
        {
            this.player = player;
        }

        public bool IsCounting { get; set; }
        public object[] Children { get; private set; }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            sprite.Texture = textures.Get(@"Play/Touhosu/player");
        }
    }
}