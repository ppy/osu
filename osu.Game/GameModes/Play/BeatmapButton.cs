//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.GameModes.Backgrounds;
using osu.Framework;
using osu.Game.Database;
using osu.Framework.Graphics.Primitives;
using OpenTK.Graphics;
using OpenTK;
using osu.Game.Graphics;
using osu.Framework.Graphics.Transformations;

namespace osu.Game.GameModes.Play
{
    class BeatmapButton : AutoSizeContainer
    {
        private BeatmapSetInfo beatmapSet;
        private BeatmapInfo beatmap;

        public Action<BeatmapInfo> MapSelected;
        
        private bool selected;

        public bool Selected
        {
            get { return selected; }
            set
            {
                if (selected == value)
                    return;
                selected = value;
                BorderColour = new Color4(
                    BorderColour.R,
                    BorderColour.G,
                    BorderColour.B,
                    selected ? 255 : 0);
                GlowRadius = selected ? 3 : 0;
            }
        }

        public BeatmapButton(BeatmapSetInfo set, BeatmapInfo beatmap)
        {
            this.beatmapSet = set;
            this.beatmap = beatmap;
            Masking = true;
            CornerRadius = 5;
            BorderThickness = 2;
            BorderColour = new Color4(221, 255, 255, 0);
            GlowColour = new Color4(166, 221, 251, 0.75f); // TODO: Get actual color for this
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = new Color4(40, 86, 102, 255), // TODO: gradient
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
                                    Text = string.Format(" mapped by {0}", (beatmap.Metadata ?? set.Metadata).Author),
                                    TextSize = 16,
                                },
                            }
                        }
                    }
                }
            };
        }
        
        protected override bool OnClick(InputState state)
        {
            MapSelected?.Invoke(beatmap);
            return true;
        }
    }
}
