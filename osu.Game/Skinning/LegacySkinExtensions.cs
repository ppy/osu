// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using static osu.Game.Skinning.SkinConfiguration;

namespace osu.Game.Skinning
{
    public static class LegacySkinExtensions
    {
        [CanBeNull]
        public static Drawable GetAnimation(this ISkin source, string componentName, bool animatable, bool looping, bool applyConfigFrameRate = false, string animationSeparator = "-",
                                            bool startAtCurrentTime = true, double? frameLength = null)
            => source.GetAnimation(componentName, default, default, animatable, looping, applyConfigFrameRate, animationSeparator, startAtCurrentTime, frameLength);

        [CanBeNull]
        public static Drawable GetAnimation(this ISkin source, string componentName, WrapMode wrapModeS, WrapMode wrapModeT, bool animatable, bool looping, bool applyConfigFrameRate = false,
                                            string animationSeparator = "-",
                                            bool startAtCurrentTime = true, double? frameLength = null)
        {
            Texture texture;

            // find the first source which provides either the animated or non-animated version.
            ISkin skin = (source as ISkinSource)?.FindProvider(s =>
            {
                if (animatable && s.GetTexture(getFrameName(0)) != null)
                    return true;

                return s.GetTexture(componentName, wrapModeS, wrapModeT) != null;
            }) ?? source;

            if (skin == null)
                return null;

            if (animatable)
            {
                var textures = getTextures().ToArray();

                if (textures.Length > 0)
                {
                    var animation = new SkinnableTextureAnimation(startAtCurrentTime)
                    {
                        DefaultFrameLength = frameLength ?? getFrameLength(skin, applyConfigFrameRate, textures),
                        Loop = looping,
                    };

                    foreach (var t in textures)
                        animation.AddFrame(t);

                    return animation;
                }
            }

            // if an animation was not allowed or not found, fall back to a sprite retrieval.
            if ((texture = skin.GetTexture(componentName, wrapModeS, wrapModeT)) != null)
                return new Sprite { Texture = texture };

            return null;

            IEnumerable<Texture> getTextures()
            {
                for (int i = 0; true; i++)
                {
                    if ((texture = skin.GetTexture(getFrameName(i), wrapModeS, wrapModeT)) == null)
                        break;

                    yield return texture;
                }
            }

            string getFrameName(int frameIndex) => $"{componentName}{animationSeparator}{frameIndex}";
        }

        public static bool HasFont(this ISkin source, LegacyFont font)
        {
            return source.GetTexture($"{source.GetFontPrefix(font)}-0") != null;
        }

        public static string GetFontPrefix(this ISkin source, LegacyFont font)
        {
            switch (font)
            {
                case LegacyFont.Score:
                    return source.GetConfig<LegacySetting, string>(LegacySetting.ScorePrefix)?.Value ?? "score";

                case LegacyFont.Combo:
                    return source.GetConfig<LegacySetting, string>(LegacySetting.ComboPrefix)?.Value ?? "score";

                case LegacyFont.HitCircle:
                    return source.GetConfig<LegacySetting, string>(LegacySetting.HitCirclePrefix)?.Value ?? "default";

                default:
                    throw new ArgumentOutOfRangeException(nameof(font));
            }
        }

        /// <summary>
        /// Returns the numeric overlap of number sprites to use.
        /// A positive number will bring the number sprites closer together, while a negative number
        /// will split them apart more.
        /// </summary>
        public static float GetFontOverlap(this ISkin source, LegacyFont font)
        {
            switch (font)
            {
                case LegacyFont.Score:
                    return source.GetConfig<LegacySetting, float>(LegacySetting.ScoreOverlap)?.Value ?? 0f;

                case LegacyFont.Combo:
                    return source.GetConfig<LegacySetting, float>(LegacySetting.ComboOverlap)?.Value ?? 0f;

                case LegacyFont.HitCircle:
                    return source.GetConfig<LegacySetting, float>(LegacySetting.HitCircleOverlap)?.Value ?? -2f;

                default:
                    throw new ArgumentOutOfRangeException(nameof(font));
            }
        }

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
