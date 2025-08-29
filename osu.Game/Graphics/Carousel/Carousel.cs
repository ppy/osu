// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Development;
using osu.Framework.Extensions.PolygonExtensions;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Layout;
using osu.Framework.Logging;
using osu.Framework.Utils;
using osu.Game.Input.Bindings;
using osu.Game.Online.Multiplayer;
using osuTK;
using osuTK.Input;

namespace osu.Game.Graphics.Carousel
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
        /// Called after a filter operation or change in items results in the visible carousel items changing.
        /// </summary>
        public Action<IEnumerable<CarouselItem>>? NewItemsPresented { private get; init; }

        /// <summary>
        /// Height of the area above the carousel that should be treated as visible due to transparency of elements in front of it.
        /// </summary>
        public float BleedTop { get; set; }

        /// <summary>
        /// Height of the area below the carousel that should be treated as visible due to transparency of elements in front of it.
        /// </summary>
        public float BleedBottom { get; set; }

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
        /// Whether absolute scrolling is currently triggered.
        /// </summary>
        public bool AbsoluteScrolling => Scroll.AbsoluteScrolling;

        /// <summary>
        /// The number of times filter operations have been triggered.
        /// </summary>
        public int FilterCount { get; private set; }

        /// <summary>
        /// The number of displayable items currently being tracked (before filtering).
        /// </summary>
        public int ItemsTracked => Items.Count;

        /// <summary>
        /// The items currently in rotation for display.
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
                if (!CheckModelEquality(currentSelection.Model, value))
                {
                    HandleItemSelected(value);

                    if (currentSelection.Model != null)
                        HandleItemDeselected(currentSelection.Model);

                    currentSelection = new Selection(value);
                    currentKeyboardSelection = currentSelection;
                    selectionValid.Invalidate();
                }

                // Check keyboard selection equality separately.
                //
                // If current selection set to an already-selected value, we want to ensure
                // that keyboard selection (which basically represents the "visual" tracking of selection)
                // is still reset back to the newly set value.
                //
                // The main case this handles is when a set header is clicked and we want to make sure one of its
                // "children" are re-selected.
                if (!CheckModelEquality(currentKeyboardSelection.Model, value))
                {
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
        /// Scroll carousel to the selected item if available.
        /// </summary>
        public void ScrollToSelection() => scrollToSelection.Invalidate();

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
        /// <param name="clearExistingPanels">Whether all existing drawable panels should be reset post filter.</param>
        protected virtual Task<IEnumerable<CarouselItem>> FilterAsync(bool clearExistingPanels = false)
        {
            FilterCount++;

            if (clearExistingPanels)
                filterReusesPanels.Invalidate();

            filterAfterItemsChanged.Validate();

            filterTask = performFilter();
            filterTask.FireAndForget();
            return filterTask;
        }

        /// <summary>
        /// Called when <see cref="Items"/> changes in any way.
        /// </summary>
        /// <returns>Whether a re-filter is required.</returns>
        protected virtual bool HandleItemsChanged(NotifyCollectionChangedEventArgs args) => true;

        /// <summary>
        /// Fired after a filter operation completed.
        /// </summary>
        protected virtual void HandleFilterCompleted()
        {
        }

        /// <summary>
        /// Check whether two models are the same for display purposes.
        /// </summary>
        protected virtual bool CheckModelEquality(object? x, object? y) => ReferenceEquals(x, y);

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
        protected virtual bool CheckValidForGroupSelection(CarouselItem item) => false;

        /// <summary>
        /// When a user is traversing the carousel via set selection keys, assert whether the item provided is a valid target.
        /// </summary>
        /// <param name="item">The candidate item.</param>
        /// <returns>Whether the provided item is a valid set target. If <c>false</c>, more panels will be checked in the user's requested direction until a valid target is found.</returns>
        protected virtual bool CheckValidForSetSelection(CarouselItem item) => true;

        /// <summary>
        /// Keyboard selection usually does not automatically activate an item. There may be exceptions to this rule.
        /// Returning <c>true</c> here will make keyboard traversal act like set traversal for the target item.
        /// </summary>
        protected virtual bool ShouldActivateOnKeyboardSelection(CarouselItem item) => false;

        /// <summary>
        /// Called after an item becomes the <see cref="CurrentSelection"/>.
        /// Should be used to handle any set expansion, item visibility changes, etc.
        /// </summary>
        protected virtual void HandleItemSelected(object? model) { }

        /// <summary>
        /// Called when the <see cref="CurrentSelection"/> changes to a new selection.
        /// Should be used to handle any set expansion, item visibility changes, etc.
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

        protected readonly ScrollContainer Scroll;

        protected Carousel()
        {
            InternalChild = Scroll = new ScrollContainer
            {
                Masking = false,
                RelativeSizeAxes = Axes.Both,
            };

            Items.BindCollectionChanged((_, args) =>
            {
                if (HandleItemsChanged(args))
                    filterAfterItemsChanged.Invalidate();
            });
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            loadSamples(audio);
        }

        #endregion

        #region Filtering and display preparation

        /// <summary>
        /// Retrieve a list of all <see cref="CarouselItem"/>s currently displayed.
        /// </summary>
        public IList<CarouselItem>? GetCarouselItems() => carouselItems;

        private List<CarouselItem>? carouselItems;

        private Task<IEnumerable<CarouselItem>> filterTask = Task.FromResult(Enumerable.Empty<CarouselItem>());
        private CancellationTokenSource cancellationSource = new CancellationTokenSource();

        /// <summary>
        /// For background re-filters, ensure we wait for the previous filter operation to complete before starting another.
        /// This avoids the carousel never updating its display in high churn scenarios.
        /// </summary>
        private readonly Cached filterAfterItemsChanged = new Cached();

        private async Task<IEnumerable<CarouselItem>> performFilter()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            var cts = new CancellationTokenSource();

            var previousCancellationSource = Interlocked.Exchange(ref cancellationSource, cts);
            await previousCancellationSource.CancelAsync().ConfigureAwait(true);

            if (DebounceDelay > 0)
            {
                log($"Filter operation queued, waiting for {DebounceDelay} ms debounce");
                await Task.Delay(DebounceDelay, cts.Token).ConfigureAwait(true);
            }

            // Copy must be performed on update thread for now (see ConfigureAwait above).
            // Could potentially be optimised in the future if it becomes an issue.
            Debug.Assert(ThreadSafety.IsUpdateThread);
            List<CarouselItem> items = new List<CarouselItem>(Items.Select(m => new CarouselItem(m)));

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
                return Enumerable.Empty<CarouselItem>();

            Schedule(() =>
            {
                log("Items ready for display");
                carouselItems = items;
                displayedRange = null;

                if (!filterReusesPanels.IsValid)
                {
                    foreach (var panel in Scroll.Panels)
                        expirePanel(panel);

                    filterReusesPanels.Validate();
                }

                HandleFilterCompleted();

                refreshAfterSelection();
                if (!Scroll.UserScrolling)
                    ScrollToSelection();

                NewItemsPresented?.Invoke(carouselItems);
            });

            return items;

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

            // ensure there are no input gaps where clicking will fall through the carousel.
            // notably, only do this where there's positive spacing between panels (negative spacing means they overlap already and there is no gap to fill).
            if (spacing > 0)
            {
                item.CarouselInputLenienceAbove = spacing / 2;
                if (previousVisible != null)
                    previousVisible.CarouselInputLenienceBelow = item.CarouselInputLenienceAbove;
            }

            if (item.IsVisible)
            {
                offset += item.DrawHeight;
                previousVisible = item;
            }
        }

        #endregion

        #region Input handling

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            switch (e.Key)
            {
                // this is a special hard-coded case; we can't rely on OnPressed as GlobalActionContainer is
                // matching with exact modifier consideration (so Ctrl+Enter would be ignored).
                case Key.Enter:
                case Key.KeypadEnter:
                    activateSelection();
                    return true;
            }

            return base.OnKeyDown(e);
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.Select:
                    activateSelection();
                    return true;

                // the selection traversal handlers below are scheduled to avoid an issue
                // wherein if the update frame rate is low, keeping one of the actions below pressed leads to selection moving back to the start / end.
                // the reason why that happens is that the code managing `current(Keyboard)?Selection` can lose track of the index of the selected item
                // if the selection is changed more than once during an update frame,
                // which can happen if repeat inputs are enqueued for processing at a rate faster than the update refresh rate.
                // `refreshAfterSelection()` is the method responsible for updating the index of the selected item here which runs once per frame.
                case GlobalAction.SelectPrevious:
                    Scheduler.AddOnce(traverseFromKey, new TraversalOperation(TraversalType.Keyboard, -1));
                    return true;

                case GlobalAction.SelectNext:
                    Scheduler.AddOnce(traverseFromKey, new TraversalOperation(TraversalType.Keyboard, 1));
                    return true;

                case GlobalAction.ActivatePreviousSet:
                    Scheduler.AddOnce(traverseFromKey, new TraversalOperation(TraversalType.Set, -1));
                    return true;

                case GlobalAction.ActivateNextSet:
                    Scheduler.AddOnce(traverseFromKey, new TraversalOperation(TraversalType.Set, 1));
                    return true;

                case GlobalAction.ExpandPreviousGroup:
                    Scheduler.AddOnce(traverseFromKey, new TraversalOperation(TraversalType.Group, -1));
                    return true;

                case GlobalAction.ExpandNextGroup:
                    Scheduler.AddOnce(traverseFromKey, new TraversalOperation(TraversalType.Group, 1));
                    return true;

                case GlobalAction.ToggleCurrentGroup:
                    if (carouselItems == null || carouselItems.Count == 0)
                        return true;

                    if (currentKeyboardSelection.CarouselItem == null || currentKeyboardSelection.Index == null)
                        return true;

                    if (CheckValidForGroupSelection(currentKeyboardSelection.CarouselItem))
                    {
                        // If keyboard selection is a group, toggle group and then change keyboard selection to actual selection.
                        Activate(currentKeyboardSelection.CarouselItem);
                    }
                    else
                    {
                        // If current keyboard selection is not a group, toggle the closest group and move keyboard selection to that group.
                        for (int i = currentKeyboardSelection.Index.Value; i >= 0; i--)
                        {
                            var newItem = carouselItems[i];

                            if (CheckValidForGroupSelection(newItem))
                            {
                                Activate(newItem);
                                return true;
                            }
                        }
                    }

                    return true;
            }

            return false;

            void traverseFromKey(TraversalOperation traversal)
            {
                switch (traversal.Type)
                {
                    case TraversalType.Keyboard:
                        traverseKeyboardSelection(traversal.Direction);
                        break;

                    case TraversalType.Set:
                        traverseSetSelection(traversal.Direction);
                        break;

                    case TraversalType.Group:
                        traverseGroupSelection(traversal.Direction);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private enum TraversalType { Keyboard, Set, Group }

        private record TraversalOperation(TraversalType Type, int Direction);

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        private void activateSelection()
        {
            if (currentKeyboardSelection.CarouselItem != null)
                Activate(currentKeyboardSelection.CarouselItem);
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
                    if (!CheckModelEquality(currentSelection.Model, newItem.Model) && ShouldActivateOnKeyboardSelection(newItem))
                        Activate(newItem);
                    else
                    {
                        playTraversalSound();
                        setKeyboardSelection(newItem.Model);
                    }

                    return;
                }
            } while (newIndex != originalIndex);
        }

        /// <summary>
        /// Select the next valid group selection relative to a current selection.
        /// This is generally for keyboard based traversal.
        /// </summary>
        /// <param name="direction">Positive for downwards, negative for upwards.</param>
        /// <returns>Whether selection was possible.</returns>
        private void traverseGroupSelection(int direction) => traverseSelection(direction, CheckValidForGroupSelection);

        /// <summary>
        /// Select the next valid set selection relative to a current selection.
        /// This is generally for keyboard based traversal.
        /// </summary>
        /// <param name="direction">Positive for downwards, negative for upwards.</param>
        /// <returns>Whether selection was possible.</returns>
        private void traverseSetSelection(int direction)
        {
            // If the user has a different keyboard selection and requests
            // set selection, first transfer the keyboard selection to actual selection.
            //
            // It is assumed that selecting a set will immediately change selection to one of its children.
            if (currentKeyboardSelection.CarouselItem != null && currentSelection.CarouselItem != currentKeyboardSelection.CarouselItem)
            {
                Activate(currentKeyboardSelection.CarouselItem);
                return;
            }

            traverseSelection(direction, CheckValidForSetSelection);
        }

        private void traverseSelection(int direction, Func<CarouselItem, bool> predicate)
        {
            if (carouselItems == null || carouselItems.Count == 0) return;

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

                // As a second special case, if we're set selecting backwards and the current selection isn't a set,
                // make sure to go back to the set header this item belongs to, so that the block below doesn't find it and stop too early.
                if (direction < 0)
                {
                    while (newIndex > 0 && !predicate(carouselItems[newIndex]))
                        newIndex--;
                }
            }

            // Iterate over every item back to the current selection, finding the first valid item.
            // The fail condition is when we reach the selection after a cyclic loop over every item.
            do
            {
                newIndex = (newIndex + direction + carouselItems.Count) % carouselItems.Count;

                if (newIndex == originalIndex)
                    break;

                var newItem = carouselItems[newIndex];

                if (!newItem.IsExpanded && predicate(newItem))
                {
                    Activate(newItem);
                    return;
                }
            } while (true);
        }

        #endregion

        #region Audio

        private Sample? sampleKeyboardTraversal;

        private double audioFeedbackLastPlaybackTime;

        private void loadSamples(AudioManager audio)
        {
            sampleKeyboardTraversal = audio.Samples.Get(@"SongSelect/select-difficulty");
        }

        private void playTraversalSound()
        {
            if (Time.Current - audioFeedbackLastPlaybackTime >= OsuGameBase.SAMPLE_DEBOUNCE_TIME)
            {
                sampleKeyboardTraversal?.Play();
                audioFeedbackLastPlaybackTime = Time.Current;
            }
        }

        #endregion

        #region Selection handling

        /// <summary>
        /// The currently selected <see cref="CarouselItem"/>, if any is selected.
        /// </summary>
        protected CarouselItem? CurrentSelectionItem => currentSelection.CarouselItem;

        /// <summary>
        /// The index in <see cref="GetCarouselItems"/> of the current selection, if available.
        /// </summary>
        protected int? CurrentSelectionIndex => currentSelection.Index;

        /// <summary>
        /// Becomes invalid when the current selection has changed and needs to be updated visually.
        /// </summary>
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

            Selection prevKeyboard = currentKeyboardSelection;

            // Importantly, we also reset the `Selection` to the most basic state.
            // Removing the index and carousel item here is important to ensure we are aware of if a selection has been filtered away.
            // If it hasn't been filtered, the full details will be re-populated just below in the loop.
            currentKeyboardSelection = new Selection(currentKeyboardSelection.Model);
            currentSelection = new Selection(currentSelection.Model);

            if (carouselItems == null)
                return;

            CarouselItem? lastVisible = null;
            int count = carouselItems.Count;

            // We are performing two important operations here:
            // - Update all Y positions. After a selection occurs, panels may have changed visibility state and therefore Y positions.
            // - Link selected models to CarouselItems. If a selection changed, this is where we find the relevant CarouselItems for further use.
            for (int i = 0; i < count; i++)
            {
                var item = carouselItems[i];

                updateItemYPosition(item, ref lastVisible, ref yPos);

                if (CheckModelEquality(item.Model, currentKeyboardSelection.Model!))
                    currentKeyboardSelection = new Selection(currentKeyboardSelection.Model, item, item.CarouselYPosition, i);

                if (CheckModelEquality(item.Model, currentSelection.Model!))
                    currentSelection = new Selection(currentSelection.Model, item, item.CarouselYPosition, i);
            }

            // Update the total height of all items (to make the scroll container scrollable through the full height even though
            // most items are not displayed / loaded).
            Scroll.SetLayoutHeight(yPos + visibleHalfHeight);

            // If a keyboard selection is currently made, we want to keep the view stable around the selection.
            // That means that we should offset the immediate scroll position by any change in Y position for the selection.
            if (prevKeyboard.YPosition != null && currentKeyboardSelection.YPosition != null && currentKeyboardSelection.YPosition != prevKeyboard.YPosition)
                Scroll.OffsetScrollPosition((float)(currentKeyboardSelection.YPosition!.Value - prevKeyboard.YPosition.Value));
        }

        #endregion

        #region Display handling

        private DisplayRange? displayedRange;

        private readonly CarouselItem carouselBoundsItem = new CarouselItem(new object());

        /// <summary>
        /// The position of the lower visible bound with respect to the current scroll position.
        /// </summary>
        private float visibleBottomBound;

        /// <summary>
        /// The position of the upper visible bound with respect to the current scroll position.
        /// </summary>
        private float visibleUpperBound;

        /// <summary>
        /// Half the height of the visible content.
        /// </summary>
        private float visibleHalfHeight;

        /// <summary>
        /// Whether existing panels can be re-used in the next filter.
        /// </summary>
        private readonly Cached filterReusesPanels = new Cached();

        /// <summary>
        /// Scrolling to selection relies on <see cref="currentKeyboardSelection"/> being fully populated.
        /// This flag ensures it runs after <see cref="refreshAfterSelection"/> validates this.
        /// </summary>
        private readonly Cached scrollToSelection = new Cached();

        protected override void Update()
        {
            base.Update();

            if (carouselItems == null)
                return;

            visibleBottomBound = (float)(Scroll.Current + DrawHeight + BleedBottom);
            visibleUpperBound = (float)(Scroll.Current - BleedTop);
            visibleHalfHeight = (DrawHeight + BleedBottom + BleedTop) / 2;

            if (!selectionValid.IsValid)
            {
                refreshAfterSelection();

                // Always scroll to selection in this case (regardless of `UserScrolling` state), centering the selection.
                ScrollToSelection();

                selectionValid.Validate();
            }

            var range = getDisplayRange();

            if (range != displayedRange)
            {
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

                float normalisedDepth = (float)(Math.Abs(selectedYPos - c.Item.CarouselYPosition) / DrawHeight);
                Scroll.Panels.ChangeChildDepth(panel, c.Item.DepthLayer + normalisedDepth);

                if (c.DrawYPosition != c.Item.CarouselYPosition)
                    c.DrawYPosition = Interpolation.DampContinuously(c.DrawYPosition, c.Item.CarouselYPosition, 50, Time.Elapsed);

                panel.X = GetPanelXOffset(panel);

                c.Selected.Value = currentSelection?.CarouselItem != null && CheckModelEquality(c.Item, currentSelection.CarouselItem);
                c.KeyboardSelected.Value = c.Item == currentKeyboardSelection?.CarouselItem;
                c.Expanded.Value = c.Item.IsExpanded;
            }

            if (!filterAfterItemsChanged.IsValid && !IsFiltering)
                FilterAsync();
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (!scrollToSelection.IsValid)
            {
                if (currentKeyboardSelection.YPosition != null)
                    Scroll.ScrollTo(currentKeyboardSelection.YPosition.Value - visibleHalfHeight + BleedTop);

                scrollToSelection.Validate();
            }
        }

        protected virtual float GetPanelXOffset(Drawable panel)
        {
            Vector2 posInScroll = Scroll.ToLocalSpace(panel.ScreenSpaceDrawQuad.Centre);
            float dist = Math.Abs(1f - (posInScroll.Y + BleedTop) / visibleHalfHeight);

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

            if (carouselItems.Count == 0)
                return DisplayRange.EMPTY;

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

            List<CarouselItem> toDisplay = range == DisplayRange.EMPTY
                ? new List<CarouselItem>()
                : carouselItems.GetRange(range.First, range.Last - range.First + 1);

            toDisplay.RemoveAll(i => !i.IsVisible);

            // Iterate over all panels which are already displayed and figure which need to be displayed / removed.
            foreach (var panel in Scroll.Panels)
            {
                var carouselPanel = (ICarouselPanel)panel;

                if (carouselPanel.Item == null)
                {
                    // Item is null when a panel is already fading away from existence; should be ignored for tracking purposes.
                    continue;
                }

                var existing = toDisplay.FirstOrDefault(i => CheckModelEquality(i.Model, carouselPanel.Item!.Model));

                if (existing != null)
                {
                    carouselPanel.Item = existing;
                    toDisplay.Remove(existing);
                    continue;
                }

                // If the new display range doesn't contain the panel, it's no longer required for display.
                expirePanel(panel);
            }

            // Add any new items which need to be displayed and haven't yet.
            foreach (var item in toDisplay)
            {
                var drawable = GetDrawableForDisplay(item);

                if (drawable is not ICarouselPanel carouselPanel)
                    throw new InvalidOperationException($"Carousel panel drawables must implement {typeof(ICarouselPanel)}");

                carouselPanel.Item = item;
                carouselPanel.DrawYPosition = item.CarouselYPosition;

                Scroll.Add(drawable);
            }

            if (toDisplay.Any())
            {
                // To make transitions of items appearing in the flow look good, do a pass and make sure newly added items spawn from
                // just beneath the *current interpolated position* of the previous panel.
                var orderedPanels = Scroll.Panels
                                          .Where(p => Scroll.ScreenSpaceDrawQuad.Intersects(p.ScreenSpaceDrawQuad))
                                          .OfType<ICarouselPanel>()
                                          .Where(p => p.Item != null)
                                          .OrderBy(p => p.Item!.CarouselYPosition)
                                          .ToList();

                for (int i = 0; i < orderedPanels.Count; i++)
                {
                    var panel = orderedPanels[i];

                    if (toDisplay.Contains(panel.Item!))
                    {
                        // Don't apply to the last because animating the tail of the list looks bad.
                        // It's usually off-screen anyway.
                        if (i > 0 && i < orderedPanels.Count - 1)
                            panel.DrawYPosition = orderedPanels[i - 1].DrawYPosition;
                    }
                }
            }
        }

        private void expirePanel(Drawable panel)
        {
            var carouselPanel = (ICarouselPanel)panel;

            // expired panels should have a depth behind all other panels to make the transition not look weird.
            Scroll.Panels.ChangeChildDepth(panel, panel.Depth + 1024);

            panel.FadeOut(150, Easing.OutQuint);
            panel.MoveToX(panel.X + 100, 200, Easing.Out);

            panel.Expire();

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

        private record DisplayRange(int First, int Last)
        {
            public static readonly DisplayRange EMPTY = new DisplayRange(-1, -1);
        }

        #endregion
    }
}
