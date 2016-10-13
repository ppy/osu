//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.GameModes.Backgrounds;
using osu.Framework;
using osu.Game.Database;
using osu.Framework.Graphics.Primitives;

namespace osu.Game.GameModes.Play
{
    class BeatmapButton : FlowContainer
    {
        private BeatmapSet beatmapSet;
        private Beatmap beatmap;

        public BeatmapButton(BeatmapSet set, Beatmap beatmap)
        {
            this.beatmapSet = set;
            this.beatmap = beatmap;
            Children = new[]
            {
                new SpriteText { Text = beatmap.Version },
            };
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);
        }
    }
}
