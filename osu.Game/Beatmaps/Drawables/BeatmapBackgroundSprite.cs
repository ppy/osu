//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Beatmaps.Drawables
{
    class BeatmapBackgroundSprite : Sprite
    {
        private readonly WorkingBeatmap working;

        public BeatmapBackgroundSprite(WorkingBeatmap working)
        {
            this.working = working;
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            Texture = working.Background;
        }
    }
}
