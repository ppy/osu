// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Select.Carousel
{
    public class DrawableCarouselBeatmap : DrawableCarouselItem, IHasContextMenu
    {
        private readonly BeatmapInfo beatmap;

        private Sprite background;

        private Action<BeatmapInfo> startRequested;
        private Action<BeatmapInfo> editRequested;
        private Action<BeatmapInfo> hideRequested;

        private Triangles triangles;
        private StarCounter starCounter;

        private BeatmapSetOverlay beatmapOverlay;

        public DrawableCarouselBeatmap(CarouselBeatmap panel)
            : base(panel)
        {
            beatmap = panel.Beatmap;
            Height *= 0.60f;
        }

        [BackgroundDependencyLoader(true)]
        private void load(SongSelect songSelect, BeatmapManager manager, BeatmapSetOverlay beatmapOverlay)
        {
            this.beatmapOverlay = beatmapOverlay;

            if (songSelect != null)
            {
                startRequested = b => songSelect.FinaliseSelection(b);
                editRequested = songSelect.Edit;
            }

            if (manager != null)
                hideRequested = manager.Hide;

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                triangles = new Triangles
                {
                    TriangleScale = 2,
                    RelativeSizeAxes = Axes.Both,
                    ColourLight = OsuColour.FromHex(@"3a7285"),
                    ColourDark = OsuColour.FromHex(@"123744")
                },
                new FillFlowContainer
                {
                    Padding = new MarginPadding(5),
                    Direction = FillDirection.Horizontal,
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Children = new Drawable[]
                    {
                        new DifficultyIcon(beatmap, shouldShowTooltip: false)
                        {
                            Scale = new Vector2(1.8f),
                        },
                        new FillFlowContainer
                        {
                            Padding = new MarginPadding { Left = 5 },
                            Direction = FillDirection.Vertical,
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(4, 0),
                                    AutoSizeAxes = Axes.Both,
                                    Children = new[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Text = beatmap.Version,
                                            Font = OsuFont.GetFont(size: 20),
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft
                                        },
                                        new OsuSpriteText
                                        {
                                            Text = "mapped by",
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft
                                        },
                                        new OsuSpriteText
                                        {
                                            Text = $"{(beatmap.Metadata ?? beatmap.BeatmapSet.Metadata).Author.Username}",
                                            Font = OsuFont.GetFont(italics: true),
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft
                                        },
                                    }
                                },
                                starCounter = new StarCounter
                                {
                                    CountStars = (float)beatmap.StarDifficulty,
                                    Scale = new Vector2(0.8f),
                                }
                            }
                        }
                    }
                }
            };
        }

        protected override void Selected()
        {
            base.Selected();

            background.Colour = ColourInfo.GradientVertical(
                new Color4(20, 43, 51, 255),
                new Color4(40, 86, 102, 255));

            triangles.Colour = Color4.White;
        }

        protected override void Deselected()
        {
            base.Deselected();

            background.Colour = new Color4(20, 43, 51, 255);
            triangles.Colour = OsuColour.Gray(0.5f);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (Item.State.Value == CarouselItemState.Selected)
                startRequested?.Invoke(beatmap);

            return base.OnClick(e);
        }

        protected override void ApplyState()
        {
            if (Item.State.Value != CarouselItemState.Collapsed && Alpha == 0)
                starCounter.ReplayAnimation();

            base.ApplyState();
        }

        public MenuItem[] ContextMenuItems
        {
            get
            {
                List<MenuItem> items = new List<MenuItem>
                {
                    new OsuMenuItem("Play", MenuItemType.Highlighted, () => startRequested?.Invoke(beatmap)),
                    new OsuMenuItem("Edit", MenuItemType.Standard, () => editRequested?.Invoke(beatmap)),
                    new OsuMenuItem("Hide", MenuItemType.Destructive, () => hideRequested?.Invoke(beatmap)),
                };

                if (beatmap.OnlineBeatmapID.HasValue)
                    items.Add(new OsuMenuItem("Details", MenuItemType.Standard, () => beatmapOverlay?.FetchAndShowBeatmap(beatmap.OnlineBeatmapID.Value)));

                return items.ToArray();
            }
        }
    }
}
