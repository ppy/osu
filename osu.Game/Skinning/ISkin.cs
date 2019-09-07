// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Provides access to skinnable elements.
    /// </summary>
    public interface ISkin
    {
        /// <summary>
        /// Retrieve a <see cref="Drawable"/> component implementation.
        /// </summary>
        /// <param name="component">The requested component.</param>
        /// <returns>A drawable representation for the requested component, or null if unavailable.</returns>
        [CanBeNull]
        Drawable GetDrawableComponent(ISkinComponent component);

        /// <summary>
        /// Retrieve a <see cref="Texture"/>.
        /// </summary>
        /// <param name="componentName">The requested texture.</param>
        /// <returns>A matching texture, or null if unavailable.</returns>
        [CanBeNull]
        Texture GetTexture(string componentName);

        /// <summary>
        /// Retrieve a <see cref="SampleChannel"/>.
        /// </summary>
        /// <param name="sampleInfo">The requested sample.</param>
        /// <returns>A matching sample channel, or null if unavailable.</returns>
        [CanBeNull]
        SampleChannel GetSample(ISampleInfo sampleInfo);

        /// <summary>
        /// Retrieve a configuration value.
        /// </summary>
        /// <param name="lookup">The requested configuration value.</param>
        /// <returns>A matching value boxed in an <see cref="IBindable{TValue}"/>, or null if unavailable.</returns>
        [CanBeNull]
        IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup);
    }
}
