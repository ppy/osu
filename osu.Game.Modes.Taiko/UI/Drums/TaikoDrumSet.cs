using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.UI.Drums
{
    public class TaikoDrumSet : Container
    {
        private Sprite drumBase;

        public TaikoDrumSet(Key[] keys)
        {
            Children = new Drawable[]
            {
                drumBase = new Sprite()
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,

                    RelativeSizeAxes = Axes.Both
                },
                new TaikoDrumInner(keys)
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,

                    RelativeSizeAxes = Axes.Both
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            drumBase.Texture = textures.Get("Play/Taiko/taiko-drum@2x");
        }
    }
}
