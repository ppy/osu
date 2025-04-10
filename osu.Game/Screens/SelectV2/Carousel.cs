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
using osu.Framework.Layout;
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
        public int VisibleItems => Scroll.Panels.Count;

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
            set
            {
                if (currentSelection.Model != value)
                {
                    HandleItemSelected(value);

                    if (currentSelection.Model != null)
                        HandleItemDeselected(currentSelection.Model);

                    currentKeyboardSelection = new Selection(value);
                    currentSelection = currentKeyboardSelection;
                    selectionValid.Invalidate();
                }
                else if (currentKeyboardSelection.Model != value)
                {
                    // Even if the current selection matches, let's ensure the keyboard selection is reset
                    // to the newly selected object. This matches user expectations (for now).
                    currentKeyboardSelection = currentSelection;
                    selectionValid.Invalidate();
                }
            }
        }

        /// <summary>
        /// Activate the specified item.
        /// </summary>
        /// <param name="item"></param>
        public void Activate(CarouselItem item)
        {
            // Regardless of how the item handles activation, update keyboard selection to the activated panel.
            // In other words, when a panel is clicked, keyboard selection should default to matching the clicked
            // item.
            setKeyboardSelection(item.Model);

            (GetMaterialisedDrawableForItem(item) as ICarouselPanel)?.Activated();
            HandleItemActivated(item);

            selectionValid.Invalidate();
        }

        /// <summary>
        /// Returns the vertical spacing between two given carousel items. Negative value can be used to create an overlapping effect.
        /// </summary>
        protected virtual float GetSpacingBetweenPanels(CarouselItem top, CarouselItem bottom) => 0f;

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
        public IEnumerable<ICarouselFilter> Filters { get; init; } = Enumerable.Empty<ICarouselFilter>();

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
            Scroll.Panels.SingleOrDefault(p => ((ICarouselPanel)p).Item == item);

        /// <summary>
        /// When a user is traversing the carousel via group selection keys, assert whether the item provided is a valid target.
        /// </summary>
        /// <param name="item">The candidate item.</param>
        /// <returns>Whether the provided item is a valid group target. If <c>false</c>, more panels will be checked in the user's requested direction until a valid target is found.</returns>
        protected virtual bool CheckValidForGroupSelection(CarouselItem item) => true;

        /// <summary>
        /// Called after an item becomes the <see cref="CurrentSelection"/>.
        /// Should be used to handle any group expansion, item visibility changes, etc.
        /// </summary>
        protected virtual void HandleItemSelected(object? model) { }

        /// <summary>
        /// Called when the <see cref="CurrentSelection"/> changes to a new selection.
        /// Should be used to handle any group expansion, item visibility changes, etc.
        /// </summary>
        protected virtual void HandleItemDeselected(object? model) { }

        /// <summary>
        /// Called when an item is activated via user input (keyboard traversal or a mouse click).
        /// </summary>
        /// <remarks>
        /// An activated item should decide to perform an action, such as:
        /// - Change its expanded state (and show / hide children items).
        /// - Set the item to the <see cref="CurrentSelection"/>.
        /// - Start gameplay on a beatmap difficulty if already selected.
        /// </remarks>
        /// <param name="item">The carousel item which was activated.</param>
        protected virtual void HandleItemActivated(CarouselItem item) { }

        #endregion

        #region Initialisation

        protected readonly CarouselScrollContainer Scroll;

        protected Carousel()
        {
            InternalChild = Scroll = new CarouselScrollContainer
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
            Stopwatch stopwatch = Stopwatch.StartNew();
            var cts = new CancellationTokenSource();

            var previousCancellationSource = Interlocked.Exchange(ref cancellationSource, cts);
            await previousCancellationSource.CancelAsync().ConfigureAwait(false);

            if (DebounceDelay > 0)
            {
                log($"Filter operation queued, waiting for {DebounceDelay} ms debounce");
                await Task.Delay(DebounceDelay, cts.Token).ConfigureAwait(false);
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
                    updateYPositions(items, visibleHalfHeight);
                }
                catch (OperationCanceledException)
                {
                    log("Cancelled due to newer request arriving");
                }
            }, cts.Token).ConfigureAwait(false);

            if (cts.Token.IsCancellationRequested)
                return;

            Schedule(() =>
            {
                log("Items ready for display");
                carouselItems = items.ToList();
                displayedRange = null;

                // Need to call this to ensure correct post-selection logic is handled on the new items list.
                HandleItemSelected(currentSelection.Model);

                refreshAfterSelection();
            });

            void log(string text) => Logger.Log($"Carousel[op {cts.GetHashCode().ToString()}] {stopwatch.ElapsedMilliseconds} ms: {text}");
        }

        private void updateYPositions(IEnumerable<CarouselItem> carouselItems, float offset)
        {
            CarouselItem? previousVisible = null;

            foreach (var item in carouselItems)
                updateItemYPosition(item, ref previousVisible, ref offset);
        }

        private void updateItemYPosition(CarouselItem item, ref CarouselItem? previousVisible, ref float offset)
        {
            float spacing = previousVisible == null || !item.IsVisible ? 0 : GetSpacingBetweenPanels(previousVisible, item);

            offset += spacing;
            item.CarouselYPosition = offset;

            if (item.IsVisible)
            {
                offset += item.DrawHeight;
                previousVisible = item;
            }
        }

        #endregion

        #region Input handling

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.Select:
                    if (currentKeyboardSelection.CarouselItem != null)
                        Activate(currentKeyboardSelection.CarouselItem);
                    return true;

                case GlobalAction.SelectNext:
                    traverseKeyboardSelection(1);
                    return true;

                case GlobalAction.SelectPrevious:
                    traverseKeyboardSelection(-1);
                    return true;

                case GlobalAction.SelectNextGroup:
                    traverseGroupSelection(1);
                    return true;

                case GlobalAction.SelectPreviousGroup:
                    traverseGroupSelection(-1);
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        private void traverseKeyboardSelection(int direction)
        {
            if (carouselItems == null || carouselItems.Count == 0) return;

            int originalIndex;

            if (currentKeyboardSelection.Index != null)
                originalIndex = currentKeyboardSelection.Index.Value;
            else if (direction > 0)
                originalIndex = carouselItems.Count - 1;
            else
                originalIndex = 0;

            int newIndex = originalIndex;

            // Iterate over every item back to the current selection, finding the first valid item.
            // The fail condition is when we reach the selection after a cyclic loop over every item.
            do
            {
                newIndex = (newIndex + direction + carouselItems.Count) % carouselItems.Count;
                var newItem = carouselItems[newIndex];

                if (newItem.IsVisible)
                {
                    setKeyboardSelection(newItem.Model);
                    return;
                }
            } while (newIndex != originalIndex);
        }

        /// <summary>
        /// Select the next valid selection relative to a current selection.
        /// This is generally for keyboard based traversal.
        /// </summary>
        /// <param name="direction">Positive for downwards, negative for upwards.</param>
        /// <returns>Whether selection was possible.</returns>
        private void traverseGroupSelection(int direction)
        {
            if (carouselItems == null || carouselItems.Count == 0) return;

            // If the user has a different keyboard selection and requests
            // group selection, first transfer the keyboard selection to actual selection.
            if (currentKeyboardSelection.CarouselItem != null && currentSelection.CarouselItem != currentKeyboardSelection.CarouselItem)
            {
                Activate(currentKeyboardSelection.CarouselItem);
                return;
            }

            int originalIndex;
            int newIndex;

            if (currentKeyboardSelection.Index == null)
            {
                // If there's no current selection, start from either end of the full list.
                newIndex = originalIndex = direction > 0 ? carouselItems.Count - 1 : 0;
            }
            else
            {
                newIndex = originalIndex = currentKeyboardSelection.Index.Value;

                // As a second special case, if we're group selecting backwards and the current selection isn't a group,
                // make sure to go back to the group header this item belongs to, so that the block below doesn't find it and stop too early.
                if (direction < 0)
                {
                    while (newIndex > 0 && !CheckValidForGroupSelection(carouselItems[newIndex]))
                        newIndex--;
                }
            }

            // Iterate over every item back to the current selection, finding the first valid item.
            // The fail condition is when we reach the selection after a cyclic loop over every item.
            do
            {
                newIndex = (newIndex + direction + carouselItems.Count) % carouselItems.Count;
                var newItem = carouselItems[newIndex];

                if (CheckValidForGroupSelection(newItem))
                {
                    HandleItemActivated(newItem);
                    return;
                }
            } while (newIndex != originalIndex);
        }

        #endregion

        #region Selection handling

        private readonly Cached selectionValid = new Cached();

        private Selection currentKeyboardSelection = new Selection();
        private Selection currentSelection = new Selection();

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

            CarouselItem? lastVisible = null;
            int count = carouselItems.Count;

            Selection prevKeyboard = currentKeyboardSelection;

            // We are performing two important operations here:
            // - Update all Y positions. After a selection occurs, panels may have changed visibility state and therefore Y positions.
            // - Link selected models to CarouselItems. If a selection changed, this is where we find the relevant CarouselItems for further use.
            for (int i = 0; i < count; i++)
            {
                var item = carouselItems[i];

                updateItemYPosition(item, ref lastVisible, ref yPos);

                if (ReferenceEquals(item.Model, currentKeyboardSelection.Model))
                    currentKeyboardSelection = new Selection(item.Model, item, item.CarouselYPosition, i);

                if (ReferenceEquals(item.Model, currentSelection.Model))
                    currentSelection = new Selection(item.Model, item, item.CarouselYPosition, i);
            }

            // If a keyboard selection is currently made, we want to keep the view stable around the selection.
            // That means that we should offset the immediate scroll position by any change in Y position for the selection.
            if (prevKeyboard.YPosition != null && currentKeyboardSelection.YPosition != prevKeyboard.YPosition)
                Scroll.OffsetScrollPosition((float)(currentKeyboardSelection.YPosition!.Value - prevKeyboard.YPosition.Value));
        }

        private void scrollToSelection()
        {
            if (currentKeyboardSelection.CarouselItem != null)
                Scroll.ScrollTo(currentKeyboardSelection.CarouselItem.CarouselYPosition - visibleHalfHeight);
        }

        #endregion

        #region Display handling

        private DisplayRange? displayedRange;

        private readonly CarouselItem carouselBoundsItem = new CarouselItem(new object());

        /// <summary>
        /// The position of the lower visible bound with respect to the current scroll position.
        /// </summary>
        private float visibleBottomBound => (float)(Scroll.Current + DrawHeight + BleedBottom);

        /// <summary>
        /// The position of the upper visible bound with respect to the current scroll position.
        /// </summary>
        private float visibleUpperBound => (float)(Scroll.Current - BleedTop);

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

            double selectedYPos = currentSelection.CarouselItem?.CarouselYPosition ?? 0;

            foreach (var panel in Scroll.Panels)
            {
                var c = (ICarouselPanel)panel;

                // panel in the process of expiring, ignore it.
                if (c.Item == null)
                    continue;

                float normalisedDepth = (float)(Math.Abs(selectedYPos - c.DrawYPosition) / DrawHeight);
                Scroll.Panels.ChangeChildDepth(panel, c.Item.DepthLayer + normalisedDepth);

                if (c.DrawYPosition != c.Item.CarouselYPosition)
                    c.DrawYPosition = Interpolation.DampContinuously(c.DrawYPosition, c.Item.CarouselYPosition, 50, Time.Elapsed);

                panel.X = GetPanelXOffset(panel);

                c.Selected.Value = c.Item == currentSelection?.CarouselItem;
                c.KeyboardSelected.Value = c.Item == currentKeyboardSelection?.CarouselItem;
                c.Expanded.Value = c.Item.IsExpanded;
            }
        }

        protected virtual float GetPanelXOffset(Drawable panel)
        {
            Vector2 posInScroll = Scroll.ToLocalSpace(panel.ScreenSpaceDrawQuad.Centre);
            float dist = Math.Abs(1f - posInScroll.Y / visibleHalfHeight);

            return offsetX(dist, visibleHalfHeight);
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
            foreach (var panel in Scroll.Panels)
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

                Scroll.Add(drawable);
            }

            // Update the total height of all items (to make the scroll container scrollable through the full height even though
            // most items are not displayed / loaded).
            if (carouselItems.Count > 0)
            {
                var lastItem = carouselItems[^1];
                Scroll.SetLayoutHeight((float)(lastItem.CarouselYPosition + lastItem.DrawHeight + visibleHalfHeight));
            }
            else
                Scroll.SetLayoutHeight(0);
        }

        private static void expirePanelImmediately(Drawable panel)
        {
            panel.FinishTransforms();
            panel.Expire();

            var carouselPanel = (ICarouselPanel)panel;

            carouselPanel.Item = null;
            carouselPanel.Selected.Value = false;
            carouselPanel.KeyboardSelected.Value = false;
            carouselPanel.Expanded.Value = false;
        }

        protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
        {
            // handles the vertical size of the carousel changing (ie. on window resize when aspect ratio has changed).
            if (invalidation.HasFlag(Invalidation.DrawSize))
                selectionValid.Invalidate();

            return base.OnInvalidate(invalidation, source);
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
        protected partial class CarouselScrollContainer : UserTrackingScrollContainer, IKeyBindingHandler<GlobalAction>
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
