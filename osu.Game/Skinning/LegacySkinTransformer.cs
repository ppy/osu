// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;

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

        public Texture GetTexture(string componentName) => Source.GetTexture(componentName);

        public virtual SampleChannel GetSample(ISampleInfo sampleInfo) => Source.GetSample(sampleInfo);

        public abstract IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup);
    }
}
