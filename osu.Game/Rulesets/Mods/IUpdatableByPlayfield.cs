// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// An interface for <see cref="Mod"/>s that are updated every frame by a <see cref="Playfield"/>.
    /// </summary>
    public interface IUpdatableByPlayfield : IApplicableMod
    {
        /// <summary>
        /// Update this <see cref="Mod"/>.
        /// </summary>
        /// <param name="playfield">The main <see cref="Playfield"/></param>
        /// <remarks>
        /// This method is called once per frame during gameplay by the main <see cref="Playfield"/> only.
        /// To access nested <see cref="Playfield"/>s, use <see cref="Playfield.NestedPlayfields"/>.
        /// </remarks>
        void Update(Playfield playfield);
    }
}
