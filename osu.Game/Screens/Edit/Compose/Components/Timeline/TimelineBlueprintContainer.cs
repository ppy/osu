// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Parts;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    internal class TimelineBlueprintContainer : BlueprintContainer
    {
        [Resolved(CanBeNull = true)]
        private Timeline timeline { get; set; }

        [Resolved]
        private EditorBeatmap beatmap { get; set; }

        private DragEvent lastDragEvent;

        private Bindable<HitObject> placement;

        private SelectionBlueprint placementBlueprint;

        public TimelineBlueprintContainer()
        {
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Height = 0.4f;

            AddInternal(new Box
            {
                Colour = Color4.Black,
                RelativeSizeAxes = Axes.Both,
                Alpha = 0.1f,
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            DragBox.Alpha = 0;

            placement = beatmap.PlacementObject.GetBoundCopy();
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

        protected override Container<SelectionBlueprint> CreateSelectionBlueprintContainer() => new TimelineSelectionBlueprintContainer { RelativeSizeAxes = Axes.Both };

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

            base.Update();
        }

        protected override SelectionHandler CreateSelectionHandler() => new TimelineSelectionHandler();

        protected override SelectionBlueprint CreateBlueprintFor(HitObject hitObject) => new TimelineHitObjectBlueprint(hitObject)
        {
            OnDragHandled = handleScrollViaDrag
        };

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

        internal class TimelineSelectionHandler : SelectionHandler
        {
            // for now we always allow movement. snapping is provided by the Timeline's "distance" snap implementation
            public override bool HandleMovement(MoveSelectionEvent moveEvent) => true;
        }

        private class TimelineDragBox : DragBox
        {
            private Vector2 lastMouseDown;
            private float localMouseDown;

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
                // store the original position of the mouse down, as we may be scrolled during selection.
                if (lastMouseDown != e.ScreenSpaceMouseDownPosition)
                {
                    lastMouseDown = e.ScreenSpaceMouseDownPosition;
                    localMouseDown = e.MouseDownPosition.X;
                }

                float selection1 = localMouseDown;
                float selection2 = e.MousePosition.X;

                Box.X = Math.Min(selection1, selection2);
                Box.Width = Math.Abs(selection1 - selection2);

                PerformSelection?.Invoke(Box.ScreenSpaceDrawQuad.AABBFloat);
                return true;
            }
        }

        protected class TimelineSelectionBlueprintContainer : Container<SelectionBlueprint>
        {
            protected override Container<SelectionBlueprint> Content { get; }

            public TimelineSelectionBlueprintContainer()
            {
                AddInternal(new TimelinePart<SelectionBlueprint>(Content = new Container<SelectionBlueprint> { RelativeSizeAxes = Axes.Both }) { RelativeSizeAxes = Axes.Both });
            }
        }
    }
}
