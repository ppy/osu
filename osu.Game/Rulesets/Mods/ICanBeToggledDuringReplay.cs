// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// An interface for mods that can be toggle during replay.
    /// </summary>
    public interface ICanBeToggledDuringReplay : IApplicableMod
    {
        /// <summary>
        /// A property to get a bool value whether the current replay state.
        /// </summary>
        Bindable<bool> ReplayLoaded
        {
            get;
        }

        /// <summary>
        /// A property to set whether the mod has been disabled.
        /// </summary>
        bool IsDisable
        {
            get;
        }

        /// <summary>
        /// Method called when mod toggle.
        /// Need check <see cref="ReplayLoaded"/> is not false and toggle the value of IsDisable.
        /// </summary>
        void OnToggle();
    }
}
