// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Utils;
using Realms;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// User settings overrides that are attached to a beatmap.
    /// </summary>
    public class BeatmapUserSettings : EmbeddedObject, IDeepCloneable<BeatmapUserSettings>
    {
        /// <summary>
        /// An audio offset that can be used for timing adjustments.
        /// </summary>
        public double Offset { get; set; }

        public BeatmapUserSettings DeepClone(IDictionary<object, object> referenceLookup)
        {
            if (referenceLookup.TryGetValue(this, out object? existing))
                return (BeatmapUserSettings)existing;

            var clone = (BeatmapUserSettings)MemberwiseClone();
            referenceLookup[this] = clone;

            return clone;
        }
    }
}
