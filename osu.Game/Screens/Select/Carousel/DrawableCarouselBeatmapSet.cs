﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Select.Carousel
{
    public class DrawableCarouselBeatmapSet : DrawableCarouselItem, IHasContextMenu
    {
        private Action<BeatmapSetInfo> deleteRequested;
        private Action<BeatmapSetInfo> restoreHiddenRequested;
        private Action<int> viewDetails;

        private readonly BeatmapSetInfo beatmapSet;

        public DrawableCarouselBeatmapSet(CarouselBeatmapSet set)
            : base(set)
        {
            beatmapSet = set.BeatmapSet;
        }

        [BackgroundDependencyLoader(true)]
        private void load(LocalisationEngine localisation, BeatmapManager manager, BeatmapSetOverlay beatmapOverlay)
        {
            if (localisation == null)
                throw new ArgumentNullException(nameof(localisation));

            restoreHiddenRequested = s => s.Beatmaps.ForEach(manager.Restore);
            deleteRequested = manager.Delete;
            if (beatmapOverlay != null)
                viewDetails = beatmapOverlay.ShowBeatmapSet;

            Children = new Drawable[]
            {
                new DelayedLoadWrapper(
                    new PanelBackground(manager.GetWorkingBeatmap(beatmapSet.Beatmaps.FirstOrDefault()))
                    {
                        RelativeSizeAxes = Axes.Both,
                        OnLoadComplete = d => d.FadeInFromZero(1000, Easing.OutQuint),
                    }, 300
                ),
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
                            Current = localisation.GetUnicodePreference(beatmapSet.Metadata.TitleUnicode, beatmapSet.Metadata.Title),
                            TextSize = 22,
                            Shadow = true,
                        },
                        new OsuSpriteText
                        {
                            Font = @"Exo2.0-SemiBoldItalic",
                            Current = localisation.GetUnicodePreference(beatmapSet.Metadata.ArtistUnicode, beatmapSet.Metadata.Artist),
                            TextSize = 17,
                            Shadow = true,
                        },
                        new FillFlowContainer<FilterableDifficultyIcon>
                        {
                            Margin = new MarginPadding { Top = 5 },
                            AutoSizeAxes = Axes.Both,
                            Children = ((CarouselBeatmapSet)Item).Beatmaps.Select(b => new FilterableDifficultyIcon(b)).ToList()
                        }
                    }
                }
            };
        }

        public MenuItem[] ContextMenuItems
        {
            get
            {
                List<MenuItem> items = new List<MenuItem>();

                if (Item.State == CarouselItemState.NotSelected)
                    items.Add(new OsuMenuItem("Expand", MenuItemType.Highlighted, () => Item.State.Value = CarouselItemState.Selected));

                if (beatmapSet.OnlineBeatmapSetID != null)
                    items.Add(new OsuMenuItem("Details...", MenuItemType.Standard, () => viewDetails?.Invoke(beatmapSet.OnlineBeatmapSetID.Value)));

                if (beatmapSet.Beatmaps.Any(b => b.Hidden))
                    items.Add(new OsuMenuItem("Restore all hidden", MenuItemType.Standard, () => restoreHiddenRequested?.Invoke(beatmapSet)));

                items.Add(new OsuMenuItem("Delete", MenuItemType.Destructive, () => deleteRequested?.Invoke(beatmapSet)));

                return items.ToArray();
            }
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
                                Colour = ColourInfo.GradientHorizontal(Color4.Black, new Color4(0f, 0f, 0f, 0.9f)),
                                Width = 0.05f,
                            },
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = ColourInfo.GradientHorizontal(new Color4(0f, 0f, 0f, 0.9f), new Color4(0f, 0f, 0f, 0.1f)),
                                Width = 0.2f,
                            },
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = ColourInfo.GradientHorizontal(new Color4(0f, 0f, 0f, 0.1f), new Color4(0, 0, 0, 0)),
                                Width = 0.05f,
                            },
                        }
                    },
                };
            }
        }

        public class FilterableDifficultyIcon : DifficultyIcon
        {
            private readonly BindableBool filtered = new BindableBool();

            public FilterableDifficultyIcon(CarouselBeatmap item)
                : base(item.Beatmap)
            {
                filtered.BindTo(item.Filtered);
                filtered.ValueChanged += v => Schedule(() => this.FadeTo(v ? 0.1f : 1, 100));
                filtered.TriggerChange();
            }
        }
    }
}
