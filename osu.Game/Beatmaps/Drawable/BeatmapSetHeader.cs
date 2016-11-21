//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Game.Database;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Game.Configuration;
using osu.Framework.Graphics.Colour;

namespace osu.Game.Beatmaps.Drawable
{
    class BeatmapSetHeader : Panel
    {
        public Action<BeatmapSetHeader> GainedSelection;
        private BeatmapSetInfo beatmapSet;
        private SpriteText title, artist;
        private OsuConfigManager config;
        private Bindable<bool> preferUnicode;

        protected override void Selected()
        {
            base.Selected();
            
            GainedSelection?.Invoke(this);
        }

        protected override void Deselected()
        {
            base.Deselected();
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            this.config = config;

            preferUnicode = config.GetBindable<bool>(OsuConfig.ShowUnicode);
            preferUnicode.ValueChanged += preferUnicode_changed;
            preferUnicode_changed(preferUnicode, null);
        }
        private void preferUnicode_changed(object sender, EventArgs e)
        {
            title.Text = config.GetUnicodeString(beatmapSet.Metadata.Title, beatmapSet.Metadata.TitleUnicode);
            artist.Text = config.GetUnicodeString(beatmapSet.Metadata.Artist, beatmapSet.Metadata.ArtistUnicode);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (preferUnicode != null)
                preferUnicode.ValueChanged -= preferUnicode_changed;
            base.Dispose(isDisposing);
        }

        public BeatmapSetHeader(BeatmapSetInfo beatmapSet, WorkingBeatmap working)
        {
            this.beatmapSet = beatmapSet;
            Children = new Framework.Graphics.Drawable[]
            {
                new BufferedContainer
                {
                    CacheDrawnFrameBuffer = true,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Framework.Graphics.Drawable[]
                    {
                        working.Background == null ? new Box{ RelativeSizeAxes = Axes.Both, Colour = new Color4(200, 200, 200, 255) } : new Sprite
                        {
                            Texture = working.Background,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Scale = new Vector2(1366 / working.Background.Width * 0.6f),
                        },
                        new FlowContainer
                        {
                            Direction = FlowDirection.HorizontalOnly,
                            RelativeSizeAxes = Axes.Both,
                            // This makes the gradient not be perfectly horizontal, but diagonal at a ~40° angle
                            Shear = new Vector2(0.8f, 0),
                            Alpha = 0.5f,
                            Children = new[]
                            {
                                // The left half with no gradient applied
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Black,
                                    Width = 0.4f,
                                },
                                // Piecewise-linear gradient with 3 segments to make it appear smoother
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    ColourInfo = ColourInfo.GradientHorizontal(Color4.Black, new Color4(0f, 0f, 0f, 0.9f)),
                                    Width = 0.05f,
                                },
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    ColourInfo = ColourInfo.GradientHorizontal(new Color4(0f, 0f, 0f, 0.9f), new Color4(0f, 0f, 0f, 0.1f)),
                                    Width = 0.2f,
                                },
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    ColourInfo = ColourInfo.GradientHorizontal(new Color4(0f, 0f, 0f, 0.1f), new Color4(0, 0, 0, 0)),
                                    Width = 0.05f,
                                },
                            }
                        },
                    }
                },
                new FlowContainer
                {
                    Direction = FlowDirection.VerticalOnly,
                    Padding = new MarginPadding { Top = 5, Left = 18, Right = 10, Bottom = 10 },
                    AutoSizeAxes = Axes.Both,
                    Children = new[]
                    {
                        title = new SpriteText
                        {
                            Font = @"Exo2.0-BoldItalic",
                            Text = beatmapSet.Metadata.Title,
                            TextSize = 22,
                            Shadow = true,
                        },
                        artist = new SpriteText
                        {
                            Margin = new MarginPadding { Top = -1 },
                            Font = @"Exo2.0-SemiBoldItalic",
                            Text = beatmapSet.Metadata.Artist,
                            TextSize = 17,
                            Shadow = true,
                        },
                        new FlowContainer
                        {
                            Margin = new MarginPadding { Top = 5 },
                            AutoSizeAxes = Axes.Both,
                            Children = new[]
                            {
                                new DifficultyIcon(FontAwesome.fa_dot_circle_o, new Color4(159, 198, 0, 255)),
                                new DifficultyIcon(FontAwesome.fa_dot_circle_o, new Color4(246, 101, 166, 255)),
                            }
                        }
                    }
                }
            };
        }
    }
}