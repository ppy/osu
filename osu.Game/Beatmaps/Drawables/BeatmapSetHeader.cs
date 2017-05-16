// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Beatmaps.Drawables
{
    public class BeatmapSetHeader : Panel
    {
        public Action<BeatmapSetHeader> GainedSelection;
        private readonly SpriteText title;
        private readonly SpriteText artist;

        private readonly WorkingBeatmap beatmap;
        private readonly FillFlowContainer difficultyIcons;

        public BeatmapSetHeader(WorkingBeatmap beatmap)
        {
            this.beatmap = beatmap;

            Children = new Drawable[]
            {
                new DelayedLoadWrapper(
                    new PanelBackground(beatmap)
                    {
                        RelativeSizeAxes = Axes.Both,
                        OnLoadComplete = d => d.FadeInFromZero(400, EasingTypes.Out),
                    }
                )
                {
                    TimeBeforeLoad = 300,
                },
                new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding { Top = 5, Left = 18, Right = 10, Bottom = 10 },
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        title = new OsuSpriteText
                        {
                            Font = @"Exo2.0-BoldItalic",
                            TextSize = 22,
                            Shadow = true,
                        },
                        artist = new OsuSpriteText
                        {
                            Font = @"Exo2.0-SemiBoldItalic",
                            TextSize = 17,
                            Shadow = true,
                        },
                        difficultyIcons = new FillFlowContainer
                        {
                            Margin = new MarginPadding { Top = 5 },
                            AutoSizeAxes = Axes.Both,
                        }
                    }
                }
            };
        }

        protected override void Selected()
        {
            base.Selected();
            GainedSelection?.Invoke(this);
        }

        [BackgroundDependencyLoader]
        private void load(LocalisationEngine localisation)
        {
            title.Current = localisation.GetUnicodePreference(beatmap.Metadata.TitleUnicode, beatmap.Metadata.Title);
            artist.Current = localisation.GetUnicodePreference(beatmap.Metadata.ArtistUnicode, beatmap.Metadata.Artist);
        }

        private class PanelBackground : BufferedContainer
        {
            public PanelBackground(WorkingBeatmap working)
            {
                CacheDrawnFrameBuffer = true;

                Children = new Drawable[]
                {
                    new BeatmapBackgroundSprite(working)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        FillMode = FillMode.Fill,
                    },
                    new FillFlowContainer
                    {
                        Depth = -1,
                        Direction = FillDirection.Horizontal,
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
        }

        public void AddDifficultyIcons(IEnumerable<BeatmapPanel> panels)
        {
            foreach (var p in panels)
                difficultyIcons.Add(new DifficultyIcon(p.Beatmap));
        }
    }
}