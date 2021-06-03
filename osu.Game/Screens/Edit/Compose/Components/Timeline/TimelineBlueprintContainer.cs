// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Parts;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    internal class TimelineBlueprintContainer : EditorBlueprintContainer
    {
        [Resolved(CanBeNull = true)]
        private Timeline timeline { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        private DragEvent lastDragEvent;
        private Bindable<HitObject> placement;
        private SelectionBlueprint<HitObject> placementBlueprint;

        private SelectableAreaBackground backgroundBox;

        // we only care about checking vertical validity.
        // this allows selecting and dragging selections before time=0.
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
        {
            float localY = ToLocalSpace(screenSpacePos).Y;
            return DrawRectangle.Top <= localY && DrawRectangle.Bottom >= localY;
        }

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
            AddInternal(backgroundBox = new SelectableAreaBackground
            {
                Colour = Color4.Black,
                Depth = float.MaxValue,
                Blending = BlendingParameters.Additive,
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            DragBox.Alpha = 0;

            placement = Beatmap.PlacementObject.GetBoundCopy();
            placement.ValueChanged += placementChanged;
        }

        private void placementChanged(ValueChangedEvent<HitObject> obj)
        {
            if (obj.NewValue == null)
            {
                if (placementBlueprint != null)
                {
                    SelectionBlueprints.Remove(placementBlueprint);
                    placementBlueprint = null;
                }
            }
            else
            {
                placementBlueprint = CreateBlueprintFor(obj.NewValue);

                placementBlueprint.Colour = Color4.MediumPurple;

                SelectionBlueprints.Add(placementBlueprint);
            }
        }

        protected override Container<SelectionBlueprint<HitObject>> CreateSelectionBlueprintContainer() => new TimelineSelectionBlueprintContainer { RelativeSizeAxes = Axes.Both };

        protected override bool OnHover(HoverEvent e)
        {
            backgroundBox.FadeColour(colours.BlueLighter, 120, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            backgroundBox.FadeColour(Color4.Black, 600, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        protected override void OnDrag(DragEvent e)
        {
            handleScrollViaDrag(e);

            base.OnDrag(e);
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            base.OnDragEnd(e);
            lastDragEvent = null;
        }

        protected override void Update()
        {
            // trigger every frame so drags continue to update selection while playback is scrolling the timeline.
            if (lastDragEvent != null)
                OnDrag(lastDragEvent);

            if (Composer != null && timeline != null)
            {
                Composer.Playfield.PastLifetimeExtension = timeline.VisibleRange / 2;
                Composer.Playfield.FutureLifetimeExtension = timeline.VisibleRange / 2;
            }

            base.Update();

            updateStacking();
        }

        private void updateStacking()
        {
            // because only blueprints of objects which are alive (via pooling) are displayed in the timeline, it's feasible to do this every-update.

            const int stack_offset = 5;

            // after the stack gets this tall, we can presume there is space underneath to draw subsequent blueprints.
            const int stack_reset_count = 3;

            Stack<HitObject> currentConcurrentObjects = new Stack<HitObject>();

            foreach (var b in SelectionBlueprints.Reverse())
            {
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
                OnDragHandled = handleScrollViaDrag
            };
        }

        protected override DragBox CreateDragBox(Action<RectangleF> performSelect) => new TimelineDragBox(performSelect);

        private void handleScrollViaDrag(DragEvent e)
        {
            lastDragEvent = e;

            if (lastDragEvent == null)
                return;

            if (timeline != null)
            {
                var timelineQuad = timeline.ScreenSpaceDrawQuad;
                var mouseX = e.ScreenSpaceMousePosition.X;

                // scroll if in a drag and dragging outside visible extents
                if (mouseX > timelineQuad.TopRight.X)
                    timeline.ScrollBy((float)((mouseX - timelineQuad.TopRight.X) / 10 * Clock.ElapsedFrameTime));
                else if (mouseX < timelineQuad.TopLeft.X)
                    timeline.ScrollBy((float)((mouseX - timelineQuad.TopLeft.X) / 10 * Clock.ElapsedFrameTime));
            }
        }

        private class SelectableAreaBackground : CompositeDrawable
        {
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
        }

        internal class TimelineSelectionHandler : EditorSelectionHandler, IKeyBindingHandler<GlobalAction>
        {
            // for now we always allow movement. snapping is provided by the Timeline's "distance" snap implementation
            public override bool HandleMovement(MoveSelectionEvent<HitObject> moveEvent) => true;

            public bool OnPressed(GlobalAction action)
            {
                switch (action)
                {
                    case GlobalAction.EditorNudgeLeft:
                        nudgeSelection(-1);
                        return true;

                    case GlobalAction.EditorNudgeRight:
                        nudgeSelection(1);
                        return true;
                }

                return false;
            }

            public void OnReleased(GlobalAction action)
            {
            }

            /// <summary>
            /// Nudge the current selection by the specified multiple of beat divisor lengths,
            /// based on the timing at the first object in the selection.
            /// </summary>
            /// <param name="amount">The direction and count of beat divisor lengths to adjust.</param>
            private void nudgeSelection(int amount)
            {
                var selected = EditorBeatmap.SelectedHitObjects;

                if (selected.Count == 0)
                    return;

                var timingPoint = EditorBeatmap.ControlPointInfo.TimingPointAt(selected.First().StartTime);
                double adjustment = timingPoint.BeatLength / EditorBeatmap.BeatDivisor * amount;

                EditorBeatmap.PerformOnSelection(h =>
                {
                    h.StartTime += adjustment;
                    EditorBeatmap.Update(h);
                });
            }
        }

        private class TimelineDragBox : DragBox
        {
            // the following values hold the start and end X positions of the drag box in the timeline's local space,
            // but with zoom unapplied in order to be able to compensate for positional changes
            // while the timeline is being zoomed in/out.
            private float? selectionStart;
            private float selectionEnd;

            [Resolved]
            private Timeline timeline { get; set; }

            public TimelineDragBox(Action<RectangleF> performSelect)
                : base(performSelect)
            {
            }

            protected override Drawable CreateBox() => new Box
            {
                RelativeSizeAxes = Axes.Y,
                Alpha = 0.3f
            };

            public override bool HandleDrag(MouseButtonEvent e)
            {
                selectionStart ??= e.MouseDownPosition.X / timeline.CurrentZoom;

                // only calculate end when a transition is not in progress to avoid bouncing.
                if (Precision.AlmostEquals(timeline.CurrentZoom, timeline.Zoom))
                    selectionEnd = e.MousePosition.X / timeline.CurrentZoom;

                updateDragBoxPosition();
                return true;
            }

            private void updateDragBoxPosition()
            {
                if (selectionStart == null)
                    return;

                float rescaledStart = selectionStart.Value * timeline.CurrentZoom;
                float rescaledEnd = selectionEnd * timeline.CurrentZoom;

                Box.X = Math.Min(rescaledStart, rescaledEnd);
                Box.Width = Math.Abs(rescaledStart - rescaledEnd);

                var boxScreenRect = Box.ScreenSpaceDrawQuad.AABBFloat;

                // we don't care about where the hitobjects are vertically. in cases like stacking display, they may be outside the box without this adjustment.
                boxScreenRect.Y -= boxScreenRect.Height;
                boxScreenRect.Height *= 2;

                PerformSelection?.Invoke(boxScreenRect);
            }

            public override void Hide()
            {
                base.Hide();
                selectionStart = null;
            }
        }

        protected class TimelineSelectionBlueprintContainer : Container<SelectionBlueprint<HitObject>>
        {
            protected override Container<SelectionBlueprint<HitObject>> Content { get; }

            public TimelineSelectionBlueprintContainer()
            {
                AddInternal(new TimelinePart<SelectionBlueprint<HitObject>>(Content = new HitObjectOrderedSelectionContainer { RelativeSizeAxes = Axes.Both }) { RelativeSizeAxes = Axes.Both });
            }
        }
    }
}
