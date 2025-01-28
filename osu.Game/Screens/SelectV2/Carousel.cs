// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Framework.Utils;
using osu.Game.Graphics.Containers;
using osu.Game.Input.Bindings;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.SelectV2
{
    /// <summary>
    /// A highly efficient vertical list display that is used primarily for the song select screen,
    /// but flexible enough to be used for other use cases.
    /// </summary>
    public abstract partial class Carousel<T> : CompositeDrawable, IKeyBindingHandler<GlobalAction>
        where T : notnull
    {
        #region Properties and methods for external usage

        /// <summary>
        /// Height of the area above the carousel that should be treated as visible due to transparency of elements in front of it.
        /// </summary>
        public float BleedTop { get; set; } = 0;

        /// <summary>
        /// Height of the area below the carousel that should be treated as visible due to transparency of elements in front of it.
        /// </summary>
        public float BleedBottom { get; set; } = 0;

        /// <summary>
        /// The number of pixels outside the carousel's vertical bounds to manifest drawables.
        /// This allows preloading content before it scrolls into view.
        /// </summary>
        public float DistanceOffscreenToPreload { get; set; }

        /// <summary>
        /// Vertical space between panel layout. Negative value can be used to create an overlapping effect.
        /// </summary>
        protected float SpacingBetweenPanels { get; set; } = -5;

        /// <summary>
        /// When a new request arrives to change filtering, the number of milliseconds to wait before performing the filter.
        /// Regardless of any external debouncing, this is a safety measure to avoid triggering too many threaded operations.
        /// </summary>
        public int DebounceDelay { get; set; }

        /// <summary>
        /// Whether an asynchronous filter / group operation is currently underway.
        /// </summary>
        public bool IsFiltering => !filterTask.IsCompleted;

        /// <summary>
        /// The number of displayable items currently being tracked (before filtering).
        /// </summary>
        public int ItemsTracked => Items.Count;

        /// <summary>
        /// The number of carousel items currently in rotation for display.
        /// </summary>
        public int DisplayableItems => carouselItems?.Count ?? 0;

        /// <summary>
        /// The number of items currently actualised into drawables.
        /// </summary>
        public int VisibleItems => scroll.Panels.Count;

        /// <summary>
        /// The currently selected model. Generally of type T.
        /// </summary>
        /// <remarks>
        /// A carousel may create panels for non-T types.
        /// To keep things simple, we therefore avoid generic constraints on the current selection.
        ///
        /// The selection is never reset due to not existing. It can be set to anything.
        /// If no matching carousel item exists, there will be no visually selected item while waiting for potential new item which matches.
        /// </remarks>
        public object? CurrentSelection
        {
            get => currentSelection.Model;
            set => setSelection(value);
        }

        /// <summary>
        /// Activate the current selection, if a selection exists and matches keyboard selection.
        /// If keyboard selection does not match selection, this will transfer the selection on first invocation.
        /// </summary>
        public void TryActivateSelection()
        {
            if (currentSelection.CarouselItem != currentKeyboardSelection.CarouselItem)
            {
                CurrentSelection = currentKeyboardSelection.Model;
                return;
            }

            if (currentSelection.CarouselItem != null)
            {
                (GetMaterialisedDrawableForItem(currentSelection.CarouselItem) as ICarouselPanel)?.Activated();
                HandleItemActivated(currentSelection.CarouselItem);
            }
        }

        #endregion

        #region Properties and methods concerning implementations

        /// <summary>
        /// A collection of filters which should be run each time a <see cref="FilterAsync"/> is executed.
        /// </summary>
        /// <remarks>
        /// Implementations should add all required filters as part of their initialisation.
        ///
        /// Importantly, each filter is sequentially run in the order provided.
        /// Each filter receives the output of the previous filter.
        ///
        /// A filter may add, mutate or remove items.
        /// </remarks>
        protected IEnumerable<ICarouselFilter> Filters { get; init; } = Enumerable.Empty<ICarouselFilter>();

        /// <summary>
        /// All items which are to be considered for display in this carousel.
        /// Mutating this list will automatically queue a <see cref="FilterAsync"/>.
        /// </summary>
        /// <remarks>
        /// Note that an <see cref="ICarouselFilter"/> may add new items which are displayed but not tracked in this list.
        /// </remarks>
        protected readonly BindableList<T> Items = new BindableList<T>();

        /// <summary>
        /// Queue an asynchronous filter operation.
        /// </summary>
        protected virtual Task FilterAsync() => filterTask = performFilter();

        /// <summary>
        /// Create a drawable for the given carousel item so it can be displayed.
        /// </summary>
        /// <remarks>
        /// For efficiency, it is recommended the drawables are retrieved from a <see cref="DrawablePool{T}"/>.
        /// </remarks>
        /// <param name="item">The item which should be represented by the returned drawable.</param>
        /// <returns>The manifested drawable.</returns>
        protected abstract Drawable GetDrawableForDisplay(CarouselItem item);

        /// <summary>
        /// Given a <see cref="CarouselItem"/>, find a drawable representation if it is currently displayed in the carousel.
        /// </summary>
        /// <remarks>
        /// This will only return a drawable if it is "on-screen".
        /// </remarks>
        /// <param name="item">The item to find a related drawable representation.</param>
        /// <returns>The drawable representation if it exists.</returns>
        protected Drawable? GetMaterialisedDrawableForItem(CarouselItem item) =>
            scroll.Panels.SingleOrDefault(p => ((ICarouselPanel)p).Item == item);

        /// <summary>
        /// Called when an item is "selected".
        /// </summary>
        /// <returns>Whether the item should be selected.</returns>
        protected virtual bool HandleItemSelected(object? model) => true;

        /// <summary>
        /// Called when an item is "deselected".
        /// </summary>
        protected virtual void HandleItemDeselected(object? model)
        {
        }

        /// <summary>
        /// Called when an item is "activated".
        /// </summary>
        /// <remarks>
        /// An activated item should for instance:
        /// - Open or close a folder
        /// - Start gameplay on a beatmap difficulty.
        /// </remarks>
        /// <param name="item">The carousel item which was activated.</param>
        protected virtual void HandleItemActivated(CarouselItem item)
        {
        }

        #endregion

        #region Initialisation

        private readonly CarouselScrollContainer scroll;

        protected Carousel()
        {
            InternalChild = scroll = new CarouselScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
            };

            Items.BindCollectionChanged((_, _) => FilterAsync());
        }

        #endregion

        #region Filtering and display preparation

        private List<CarouselItem>? carouselItems;

        private Task filterTask = Task.CompletedTask;
        private CancellationTokenSource cancellationSource = new CancellationTokenSource();

        private async Task performFilter()
        {
            Debug.Assert(SynchronizationContext.Current != null);

            Stopwatch stopwatch = Stopwatch.StartNew();
            var cts = new CancellationTokenSource();

            lock (this)
            {
                cancellationSource.Cancel();
                cancellationSource = cts;
            }

            if (DebounceDelay > 0)
            {
                log($"Filter operation queued, waiting for {DebounceDelay} ms debounce");
                await Task.Delay(DebounceDelay, cts.Token).ConfigureAwait(true);
            }

            // Copy must be performed on update thread for now (see ConfigureAwait above).
            // Could potentially be optimised in the future if it becomes an issue.
            IEnumerable<CarouselItem> items = new List<CarouselItem>(Items.Select(m => new CarouselItem(m)));

            await Task.Run(async () =>
            {
                try
                {
                    foreach (var filter in Filters)
                    {
                        log($"Performing {filter.GetType().ReadableName()}");
                        items = await filter.Run(items, cts.Token).ConfigureAwait(false);
                    }

                    log("Updating Y positions");
                    updateYPositions(items, visibleHalfHeight, SpacingBetweenPanels);
                }
                catch (OperationCanceledException)
                {
                    log("Cancelled due to newer request arriving");
                }
            }, cts.Token).ConfigureAwait(true);

            if (cts.Token.IsCancellationRequested)
                return;

            log("Items ready for display");
            carouselItems = items.ToList();
            displayedRange = null;

            // Need to call this to ensure correct post-selection logic is handled on the new items list.
            HandleItemSelected(currentSelection.Model);

            refreshAfterSelection();

            void log(string text) => Logger.Log($"Carousel[op {cts.GetHashCode().ToString()}] {stopwatch.ElapsedMilliseconds} ms: {text}");
        }

        private static void updateYPositions(IEnumerable<CarouselItem> carouselItems, float offset, float spacing)
        {
            foreach (var item in carouselItems)
                updateItemYPosition(item, ref offset, spacing);
        }

        private static void updateItemYPosition(CarouselItem item, ref float offset, float spacing)
        {
            item.CarouselYPosition = offset;
            if (item.IsVisible)
                offset += item.DrawHeight + spacing;
        }

        #endregion

        #region Input handling

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.Select:
                    TryActivateSelection();
                    return true;

                case GlobalAction.SelectNext:
                    selectNext(1, isGroupSelection: false);
                    return true;

                case GlobalAction.SelectNextGroup:
                    selectNext(1, isGroupSelection: true);
                    return true;

                case GlobalAction.SelectPrevious:
                    selectNext(-1, isGroupSelection: false);
                    return true;

                case GlobalAction.SelectPreviousGroup:
                    selectNext(-1, isGroupSelection: true);
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        /// <summary>
        /// Select the next valid selection relative to a current selection.
        /// This is generally for keyboard based traversal.
        /// </summary>
        /// <param name="direction">Positive for downwards, negative for upwards.</param>
        /// <param name="isGroupSelection">Whether the selection should traverse groups. Group selection updates the actual selection immediately, while non-group selection will only prepare a future keyboard selection.</param>
        /// <returns>Whether selection was possible.</returns>
        private bool selectNext(int direction, bool isGroupSelection)
        {
            // Ensure sanity
            Debug.Assert(direction != 0);
            direction = direction > 0 ? 1 : -1;

            if (carouselItems == null || carouselItems.Count == 0)
                return false;

            // If the user has a different keyboard selection and requests
            // group selection, first transfer the keyboard selection to actual selection.
            if (isGroupSelection && currentSelection.CarouselItem != currentKeyboardSelection.CarouselItem)
            {
                TryActivateSelection();
                return true;
            }

            CarouselItem? selectionItem = currentKeyboardSelection.CarouselItem;
            int selectionIndex = currentKeyboardSelection.Index ?? -1;

            // To keep things simple, let's first handle the cases where there's no selection yet.
            if (selectionItem == null || selectionIndex < 0)
            {
                // Start by selecting the first item.
                selectionItem = carouselItems.First();
                selectionIndex = 0;

                // In the forwards case, immediately attempt selection of this panel.
                // If selection fails, continue with standard logic to find the next valid selection.
                if (direction > 0 && attemptSelection(selectionItem))
                    return true;

                // In the backwards direction we can just allow the selection logic to go ahead and loop around to the last valid.
            }

            Debug.Assert(selectionItem != null);

            // As a second special case, if we're group selecting backwards and the current selection isn't a group,
            // make sure to go back to the group header this item belongs to, so that the block below doesn't find it and stop too early.
            if (isGroupSelection && direction < 0)
            {
                while (!carouselItems[selectionIndex].IsGroupSelectionTarget)
                    selectionIndex--;
            }

            CarouselItem? newItem;

            // Iterate over every item back to the current selection, finding the first valid item.
            // The fail condition is when we reach the selection after a cyclic loop over every item.
            do
            {
                selectionIndex += direction;
                newItem = carouselItems[(selectionIndex + carouselItems.Count) % carouselItems.Count];

                if (attemptSelection(newItem))
                    return true;
            } while (newItem != selectionItem);

            return false;

            bool attemptSelection(CarouselItem item)
            {
                if (!item.IsVisible || (isGroupSelection && !item.IsGroupSelectionTarget))
                    return false;

                if (isGroupSelection)
                    setSelection(item.Model);
                else
                    setKeyboardSelection(item.Model);

                return true;
            }
        }

        #endregion

        #region Selection handling

        private readonly Cached selectionValid = new Cached();

        private Selection currentKeyboardSelection = new Selection();
        private Selection currentSelection = new Selection();

        private void setSelection(object? model)
        {
            if (currentSelection.Model == model)
                return;

            if (HandleItemSelected(model))
            {
                if (currentSelection.Model != null)
                    HandleItemDeselected(currentSelection.Model);

                currentKeyboardSelection = new Selection(model);
                currentSelection = currentKeyboardSelection;
                selectionValid.Invalidate();
            }
        }

        private void setKeyboardSelection(object? model)
        {
            currentKeyboardSelection = new Selection(model);
            selectionValid.Invalidate();
        }

        /// <summary>
        /// Call after a selection of items change to re-attach <see cref="CarouselItem"/>s to current <see cref="Selection"/>s.
        /// </summary>
        private void refreshAfterSelection()
        {
            float yPos = visibleHalfHeight;

            // Invalidate display range as panel positions and visible status may have changed.
            // Position transfer won't happen unless we invalidate this.
            displayedRange = null;

            // The case where no items are available for display yet.
            if (carouselItems == null)
            {
                currentKeyboardSelection = new Selection();
                currentSelection = new Selection();
                return;
            }

            float spacing = SpacingBetweenPanels;
            int count = carouselItems.Count;

            Selection prevKeyboard = currentKeyboardSelection;

            // We are performing two important operations here:
            // - Update all Y positions. After a selection occurs, panels may have changed visibility state and therefore Y positions.
            // - Link selected models to CarouselItems. If a selection changed, this is where we find the relevant CarouselItems for further use.
            for (int i = 0; i < count; i++)
            {
                var item = carouselItems[i];

                updateItemYPosition(item, ref yPos, spacing);

                if (ReferenceEquals(item.Model, currentKeyboardSelection.Model))
                    currentKeyboardSelection = new Selection(item.Model, item, item.CarouselYPosition, i);

                if (ReferenceEquals(item.Model, currentSelection.Model))
                    currentSelection = new Selection(item.Model, item, item.CarouselYPosition, i);
            }

            // If a keyboard selection is currently made, we want to keep the view stable around the selection.
            // That means that we should offset the immediate scroll position by any change in Y position for the selection.
            if (prevKeyboard.YPosition != null && currentKeyboardSelection.YPosition != prevKeyboard.YPosition)
                scroll.OffsetScrollPosition((float)(currentKeyboardSelection.YPosition!.Value - prevKeyboard.YPosition.Value));
        }

        private void scrollToSelection()
        {
            if (currentKeyboardSelection.CarouselItem != null)
                scroll.ScrollTo(currentKeyboardSelection.CarouselItem.CarouselYPosition - visibleHalfHeight);
        }

        #endregion

        #region Display handling

        private DisplayRange? displayedRange;

        private readonly CarouselItem carouselBoundsItem = new CarouselItem(new object());

        /// <summary>
        /// The position of the lower visible bound with respect to the current scroll position.
        /// </summary>
        private float visibleBottomBound => (float)(scroll.Current + DrawHeight + BleedBottom);

        /// <summary>
        /// The position of the upper visible bound with respect to the current scroll position.
        /// </summary>
        private float visibleUpperBound => (float)(scroll.Current - BleedTop);

        /// <summary>
        /// Half the height of the visible content.
        /// </summary>
        private float visibleHalfHeight => (DrawHeight + BleedBottom + BleedTop) / 2;

        protected override void Update()
        {
            base.Update();

            if (carouselItems == null)
                return;

            if (!selectionValid.IsValid)
            {
                refreshAfterSelection();
                scrollToSelection();
                selectionValid.Validate();
            }

            var range = getDisplayRange();

            if (range != displayedRange)
            {
                Logger.Log($"Updating displayed range of carousel from {displayedRange} to {range}");
                displayedRange = range;

                updateDisplayedRange(range);
            }

            foreach (var panel in scroll.Panels)
            {
                var c = (ICarouselPanel)panel;

                // panel in the process of expiring, ignore it.
                if (c.Item == null)
                    continue;

                if (panel.Depth != c.DrawYPosition)
                    scroll.Panels.ChangeChildDepth(panel, (float)c.DrawYPosition);

                if (c.DrawYPosition != c.Item.CarouselYPosition)
                    c.DrawYPosition = Interpolation.DampContinuously(c.DrawYPosition, c.Item.CarouselYPosition, 50, Time.Elapsed);

                Vector2 posInScroll = scroll.ToLocalSpace(panel.ScreenSpaceDrawQuad.Centre);
                float dist = Math.Abs(1f - posInScroll.Y / visibleHalfHeight);

                panel.X = offsetX(dist, visibleHalfHeight);

                c.Selected.Value = c.Item == currentSelection?.CarouselItem;
                c.KeyboardSelected.Value = c.Item == currentKeyboardSelection?.CarouselItem;
            }
        }

        /// <summary>
        /// Computes the x-offset of currently visible items. Makes the carousel appear round.
        /// </summary>
        /// <param name="dist">
        /// Vertical distance from the center of the carousel container
        /// ranging from -1 to 1.
        /// </param>
        /// <param name="halfHeight">Half the height of the carousel container.</param>
        private static float offsetX(float dist, float halfHeight)
        {
            // The radius of the circle the carousel moves on.
            const float circle_radius = 3;
            float discriminant = MathF.Max(0, circle_radius * circle_radius - dist * dist);
            return (circle_radius - MathF.Sqrt(discriminant)) * halfHeight;
        }

        private DisplayRange getDisplayRange()
        {
            Debug.Assert(carouselItems != null);

            // Find index range of all items that should be on-screen
            carouselBoundsItem.CarouselYPosition = visibleUpperBound - DistanceOffscreenToPreload;
            int firstIndex = carouselItems.BinarySearch(carouselBoundsItem);
            if (firstIndex < 0) firstIndex = ~firstIndex;

            carouselBoundsItem.CarouselYPosition = visibleBottomBound + DistanceOffscreenToPreload;
            int lastIndex = carouselItems.BinarySearch(carouselBoundsItem);
            if (lastIndex < 0) lastIndex = ~lastIndex;

            firstIndex = Math.Max(0, firstIndex - 1);
            lastIndex = Math.Max(0, lastIndex - 1);

            return new DisplayRange(firstIndex, lastIndex);
        }

        private void updateDisplayedRange(DisplayRange range)
        {
            Debug.Assert(carouselItems != null);

            List<CarouselItem> toDisplay = range.Last - range.First == 0
                ? new List<CarouselItem>()
                : carouselItems.GetRange(range.First, range.Last - range.First + 1);

            toDisplay.RemoveAll(i => !i.IsVisible);

            // Iterate over all panels which are already displayed and figure which need to be displayed / removed.
            foreach (var panel in scroll.Panels)
            {
                var carouselPanel = (ICarouselPanel)panel;

                // The case where we're intending to display this panel, but it's already displayed.
                // Note that we **must compare the model here** as the CarouselItems may be fresh instances due to a filter operation.
                var existing = toDisplay.FirstOrDefault(i => i.Model == carouselPanel.Item!.Model);

                if (existing != null)
                {
                    carouselPanel.Item = existing;
                    toDisplay.Remove(existing);
                    continue;
                }

                // If the new display range doesn't contain the panel, it's no longer required for display.
                expirePanelImmediately(panel);
            }

            // Add any new items which need to be displayed and haven't yet.
            foreach (var item in toDisplay)
            {
                var drawable = GetDrawableForDisplay(item);

                if (drawable is not ICarouselPanel carouselPanel)
                    throw new InvalidOperationException($"Carousel panel drawables must implement {typeof(ICarouselPanel)}");

                carouselPanel.DrawYPosition = item.CarouselYPosition;
                carouselPanel.Item = item;

                scroll.Add(drawable);
            }

            // Update the total height of all items (to make the scroll container scrollable through the full height even though
            // most items are not displayed / loaded).
            if (carouselItems.Count > 0)
            {
                var lastItem = carouselItems[^1];
                scroll.SetLayoutHeight((float)(lastItem.CarouselYPosition + lastItem.DrawHeight + visibleHalfHeight));
            }
            else
                scroll.SetLayoutHeight(0);
        }

        private static void expirePanelImmediately(Drawable panel)
        {
            panel.FinishTransforms();
            panel.Expire();

            var carouselPanel = (ICarouselPanel)panel;

            carouselPanel.Item = null;
            carouselPanel.Selected.Value = false;
            carouselPanel.KeyboardSelected.Value = false;
        }

        #endregion

        #region Internal helper classes

        /// <summary>
        /// Bookkeeping for a current selection.
        /// </summary>
        /// <param name="Model">The selected model. If <c>null</c>, there's no selection.</param>
        /// <param name="CarouselItem">A related carousel item representation for the model. May be null if selection is not present as an item, or if <see cref="Carousel{T}.refreshAfterSelection"/> has not been run yet.</param>
        /// <param name="YPosition">The Y position of the selection as of the last run of <see cref="Carousel{T}.refreshAfterSelection"/>. May be null if selection is not present as an item, or if <see cref="Carousel{T}.refreshAfterSelection"/> has not been run yet.</param>
        /// <param name="Index">The index of the selection as of the last run of <see cref="Carousel{T}.refreshAfterSelection"/>. May be null if selection is not present as an item, or if <see cref="Carousel{T}.refreshAfterSelection"/> has not been run yet.</param>
        private record Selection(object? Model = null, CarouselItem? CarouselItem = null, double? YPosition = null, int? Index = null);

        private record DisplayRange(int First, int Last);

        /// <summary>
        /// Implementation of scroll container which handles very large vertical lists by internally using <c>double</c> precision
        /// for pre-display Y values.
        /// </summary>
        private partial class CarouselScrollContainer : UserTrackingScrollContainer, IKeyBindingHandler<GlobalAction>
        {
            public readonly Container Panels;

            public void SetLayoutHeight(float height) => Panels.Height = height;

            public CarouselScrollContainer()
            {
                // Managing our own custom layout within ScrollContent causes feedback with public ScrollContainer calculations,
                // so we must maintain one level of separation from ScrollContent.
                base.Add(Panels = new Container
                {
                    Name = "Layout content",
                    RelativeSizeAxes = Axes.X,
                });
            }

            public override void OffsetScrollPosition(double offset)
            {
                base.OffsetScrollPosition(offset);

                foreach (var panel in Panels)
                {
                    var c = (ICarouselPanel)panel;
                    Debug.Assert(c.Item != null);

                    c.DrawYPosition += offset;
                }
            }

            public override void Clear(bool disposeChildren)
            {
                Panels.Height = 0;
                Panels.Clear(disposeChildren);
            }

            public override void Add(Drawable drawable)
            {
                if (drawable is not ICarouselPanel)
                    throw new InvalidOperationException($"Carousel panel drawables must implement {typeof(ICarouselPanel)}");

                Panels.Add(drawable);
            }

            public override double GetChildPosInContent(Drawable d, Vector2 offset)
            {
                if (d is not ICarouselPanel panel)
                    return base.GetChildPosInContent(d, offset);

                return panel.DrawYPosition + offset.X;
            }

            protected override void ApplyCurrentToContent()
            {
                Debug.Assert(ScrollDirection == Direction.Vertical);

                double scrollableExtent = -Current + ScrollableExtent * ScrollContent.RelativeAnchorPosition.Y;

                foreach (var d in Panels)
                    d.Y = (float)(((ICarouselPanel)d).DrawYPosition + scrollableExtent);
            }

            #region Absolute scrolling

            private bool absoluteScrolling;

            protected override bool IsDragging => base.IsDragging || absoluteScrolling;

            public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
            {
                switch (e.Action)
                {
                    case GlobalAction.AbsoluteScrollSongList:
                        beginAbsoluteScrolling(e);
                        return true;
                }

                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
            {
                switch (e.Action)
                {
                    case GlobalAction.AbsoluteScrollSongList:
                        endAbsoluteScrolling();
                        break;
                }
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                if (e.Button == MouseButton.Right)
                {
                    // To avoid conflicts with context menus, disallow absolute scroll if it looks like things will fall over.
                    if (GetContainingInputManager()!.HoveredDrawables.OfType<IHasContextMenu>().Any())
                        return false;

                    beginAbsoluteScrolling(e);
                }

                return base.OnMouseDown(e);
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                if (e.Button == MouseButton.Right)
                    endAbsoluteScrolling();
                base.OnMouseUp(e);
            }

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                if (absoluteScrolling)
                {
                    ScrollToAbsolutePosition(e.CurrentState.Mouse.Position);
                    return true;
                }

                return base.OnMouseMove(e);
            }

            private void beginAbsoluteScrolling(UIEvent e)
            {
                ScrollToAbsolutePosition(e.CurrentState.Mouse.Position);
                absoluteScrolling = true;
            }

            private void endAbsoluteScrolling() => absoluteScrolling = false;

            #endregion
        }

        #endregion
    }
}
