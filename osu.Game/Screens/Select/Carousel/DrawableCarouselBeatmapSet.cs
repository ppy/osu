// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Database;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;

namespace osu.Game.Screens.Select.Carousel
{
    public partial class DrawableCarouselBeatmapSet : DrawableCarouselItem, IHasContextMenu
    {
        public const float HEIGHT = MAX_HEIGHT;

        private Action<BeatmapSetInfo> restoreHiddenRequested = null!;
        private Action<int>? viewDetails;

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        [Resolved]
        private ManageCollectionsDialog? manageCollectionsDialog { get; set; }

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        public IEnumerable<DrawableCarouselItem> DrawableBeatmaps => beatmapContainer?.IsLoaded != true ? Enumerable.Empty<DrawableCarouselItem>() : beatmapContainer.AliveChildren;

        private Container<DrawableCarouselItem>? beatmapContainer;

        private BeatmapSetInfo beatmapSet = null!;

        private Task? beatmapsLoadTask;

        private MenuItem[]? mainMenuItems;

        [Resolved]
        private BeatmapManager manager { get; set; } = null!;

        protected override void FreeAfterUse()
        {
            base.FreeAfterUse();

            Item = null;

            ClearTransforms();
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapSetOverlay? beatmapOverlay, SongSelect? songSelect)
        {
            if (songSelect != null)
                mainMenuItems = songSelect.CreateForwardNavigationMenuItemsForBeatmap(() => (((CarouselBeatmapSet)Item!).GetNextToSelect() as CarouselBeatmap)!.BeatmapInfo);

            restoreHiddenRequested = s =>
            {
                foreach (var b in s.Beatmaps)
                    manager.Restore(b);
            };

            if (beatmapOverlay != null)
                viewDetails = beatmapOverlay.FetchAndShowBeatmapSet;
        }

        protected override void Update()
        {
            base.Update();

            Debug.Assert(Item != null);

            // position updates should not occur if the item is filtered away.
            // this avoids panels flying across the screen only to be eventually off-screen or faded out.
            if (!Item.Visible) return;

            float targetY = Item.CarouselYPosition;

            if (Precision.AlmostEquals(targetY, Y))
                Y = targetY;
            else
                // algorithm for this is taken from ScrollContainer.
                // while it doesn't necessarily need to match 1:1, as we are emulating scroll in some cases this feels most correct.
                Y = (float)Interpolation.Lerp(targetY, Y, Math.Exp(-0.01 * Time.Elapsed));
        }

        protected override void UpdateItem()
        {
            base.UpdateItem();

            Content.Clear();

            beatmapContainer = null;
            beatmapsLoadTask = null;

            if (Item == null)
                return;

            beatmapSet = ((CarouselBeatmapSet)Item).BeatmapSet;

            DelayedLoadWrapper background;
            DelayedLoadWrapper mainFlow;

            Header.Children = new Drawable[]
            {
                // Choice of background image matches BSS implementation (always uses the lowest `beatmap_id` from the set).
                background = new DelayedLoadWrapper(() => new SetPanelBackground(manager.GetWorkingBeatmap(beatmapSet.Beatmaps.MinBy(b => b.OnlineID)))
                {
                    RelativeSizeAxes = Axes.Both,
                }, 200)
                {
                    RelativeSizeAxes = Axes.Both
                },
                mainFlow = new DelayedLoadWrapper(() => new SetPanelContent((CarouselBeatmapSet)Item), 50)
                {
                    RelativeSizeAxes = Axes.Both
                },
            };

            background.DelayedLoadComplete += fadeContentIn;
            mainFlow.DelayedLoadComplete += fadeContentIn;
        }

        private void fadeContentIn(Drawable d) => d.FadeInFromZero(150);

        protected override void Deselected()
        {
            base.Deselected();

            MovementContainer.MoveToX(0, 500, Easing.OutExpo);

            updateBeatmapYPositions();
        }

        protected override void Selected()
        {
            base.Selected();

            MovementContainer.MoveToX(-100, 500, Easing.OutExpo);

            updateBeatmapDifficulties();
        }

