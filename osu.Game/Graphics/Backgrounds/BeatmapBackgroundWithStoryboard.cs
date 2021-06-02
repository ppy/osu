// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
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

        protected override void Initialize()
        {
            if (Beatmap.Storyboard.HasDrawable)
            {
                LoadComponentAsync(new DrawableStoryboard(Beatmap.Storyboard) { Clock = new InterpolatingFramedClock(Beatmap.Track) }, AddInternal);
            }

            if (Beatmap.Storyboard.ReplacesBackground || Beatmap.Storyboard.Layers.First(l => l.Name == "Video").Elements.Any())
            {
                Sprite.Expire();
            }
            else
            {
                base.Initialize();
            }
        }
    }
}
