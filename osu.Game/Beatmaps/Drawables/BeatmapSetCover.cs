﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Beatmaps.Drawables
{
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
        private void load(TextureStore textures)
        {
            string resource = null;

            switch (type)
            {
                case BeatmapSetCoverType.Cover:
                    resource = set.OnlineInfo.Covers.Cover;
                    break;
                case BeatmapSetCoverType.Card:
                    resource = set.OnlineInfo.Covers.Card;
                    break;
                case BeatmapSetCoverType.List:
                    resource = set.OnlineInfo.Covers.List;
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
