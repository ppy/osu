// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Beatmaps.Drawables
{
    public class BeatmapSetCover : Sprite
    {
        public readonly BeatmapSetCoverType CoverType;
        public readonly BeatmapSetInfo BeatmapSet;

        public BeatmapSetCover(BeatmapSetInfo set, BeatmapSetCoverType type = BeatmapSetCoverType.Cover)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));

            BeatmapSet = set;
            CoverType = type;
        }

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures)
        {
            string resource = null;

            switch (CoverType)
            {
                case BeatmapSetCoverType.Cover:
                    resource = BeatmapSet.OnlineInfo.Covers.Cover;
                    break;

                case BeatmapSetCoverType.Card:
                    resource = BeatmapSet.OnlineInfo.Covers.Card;
                    break;

                case BeatmapSetCoverType.List:
                    resource = BeatmapSet.OnlineInfo.Covers.List;
                    break;
            }

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
