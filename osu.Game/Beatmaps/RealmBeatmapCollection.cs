// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using osu.Game.Database;
using Realms;

namespace osu.Game.Beatmaps
{
    public class RealmBeatmapCollection : RealmObject, IHasGuidPrimaryKey
    {
        [PrimaryKey]
        public Guid ID { get; }

        public string Name { get; set; } = string.Empty;

        public IList<string> BeatmapMD5Hashes { get; } = null!;

        /// <summary>
        /// The date when this collection was last modified.
        /// </summary>
        public DateTimeOffset LastModified { get; set; }

        public RealmBeatmapCollection(string? name = null, IList<string>? beatmapMD5Hashes = null)
        {
            ID = Guid.NewGuid();
            Name = name ?? string.Empty;
            BeatmapMD5Hashes = beatmapMD5Hashes ?? new List<string>();

            LastModified = DateTimeOffset.UtcNow;
        }

        [UsedImplicitly]
        private RealmBeatmapCollection()
        {
        }
    }
}
