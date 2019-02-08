﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;

namespace osu.Game.Database
{
    /// <summary>
    /// A model that contains a list of files it is responsible for.
    /// </summary>
    /// <typeparam name="TFile">The model representing a file.</typeparam>
    public interface IHasFiles<TFile>
        where TFile : INamedFileInfo
    {
        List<TFile> Files { get; set; }

        string Hash { get; set; }
    }
}