        private void updateBeatmapDifficulties()
        {
            Debug.Assert(Item != null);

            var carouselBeatmapSet = (CarouselBeatmapSet)Item;

            var visibleBeatmaps = carouselBeatmapSet.Items.Where(c => c.Visible).ToArray();

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
                    ChildrenEnumerable = visibleBeatmaps.Select(c => c.CreateDrawableRepresentation()!)
                };

                beatmapsLoadTask = LoadComponentAsync(beatmapContainer, loaded =>
                {
                    // make sure the pooled target hasn't changed.
                    if (beatmapContainer != loaded)
                        return;

                    Content.Child = loaded;
                    updateBeatmapYPositions();
                });
            }
        }

        private void updateBeatmapYPositions()
        {
            if (beatmapContainer == null)
                return;

            if (beatmapsLoadTask == null || !beatmapsLoadTask.IsCompleted)
                return;

            float yPos = DrawableCarouselBeatmap.CAROUSEL_BEATMAP_SPACING;

            bool isSelected = Item?.State.Value == CarouselItemState.Selected;

            foreach (var panel in beatmapContainer.Children)
            {
                Debug.Assert(panel.Item != null);

                if (isSelected)
                {
                    panel.MoveToY(yPos, 800, Easing.OutQuint);
                    yPos += panel.Item.TotalHeight;
                }
                else
                    panel.MoveToY(0, 800, Easing.OutQuint);
            }
        }

        public MenuItem[] ContextMenuItems
        {
            get
            {
                Debug.Assert(beatmapSet != null);

                List<MenuItem> items = new List<MenuItem>();

                if (Item?.State.Value == CarouselItemState.NotSelected)
                    items.Add(new OsuMenuItem("Expand", MenuItemType.Highlighted, () => Item.State.Value = CarouselItemState.Selected));

                if (mainMenuItems != null)
                    items.AddRange(mainMenuItems);

                if (beatmapSet.OnlineID > 0 && viewDetails != null)
                    items.Add(new OsuMenuItem("Details...", MenuItemType.Standard, () => viewDetails(beatmapSet.OnlineID)));

                var collectionItems = realm.Realm.All<BeatmapCollection>()
                                           .OrderBy(c => c.Name)
                                           .AsEnumerable()
                                           .Select(createCollectionMenuItem)
                                           .ToList();

                if (manageCollectionsDialog != null)
                    collectionItems.Add(new OsuMenuItem("Manage...", MenuItemType.Standard, manageCollectionsDialog.Show));

                items.Add(new OsuMenuItem("Collections") { Items = collectionItems });

                if (beatmapSet.Beatmaps.Any(b => b.Hidden))
                    items.Add(new OsuMenuItem("Restore all hidden", MenuItemType.Standard, () => restoreHiddenRequested(beatmapSet)));

                if (dialogOverlay != null)
                    items.Add(new OsuMenuItem("Delete...", MenuItemType.Destructive, () => dialogOverlay.Push(new BeatmapDeleteDialog(beatmapSet))));
                return items.ToArray();
            }
        }

        private MenuItem createCollectionMenuItem(BeatmapCollection collection)
        {
            Debug.Assert(beatmapSet != null);

            TernaryState state;

            int countExisting = beatmapSet.Beatmaps.Count(b => collection.BeatmapMD5Hashes.Contains(b.MD5Hash));

            if (countExisting == beatmapSet.Beatmaps.Count)
                state = TernaryState.True;
            else if (countExisting > 0)
                state = TernaryState.Indeterminate;
            else
                state = TernaryState.False;

            var liveCollection = collection.ToLive(realm);

            return new TernaryStateToggleMenuItem(collection.Name, MenuItemType.Standard, s =>
            {
                liveCollection.PerformWrite(c =>
                {
                    foreach (var b in beatmapSet.Beatmaps)
                    {
                        switch (s)
                        {
                            case TernaryState.True:
                                if (c.BeatmapMD5Hashes.Contains(b.MD5Hash))
                                    continue;

                                c.BeatmapMD5Hashes.Add(b.MD5Hash);
                                break;

                            case TernaryState.False:
                                c.BeatmapMD5Hashes.Remove(b.MD5Hash);
                                break;
                        }
                    }
                });
            })
            {
                State = { Value = state }
            };
        }
    }
}
