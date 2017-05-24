// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;

namespace osu.Game.Database
{
    internal class OnlineWorkingBeatmap : WorkingBeatmap
    {
        private readonly TextureStore textures;
        private readonly TrackManager tracks;

        public OnlineWorkingBeatmap(BeatmapInfo beatmapInfo, TextureStore textures, TrackManager tracks) : base(beatmapInfo)
        {
            this.textures = textures;
            this.tracks = tracks;
        }

        protected override Beatmap GetBeatmap()
        {
            return new Beatmap();
        }

        protected override Texture GetBackground()
        {
            return textures.Get(BeatmapInfo.OnlineInfo.Covers.FirstOrDefault());
        }

        protected override Track GetTrack()
        {
            return tracks.Get(BeatmapInfo.OnlineInfo.Preview);
        }
    }
}
