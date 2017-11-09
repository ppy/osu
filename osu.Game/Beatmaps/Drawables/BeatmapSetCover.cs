// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
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
        public BeatmapSetCover(BeatmapSetInfo set)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));

            this.set = set;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            string resource = set.OnlineInfo.Covers.Cover;

            if (resource != null)
                Texture = textures.Get(resource);
        }
    }
}
