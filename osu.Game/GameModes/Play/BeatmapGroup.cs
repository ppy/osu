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
using OpenTK;
using System.Linq;
using osu.Framework.Graphics.Drawables;

namespace osu.Game.GameModes.Play
{
    class BeatmapGroup : FlowContainer
    {
        private BeatmapSet beatmapSet;
        private bool collapsed;
        public bool Collapsed
        {
            get { return collapsed; }
            set
            {
                collapsed = value;
                if (collapsed)
                    Alpha = 0.75f;
                else
                    Alpha = 1;
                // TODO: whatever
            }
        }

        public BeatmapGroup(BeatmapSet beatmapSet)
        {
            this.beatmapSet = beatmapSet;
            this.collapsed = true;
            Direction = FlowDirection.VerticalOnly;
            Children = new[]
            {
                new SpriteText() { Text = this.beatmapSet.Metadata.Title, TextSize = 25 },
                new FlowContainer
                {
                    Spacing = new Vector2(0, 10),
                    Padding = new MarginPadding { Left = 50 },
                    Direction = FlowDirection.VerticalOnly,
                    Children = this.beatmapSet.Beatmaps.Select(b => new BeatmapButton(this.beatmapSet, b))
                },
            };
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);
        }
    }
}
