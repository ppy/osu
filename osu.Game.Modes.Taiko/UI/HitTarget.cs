using OpenTK;
using OpenTK.Graphics;
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

namespace osu.Game.Modes.Taiko.UI
{
    class HitTarget : Container
    {
        private Sprite outer;
        private Sprite inner;

        public HitTarget()
        {
            AutoSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                    new Box()
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,

                        Size = new Vector2(5, 106),

                        Colour = Color4.Black
                    },
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,

                        Size = new Vector2(106),
                        Scale = new Vector2(0.7f),

                        Children = new[]
                        {
                            outer = new Sprite()
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,

                                RelativeSizeAxes = Axes.Both,
                            },
                            inner = new Sprite()
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,

                                RelativeSizeAxes = Axes.Both,
                                Size = new Vector2(0.7f)
                            }
                        }
                    }
            };
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            outer.Texture = textures.Get(@"Play/Taiko/taiko-drum-outer");
            inner.Texture = textures.Get(@"Play/Taiko/taiko-drum-inner");
        }
    }
}
