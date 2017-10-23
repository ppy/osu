﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace osu.Game.Beatmaps
{
    public class BeatmapSetInfo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int? OnlineBeatmapSetID { get; set; }

        public BeatmapMetadata Metadata { get; set; }

        public List<BeatmapInfo> Beatmaps { get; set; }

        [NotMapped]
        public BeatmapSetOnlineInfo OnlineInfo { get; set; }

        public double MaxStarDifficulty => Beatmaps.Max(b => b.StarDifficulty);

        [NotMapped]
        public bool DeletePending { get; set; }

        public string Hash { get; set; }

        public string StoryboardFile => Files.FirstOrDefault(f => f.Filename.EndsWith(".osb"))?.Filename;

        public List<BeatmapSetFileInfo> Files { get; set; }

        public bool Protected { get; set; }
    }
}
