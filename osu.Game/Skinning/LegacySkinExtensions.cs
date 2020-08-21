// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK.Graphics;
using static osu.Game.Skinning.LegacySkinConfiguration;

namespace osu.Game.Skinning
{
    public static class LegacySkinExtensions
    {
        public static Drawable GetAnimation(this ISkin source, string componentName, bool animatable, bool looping, bool applyConfigFrameRate = false, string animationSeparator = "-",
                                            bool startAtCurrentTime = true, double? frameLength = null)
            => source.GetAnimation(componentName, default, default, animatable, looping, applyConfigFrameRate, animationSeparator, startAtCurrentTime, frameLength);

        public static Drawable GetAnimation(this ISkin source, string componentName, WrapMode wrapModeS, WrapMode wrapModeT, bool animatable, bool looping, bool applyConfigFrameRate = false,
                                            string animationSeparator = "-",
                                            bool startAtCurrentTime = true, double? frameLength = null)
        {
            Texture texture;

            if (animatable)
            {
                var textures = getTextures().ToArray();

                if (textures.Length > 0)
                {
                    var animation = new SkinnableTextureAnimation(startAtCurrentTime)
                    {
                        DefaultFrameLength = frameLength ?? getFrameLength(source, applyConfigFrameRate, textures),
                        Loop = looping,
                    };

                    foreach (var t in textures)
                        animation.AddFrame(t);

                    return animation;
                }
            }

            // if an animation was not allowed or not found, fall back to a sprite retrieval.
            if ((texture = source.GetTexture(componentName, wrapModeS, wrapModeT)) != null)
                return new Sprite { Texture = texture };

            return null;

            IEnumerable<Texture> getTextures()
            {
                for (int i = 0; true; i++)
                {
                    if ((texture = source.GetTexture($"{componentName}{animationSeparator}{i}", wrapModeS, wrapModeT)) == null)
                        break;

                    yield return texture;
                }
            }
        }

        /// <summary>
        /// The resultant colour after setting a post-constructor colour in osu!stable.
        /// </summary>
        /// <param name="colour">The <see cref="Color4"/> to convert.</param>
        /// <returns>The converted <see cref="Color4"/>.</returns>
        public static Color4 ToLegacyColour(this Color4 colour)
        {
            if (colour.A == 0)
                colour.A = 1;
            return colour;
        }

        /// <summary>
        /// Equivalent of setting a colour in the constructor in osu!stable.
        /// Doubles the alpha channel into <see cref="Drawable.Alpha"/> and uses <see cref="ToLegacyColour"/> to set <see cref="Drawable.Colour"/>.
        /// </summary>
        /// <remarks>
        /// Beware: Any existing value in <see cref="Drawable.Alpha"/> is overwritten.
        /// </remarks>
        /// <param name="drawable">The <see cref="Drawable"/> to set the colour of.</param>
        /// <param name="colour">The <see cref="Color4"/> to set.</param>
        /// <returns>The given <see cref="Drawable"/>.</returns>
        public static T WithLegacyColour<T>(this T drawable, Color4 colour)
            where T : Drawable
        {
            drawable.Alpha = colour.A;
            drawable.Colour = ToLegacyColour(colour);
            return drawable;
        }

        public class SkinnableTextureAnimation : TextureAnimation
        {
            [Resolved(canBeNull: true)]
            private IAnimationTimeReference timeReference { get; set; }

            public SkinnableTextureAnimation(bool startAtCurrentTime = true)
                : base(startAtCurrentTime)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                if (timeReference != null)
                {
                    Clock = timeReference.Clock;
                    PlaybackPosition = timeReference.Clock.CurrentTime - timeReference.AnimationStartTime;
                }
            }
        }

        private const double default_frame_time = 1000 / 60d;

        private static double getFrameLength(ISkin source, bool applyConfigFrameRate, Texture[] textures)
        {
            if (applyConfigFrameRate)
            {
                var iniRate = source.GetConfig<LegacySetting, int>(LegacySetting.AnimationFramerate);

                if (iniRate?.Value > 0)
                    return 1000f / iniRate.Value;

                return 1000f / textures.Length;
            }

            return default_frame_time;
        }
    }
}
