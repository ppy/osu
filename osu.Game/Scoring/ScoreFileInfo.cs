// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel.DataAnnotations;
using osu.Game.Database;
using osu.Game.IO;

namespace osu.Game.Scoring
{
    public class ScoreFileInfo : INamedFileInfo, IHasPrimaryKey, INamedFileUsage
    {
        public int ID { get; set; }

        public bool IsManaged => ID > 0;

        public int FileInfoID { get; set; }

        public FileInfo FileInfo { get; set; }

        [Required]
        public string Filename { get; set; }

        IFileInfo INamedFileUsage.File => FileInfo;
    }
}
