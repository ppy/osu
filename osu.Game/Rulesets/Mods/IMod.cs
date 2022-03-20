// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Rulesets.Mods
{
    public interface IMod : IEquatable<IMod>
    {
        /// <summary>
        /// The shortened name of this mod.
        /// </summary>
        string Acronym { get; }

        /// <summary>
        /// The name of this mod.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The user readable description of this mod.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// The type of this mod.
        /// </summary>
        ModType Type { get; }

        /// <summary>
        /// The icon of this mod.
        /// </summary>
        IconUsage? Icon { get; }

        /// <summary>
        /// Whether this mod is playable for the given usage.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>Should be always <c>false</c> for cases where the user is not interacting with the game.</item>
        /// <item>Should be <c>false</c> in <see cref="ModUsage.MultiplayerRoomWide"/> for mods that make gameplay duration dependent on user input (e.g. <see cref="ModAdaptiveSpeed"/>).</item>
        /// <item>Should be <c>false</c> in <see cref="ModUsage.MultiplayerPerPlayer"/> for mods that affect the gameplay duration (e.g. <see cref="ModRateAdjust"/> and <see cref="ModTimeRamp"/>).</item>
        /// </list>
        /// </remarks>
        /// <param name="usage">The mod usage.</param>
        bool IsPlayable(ModUsage usage);

        /// <summary>
        /// Whether this mod is playable by an end user.
        /// Should be <c>false</c> for cases where the user is not interacting with the game (so it can be excluded from multiplayer selection, for example).
        /// </summary>
        [Obsolete("Override IsPlayable instead.")] // Can be removed 20220918
        bool UserPlayable { get; }

        /// <summary>
        /// Create a fresh <see cref="Mod"/> instance based on this mod.
        /// </summary>
        Mod CreateInstance() => (Mod)Activator.CreateInstance(GetType());
    }
}
