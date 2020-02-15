// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Skinning
{
    public static class LegacySkinExtensions
    {
        public static Drawable GetAnimation(this ISkin source, string componentName, bool animatable, bool looping, bool applyConfigFrameRate = false, string animationSeparator = "-")
        {
            Texture texture;

            if (animatable)
            {
                var textures = getTextures().ToArray();

                if (textures.Length > 0)
                {
                    var animation = new TextureAnimation
                    {
                        DefaultFrameLength = getFrameLength(source, applyConfigFrameRate, textures),
                        Repeat = looping,
                    };

                    foreach (var t in textures)
                        animation.AddFrame(t);

                    return animation;
                }
            }

            // if an animation was not allowed or not found, fall back to a sprite retrieval.
            if ((texture = source.GetTexture(componentName)) != null)
                return new Sprite { Texture = texture };

            return null;

            IEnumerable<Texture> getTextures()
            {
                for (int i = 0; true; i++)
                {
                    if ((texture = source.GetTexture($"{componentName}{animationSeparator}{i}")) == null)
                        break;

                    yield return texture;
                }
            }
        }

        private const double default_frame_time = 1000 / 60d;

        private static double getFrameLength(ISkin source, bool applyConfigFrameRate, Texture[] textures)
        {
            if (applyConfigFrameRate)
            {
                var iniRate = source.GetConfig<GlobalSkinConfiguration, int>(GlobalSkinConfiguration.AnimationFramerate);

                if (iniRate != null)
                    return 1000f / iniRate.Value;

                return 1000f / textures.Length;
            }

            return default_frame_time;
        }
    }
}
