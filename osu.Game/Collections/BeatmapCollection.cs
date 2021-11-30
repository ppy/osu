// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;

namespace osu.Game.Collections
{
    /// <summary>
    /// A collection of beatmaps grouped by a name.
    /// </summary>
    public class BeatmapCollection
    {
        /// <summary>
        /// Invoked whenever any change occurs on this <see cref="BeatmapCollection"/>.
        /// </summary>
        public event Action Changed;

        /// <summary>
        /// The collection's name.
        /// </summary>
        public readonly Bindable<string> Name = new Bindable<string>();

        /// <summary>
        /// The beatmaps contained by the collection.
        /// </summary>
        public readonly BindableList<IBeatmapInfo> Beatmaps = new BindableList<IBeatmapInfo>();

        /// <summary>
        /// The date when this collection was last modified.
        /// </summary>
        public DateTimeOffset LastModifyDate { get; private set; } = DateTimeOffset.UtcNow;

        public BeatmapCollection()
        {
            Beatmaps.CollectionChanged += (_, __) => onChange();
            Name.ValueChanged += _ => onChange();
        }

        private void onChange()
        {
            LastModifyDate = DateTimeOffset.Now;
            Changed?.Invoke();
        }
    }
}
