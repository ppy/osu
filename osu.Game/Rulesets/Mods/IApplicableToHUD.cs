// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// An interface for mods that apply changes to the <see cref="HUDOverlay"/>.
    /// </summary>
    public interface IApplicableToHUD : IApplicableMod
    {
        /// <summary>
        /// Provide a <see cref="HUDOverlay"/>. Called once on initialisation of a play instance.
        /// </summary>
        void ApplyToHUD(HUDOverlay overlay);
    }
}
