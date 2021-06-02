// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Graphics.Backgrounds
{
    public class BeatmapBackgroundWithStoryboard : BeatmapBackground
    {
        private DrawableStoryboard storyboard;

        public BeatmapBackgroundWithStoryboard(WorkingBeatmap beatmap, string fallbackTextureName = "Backgrounds/bg1")
            : base(beatmap, fallbackTextureName)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var clock = new InterpolatingFramedClock(Beatmap.Track);
            LoadComponentAsync(new DrawableStoryboard(Beatmap.Storyboard)
            {
                Alpha = 0,
                Clock = clock,
            },
            loaded =>
            {
                AddInternal(storyboard = loaded);
                storyboard.FadeIn(300, Easing.OutQuint);

                if (Beatmap.Storyboard.ReplacesBackground)
                    Sprite.FadeOut(300, Easing.OutQuint);
            });
        }
    }
}
