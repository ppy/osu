// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;
using osu.Framework.Bindables;

namespace osu.Game.Tournament.Models
{
    public class SeedingBeatmap
    {
        public int ID;

        [JsonProperty("BeatmapInfo")]
        public TournamentBeatmap? Beatmap;

        public long Score;

        public Bindable<int> Seed = new BindableInt
        {
            MinValue = 1,
            MaxValue = 256
        };
    }
}
