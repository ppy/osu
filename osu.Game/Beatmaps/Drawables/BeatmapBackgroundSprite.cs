// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Beatmaps.Drawables
{
    public partial class BeatmapBackgroundSprite : Sprite
    {
        private readonly IWorkingBeatmap working;

        public BeatmapBackgroundSprite(IWorkingBeatmap working)
        {
            ArgumentNullException.ThrowIfNull(working);

            this.working = working;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var background = working.GetBackground();
            if (background != null)
                Texture = background;
        }
    }
}
