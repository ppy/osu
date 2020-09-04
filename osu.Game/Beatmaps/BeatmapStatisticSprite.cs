// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Beatmaps
{
    public class BeatmapStatisticSprite : Sprite
    {
        private readonly string iconName;

        public BeatmapStatisticSprite(string iconName)
        {
            this.iconName = iconName;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Texture = textures.Get($"Icons/BeatmapDetails/{iconName}");
        }
    }
}
