// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Parts;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    internal class TimelineHitObjectDisplay : BlueprintContainer
    {
        private EditorBeatmap beatmap { get; }

        private readonly TimelinePart content;

        public TimelineHitObjectDisplay(EditorBeatmap beatmap)
        {
            RelativeSizeAxes = Axes.Both;

            this.beatmap = beatmap;

            AddInternal(content = new TimelinePart { RelativeSizeAxes = Axes.Both });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            foreach (var h in beatmap.HitObjects)
                add(h);

            beatmap.HitObjectAdded += add;
            beatmap.HitObjectRemoved += remove;
            beatmap.StartTimeChanged += h =>
            {
                remove(h);
                add(h);
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            DragBox.Alpha = 0;
        }

        private void remove(HitObject h)
        {
            foreach (var d in content.OfType<TimelineHitObjectRepresentation>().Where(c => c.HitObject == h))
                d.Expire();
        }

        private void add(HitObject h)
        {
            var yOffset = content.Count(d => d.X == h.StartTime);

            content.Add(new TimelineHitObjectRepresentation(h) { Y = -yOffset * TimelineHitObjectRepresentation.THICKNESS });
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            base.OnMouseDown(e);

            return false; // tempoerary until we correctly handle selections.
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

        private class TimelineHitObjectRepresentation : CompositeDrawable
        {
            public const float THICKNESS = 3;

            public readonly HitObject HitObject;

            public TimelineHitObjectRepresentation(HitObject hitObject)
            {
                HitObject = hitObject;
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