// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Parts;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    [Cached]
    internal partial class TimelineBlueprintContainer : EditorBlueprintContainer
    {
        [Resolved(CanBeNull = true)]
        private Timeline timeline { get; set; }

        [Resolved(CanBeNull = true)]
        private EditorClock editorClock { get; set; }

        private Bindable<HitObject> placement;
        private SelectionBlueprint<HitObject> placementBlueprint;

        private bool hitObjectDragged;

        /// <remarks>
        /// Positional input must be received outside the container's bounds,
        /// in order to handle timeline blueprints which are stacked offscreen.
        /// </remarks>
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => timeline.ReceivePositionalInputAt(screenSpacePos);

        public TimelineBlueprintContainer(HitObjectComposer composer)
            : base(composer)
        {
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Height = 0.6f;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(new SelectableAreaBackground
            {
                Colour = Color4.Black,
                Depth = float.MaxValue,
                Blending = BlendingParameters.Additive,
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            placement = Beatmap.PlacementObject.GetBoundCopy();
            placement.ValueChanged += placementChanged;
        }

        private void placementChanged(ValueChangedEvent<HitObject> obj)
        {
            if (obj.NewValue == null)
            {
                if (placementBlueprint != null)
                {
                    SelectionBlueprints.Remove(placementBlueprint, true);
                    placementBlueprint = null;
                }
            }
            else
            {
                placementBlueprint = CreateBlueprintFor(obj.NewValue).AsNonNull();

                // just to show the border. using the selection state doesn't seem to backfire.
                // if it does then we'll probably want to just make `new` object above rather than rely on `CreateBlueprintFor`.
                placementBlueprint.State = SelectionState.Selected;

                // TODO: this is out of order, causing incorrect stacking height.
                SelectionBlueprints.Add(placementBlueprint);
            }
        }

        protected override SelectionBlueprintContainer CreateSelectionBlueprintContainer() => new TimelineSelectionBlueprintContainer { RelativeSizeAxes = Axes.Both };

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (!base.ReceivePositionalInputAt(e.ScreenSpaceMouseDownPosition))
                return false;

            return base.OnDragStart(e);
        }

        protected override bool TryMoveBlueprints(DragEvent e, IList<(SelectionBlueprint<HitObject> blueprint, Vector2[] originalSnapPositions)> blueprints)
        {
            Vector2 distanceTravelled = e.ScreenSpaceMousePosition - e.ScreenSpaceMouseDownPosition;

            // The final movement position, relative to movementBlueprintOriginalPosition.
            Vector2 movePosition = blueprints.First().originalSnapPositions.First() + distanceTravelled;

            // Retrieve a snapped position.
            var result = timeline?.FindSnappedPositionAndTime(movePosition) ?? new SnapResult(movePosition, null);

            var referenceBlueprint = blueprints.First().blueprint;
            bool moved = SelectionHandler.HandleMovement(new MoveSelectionEvent<HitObject>(referenceBlueprint, result.ScreenSpacePosition - referenceBlueprint.ScreenSpaceSelectionPoint));
            if (moved)
                ApplySnapResultTime(result, referenceBlueprint.Item.StartTime);
            return moved;
        }

        private float dragTimeAccumulated;

        protected override void Update()
        {
            if (IsDragged || hitObjectDragged)
                handleScrollViaDrag();
            else
                dragTimeAccumulated = 0;

            if (Composer != null && timeline != null)
            {
                Composer.Playfield.PastLifetimeExtension = timeline.VisibleRange / 2;
                Composer.Playfield.FutureLifetimeExtension = timeline.VisibleRange / 2;
            }

            base.Update();

            updateSamplePointContractedState();
            updateStacking();
        }

        public Bindable<bool> SamplePointContracted = new Bindable<bool>();

        private void updateSamplePointContractedState()
        {
            const double absolute_minimum_gap = 31; // assumes single letter bank name for default banks
            double minimumGap = absolute_minimum_gap;

            if (timeline == null || editorClock == null)
                return;

            // Find the smallest time gap between any two sample point pieces
            double smallestTimeGap = double.PositiveInfinity;
            double lastTime = double.PositiveInfinity;

            // The blueprints are ordered in reverse chronological order
            foreach (var selectionBlueprint in SelectionBlueprints)
            {
                var hitObject = selectionBlueprint.Item;

                // Only check the hit objects which are visible in the timeline
                // SelectionBlueprints can contain hit objects which are not visible in the timeline due to selection keeping them alive
                if (hitObject.StartTime > editorClock.CurrentTime + timeline.VisibleRange / 2)
                    continue;

                if (hitObject.GetEndTime() < editorClock.CurrentTime - timeline.VisibleRange / 2)
                    break;

                for (int i = 0; i < hitObject.Samples.Count; i++)
                {
                    var sample = hitObject.Samples[i];

                    if (!HitSampleInfo.ALL_BANKS.Contains(sample.Bank))
                        minimumGap = Math.Max(minimumGap, absolute_minimum_gap + sample.Bank.Length * 3);
                }

                if (hitObject is IHasRepeats hasRepeats)
                {
                    smallestTimeGap = Math.Min(smallestTimeGap, hasRepeats.Duration / hasRepeats.SpanCount() / 2);

                    for (int i = 0; i < hasRepeats.NodeSamples.Count; i++)
                    {
                        var node = hasRepeats.NodeSamples[i];

                        for (int j = 0; j < node.Count; j++)
                        {
                            var sample = node[j];

                            if (!HitSampleInfo.ALL_BANKS.Contains(sample.Bank))
                                minimumGap = Math.Max(minimumGap, absolute_minimum_gap + sample.Bank.Length * 3);
                        }
                    }
                }

                double gap = lastTime - hitObject.GetEndTime();

                // If the gap is less than 1ms, we can assume that the objects are stacked on top of each other
                // Contracting doesn't make sense in this case
                if (gap > 1 && gap < smallestTimeGap)
                    smallestTimeGap = gap;

                lastTime = hitObject.StartTime;
            }

            double smallestAbsoluteGap = ((TimelineSelectionBlueprintContainer)SelectionBlueprints).ContentRelativeToAbsoluteFactor.X * smallestTimeGap;
            SamplePointContracted.Value = smallestAbsoluteGap < minimumGap;
        }

        private readonly Stack<HitObject> currentConcurrentObjects = new Stack<HitObject>();

        private void updateStacking()
        {
            // because only blueprints of objects which are alive (via pooling) are displayed in the timeline, it's feasible to do this every-update.

            const int stack_offset = 5;

            // after the stack gets this tall, we can presume there is space underneath to draw subsequent blueprints.
            const int stack_reset_count = 3;

            currentConcurrentObjects.Clear();

            for (int i = SelectionBlueprints.Count - 1; i >= 0; i--)
            {
                var b = SelectionBlueprints[i];

                // remove objects from the stack as long as their end time is in the past.
                while (currentConcurrentObjects.TryPeek(out HitObject hitObject))
                {
                    if (Precision.AlmostBigger(hitObject.GetEndTime(), b.Item.StartTime, 1))
                        break;

                    currentConcurrentObjects.Pop();
                }

                // if the stack gets too high, we should have space below it to display the next batch of objects.
                // importantly, we only do this if time has incremented, else a stack of hitobjects all at the same time value would start to overlap themselves.
                if (currentConcurrentObjects.TryPeek(out HitObject h) && !Precision.AlmostEquals(h.StartTime, b.Item.StartTime, 1))
                {
                    if (currentConcurrentObjects.Count >= stack_reset_count)
                        currentConcurrentObjects.Clear();
                }

                b.Y = -(stack_offset * currentConcurrentObjects.Count);

                currentConcurrentObjects.Push(b.Item);
            }
        }

        protected override SelectionHandler<HitObject> CreateSelectionHandler() => new TimelineSelectionHandler();

        protected override SelectionBlueprint<HitObject> CreateBlueprintFor(HitObject item)
        {
            return new TimelineHitObjectBlueprint(item)
            {
                OnDragHandled = e => hitObjectDragged = e != null,
            };
        }

        protected sealed override DragBox CreateDragBox() => new TimelineDragBox();

        protected override void UpdateSelectionFromDragBox(HashSet<HitObject> selectionBeforeDrag)
        {
            Composer.BlueprintContainer.CommitIfPlacementActive();

            var dragBox = (TimelineDragBox)DragBox;
            double minTime = dragBox.MinTime;
            double maxTime = dragBox.MaxTime;

            SelectedItems.RemoveAll(hitObject => !shouldBeSelected(hitObject));

            foreach (var hitObject in Beatmap.HitObjects.Except(SelectedItems).Where(shouldBeSelected))
            {
                Composer.Playfield.SetKeepAlive(hitObject, true);
                SelectedItems.Add(hitObject);
            }

            bool shouldBeSelected(HitObject hitObject)
            {
                if (selectionBeforeDrag.Contains(hitObject))
                    return true;

                double midTime = (hitObject.StartTime + hitObject.GetEndTime()) / 2;
                return minTime <= midTime && midTime <= maxTime;
            }
        }

        private void handleScrollViaDrag()
        {
            // The amount of time dragging before we reach maximum drag speed.
            const float time_ramp_multiplier = 5000;

            // A maximum drag speed to ensure things don't get out of hand.
            const float max_velocity = 10;

            if (timeline == null) return;

            var mousePos = timeline.ToLocalSpace(InputManager.CurrentState.Mouse.Position);

            // for better UX do not require the user to drag all the way to the edge and beyond to initiate a drag-scroll.
            // this is especially important in scenarios like fullscreen, where mouse confine will usually be on
            // and the user physically *won't be able to* drag beyond the edge of the timeline
            // (since its left edge is co-incident with the window edge).
            const float scroll_tolerance = 40;

            float leftBound = timeline.BoundingBox.TopLeft.X + scroll_tolerance;
            float rightBound = timeline.BoundingBox.TopRight.X - scroll_tolerance;

            float amount = 0;

            if (mousePos.X > rightBound)
                amount = mousePos.X - rightBound;
            else if (mousePos.X < leftBound)
                amount = mousePos.X - leftBound;

            if (amount == 0)
            {
                dragTimeAccumulated = 0;
                return;
            }

            amount = Math.Sign(amount) * Math.Min(max_velocity, MathF.Pow(Math.Clamp(Math.Abs(amount), 0, scroll_tolerance), 2));
            dragTimeAccumulated += (float)Clock.ElapsedFrameTime;

            timeline.ScrollBy(amount * (float)Clock.ElapsedFrameTime * Math.Min(1, dragTimeAccumulated / time_ramp_multiplier));
        }

        private partial class SelectableAreaBackground : CompositeDrawable
        {
            [Resolved]
            private OsuColour colours { get; set; }

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
            {
                float localY = ToLocalSpace(screenSpacePos).Y;
                return DrawRectangle.Top <= localY && DrawRectangle.Bottom >= localY;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                RelativeSizeAxes = Axes.Both;
                Alpha = 0.1f;

                AddRangeInternal(new[]
                {
                    // fade out over intro time, outside the valid time bounds.
                    new Box
                    {
                        RelativeSizeAxes = Axes.Y,
                        Width = 200,
                        Origin = Anchor.TopRight,
                        Colour = ColourInfo.GradientHorizontal(Color4.White.Opacity(0), Color4.White),
                    },
                    new Box
                    {
                        Colour = Color4.White,
                        RelativeSizeAxes = Axes.Both,
                    }
                });
            }

            protected override bool OnHover(HoverEvent e)
            {
                this.FadeColour(colours.BlueLighter, 120, Easing.OutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                this.FadeColour(Color4.Black, 600, Easing.OutQuint);
                base.OnHoverLost(e);
            }
        }

        protected partial class TimelineSelectionBlueprintContainer : SelectionBlueprintContainer
        {
            protected override HitObjectOrderedSelectionContainer Content { get; }

            public Vector2 ContentRelativeToAbsoluteFactor => Content.RelativeToAbsoluteFactor;

            public TimelineSelectionBlueprintContainer()
            {
                AddInternal(new TimelinePart<SelectionBlueprint<HitObject>>(Content = new HitObjectOrderedSelectionContainer { RelativeSizeAxes = Axes.Both }) { RelativeSizeAxes = Axes.Both });
            }

            public override void ChangeChildDepth(SelectionBlueprint<HitObject> child, float newDepth)
            {
                // timeline blueprint container also contains a blueprint for current placement, if present
                // (see `placementChanged()` callback above).
                // because the current placement hitobject is generally going to be mutated during the placement,
                // it is possible for `Content`'s children to become unsorted when the user moves the placement around,
                // which can culminate in a critical failure when attempting to binary-search children here
                // using `HitObjectOrderedSelectionContainer`'s custom comparer.
                // thus, always force a re-sort of objects before attempting to change child depth to avoid this scenario.
                Content.Sort();
                base.ChangeChildDepth(child, newDepth);
            }
        }
    }
}
