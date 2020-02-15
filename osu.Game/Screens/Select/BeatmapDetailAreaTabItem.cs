// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Screens.Select
{
    public abstract class BeatmapDetailAreaTabItem : IEquatable<BeatmapDetailAreaTabItem>
    {
        /// <summary>
        /// The name of this tab, to be displayed in the tab control.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Whether the contents of this tab can be filtered by the user's currently-selected mods.
        /// </summary>
        public virtual bool FilterableByMods => false;

        public override string ToString() => Name;

        public bool Equals(BeatmapDetailAreaTabItem other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name != null ? Name.GetHashCode() : 0;
        }
    }
}
