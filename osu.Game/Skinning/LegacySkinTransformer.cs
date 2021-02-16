// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects.Legacy;
using static osu.Game.Skinning.LegacySkinConfiguration;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Transformer used to handle support of legacy features for individual rulesets.
    /// </summary>
    public abstract class LegacySkinTransformer : ISkin
    {
        /// <summary>
        /// Source of the <see cref="ISkin"/> which is being transformed.
        /// </summary>
        protected ISkinSource Source { get; }

        protected LegacySkinTransformer(ISkinSource source)
        {
            Source = source;
        }

        public abstract Drawable GetDrawableComponent(ISkinComponent component);

        public Texture GetTexture(string componentName) => GetTexture(componentName, default, default);

        public Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT)
            => Source.GetTexture(componentName, wrapModeS, wrapModeT);

        public virtual Sample GetSample(ISampleInfo sampleInfo)
        {
            if (!(sampleInfo is ConvertHitObjectParser.LegacyHitSampleInfo legacySample))
                return Source.GetSample(sampleInfo);

            var playLayeredHitSounds = GetConfig<LegacySetting, bool>(LegacySetting.LayeredHitSounds);
            if (legacySample.IsLayered && playLayeredHitSounds?.Value == false)
                return new SampleVirtual();

            return Source.GetSample(sampleInfo);
        }

        public abstract IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup);
    }
}
