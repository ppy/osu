// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Skinning
{
    /// <summary>
    /// A skin transformer takes in an <see cref="ISkin"/> and applies transformations to it.
    /// The most common use case is allowing individual rulesets to add skinnable components without directly coupling to underlying skins.
    /// </summary>
    public interface ISkinTransformer : ISkin
    {
        /// <summary>
        /// The original skin that is being transformed.
        /// </summary>
        ISkin Skin { get; }
    }
}
