// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Mods
{
    public interface IPlayfieldTypeMod : IApplicableMod
    {
        /// <summary>
        /// The <see cref="PlayfieldType"/> which this <see cref="IPlayfieldTypeMod"/> requires.
        /// </summary>
        PlayfieldType PlayfieldType { get; }
    }
}
