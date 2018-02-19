// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.IO;

namespace osu.Game.Database
{
    /// <summary>
    /// Represent a join model which gives a filename and scope to a <see cref="FileInfo"/>.
    /// </summary>
    public interface INamedFileInfo
    {
        FileInfo FileInfo { get; set; }
        string Filename { get; set; }
    }
}
