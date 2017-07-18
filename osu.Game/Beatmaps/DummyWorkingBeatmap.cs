// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Database;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Beatmaps
{
    internal class DummyWorkingBeatmap : WorkingBeatmap
    {
        public DummyWorkingBeatmap()
            : base(new BeatmapInfo
            {
                Metadata = new BeatmapMetadata
                {
                    Artist = "please load a beatmap!",
                    Title = "no beatmaps available!",
                    Author = "no one",
                },
                BeatmapSet = new BeatmapSetInfo(),
                Difficulty = new BeatmapDifficulty(),
            })
        {
        }

        protected override Beatmap GetBeatmap() => new Beatmap
        {
            HitObjects = new List<HitObject>(),
        };

        protected override Texture GetBackground() => null;

        protected override Track GetTrack() => new TrackVirtual();
    }
}
