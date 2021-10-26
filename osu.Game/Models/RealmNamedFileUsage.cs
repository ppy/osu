// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.IO;
using Realms;

#nullable enable

namespace osu.Game.Models
{
    [ExcludeFromDynamicCompile]
    public class RealmNamedFileUsage : EmbeddedObject, INamedFile, INamedFileUsage
    {
        public RealmFile File { get; set; } = null!;

        public string Filename { get; set; } = null!;

        public RealmNamedFileUsage(RealmFile file, string filename)
        {
            File = file;
            Filename = filename;
        }

        [UsedImplicitly]
        private RealmNamedFileUsage()
        {
        }

        IFileInfo INamedFileUsage.File => File;
    }
}
