// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.IO;
using SQLiteNetExtensions.Attributes;

namespace osu.Game.Beatmaps
{
    public class BeatmapSetFileInfo
    {
        [ForeignKey(typeof(BeatmapSetInfo))]
        public int BeatmapSetInfoID { get; set; }

        [ForeignKey(typeof(FileInfo))]
        public int FileInfoID { get; set; }
    }
}