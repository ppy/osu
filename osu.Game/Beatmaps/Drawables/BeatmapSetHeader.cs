//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Beatmaps.Drawables
{
    class BeatmapSetHeader : Panel
    {
        public Action<BeatmapSetHeader> GainedSelection;
        private SpriteText title, artist;
        private OsuConfigManager config;
        private Bindable<bool> preferUnicode;
        private WorkingBeatmap beatmap;

        public BeatmapSetHeader(WorkingBeatmap beatmap)
        {
            this.beatmap = beatmap;

            Children = new Drawable[]
            {
                new PanelBackground(beatmap)
                {
                    RelativeSizeAxes = Axes.Both,
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
                            Text = beatmap.BeatmapSetInfo.Metadata.Title,
                            TextSize = 22,
                            Shadow = true,
                        },
                        artist = new SpriteText
                        {
                            Margin = new MarginPadding { Top = -1 },
                            Font = @"Exo2.0-SemiBoldItalic",
                            Text = beatmap.BeatmapSetInfo.Metadata.Artist,
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

        protected override void LoadComplete()
        {
            base.LoadComplete();
            FadeInFromZero(250);
        }

        protected override void Selected()
        {
            base.Selected();
            GainedSelection?.Invoke(this);
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
            title.Text = config.GetUnicodeString(beatmap.BeatmapSetInfo.Metadata.Title, beatmap.BeatmapSetInfo.Metadata.TitleUnicode);
            artist.Text = config.GetUnicodeString(beatmap.BeatmapSetInfo.Metadata.Artist, beatmap.BeatmapSetInfo.Metadata.ArtistUnicode);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (preferUnicode != null)
                preferUnicode.ValueChanged -= preferUnicode_changed;
            base.Dispose(isDisposing);
        }

        class PanelBackground : BufferedContainer
        {
            private readonly WorkingBeatmap working;

            public PanelBackground(WorkingBeatmap working)
            {
                this.working = working;

                CacheDrawnFrameBuffer = true;

                Children = new[]
                {
                    new FlowContainer
                    {
                        Depth = -1,
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
                                ColourInfo = ColourInfo.GradientHorizontal(
                                    Color4.Black, new Color4(0f, 0f, 0f, 0.9f)),
                                Width = 0.05f,
                            },
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                ColourInfo = ColourInfo.GradientHorizontal(
                                    new Color4(0f, 0f, 0f, 0.9f), new Color4(0f, 0f, 0f, 0.1f)),
                                Width = 0.2f,
                            },
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                ColourInfo = ColourInfo.GradientHorizontal(
                                    new Color4(0f, 0f, 0f, 0.1f), new Color4(0, 0, 0, 0)),
                                Width = 0.05f,
                            },
                        }
                    },
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuGameBase game)
            {
                new BeatmapBackgroundSprite(working)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    FillMode = FillMode.Fill,
                }.Preload(game, (bg) =>
                {
                    Add(bg);
                    ForceRedraw();
                });
            }
        }
    }
}