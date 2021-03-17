// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
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

        public static bool HasFont(this ISkin source, string fontPrefix)
            => source.GetTexture($"{fontPrefix}-0") != null;

        public class SkinnableTextureAnimation : TextureAnimation
        {
            [Resolved(canBeNull: true)]
            private IAnimationTimeReference timeReference { get; set; }

            private readonly Bindable<double> animationStartTime = new BindableDouble();

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
                    animationStartTime.BindTo(timeReference.AnimationStartTime);
                }

                animationStartTime.BindValueChanged(_ => updatePlaybackPosition(), true);
            }

            private void updatePlaybackPosition()
            {
                if (timeReference == null)
                    return;

                PlaybackPosition = timeReference.Clock.CurrentTime - timeReference.AnimationStartTime.Value;
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
