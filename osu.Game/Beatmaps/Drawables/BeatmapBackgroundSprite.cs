// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Beatmaps.Drawables
{
    public class BeatmapBackgroundSprite : Sprite
    {
        private readonly IWorkingBeatmap working;

        public BeatmapBackgroundSprite(IWorkingBeatmap working)
        {
            if (working == null)
                throw new ArgumentNullException(nameof(working));

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
