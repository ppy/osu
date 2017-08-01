// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.IO;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;

namespace osu.Game.Beatmaps
{
    public class BeatmapSetFileInfo
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [ForeignKey(typeof(BeatmapSetInfo)), NotNull]
        public int BeatmapSetInfoID { get; set; }

        [ForeignKey(typeof(FileInfo)), NotNull]
        public int FileInfoID { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public FileInfo FileInfo { get; set; }

        [NotNull]
        public string Filename { get; set; }
    }
}