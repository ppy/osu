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
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Parts;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    internal class TimelineBlueprintContainer : BlueprintContainer
    {
        [Resolved(CanBeNull = true)]
        private Timeline timeline { get; set; }

        [Resolved]
        private EditorBeatmap beatmap { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        private DragEvent lastDragEvent;
        private Bindable<HitObject> placement;
        private SelectionBlueprint placementBlueprint;

        private readonly Box backgroundBox;

        public TimelineBlueprintContainer(HitObjectComposer composer)
            : base(composer)
        {
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Height = 0.6f;

            AddInternal(backgroundBox = new Box
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

                PerformSelection?.Invoke(Box.ScreenSpaceDrawQuad.AABBFloat);
            }

            public override void Hide()
            {
                base.Hide();
                selectionStart = null;
            }
        }

        protected class TimelineSelectionBlueprintContainer : Container<SelectionBlueprint>
        {
            protected override Container<SelectionBlueprint> Content { get; }

            public TimelineSelectionBlueprintContainer()
            {
                AddInternal(new TimelinePart<SelectionBlueprint>(Content = new HitObjectOrderedSelectionContainer { RelativeSizeAxes = Axes.Both }) { RelativeSizeAxes = Axes.Both });
            }
        }
    }
}
