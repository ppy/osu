// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;

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
        /// Short important information to display on the mod icon. For example, a rate adjust mod's rate
        /// or similarly important setting.
        /// Use <see cref="string.Empty"/> if the icon should not display any additional info.
        /// </summary>
        string ExtendedIconInformation { get; }

        /// <summary>
        /// The user readable description of this mod.
        /// </summary>
        LocalisableString Description { get; }

        /// <summary>
        /// The type of this mod.
        /// </summary>
        ModType Type { get; }

        /// <summary>
        /// The icon of this mod.
        /// </summary>
        IconUsage? Icon { get; }

        /// <summary>
        /// Whether this mod is playable by a real human user.
        /// Should be <c>false</c> for cases where the user is not interacting with the game (so it can be excluded from multiplayer selection, for example).
        /// </summary>
        bool UserPlayable { get; }

        /// <summary>
        /// Whether this mod is valid for multiplayer matches.
        /// Should be <c>false</c> for mods that make gameplay duration dependent on user input (e.g. <see cref="ModAdaptiveSpeed"/>).
        /// </summary>
        bool ValidForMultiplayer { get; }

        /// <summary>
        /// Whether this mod is valid as a required mod when freestyle is enabled.
        /// Should be <c>true</c> for mods that are guaranteed to be implemented across all rulesets.
        /// </summary>
        bool ValidForFreestyleAsRequiredMod { get; }

        /// <summary>
        /// Whether this mod is valid as a free mod in multiplayer matches.
        /// Should be <c>false</c> for mods that affect the gameplay duration (e.g. <see cref="ModRateAdjust"/> and <see cref="ModTimeRamp"/>).
        /// </summary>
        bool ValidForMultiplayerAsFreeMod { get; }

        /// <summary>
        /// Indicates that this mod is always permitted in scenarios wherein a user is submitting a score regardless of other circumstances.
        /// Intended for mods that are informational in nature and do not really affect gameplay by themselves,
        /// but are more of a gauge of increased/decreased difficulty due to the user's configuration (e.g. <see cref="ModTouchDevice"/>).
        /// </summary>
        bool AlwaysValidForSubmission { get; }

        /// <summary>
        /// Whether scores with this mod active can give performance points.
        /// </summary>
        bool Ranked { get; }

        /// <summary>
        /// Create a fresh <see cref="Mod"/> instance based on this mod.
        /// </summary>
        Mod CreateInstance() => (Mod)Activator.CreateInstance(GetType())!;

        /// <summary>
        /// Whether any user adjustable setting attached to this mod has a non-default value.
        /// </summary>
        /// <remarks>
        /// This returns the instantaneous state of this mod. It may change over time.
        /// For tracking changes on a dynamic display, make sure to setup a <see cref="ModSettingChangeTracker"/>.
        /// </remarks>
        bool HasNonDefaultSettings
        {
            get
            {
                bool hasAdjustments = false;

                foreach (var (_, property) in this.GetSettingsSourceProperties())
                {
                    var bindable = (IBindable)property.GetValue(this)!;

                    if (!bindable.IsDefault)
                    {
                        hasAdjustments = true;
                        break;
                    }
                }

                return hasAdjustments;
            }
        }
    }
}
