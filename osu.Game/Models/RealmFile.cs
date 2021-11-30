// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
    }
}
