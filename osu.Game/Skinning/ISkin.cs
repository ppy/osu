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
    /// Provides access to various elements contained by a skin.
    /// </summary>
    public interface ISkin
    {
        /// <summary>
        /// Retrieve a <see cref="Drawable"/> component implementation.
        /// </summary>
        /// <param name="lookup">The requested component.</param>
        /// <returns>A drawable representation for the requested component, or null if unavailable.</returns>
        Drawable? GetDrawableComponent(ISkinComponentLookup lookup);

        /// <summary>
        /// Retrieve a <see cref="Texture"/>.
        /// </summary>
        /// <param name="componentName">The requested texture.</param>
        /// <returns>A matching texture, or null if unavailable.</returns>
        Texture? GetTexture(string componentName) => GetTexture(componentName, default, default);

        /// <summary>
        /// Retrieve a <see cref="Texture"/>.
        /// </summary>
        /// <param name="componentName">The requested texture.</param>
        /// <param name="wrapModeS">The texture wrap mode in horizontal direction.</param>
        /// <param name="wrapModeT">The texture wrap mode in vertical direction.</param>
        /// <returns>A matching texture, or null if unavailable.</returns>
        Texture? GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT);

        /// <summary>
        /// Retrieve a <see cref="SampleChannel"/>.
        /// </summary>
        /// <param name="sampleInfo">The requested sample.</param>
        /// <returns>A matching sample channel, or null if unavailable.</returns>
        ISample? GetSample(ISampleInfo sampleInfo);

        /// <summary>
        /// Retrieve a configuration value.
        /// </summary>
        /// <param name="lookup">The requested configuration value.</param>
        /// <returns>A matching value boxed in an <see cref="IBindable{TValue}"/>, or null if unavailable.</returns>
        IBindable<TValue>? GetConfig<TLookup, TValue>(TLookup lookup)
            where TLookup : notnull
            where TValue : notnull;
    }
}
