using System.Collections.Generic;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Tau.Objects;
using System.Linq;

namespace osu.Game.Rulesets.Tau.Beatmaps
{
    public class TauBeatmap : Beatmap<TauHitObject>
    {
        public override IEnumerable<BeatmapStatistic> GetStatistics()
        {
            int beats = HitObjects.Count(b => b is Beat);
            int hardbeats = HitObjects.Count(b => b is HardBeat);

            return new[]
            {
                new BeatmapStatistic
                {
                    Name = "Beat count",
                    Content = beats.ToString(),
                },
                new BeatmapStatistic
                {
                    Name = "HardBeat count",
                    Content = hardbeats.ToString(),
                }
            };
        }
    }
}
