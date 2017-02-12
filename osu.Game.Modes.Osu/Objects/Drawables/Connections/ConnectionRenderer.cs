// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Objects;
using System.Collections.Generic;

namespace osu.Game.Modes.Osu.Objects.Drawables.Connections
{
    public abstract class ConnectionRenderer<T> : Container
        where T : HitObject
    {
        /// <summary>
        /// Create drawables inside this container, connecting hit objects visually, for example with follow points.
        /// </summary>
        /// <param name="hitObjects">Hit objects to create connections for</param>
        public abstract void AddConnections(IEnumerable<T> hitObjects);
    }
}
