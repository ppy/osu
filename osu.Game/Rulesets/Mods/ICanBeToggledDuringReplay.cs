// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// An interface for mods that can be toggle during replay.
    /// </summary>
    public interface ICanBeToggledDuringReplay : IApplicableMod
    {
        /// <summary>
        /// A property to set whether the mod has been disabled.
        /// </summary>
        bool IsDisable
        {
            get;
        }

        /// <summary>
        /// Method called when mod toggle.
        /// </summary>
        void OnToggle();
    }
}
