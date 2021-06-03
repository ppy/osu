// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Storyboards;
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
            var storyboard = new Storyboard { BeatmapInfo = Beatmap.BeatmapInfo };

            foreach (var layer in storyboard.Layers)
            {
                if (layer.Name != "Fail")
                    layer.Elements = Beatmap.Storyboard.GetLayer(layer.Name).Elements.Where(e => !(e is StoryboardSampleInfo)).ToList();
            }

            if (!storyboard.HasDrawable)
                return;

            if (storyboard.ReplacesBackground)
            {
                Sprite.Texture = Texture.WhitePixel;
                Sprite.Colour = Colour4.Black;
            }

            LoadComponentAsync(new DrawableStoryboard(storyboard) { Clock = new InterpolatingFramedClock(Beatmap.Track) }, AddInternal);
        }
    }
}
