﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Beatmaps.Drawables
{
    internal class BeatmapBackgroundSprite : Sprite
    {
        private readonly WorkingBeatmap working;

        public BeatmapBackgroundSprite(WorkingBeatmap working)
        {
            this.working = working;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (working.Background != null)
                Texture = working.Background;
        }
    }
}
