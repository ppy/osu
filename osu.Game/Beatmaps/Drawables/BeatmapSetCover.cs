// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Beatmaps.Drawables
{
    [LongRunningLoad]
    public class BeatmapSetCover : Sprite
    {
        private readonly BeatmapSetInfo set;
        private readonly BeatmapSetCoverType type;

        public BeatmapSetCover(BeatmapSetInfo set, BeatmapSetCoverType type = BeatmapSetCoverType.Cover)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));

            this.set = set;
            this.type = type;
        }

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures)
        {
            string resource = type switch
            {
                BeatmapSetCoverType.Cover => set.OnlineInfo.Covers.Cover,
                BeatmapSetCoverType.Card => set.OnlineInfo.Covers.Card,
                BeatmapSetCoverType.List => set.OnlineInfo.Covers.List,
                _ => null,
            };

            if (resource != null)
                Texture = textures.Get(resource);
        }
    }

    public enum BeatmapSetCoverType
    {
        Cover,
        Card,
        List,
    }
}
