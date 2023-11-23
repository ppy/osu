// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Database;
using osu.Game.IO;
using osu.Game.Utils;
using Realms;

namespace osu.Game.Models
{
    [MapTo("File")]
    public class RealmFile : RealmObject, IFileInfo, IDeepCloneable<RealmFile>
    {
        [PrimaryKey]
        public string Hash { get; set; } = string.Empty;

        public RealmFile DeepClone(IDictionary<object, object> referenceLookup)
        {
            if (referenceLookup.TryGetValue(this, out object? existing))
                return (RealmFile)existing;

            var clone = this.Detach();
            if (ReferenceEquals(clone, this))
                clone = (RealmFile)MemberwiseClone();

            referenceLookup[this] = clone;

            return clone;
        }
    }
}
