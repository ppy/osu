// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Objects.Types
{

    /// <summary>
    /// A HitObject that is part of a snap and has extended information about its position relative to other snap objects.
    /// </summary>
    public interface IHasSnapInformation
    {

        int SnapIndex { get; set; }
        /// <summary>
        /// Retrieves the colour of the snap described by this <see cref="IHasSnapInformation"/> object.
        /// </summary>
        /// <param name="skin">The skin to retrieve the snap colour from, if wanted.</param>
        Color4 GetSnapColour(ISkin skin) => GetSkinSnapColour(this, skin, SnapIndex);

        /// <summary>
        /// Retrieves the colour of the snap described by a given <see cref="IHasSnapInformation"/> object from a given skin.
        /// </summary>
        /// <param name="snap">The snap information, should be <c>this</c>.</param>
        /// <param name="skin">The skin to retrieve the snap colour from.</param>
        /// <param name="snapIndex">The index to retrieve the snap colour with.</param>
        /// <returns></returns>
        protected static Color4 GetSkinSnapColour(IHasSnapInformation snap, ISkin skin, int snapIndex)
        {
            return skin.GetConfig<SkinSnapColourLookup, Color4>(new SkinSnapColourLookup(snapIndex, snap))?.Value ?? Color4.White;
        }
    }
}
