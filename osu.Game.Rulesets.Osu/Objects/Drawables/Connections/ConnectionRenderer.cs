// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Connections
{
    /// <summary>
    /// Connects hit objects visually, for example with follow points.
    /// </summary>
    public abstract class ConnectionRenderer<T> : LifetimeManagementContainer
        where T : HitObject
    {
        /// <summary>
        /// Hit objects to create connections for
        /// </summary>
        public abstract IEnumerable<T> HitObjects { get; set; }
    }
}
