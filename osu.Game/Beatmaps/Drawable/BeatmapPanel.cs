//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Beatmaps.Drawable
{
    class BeatmapPanel : Panel
    {
        public BeatmapInfo Beatmap;

        public Action<BeatmapPanel> GainedSelection;

        protected override void Selected()
        {
            base.Selected();
            GainedSelection?.Invoke(this);
        }

        public BeatmapPanel(BeatmapInfo beatmap)
        {
            Beatmap = beatmap;
            Height *= 0.75f;

            Children = new Framework.Graphics.Drawable[]
            {
                new Box
                {
                    Colour = new Color4(40, 86, 102, 255), // TODO: gradient
                    RelativeSizeAxes = Axes.Both,
                    Size = Vector2.One,
                },
                new FlowContainer
                {
                    Padding = new MarginPadding(5),
                    Direction = FlowDirection.HorizontalOnly,
                    AutoSizeAxes = Axes.Both,
                    Children = new Framework.Graphics.Drawable[]
                    {
                        new DifficultyIcon(FontAwesome.dot_circle_o, new Color4(159, 198, 0, 255)),
                        new FlowContainer
                        {
                            Padding = new MarginPadding { Left = 10 },
                            Direction = FlowDirection.VerticalOnly,
                            AutoSizeAxes = Axes.Both,
                            Children = new Framework.Graphics.Drawable[]
                            {
                                new FlowContainer
                                {
                                    Direction = FlowDirection.HorizontalOnly,
                                    AutoSizeAxes = Axes.Both,
                                    Children = new[]
                                    {
                                        new SpriteText
                                        {
                                            Text = beatmap.Version,
                                            TextSize = 20,
                                        },
                                        new SpriteText
                                        {
                                            Text = $" mapped by {(beatmap.Metadata ?? beatmap.BeatmapSet.Metadata).Author}",
                                            TextSize = 16,
                                        },
                                    }
                                },
                                new StarCounter { Count = beatmap.BaseDifficulty?.OverallDifficulty ?? 5, StarSize = 8 }
                            }
                        }
                    }
                }
            };
        }
    }
}
