// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Tests.Visual
{
    /// <summary>
    /// A <see cref="Container"/> which providing ad-hoc dependencies to the child drawables.
    /// <para>
    /// To provide a dependency, specify the dependency type to <see cref="Types"/>, then specify the dependency value to either <see cref="Values"/> or <see cref="Container{Drawable}.Children"/>.
    /// For each type specified in <see cref="Types"/>, the first value compatible with the type is selected and provided to the children.
    /// </para>
    /// </summary>
    /// <remarks>
    /// The <see cref="Types"/> and values of the dependencies must be set while this <see cref="DependencyProvidingContainer"/> is not loaded.
    /// </remarks>
    public class DependencyProvidingContainer : Container
    {
        /// <summary>
        /// The types of the dependencies provided to the children.
        /// </summary>
        // TODO: should be an init-only property when C# 9
        public Type[] Types { get; set; } = Array.Empty<Type>();

        /// <summary>
        /// The dependency values provided to the children.
        /// </summary>
        public object[] Values { get; set; } = Array.Empty<object>();

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencyContainer = new DependencyContainer(base.CreateChildDependencies(parent));

            foreach (var type in Types)
            {
                object value = Values.FirstOrDefault(v => type.IsInstanceOfType(v)) ??
                               Children.FirstOrDefault(d => type.IsInstanceOfType(d)) ??
                               throw new InvalidOperationException($"The type {type} is specified in this {nameof(DependencyProvidingContainer)}, but no corresponding value is provided.");

                dependencyContainer.CacheAs(type, value);
            }

            return dependencyContainer;
        }
    }
}
