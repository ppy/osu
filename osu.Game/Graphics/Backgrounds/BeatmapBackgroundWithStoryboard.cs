// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Graphics.Backgrounds
{
    public class BeatmapBackgroundWithStoryboard : BeatmapBackground
    {
        public BeatmapBackgroundWithStoryboard(WorkingBeatmap beatmap, string fallbackTextureName = "Backgrounds/bg1")
            : base(beatmap, fallbackTextureName)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (!Beatmap.Storyboard.HasDrawable)
                return;

            if (Beatmap.Storyboard.ReplacesBackground)
                Sprite.Alpha = 0;

            LoadComponentAsync(new AudioContainer
            {
                RelativeSizeAxes = Axes.Both,
                Volume = { Value = 0 },
                Child = new DrawableStoryboard(Beatmap.Storyboard) { Clock = new InterpolatingFramedClock(Beatmap.Track) }
            }, AddInternal);
        }
    }
}
