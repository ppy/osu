// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
    internal class TimelineHitObjectDisplay : BlueprintContainer
    {
        public TimelineHitObjectDisplay(EditorBeatmap beatmap)
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

        protected class TimelineSelectionBlueprintContainer : SelectionBlueprintContainer
        {
            protected override Container<SelectionBlueprint> Content { get; }

            public TimelineSelectionBlueprintContainer()
            {
                AddInternal(new TimelinePart<SelectionBlueprint>(Content = new Container<SelectionBlueprint> { RelativeSizeAxes = Axes.Both }) { RelativeSizeAxes = Axes.Both });
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            DragBox.Alpha = 0;
        }

        protected override SelectionBlueprint CreateBlueprintFor(HitObject hitObject)
        {
            //var yOffset = content.Count(d => d.X == h.StartTime);
            //var yOffset = 0;

            return new TimelineHitObjectRepresentation(hitObject);
        }

        protected override DragBox CreateDragBox(Action<RectangleF> performSelect) => new CustomDragBox(performSelect);

        internal class CustomDragBox : DragBox
        {
            public CustomDragBox(Action<RectangleF> performSelect)
                : base(performSelect)
            {
            }

            protected override Drawable CreateBox() => new Box
            {
                RelativeSizeAxes = Axes.Y,
                Alpha = 0.3f
            };

            public override bool UpdateDrag(MouseButtonEvent e)
            {
                float selection1 = e.MouseDownPosition.X;
                float selection2 = e.MousePosition.X;

                Box.X = Math.Min(selection1, selection2);
                Box.Width = Math.Abs(selection1 - selection2);

                PerformSelection?.Invoke(Box.ScreenSpaceDrawQuad.AABBFloat);
                return true;
            }
        }

        private class TimelineHitObjectRepresentation : SelectionBlueprint
        {
            private readonly Circle circle;

            private Container extensionBar;

            public const float THICKNESS = 3;

            private const float circle_size = 16;

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => base.ReceivePositionalInputAt(screenSpacePos) || circle.ReceivePositionalInputAt(screenSpacePos);

            public TimelineHitObjectRepresentation(HitObject hitObject)
                : base(hitObject)
            {
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;

                Width = (float)(hitObject.GetEndTime() - hitObject.StartTime);

                X = (float)hitObject.StartTime;

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
        }
    }
}
