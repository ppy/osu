// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Models;

#nullable enable

namespace osu.Game.Database
{
    /// <summary>
    /// A model that contains a list of files it is responsible for.
    /// </summary>
    public interface IHasRealmFiles
    {
        IList<RealmNamedFileUsage> Files { get; }

        string Hash { get; set; }
    }
}
