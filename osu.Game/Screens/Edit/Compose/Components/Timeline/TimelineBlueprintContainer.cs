// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Parts;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    internal class TimelineBlueprintContainer : BlueprintContainer
    {
        [Resolved(CanBeNull = true)]
        private Timeline timeline { get; set; }

        private DragEvent lastDragEvent;

        public TimelineBlueprintContainer(EditorBeatmap beatmap)
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

        protected override SelectionBlueprintContainer CreateSelectionBlueprintContainer() => new TimelineSelectionBlueprintContainer { RelativeSizeAxes = Axes.Both };

        protected override void LoadComplete()
        {
            base.LoadComplete();
            DragBox.Alpha = 0;
        }

        protected override void OnDrag(DragEvent e)
        {
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

            base.OnDrag(e);
            lastDragEvent = e;
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            base.OnDragEnd(e);
            lastDragEvent = null;
        }

        protected override void Update()
        {
            // trigger every frame so drags continue to update selection while playback is scrolling the timeline.
            if (IsDragged)
                OnDrag(lastDragEvent);

            base.Update();
        }

        protected override SelectionHandler CreateSelectionHandler() => new TimelineSelectionHandler();

        internal class TimelineSelectionHandler : SelectionHandler
        {
            public override bool HandleMovement(MoveSelectionEvent moveEvent) => true;
        }

        protected override SelectionBlueprint CreateBlueprintFor(HitObject hitObject) => new TimelineHitObjectBlueprint(hitObject);

        protected override DragBox CreateDragBox(Action<RectangleF> performSelect) => new TimelineDragBox(performSelect);

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

        protected class TimelineSelectionBlueprintContainer : SelectionBlueprintContainer
        {
            protected override Container<SelectionBlueprint> Content { get; }

            public TimelineSelectionBlueprintContainer()
            {
                AddInternal(new TimelinePart<SelectionBlueprint>(Content = new Container<SelectionBlueprint> { RelativeSizeAxes = Axes.Both }) { RelativeSizeAxes = Axes.Both });
            }
        }

        private class TimelineHitObjectBlueprint : SelectionBlueprint
        {
            private readonly Circle circle;

            private readonly Container extensionBar;

            [UsedImplicitly]
            private readonly Bindable<double> startTime;

            public const float THICKNESS = 3;

            private const float circle_size = 16;

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => base.ReceivePositionalInputAt(screenSpacePos) || circle.ReceivePositionalInputAt(screenSpacePos);

            public TimelineHitObjectBlueprint(HitObject hitObject)
                : base(hitObject)
            {
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;

                startTime = hitObject.StartTimeBindable.GetBoundCopy();
                startTime.BindValueChanged(time => X = (float)time.NewValue, true);

                RelativePositionAxes = Axes.X;

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                if (hitObject is IHasEndTime)
                {
                    AddInternal(extensionBar = new Container
                    {
                        CornerRadius = 2,
                        Masking = true,
                        Size = new Vector2(1, THICKNESS),
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativePositionAxes = Axes.X,
                        RelativeSizeAxes = Axes.X,
                        Colour = Color4.Black,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        }
                    });
                }

                AddInternal(circle = new Circle
                {
                    Size = new Vector2(circle_size),
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.Centre,
                    RelativePositionAxes = Axes.X,
                    AlwaysPresent = true,
                    Colour = Color4.White,
                    BorderColour = Color4.Black,
                    BorderThickness = THICKNESS,
                });
            }

            protected override void Update()
            {
                base.Update();

                // no bindable so we perform this every update
                Width = (float)(HitObject.GetEndTime() - HitObject.StartTime);
            }

            protected override void OnSelected()
            {
                circle.BorderColour = Color4.Orange;
                if (extensionBar != null)
                    extensionBar.Colour = Color4.Orange;
            }

            protected override void OnDeselected()
            {
                circle.BorderColour = Color4.Black;
                if (extensionBar != null)
                    extensionBar.Colour = Color4.Black;
            }

            public override Quad SelectionQuad
            {
                get
                {
                    // correctly include the circle in the selection quad region, as it is usually outside the blueprint itself.
                    var circleQuad = circle.ScreenSpaceDrawQuad;
                    var actualQuad = ScreenSpaceDrawQuad;

                    return new Quad(circleQuad.TopLeft, Vector2.ComponentMax(actualQuad.TopRight, circleQuad.TopRight),
                        circleQuad.BottomLeft, Vector2.ComponentMax(actualQuad.BottomRight, circleQuad.BottomRight));
                }
            }

            public override Vector2 SelectionPoint => ScreenSpaceDrawQuad.TopLeft;
        }
    }
}
