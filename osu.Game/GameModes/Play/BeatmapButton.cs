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
using osu.Framework.Graphics.Drawables;
using OpenTK.Graphics;
using OpenTK;
using osu.Game.Graphics;

namespace osu.Game.GameModes.Play
{
    class BeatmapButton : AutoSizeContainer
    {
        private BeatmapSet beatmapSet;
        private Beatmap beatmap;

        public BeatmapButton(BeatmapSet set, Beatmap beatmap)
        {
            this.beatmapSet = set;
            this.beatmap = beatmap;
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = new Color4(40, 86, 102, 255), // TODO: texture
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(1),
                },
                new FlowContainer
                {
                    Padding = new MarginPadding(5),
                    Direction = FlowDirection.HorizontalOnly,
                    Children = new Drawable[]
                    {
                        new DifficultyIcon(FontAwesome.dot_circle_o, new Color4(159, 198, 0, 255)),
                        new FlowContainer
                        {
                            Padding = new MarginPadding { Left = 10 },
                            Direction = FlowDirection.HorizontalOnly,
                            Children = new[]
                            {
                                new SpriteText
                                {
                                    Text = beatmap.Version,
                                    TextSize = 20,
                                },
                                new SpriteText
                                {
                                    Text = string.Format(" mapped by {0}", beatmap.Version),
                                    TextSize = 16,
                                },
                            }
                        }
                    }
                }
            };
        }
    }
}
