using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Beatmaps;

namespace osu.Game.Modes.Taiko.Objects
{
    public class Bash : TaikoHitObject
    {
        public double Length;

        public override double EndTime => StartTime + Length;

        public int RequiredHits;

        public override void SetDefaultsFromBeatmap(Beatmap beatmap)
        {
            base.SetDefaultsFromBeatmap(beatmap);

            // Todo: Diff range
            float spinnerRotationRatio = 5;

            RequiredHits = (int)Math.Max(1, Length / 1000f * spinnerRotationRatio * 1.65f);
        }
    }
}
