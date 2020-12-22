// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class OsuSelectionHandler : SelectionHandler
    {
        protected override void OnSelectionChanged()
        {
            base.OnSelectionChanged();

            Quad quad = selectedMovableObjects.Length > 0 ? getSurroundingQuad(selectedMovableObjects) : new Quad();

            SelectionBox.CanRotate = quad.Width > 0 || quad.Height > 0;
            SelectionBox.CanScaleX = quad.Width > 0;
            SelectionBox.CanScaleY = quad.Height > 0;
            SelectionBox.CanReverse = EditorBeatmap.SelectedHitObjects.Count > 1 || EditorBeatmap.SelectedHitObjects.Any(s => s is Slider);
        }

        protected override void OnOperationEnded()
        {
            base.OnOperationEnded();
            referenceOrigin = null;
        }

        public override bool HandleMovement(MoveSelectionEvent moveEvent) =>
            moveSelection(moveEvent.InstantDelta);

        /// <summary>
        /// During a transform, the initial origin is stored so it can be used throughout the operation.
        /// </summary>
        private Vector2? referenceOrigin;

        public override bool HandleReverse()
        {
            var hitObjects = EditorBeatmap.SelectedHitObjects;

            double endTime = hitObjects.Max(h => h.GetEndTime());
            double startTime = hitObjects.Min(h => h.StartTime);

            bool moreThanOneObject = hitObjects.Count > 1;

            foreach (var h in hitObjects)
            {
                if (moreThanOneObject)
                    h.StartTime = endTime - (h.GetEndTime() - startTime);

                if (h is Slider slider)
                {
                    var points = slider.Path.ControlPoints.ToArray();
                    Vector2 endPos = points.Last().Position.Value;

                    slider.Path.ControlPoints.Clear();

                    slider.Position += endPos;

                    PathType? lastType = null;

                    for (var i = 0; i < points.Length; i++)
                    {
                        var p = points[i];
                        p.Position.Value -= endPos;

                        // propagate types forwards to last null type
                        if (i == points.Length - 1)
                            p.Type.Value = lastType;
                        else if (p.Type.Value != null)
                        {
                            var newType = p.Type.Value;
                            p.Type.Value = lastType;
                            lastType = newType;
                        }

                        slider.Path.ControlPoints.Insert(0, p);
                    }
                }
            }

            return true;
        }

        public override bool HandleFlip(Direction direction)
        {
            var hitObjects = selectedMovableObjects;

            var selectedObjectsQuad = getSurroundingQuad(hitObjects);
            var centre = selectedObjectsQuad.Centre;

            foreach (var h in hitObjects)
            {
                var pos = h.Position;

                switch (direction)
                {
                    case Direction.Horizontal:
                        pos.X = centre.X - (pos.X - centre.X);
                        break;

                    case Direction.Vertical:
                        pos.Y = centre.Y - (pos.Y - centre.Y);
                        break;
                }

                h.Position = pos;

                if (h is Slider slider)
                {
                    foreach (var point in slider.Path.ControlPoints)
                    {
                        point.Position.Value = new Vector2(
                            (direction == Direction.Horizontal ? -1 : 1) * point.Position.Value.X,
                            (direction == Direction.Vertical ? -1 : 1) * point.Position.Value.Y
                        );
                    }
                }
            }

            return true;
        }

        public override bool HandleScale(Vector2 scale, Anchor reference)
        {
            adjustScaleFromAnchor(ref scale, reference);

            var hitObjects = selectedMovableObjects;

            // for the time being, allow resizing of slider paths only if the slider is
            // the only hit object selected. with a group selection, it's likely the user
            // is not looking to change the duration of the slider but expand the whole pattern.
            if (hitObjects.Length == 1 && hitObjects.First() is Slider slider)
            {
                Quad quad = getSurroundingQuad(slider.Path.ControlPoints.Select(p => p.Position.Value));
                Vector2 pathRelativeDeltaScale = new Vector2(1 + scale.X / quad.Width, 1 + scale.Y / quad.Height);

                foreach (var point in slider.Path.ControlPoints)
                    point.Position.Value *= pathRelativeDeltaScale;
            }
            else
            {
                // move the selection before scaling if dragging from top or left anchors.
                if ((reference & Anchor.x0) > 0 && !moveSelection(new Vector2(-scale.X, 0))) return false;
                if ((reference & Anchor.y0) > 0 && !moveSelection(new Vector2(0, -scale.Y))) return false;

                Quad quad = getSurroundingQuad(hitObjects);

                foreach (var h in hitObjects)
                {
                    var newPosition = h.Position;

                    // guard against no-ops and NaN.
                    if (scale.X != 0 && quad.Width > 0)
                        newPosition.X = quad.TopLeft.X + (h.X - quad.TopLeft.X) / quad.Width * (quad.Width + scale.X);

                    if (scale.Y != 0 && quad.Height > 0)
                        newPosition.Y = quad.TopLeft.Y + (h.Y - quad.TopLeft.Y) / quad.Height * (quad.Height + scale.Y);

                    h.Position = newPosition;
                }
            }

            return true;
        }

        private static void adjustScaleFromAnchor(ref Vector2 scale, Anchor reference)
        {
            // cancel out scale in axes we don't care about (based on which drag handle was used).
            if ((reference & Anchor.x1) > 0) scale.X = 0;
            if ((reference & Anchor.y1) > 0) scale.Y = 0;

            // reverse the scale direction if dragging from top or left.
            if ((reference & Anchor.x0) > 0) scale.X = -scale.X;
            if ((reference & Anchor.y0) > 0) scale.Y = -scale.Y;
        }

        public override bool HandleRotation(float delta)
        {
            var hitObjects = selectedMovableObjects;

            Quad quad = getSurroundingQuad(hitObjects);

            referenceOrigin ??= quad.Centre;

            foreach (var h in hitObjects)
            {
                h.Position = rotatePointAroundOrigin(h.Position, referenceOrigin.Value, delta);

                if (h is IHasPath path)
                {
                    foreach (var point in path.Path.ControlPoints)
                        point.Position.Value = rotatePointAroundOrigin(point.Position.Value, Vector2.Zero, delta);
                }
            }

            // this isn't always the case but let's be lenient for now.
            return true;
        }

        private bool moveSelection(Vector2 delta)
        {
            var hitObjects = selectedMovableObjects;

            Quad quad = getSurroundingQuad(hitObjects);

            Vector2 newTopLeft = quad.TopLeft + delta;
            if (newTopLeft.X < 0)
                delta.X -= newTopLeft.X;
            if (newTopLeft.Y < 0)
                delta.Y -= newTopLeft.Y;

            Vector2 newBottomRight = quad.BottomRight + delta;
            if (newBottomRight.X > DrawWidth)
                delta.X -= newBottomRight.X - DrawWidth;
            if (newBottomRight.Y > DrawHeight)
                delta.Y -= newBottomRight.Y - DrawHeight;

            foreach (var h in hitObjects)
                h.Position += delta;

            return true;
        }

        /// <summary>
        /// Returns a gamefield-space quad surrounding the provided hit objects.
        /// </summary>
        /// <param name="hitObjects">The hit objects to calculate a quad for.</param>
        private Quad getSurroundingQuad(OsuHitObject[] hitObjects) =>
            getSurroundingQuad(hitObjects.SelectMany(h => new[] { h.Position, h.EndPosition }));

        /// <summary>
        /// Returns a gamefield-space quad surrounding the provided points.
        /// </summary>
        /// <param name="points">The points to calculate a quad for.</param>
        private Quad getSurroundingQuad(IEnumerable<Vector2> points)
        {
            if (!EditorBeatmap.SelectedHitObjects.Any())
                return new Quad();

            Vector2 minPosition = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 maxPosition = new Vector2(float.MinValue, float.MinValue);

            // Go through all hitobjects to make sure they would remain in the bounds of the editor after movement, before any movement is attempted
            foreach (var p in points)
            {
                minPosition = Vector2.ComponentMin(minPosition, p);
                maxPosition = Vector2.ComponentMax(maxPosition, p);
            }

            Vector2 size = maxPosition - minPosition;

            return new Quad(minPosition.X, minPosition.Y, size.X, size.Y);
        }

        /// <summary>
        /// All osu! hitobjects which can be moved/rotated/scaled.
        /// </summary>
        private OsuHitObject[] selectedMovableObjects => EditorBeatmap.SelectedHitObjects
                                                                      .OfType<OsuHitObject>()
                                                                      .Where(h => !(h is Spinner))
                                                                      .ToArray();

        /// <summary>
        /// Rotate a point around an arbitrary origin.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="origin">The centre origin to rotate around.</param>
        /// <param name="angle">The angle to rotate (in degrees).</param>
        private static Vector2 rotatePointAroundOrigin(Vector2 point, Vector2 origin, float angle)
        {
            angle = -angle;

            point.X -= origin.X;
            point.Y -= origin.Y;

            Vector2 ret;
            ret.X = point.X * MathF.Cos(MathUtils.DegreesToRadians(angle)) + point.Y * MathF.Sin(MathUtils.DegreesToRadians(angle));
            ret.Y = point.X * -MathF.Sin(MathUtils.DegreesToRadians(angle)) + point.Y * MathF.Cos(MathUtils.DegreesToRadians(angle));

            ret.X += origin.X;
            ret.Y += origin.Y;

            return ret;
        }
    }
}
