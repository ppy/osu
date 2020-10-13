// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Collections;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Select.Carousel
{
    public class DrawableCarouselBeatmapSet : DrawableCarouselItem, IHasContextMenu
    {
        public const float HEIGHT = MAX_HEIGHT;

        // TODO: don't do this. need to split out the base class' style so our height isn't fixed to the panel display height (and autosize?).
        protected override bool ComputeIsMaskedAway(RectangleF maskingBounds) => false;

        private Action<BeatmapSetInfo> restoreHiddenRequested;
        private Action<int> viewDetails;

        [Resolved(CanBeNull = true)]
        private DialogOverlay dialogOverlay { get; set; }

        [Resolved(CanBeNull = true)]
        private CollectionManager collectionManager { get; set; }

        [Resolved(CanBeNull = true)]
        private ManageCollectionsDialog manageCollectionsDialog { get; set; }

        public override IEnumerable<DrawableCarouselItem> ChildItems => beatmapContainer?.Children ?? base.ChildItems;

        private BeatmapSetInfo beatmapSet => (Item as CarouselBeatmapSet)?.BeatmapSet;

        private Container<DrawableCarouselItem> beatmapContainer;

        [Resolved]
        private BeatmapManager manager { get; set; }

        protected override void FreeAfterUse()
        {
            base.FreeAfterUse();

            Item = null;

            ClearTransforms();
        }

        [BackgroundDependencyLoader(true)]
        private void load(BeatmapSetOverlay beatmapOverlay)
        {
            restoreHiddenRequested = s => s.Beatmaps.ForEach(manager.Restore);

            if (beatmapOverlay != null)
                viewDetails = beatmapOverlay.FetchAndShowBeatmapSet;
        }

        protected override void UpdateItem()
        {
            base.UpdateItem();

            Content.Clear();
            beatmapContainer = null;

            if (Item == null)
                return;

            Logger.Log($"updating item {beatmapSet}");
            Header.Children = new Drawable[]
            {
                new DelayedLoadUnloadWrapper(() =>
                {
                    Logger.Log($"loaded background item {beatmapSet}");
                    var background = new PanelBackground(manager.GetWorkingBeatmap(beatmapSet.Beatmaps.FirstOrDefault()))
                    {
                        RelativeSizeAxes = Axes.Both,
                    };

                    background.OnLoadComplete += d => d.FadeInFromZero(1000, Easing.OutQuint);

                    return background;
                }, 300, 5000),
                new DelayedLoadUnloadWrapper(() =>
                {
                    var mainFlow = new FillFlowContainer
                    {
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding { Top = 5, Left = 18, Right = 10, Bottom = 10 },
                        AutoSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = new LocalisedString((beatmapSet.Metadata.TitleUnicode, beatmapSet.Metadata.Title)),
                                Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 22, italics: true),
                                Shadow = true,
                            },
                            new OsuSpriteText
                            {
                                Text = new LocalisedString((beatmapSet.Metadata.ArtistUnicode, beatmapSet.Metadata.Artist)),
                                Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: 17, italics: true),
                                Shadow = true,
                            },
                            new FillFlowContainer
                            {
                                Direction = FillDirection.Horizontal,
                                AutoSizeAxes = Axes.Both,
                                Margin = new MarginPadding { Top = 5 },
                                Children = new Drawable[]
                                {
                                    new BeatmapSetOnlineStatusPill
                                    {
                                        Origin = Anchor.CentreLeft,
                                        Anchor = Anchor.CentreLeft,
                                        Margin = new MarginPadding { Right = 5 },
                                        TextSize = 11,
                                        TextPadding = new MarginPadding { Horizontal = 8, Vertical = 2 },
                                        Status = beatmapSet.Status
                                    },
                                    new FillFlowContainer<DifficultyIcon>
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        Spacing = new Vector2(3),
                                        ChildrenEnumerable = getDifficultyIcons(),
                                    },
                                }
                            }
                        }
                    };

                    mainFlow.OnLoadComplete += d => d.FadeInFromZero(1000, Easing.OutQuint);

                    return mainFlow;
                }, 100, 5000)
            };
        }

        protected override void Deselected()
        {
            base.Deselected();

            MovementContainer.MoveToX(0, 500, Easing.OutExpo);

            if (beatmapContainer != null)
            {
                foreach (var beatmap in beatmapContainer)
                {
                    beatmap.MoveToY(0, 800, Easing.OutQuint);
                    beatmap.FadeOut(80);
                }
            }
        }

        protected override void Selected()
        {
            base.Selected();

            MovementContainer.MoveToX(-100, 500, Easing.OutExpo);

            updateBeatmapDifficulties();
        }

        private void updateBeatmapDifficulties()
        {
            var carouselBeatmapSet = (CarouselBeatmapSet)Item;

            var visibleBeatmaps = carouselBeatmapSet.Children
                                                    .Where(c => c.Visible)
                                                    .ToArray();

            // if we are already displaying all the correct beatmaps, only run animation updates.
            // note that the displayed beatmaps may change due to the applied filter.
            // a future optimisation could add/remove only changed difficulties rather than reinitialise.
            if (beatmapContainer != null && visibleBeatmaps.Length == beatmapContainer.Count && visibleBeatmaps.All(b => beatmapContainer.Any(c => c.Item == b)))
            {
                updateBeatmapYPositions();
            }
            else
            {
                // on selection we show our child beatmaps.
                // for now this is a simple drawable construction each selection.
                // can be improved in the future.

                beatmapContainer = new Container<DrawableCarouselItem>
                {
                    X = 100,
                    RelativeSizeAxes = Axes.Both,
                    // ToArray() in this line is required due to framework oversight: https://github.com/ppy/osu-framework/pull/3929
                    ChildrenEnumerable = visibleBeatmaps.Select(c => c.CreateDrawableRepresentation()).ToArray()
                };

                Logger.Log($"loading {visibleBeatmaps.Length} beatmaps for {Item}");

                LoadComponentAsync(beatmapContainer, loaded =>
                {
                    // make sure the pooled target hasn't changed.
                    if (carouselBeatmapSet != Item)
                        return;

                    Content.Child = loaded;
                    updateBeatmapYPositions();
                });
            }

            void updateBeatmapYPositions()
            {
                float yPos = DrawableCarouselBeatmap.CAROUSEL_BEATMAP_SPACING;

                foreach (var panel in beatmapContainer.Children)
                {
                    panel.MoveToY(yPos, 800, Easing.OutQuint);
                    yPos += panel.Item.TotalHeight + DrawableCarouselBeatmap.CAROUSEL_BEATMAP_SPACING;
                }
            }
        }

        private const int maximum_difficulty_icons = 18;

        private IEnumerable<DifficultyIcon> getDifficultyIcons()
        {
            var beatmaps = ((CarouselBeatmapSet)Item).Beatmaps.ToList();

            return beatmaps.Count > maximum_difficulty_icons
                ? (IEnumerable<DifficultyIcon>)beatmaps.GroupBy(b => b.Beatmap.Ruleset).Select(group => new FilterableGroupedDifficultyIcon(group.ToList(), group.Key))
                : beatmaps.Select(b => new FilterableDifficultyIcon(b));
        }

        public MenuItem[] ContextMenuItems
        {
            get
            {
                List<MenuItem> items = new List<MenuItem>();

                if (Item.State.Value == CarouselItemState.NotSelected)
                    items.Add(new OsuMenuItem("Expand", MenuItemType.Highlighted, () => Item.State.Value = CarouselItemState.Selected));

                if (beatmapSet.OnlineBeatmapSetID != null && viewDetails != null)
                    items.Add(new OsuMenuItem("Details...", MenuItemType.Standard, () => viewDetails(beatmapSet.OnlineBeatmapSetID.Value)));

                if (collectionManager != null)
                {
                    var collectionItems = collectionManager.Collections.Select(createCollectionMenuItem).ToList();
                    if (manageCollectionsDialog != null)
                        collectionItems.Add(new OsuMenuItem("Manage...", MenuItemType.Standard, manageCollectionsDialog.Show));

                    items.Add(new OsuMenuItem("Collections") { Items = collectionItems });
                }

                if (beatmapSet.Beatmaps.Any(b => b.Hidden))
                    items.Add(new OsuMenuItem("Restore all hidden", MenuItemType.Standard, () => restoreHiddenRequested(beatmapSet)));

                if (dialogOverlay != null)
                    items.Add(new OsuMenuItem("Delete...", MenuItemType.Destructive, () => dialogOverlay.Push(new BeatmapDeleteDialog(beatmapSet))));
                return items.ToArray();
            }
        }

        private MenuItem createCollectionMenuItem(BeatmapCollection collection)
        {
            TernaryState state;

            var countExisting = beatmapSet.Beatmaps.Count(b => collection.Beatmaps.Contains(b));

            if (countExisting == beatmapSet.Beatmaps.Count)
                state = TernaryState.True;
            else if (countExisting > 0)
                state = TernaryState.Indeterminate;
            else
                state = TernaryState.False;

            return new TernaryStateMenuItem(collection.Name.Value, MenuItemType.Standard, s =>
            {
                foreach (var b in beatmapSet.Beatmaps)
                {
                    switch (s)
                    {
                        case TernaryState.True:
                            if (collection.Beatmaps.Contains(b))
                                continue;

                            collection.Beatmaps.Add(b);
                            break;

                        case TernaryState.False:
                            collection.Beatmaps.Remove(b);
                            break;
                    }
                }
            })
            {
                State = { Value = state }
            };
        }
    }
}
