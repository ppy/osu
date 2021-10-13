// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using osu.Framework.Testing;
using osu.Game.IO;
using Realms;

#nullable enable

namespace osu.Game.Models
{
    [ExcludeFromDynamicCompile]
    [MapTo("File")]
    public class RealmFile : RealmObject, IFileInfo
    {
        [PrimaryKey]
        public string Hash { get; set; } = string.Empty;

        public string StoragePath => Path.Combine(Hash.Remove(1), Hash.Remove(2), Hash);
    }
}
