// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Tests.Visual
{
    /// <summary>
    /// A <see cref="Container"/> which providing ad-hoc dependencies to the child drawables.
    /// </summary>
    /// <remarks>
    /// The <see cref="CachedDependencies"/> must be set while this <see cref="DependencyProvidingContainer"/> is not loaded.
    /// </remarks>
    public partial class DependencyProvidingContainer : Container
    {
        /// <summary>
        /// The dependencies provided to the children.
        /// </summary>
        public (Type, object)[] CachedDependencies { get; init; } = Array.Empty<(Type, object)>();

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencyContainer = new DependencyContainer(base.CreateChildDependencies(parent));

            foreach ((var type, object value) in CachedDependencies)
                dependencyContainer.CacheAs(type, value);

            return dependencyContainer;
        }
    }
}
