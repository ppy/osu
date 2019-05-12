// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using osu.Framework.Graphics.Sprites;
using osu.Game.IO.Serialization;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// The base class for gameplay modifiers.
    /// </summary>
    public abstract class Mod : IMod, IJsonSerializable
    {
        /// <summary>
        /// The name of this mod.
        /// </summary>
        [JsonIgnore]
        public abstract string Name { get; }

        /// <summary>
        /// The shortened name of this mod.
        /// </summary>
        public abstract string Acronym { get; }

        /// <summary>
        /// The icon of this mod.
        /// </summary>
        [JsonIgnore]
        public virtual IconUsage Icon => FontAwesome.Solid.Question;

        /// <summary>
        /// The type of this mod.
        /// </summary>
        [JsonIgnore]
        public virtual ModType Type => ModType.Fun;

        /// <summary>
        /// The user readable description of this mod.
        /// </summary>
        [JsonIgnore]
        public virtual string Description => string.Empty;

        /// <summary>
        /// The score multiplier of this mod.
        /// </summary>
        [JsonIgnore]
        public abstract double ScoreMultiplier { get; }

        /// <summary>
        /// Returns true if this mod is implemented (and playable).
        /// </summary>
        [JsonIgnore]
        public virtual bool HasImplementation => this is IApplicableMod;

        /// <summary>
        /// Returns if this mod is ranked.
        /// </summary>
        [JsonIgnore]
        public virtual bool Ranked => false;

        /// <summary>
        /// The mods this mod cannot be enabled with.
        /// </summary>
        [JsonIgnore]
        public virtual Type[] IncompatibleMods => new Type[] { };

        /// <summary>
        /// Creates a copy of this <see cref="Mod"/> initialised to a default state.
        /// </summary>
        public virtual Mod CreateCopy() => (Mod)Activator.CreateInstance(GetType());

        public bool Equals(IMod other) => GetType() == other?.GetType();
    }
}
