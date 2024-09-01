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
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Parts;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    internal partial class TimelineBlueprintContainer : EditorBlueprintContainer
    {
        [Resolved(CanBeNull = true)]
        private Timeline timeline { get; set; }

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

                placementBlueprint.Colour = OsuColour.Gray(0.9f);

                // TODO: this is out of order, causing incorrect stacking height.
                SelectionBlueprints.Add(placementBlueprint);
            }
        }

        protected override Container<SelectionBlueprint<HitObject>> CreateSelectionBlueprintContainer() => new TimelineSelectionBlueprintContainer { RelativeSizeAxes = Axes.Both };

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (!base.ReceivePositionalInputAt(e.ScreenSpaceMouseDownPosition))
                return false;

            return base.OnDragStart(e);
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

            updateStacking();
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

        protected override void UpdateSelectionFromDragBox()
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

        protected partial class TimelineSelectionBlueprintContainer : Container<SelectionBlueprint<HitObject>>
        {
            protected override Container<SelectionBlueprint<HitObject>> Content { get; }

            public TimelineSelectionBlueprintContainer()
            {
                AddInternal(new TimelinePart<SelectionBlueprint<HitObject>>(Content = new HitObjectOrderedSelectionContainer { RelativeSizeAxes = Axes.Both }) { RelativeSizeAxes = Axes.Both });
            }
        }
    }
}
