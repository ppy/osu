// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Taiko.Mods
{
    /// <summary>
    /// An interface for <see cref="Mod"/>s that can be applied to <see cref="TaikoModClassic"/>.
    /// </summary>
    public interface IApplicableToTaikoClassic : IApplicableMod
    {
        /// <summary>
        /// Applies this <see cref="IApplicableToTaikoClassic"/> to a <see cref="TaikoModClassic"/>.
        /// This will be called if and only if <see cref="TaikoModClassic"/> is enabled.
        /// </summary>
        public void ApplyToTaikoModClassic(TaikoModClassic taikoModClassic);
    }
}