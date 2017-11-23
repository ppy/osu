// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Beatmaps.Drawables
{
    public class BeatmapSetHeader : Panel, IHasContextMenu
    {
        public Action<BeatmapSetHeader> GainedSelection;

        public Action<BeatmapSetInfo> DeleteRequested;

        public Action<BeatmapSetInfo> RestoreHiddenRequested;

        private readonly WorkingBeatmap beatmap;

        private readonly FillFlowContainer difficultyIcons;

        public BeatmapSetHeader(WorkingBeatmap beatmap)
        {
            if (beatmap == null)
                throw new ArgumentNullException(nameof(beatmap));

            this.beatmap = beatmap;

            difficultyIcons = new FillFlowContainer
            {
                Margin = new MarginPadding { Top = 5 },
                AutoSizeAxes = Axes.Both,
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
            if (localisation == null)
                throw new ArgumentNullException(nameof(localisation));

            Children = new Drawable[]
            {
                new DelayedLoadWrapper(
                    new PanelBackground(beatmap)
                    {
                        RelativeSizeAxes = Axes.Both,
                        OnLoadComplete = d => d.FadeInFromZero(400, Easing.Out),
                    }, 300),
                new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding { Top = 5, Left = 18, Right = 10, Bottom = 10 },
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Font = @"Exo2.0-BoldItalic",
                            Current = localisation.GetUnicodePreference(beatmap.Metadata.TitleUnicode, beatmap.Metadata.Title),
                            TextSize = 22,
                            Shadow = true,
                        },
                        new OsuSpriteText
                        {
                            Font = @"Exo2.0-SemiBoldItalic",
                            Current = localisation.GetUnicodePreference(beatmap.Metadata.ArtistUnicode, beatmap.Metadata.Artist),
                            TextSize = 17,
                            Shadow = true,
                        },
                        difficultyIcons
                    }
                }
            };
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
                        RelativeSizeAxes = Axes.Both,
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
                                Colour = ColourInfo.GradientHorizontal(
                                    Color4.Black, new Color4(0f, 0f, 0f, 0.9f)),
                                Width = 0.05f,
                            },
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = ColourInfo.GradientHorizontal(
                                    new Color4(0f, 0f, 0f, 0.9f), new Color4(0f, 0f, 0f, 0.1f)),
                                Width = 0.2f,
                            },
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = ColourInfo.GradientHorizontal(
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
            if (panels == null)
                throw new ArgumentNullException(nameof(panels));

            foreach (var p in panels)
                difficultyIcons.Add(new DifficultyIcon(p.Beatmap));
        }

        public MenuItem[] ContextMenuItems
        {
            get
            {
                List<MenuItem> items = new List<MenuItem>();

                if (State == PanelSelectedState.NotSelected)
                    items.Add(new OsuMenuItem("Expand", MenuItemType.Highlighted, () => State = PanelSelectedState.Selected));

                if (beatmap.BeatmapSetInfo.Beatmaps.Any(b => b.Hidden))
                    items.Add(new OsuMenuItem("Restore all hidden", MenuItemType.Standard, () => RestoreHiddenRequested?.Invoke(beatmap.BeatmapSetInfo)));

                items.Add(new OsuMenuItem("Delete", MenuItemType.Destructive, () => DeleteRequested?.Invoke(beatmap.BeatmapSetInfo)));

                return items.ToArray();
            }
        }
    }
}
