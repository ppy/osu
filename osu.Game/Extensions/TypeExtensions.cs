// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;

namespace osu.Game.Extensions
{
    internal static class TypeExtensions
    {
        /// <summary>
        /// Returns <paramref name="type"/>'s <see cref="Type.AssemblyQualifiedName"/>
        /// with the assembly version, culture and public key token values removed.
        /// </summary>
        /// <remarks>
        /// This method is usually used in extensibility scenarios (i.e. for custom rulesets or skins)
        /// when a version-agnostic identifier associated with a C# class - potentially originating from
        /// an external assembly - is needed.
        /// Leaving only the type and assembly names in such a scenario allows to preserve compatibility
        /// across assembly versions.
        /// </remarks>
        internal static string GetInvariantInstantiationInfo(this Type type)
        {
            string? assemblyQualifiedName = type.AssemblyQualifiedName;
            if (assemblyQualifiedName == null)
                throw new ArgumentException($"{type}'s assembly-qualified name is null. Ensure that it is a concrete type and not a generic type parameter.", nameof(type));

            return string.Join(',', assemblyQualifiedName.Split(',').Take(2));
        }
    }
}
