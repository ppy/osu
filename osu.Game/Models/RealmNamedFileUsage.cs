// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Game.Database;
using osu.Game.IO;
using Realms;

namespace osu.Game.Models
{
    public class RealmNamedFileUsage : EmbeddedObject, INamedFile, INamedFileUsage
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
    }
}
