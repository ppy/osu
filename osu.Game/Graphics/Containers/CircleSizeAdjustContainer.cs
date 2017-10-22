// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;

namespace osu.Game.Graphics.Containers
{
    public class CircleSizeAdjustContainer : Container
    {
        private Bindable<WorkingBeatmap> beatmap;

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            beatmap = game.Beatmap.GetBoundCopy();
            beatmap.ValueChanged += v => ApplyScale();
            ApplyScale(Scale.X);
        }

        public virtual void ApplyScale(float scale = 1, bool useScalingFactor = true, double duration = 0, Easing easing = Easing.None)
        {
            float scalingFactor = beatmap.Value != null ? (float)(1 - 0.7 * (1 + beatmap.Value.BeatmapInfo.BaseDifficulty.CircleSize - BeatmapDifficulty.DEFAULT_DIFFICULTY) / BeatmapDifficulty.DEFAULT_DIFFICULTY) : 1;
            this.ScaleTo(scale * (useScalingFactor ? scalingFactor : 1), duration, easing);
        }
    }
}
