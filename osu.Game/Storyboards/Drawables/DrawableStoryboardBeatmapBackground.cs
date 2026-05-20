// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Storyboards.Drawables
{
    public partial class DrawableStoryboardBeatmapBackground : Sprite
    {
        public StoryboardBeatmapBackground BeatmapBackground { get; }

        public override bool RemoveWhenNotAlive => false;

        [Resolved]
        private TextureStore textureStore { get; set; } = null!;

        public DrawableStoryboardBeatmapBackground(StoryboardBeatmapBackground beatmapBackground)
        {
            BeatmapBackground = beatmapBackground;
            Name = beatmapBackground.Path;
            Anchor = beatmapBackground.Anchor;
            Origin = beatmapBackground.Origin;
            RelativeSizeAxes = Axes.Both;
            FillMode = FillMode.Fill;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Texture = textureStore.Get(BeatmapBackground.Path);
        }
    }
}
