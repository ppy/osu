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
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public class TimelineHitObjectBlueprint : SelectionBlueprint
    {
        private readonly Circle circle;

        private readonly Container extensionBar;

        protected override bool ShouldBeConsideredForInput(Drawable child) => true;

        [UsedImplicitly]
        private readonly Bindable<double> startTime;

        public Action<DragEvent> OnDragHandled;

        public const float THICKNESS = 5;

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

            if (hitObject is IHasEndTime)
            {
                AddRangeInternal(new Drawable[]
                {
                    extensionBar = new Container
                    {
                        CornerRadius = 2,
                        Masking = true,
                        Size = new Vector2(1, THICKNESS),
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativePositionAxes = Axes.X,
                        RelativeSizeAxes = Axes.X,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        }
                    },
                    new DragBar(hitObject) { OnDragHandled = e => OnDragHandled?.Invoke(e) }
                });
            }
        }

        public class DragBar : CompositeDrawable
        {
            private readonly HitObject hitObject;

            [Resolved]
            private Timeline timeline { get; set; }

            public Action<DragEvent> OnDragHandled;

            public DragBar(HitObject hitObject)
            {
                this.hitObject = hitObject;

                CornerRadius = 2;
                Masking = true;
                Size = new Vector2(THICKNESS, 1.5f);
                Anchor = Anchor.CentreRight;
                Origin = Anchor.CentreRight;
                RelativePositionAxes = Axes.X;
                RelativeSizeAxes = Axes.Y;
                InternalChild = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                };
            }

            protected override bool OnHover(HoverEvent e)
            {
                updateState();

                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                updateState();
                base.OnHoverLost(e);
            }

            private bool hasMouseDown;

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                hasMouseDown = true;
                updateState();
                return true;
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                hasMouseDown = false;
                updateState();
                base.OnMouseUp(e);
            }

            private void updateState()
            {
                if (IsHovered || hasMouseDown)
                    Colour = Color4.Orange;
                else
                {
                    Colour = Color4.White;
                }
            }

            protected override bool OnDragStart(DragStartEvent e) => true;

            [Resolved]
            private EditorBeatmap beatmap { get; set; }

            protected override void OnDrag(DragEvent e)
            {
                base.OnDrag(e);

                OnDragHandled?.Invoke(e);

                var time = timeline.GetTimeFromScreenSpacePosition(e.ScreenSpaceMousePosition);

                switch (hitObject)
                {
                    case IHasRepeats repeatHitObject:
                        // find the number of repeats which can fit in the requested time.
                        var lengthOfOneRepeat = repeatHitObject.Duration / (repeatHitObject.RepeatCount + 1);
                        var proposedCount = (int)((time - hitObject.StartTime) / lengthOfOneRepeat) - 1;

                        if (proposedCount == repeatHitObject.RepeatCount || proposedCount < 0)
                            return;

                        repeatHitObject.RepeatCount = proposedCount;
                        break;

                    case IHasEndTime endTimeHitObject:
                        endTimeHitObject.EndTime = time;
                        break;
                }

                beatmap.UpdateHitObject(hitObject);
            }

            protected override void OnDragEnd(DragEndEvent e)
            {
                base.OnDragEnd(e);

                OnDragHandled?.Invoke(null);
            }
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
                extensionBar.BorderColour = Color4.Orange;
        }

        protected override void OnDeselected()
        {
            circle.BorderColour = Color4.Black;
            if (extensionBar != null)
                extensionBar.BorderColour = Color4.Black;
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
