// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;
using static osu.Game.Skinning.SkinConfiguration;

namespace osu.Game.Skinning
{
    public static partial class LegacySkinExtensions
    {
        public static Drawable? GetAnimation(this ISkin? source, string componentName, bool animatable, bool looping, bool applyConfigFrameRate = false, string animationSeparator = "-",
                                             bool startAtCurrentTime = true, double? frameLength = null, Vector2? maxSize = null)
            => source.GetAnimation(componentName, default, default, animatable, looping, applyConfigFrameRate, animationSeparator, startAtCurrentTime, frameLength, maxSize);

        public static Drawable? GetAnimation(this ISkin? source, string componentName, WrapMode wrapModeS, WrapMode wrapModeT, bool animatable, bool looping, bool applyConfigFrameRate = false,
                                             string animationSeparator = "-", bool startAtCurrentTime = true, double? frameLength = null, Vector2? maxSize = null)
        {
            if (source == null)
                return null;

            var textures = GetTextures(source, componentName, wrapModeS, wrapModeT, animatable, animationSeparator, maxSize, out var retrievalSource);

            switch (textures.Length)
            {
                case 0:
                    return null;

                case 1:
                    return new Sprite { Texture = textures[0] };

                default:
                    Debug.Assert(retrievalSource != null);

                    var animation = new SkinnableTextureAnimation(startAtCurrentTime)
                    {
                        DefaultFrameLength = frameLength ?? getFrameLength(retrievalSource, applyConfigFrameRate, textures),
                        Loop = looping,
                    };

                    foreach (var t in textures)
                        animation.AddFrame(t);

                    return animation;
            }
        }

        public static Texture[] GetTextures(this ISkin? source, string componentName, WrapMode wrapModeS, WrapMode wrapModeT, bool animatable, string animationSeparator, Vector2? maxSize, out ISkin? retrievalSource)
        {
            retrievalSource = null;

            if (source == null)
                return Array.Empty<Texture>();

            // find the first source which provides either the animated or non-animated version.
            retrievalSource = (source as ISkinSource)?.FindProvider(s =>
            {
                if (animatable && s.GetTexture(getFrameName(0)) != null)
                    return true;

                return s.GetTexture(componentName, wrapModeS, wrapModeT) != null;
            }) ?? source;

            if (animatable)
            {
                var textures = getTextures(retrievalSource).ToArray();

                if (textures.Length > 0)
                    return textures;
            }

            // if an animation was not allowed or not found, fall back to a sprite retrieval.
            var singleTexture = retrievalSource.GetTexture(componentName, wrapModeS, wrapModeT);

            if (singleTexture != null && maxSize != null)
                singleTexture = singleTexture.WithMaximumSize(maxSize.Value);

            return singleTexture != null
                ? new[] { singleTexture }
                : Array.Empty<Texture>();

            IEnumerable<Texture> getTextures(ISkin skin)
            {
                for (int i = 0; true; i++)
                {
                    var texture = skin.GetTexture(getFrameName(i), wrapModeS, wrapModeT);

                    if (texture == null)
                        break;

                    if (maxSize != null)
                        texture = texture.WithMaximumSize(maxSize.Value);

                    yield return texture;
                }
            }

            string getFrameName(int frameIndex) => $"{componentName}{animationSeparator}{frameIndex}";
        }

        public static Texture WithMaximumSize(this Texture texture, Vector2 maxSize)
        {
            if (texture.DisplayWidth <= maxSize.X && texture.DisplayHeight <= maxSize.Y)
                return texture;

            maxSize *= texture.ScaleAdjust;

            var croppedTexture = texture.Crop(new RectangleF(texture.Width / 2f - maxSize.X / 2f, texture.Height / 2f - maxSize.Y / 2f, maxSize.X, maxSize.Y));
            croppedTexture.ScaleAdjust = texture.ScaleAdjust;
            return croppedTexture;
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

        public partial class SkinnableTextureAnimation : TextureAnimation
        {
            [Resolved(canBeNull: true)]
            private IAnimationTimeReference? timeReference { get; set; }

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

        /// <summary>
        /// The frame length of each frame at a 60 FPS rate.
        /// Default frame rate for legacy skin animations.
        /// </summary>
        public const double SIXTY_FRAME_TIME = 1000 / 60d;

        private static double getFrameLength(ISkin source, bool applyConfigFrameRate, Texture[] textures)
        {
            if (applyConfigFrameRate)
            {
                var iniRate = source.GetConfig<LegacySetting, int>(LegacySetting.AnimationFramerate);

                if (iniRate?.Value > 0)
                    return 1000f / iniRate.Value;

                return 1000f / textures.Length;
            }

            return SIXTY_FRAME_TIME;
        }
    }
}
