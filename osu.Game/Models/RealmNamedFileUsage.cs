// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using osu.Game.Database;
using osu.Game.IO;
using osu.Game.Utils;
using Realms;

namespace osu.Game.Models
{
    public class RealmNamedFileUsage : EmbeddedObject, INamedFile, INamedFileUsage, IDeepCloneable<RealmNamedFileUsage>
    {
        public RealmFile File { get; set; } = null!;

        // [Indexed] cannot be used on `EmbeddedObject`s as it only applies to top-level queries. May need to reconsider this if performance becomes a concern.
        public string Filename { get; set; } = null!;

        public RealmNamedFileUsage(RealmFile file, string filename)
        {
            File = file ?? throw new ArgumentNullException(nameof(file));
            Filename = filename ?? throw new ArgumentNullException(nameof(filename));
        }

        [UsedImplicitly]
        private RealmNamedFileUsage()
        {
        }

        IFileInfo INamedFileUsage.File => File;

        public RealmNamedFileUsage DeepClone(IDictionary<object, object> referenceLookup)
        {
            if (referenceLookup.TryGetValue(this, out object? existing))
                return (RealmNamedFileUsage)existing;

            var clone = this.Detach();
            if (ReferenceEquals(clone, this))
                clone = (RealmNamedFileUsage)MemberwiseClone();

            referenceLookup[this] = clone;

            clone.File = File.DeepClone(referenceLookup);

            return clone;
        }
    }
}
