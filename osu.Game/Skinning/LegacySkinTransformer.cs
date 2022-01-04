// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects.Legacy;
using static osu.Game.Skinning.SkinConfiguration;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Transformer used to handle support of legacy features for individual rulesets.
    /// </summary>
    public abstract class LegacySkinTransformer : ISkin
    {
        /// <summary>
        /// The <see cref="ISkin"/> which is being transformed.
        /// </summary>
        [NotNull]
        protected ISkin Skin { get; }

        protected LegacySkinTransformer([NotNull] ISkin skin)
        {
            Skin = skin ?? throw new ArgumentNullException(nameof(skin));
        }

        public virtual Drawable GetDrawableComponent(ISkinComponent component) => Skin.GetDrawableComponent(component);

        public Texture GetTexture(string componentName) => GetTexture(componentName, default, default);

        public Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT)
            => Skin.GetTexture(componentName, wrapModeS, wrapModeT);

        public virtual ISample GetSample(ISampleInfo sampleInfo)
        {
            if (!(sampleInfo is ConvertHitObjectParser.LegacyHitSampleInfo legacySample))
                return Skin.GetSample(sampleInfo);

            var playLayeredHitSounds = GetConfig<LegacySetting, bool>(LegacySetting.LayeredHitSounds);
            if (legacySample.IsLayered && playLayeredHitSounds?.Value == false)
                return new SampleVirtual();

            return Skin.GetSample(sampleInfo);
        }

        public virtual IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup) => Skin.GetConfig<TLookup, TValue>(lookup);
    }
}
