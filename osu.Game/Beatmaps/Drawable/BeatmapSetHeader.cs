//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Game.Database;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Beatmaps.Drawable
{
    class BeatmapSetHeader : Container
    {
        public BeatmapSetHeader(BeatmapSetInfo beatmapSet)
        {
            AutoSizeAxes = Axes.Y;
            Masking = true;
            CornerRadius = 5;
            BorderThickness = 2;
            BorderColour = new Color4(221, 255, 255, 0);
            GlowColour = new Color4(166, 221, 251, 0.5f); // TODO: Get actual color for this
            Children = new Framework.Graphics.Drawable[]
            {
                new Box
                {
                    Colour = new Color4(85, 85, 85, 255),
                    RelativeSizeAxes = Axes.Both,
                    Size = Vector2.One,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = Vector2.One,
                    Children = new Framework.Graphics.Drawable[]
                    {
                        new Box // TODO: Gradient
                        {
                            Colour = new Color4(0, 0, 0, 100),
                            RelativeSizeAxes = Axes.Both,
                            Size = Vector2.One,
                        }
                    }
                },
                new FlowContainer
                {
                    Direction = FlowDirection.VerticalOnly,
                    Spacing = new Vector2(0, 2),
                    Padding = new MarginPadding { Top = 3, Left = 20, Right = 20, Bottom = 3 },
                    AutoSizeAxes = Axes.Both,
                    Children = new[]
                    {
                        // TODO: Make these italic
                        new SpriteText
                        {
                            Text = beatmapSet.Metadata.Title ?? beatmapSet.Metadata.TitleUnicode,
                            TextSize = 20
                        },
                        new SpriteText
                        {
                            Text = beatmapSet.Metadata.Artist ?? beatmapSet.Metadata.ArtistUnicode,
                            TextSize = 16
                        },
                        new FlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Children = new[]
                            {
                                new DifficultyIcon(FontAwesome.dot_circle_o, new Color4(159, 198, 0, 255)),
                                new DifficultyIcon(FontAwesome.dot_circle_o, new Color4(246, 101, 166, 255)),
                            }
                        }
                    }
                }
            };
        }
    }
}