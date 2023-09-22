// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// A target expanding container that should be resolved by children <see cref="IExpandable"/>s to propagate state changes.
    /// </summary>
    [Cached(typeof(IExpandingContainer))]
    public interface IExpandingContainer : IContainer, IExpandable
    {
    }
}
