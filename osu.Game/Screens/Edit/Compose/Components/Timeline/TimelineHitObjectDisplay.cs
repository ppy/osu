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
            var yOffset = 0;

            return new TimelineHitObjectRepresentation(hitObject) { Y = -yOffset * TimelineHitObjectRepresentation.THICKNESS };
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            base.OnMouseDown(e);

            return false; // temporary until we correctly handle selections.
        }

        protected override DragBox CreateDragBox(Action<RectangleF> performSelect) => new NoDragDragBox(performSelect);

        internal class NoDragDragBox : DragBox
        {
            public NoDragDragBox(Action<RectangleF> performSelect)
                : base(performSelect)
            {
            }

            public override bool UpdateDrag(MouseButtonEvent e) => false;
        }

        private class TimelineHitObjectRepresentation : SelectionBlueprint
        {
            public const float THICKNESS = 3;

            public TimelineHitObjectRepresentation(HitObject hitObject)
                : base(hitObject)
            {
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;

                Width = (float)(hitObject.GetEndTime() - hitObject.StartTime);

                X = (float)hitObject.StartTime;

                RelativePositionAxes = Axes.X;
                RelativeSizeAxes = Axes.X;

                if (hitObject is IHasEndTime)
                {
                    AddInternal(new Container
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

                AddInternal(new Circle
                {
                    Size = new Vector2(16),
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.Centre,
                    RelativePositionAxes = Axes.X,
                    AlwaysPresent = true,
                    Colour = Color4.White,
                    BorderColour = Color4.Black,
                    BorderThickness = THICKNESS,
                });
            }
        }
    }
}
