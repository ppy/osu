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
        /// A property to get whether the mod has been disabled, can register event handle in constructor.
        /// </summary>
        BindableBool IsDisabled
        {
            get;
        }
    }
}
