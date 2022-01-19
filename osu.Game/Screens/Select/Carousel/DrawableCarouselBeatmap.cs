// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
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
using osu.Game.Collections;
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
        public const float CAROUSEL_BEATMAP_SPACING = 5;

        /// <summary>
        /// The height of a carousel beatmap, including vertical spacing.
        /// </summary>
        public const float HEIGHT = height + CAROUSEL_BEATMAP_SPACING;

        private const float height = MAX_HEIGHT * 0.6f;

        private readonly BeatmapInfo beatmapInfo;

        private Sprite background;

        private Action<BeatmapInfo> startRequested;
        private Action<BeatmapInfo> editRequested;
        private Action<BeatmapInfo> hideRequested;

        private Triangles triangles;
        private StarCounter starCounter;

        [Resolved(CanBeNull = true)]
        private BeatmapSetOverlay beatmapOverlay { get; set; }

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; }

        [Resolved(CanBeNull = true)]
        private CollectionManager collectionManager { get; set; }

        [Resolved(CanBeNull = true)]
        private ManageCollectionsDialog manageCollectionsDialog { get; set; }

        private IBindable<StarDifficulty?> starDifficultyBindable;
        private CancellationTokenSource starDifficultyCancellationSource;

        public DrawableCarouselBeatmap(CarouselBeatmap panel)
        {
            beatmapInfo = panel.BeatmapInfo;
            Item = panel;
        }

        [BackgroundDependencyLoader(true)]
        private void load(BeatmapManager manager, SongSelect songSelect)
        {
            Header.Height = height;

            if (songSelect != null)
            {
                startRequested = b => songSelect.FinaliseSelection(b);
                if (songSelect.AllowEditing)
                    editRequested = songSelect.Edit;
            }

            if (manager != null)
                hideRequested = manager.Hide;

            Header.Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                triangles = new Triangles
                {
                    TriangleScale = 2,
                    RelativeSizeAxes = Axes.Both,
                    ColourLight = Color4Extensions.FromHex(@"3a7285"),
                    ColourDark = Color4Extensions.FromHex(@"123744")
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
                        new DifficultyIcon(beatmapInfo, shouldShowTooltip: false)
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
                                            Text = beatmapInfo.DifficultyName,
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
                                            Text = $"{beatmapInfo.Metadata.Author.Username}",
                                            Font = OsuFont.GetFont(italics: true),
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft
                                        },
                                    }
                                },
                                new FillFlowContainer
                                {
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(4, 0),
                                    AutoSizeAxes = Axes.Both,
                                    Children = new Drawable[]
                                    {
                                        new TopLocalRank(beatmapInfo)
                                        {
                                            Scale = new Vector2(0.8f),
                                        },
                                        starCounter = new StarCounter
                                        {
                                            Scale = new Vector2(0.8f),
                                        }
                                    }
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

            MovementContainer.MoveToX(-50, 500, Easing.OutExpo);

            background.Colour = ColourInfo.GradientVertical(
                new Color4(20, 43, 51, 255),
                new Color4(40, 86, 102, 255));

            triangles.Colour = Color4.White;
        }

        protected override void Deselected()
        {
            base.Deselected();

            MovementContainer.MoveToX(0, 500, Easing.OutExpo);

            background.Colour = new Color4(20, 43, 51, 255);
            triangles.Colour = OsuColour.Gray(0.5f);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (Item.State.Value == CarouselItemState.Selected)
                startRequested?.Invoke(beatmapInfo);

            return base.OnClick(e);
        }

        protected override void ApplyState()
        {
            if (Item.State.Value != CarouselItemState.Collapsed && Alpha == 0)
                starCounter.ReplayAnimation();

            starDifficultyCancellationSource?.Cancel();

            // Only compute difficulty when the item is visible.
            if (Item.State.Value != CarouselItemState.Collapsed)
            {
                // We've potentially cancelled the computation above so a new bindable is required.
                starDifficultyBindable = difficultyCache.GetBindableDifficulty(beatmapInfo, (starDifficultyCancellationSource = new CancellationTokenSource()).Token);
                starDifficultyBindable.BindValueChanged(d =>
                {
                    starCounter.Current = (float)(d.NewValue?.Stars ?? 0);
                }, true);
            }

            base.ApplyState();
        }

        public MenuItem[] ContextMenuItems
        {
            get
            {
                List<MenuItem> items = new List<MenuItem>();

                if (startRequested != null)
                    items.Add(new OsuMenuItem("Play", MenuItemType.Highlighted, () => startRequested(beatmapInfo)));

                if (editRequested != null)
                    items.Add(new OsuMenuItem("Edit", MenuItemType.Standard, () => editRequested(beatmapInfo)));

                if (beatmapInfo.OnlineID > 0 && beatmapOverlay != null)
                    items.Add(new OsuMenuItem("Details...", MenuItemType.Standard, () => beatmapOverlay.FetchAndShowBeatmap(beatmapInfo.OnlineID)));

                if (collectionManager != null)
                {
                    var collectionItems = collectionManager.Collections.Select(createCollectionMenuItem).ToList();
                    if (manageCollectionsDialog != null)
                        collectionItems.Add(new OsuMenuItem("Manage...", MenuItemType.Standard, manageCollectionsDialog.Show));

                    items.Add(new OsuMenuItem("Collections") { Items = collectionItems });
                }

                if (hideRequested != null)
                    items.Add(new OsuMenuItem("Hide", MenuItemType.Destructive, () => hideRequested(beatmapInfo)));

                return items.ToArray();
            }
        }

        private MenuItem createCollectionMenuItem(BeatmapCollection collection)
        {
            return new ToggleMenuItem(collection.Name.Value, MenuItemType.Standard, s =>
            {
                if (s)
                    collection.Beatmaps.Add(beatmapInfo);
                else
                    collection.Beatmaps.Remove(beatmapInfo);
            })
            {
                State = { Value = collection.Beatmaps.Contains(beatmapInfo) }
            };
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            starDifficultyCancellationSource?.Cancel();
        }
    }
}
