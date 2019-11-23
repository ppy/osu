// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Skinning
{
    public static class LegacySkinExtensions
    {
        public static Drawable GetAnimation(this ISkin source, string componentName, bool animatable, bool looping, string animationSeparator = "-")
        {
            const double default_frame_time = 1000 / 60d;

            Texture texture;

            Texture getFrameTexture(int frame) => source.GetTexture($"{componentName}{animationSeparator}{frame}");

            TextureAnimation animation = null;

            if (animatable)
            {
                for (int i = 0;; i++)
                {
                    if ((texture = getFrameTexture(i)) == null)
                        break;

                    if (animation == null)
                        animation = new TextureAnimation
                        {
                            DefaultFrameLength = default_frame_time,
                            Repeat = looping
                        };

                    animation.AddFrame(texture);
                }
            }

            if (animation != null)
                return animation;

            if ((texture = source.GetTexture(componentName)) != null)
                return new Sprite
                {
                    Texture = texture
                };

            return null;
        }
    }
}
