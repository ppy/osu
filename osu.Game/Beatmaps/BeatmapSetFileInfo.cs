// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using osu.Game.IO;

namespace osu.Game.Beatmaps
{
    public class BeatmapSetFileInfo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int BeatmapSetInfoId { get; set; }

        public int FileInfoId { get; set; }

        public FileInfo FileInfo { get; set; }

        [Required]
        public string Filename { get; set; }
    }
}
