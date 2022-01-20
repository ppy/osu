// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Utils;
using osu.Game.Extensions;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class OsuSelectionHandler : EditorSelectionHandler
    {
        [Resolved(CanBeNull = true)]
        private IPositionSnapProvider? positionSnapProvider { get; set; }

        /// <summary>
        /// During a transform, the initial origin is stored so it can be used throughout the operation.
        /// </summary>
        private Vector2? referenceOrigin;

        /// <summary>
        /// During a transform, the initial path types of a single selected slider are stored so they
        /// can be maintained throughout the operation.
        /// </summary>
        private List<PathType?>? referencePathTypes;

        protected override void OnSelectionChanged()
        {
            base.OnSelectionChanged();

            Quad quad = selectedMovableObjects.Length > 0 ? getSurroundingQuad(selectedMovableObjects) : new Quad();

            SelectionBox.CanRotate = quad.Width > 0 || quad.Height > 0;
            SelectionBox.CanFlipX = SelectionBox.CanScaleX = quad.Width > 0;
            SelectionBox.CanFlipY = SelectionBox.CanScaleY = quad.Height > 0;
            SelectionBox.CanReverse = EditorBeatmap.SelectedHitObjects.Count > 1 || EditorBeatmap.SelectedHitObjects.Any(s => s is Slider);
        }

        protected override void OnOperationEnded()
        {
            base.OnOperationEnded();
            referenceOrigin = null;
            referencePathTypes = null;
        }

        public override bool HandleMovement(MoveSelectionEvent<HitObject> moveEvent)
        {
            var hitObjects = selectedMovableObjects;

            // this will potentially move the selection out of bounds...
            foreach (var h in hitObjects)
                h.Position += this.ScreenSpaceDeltaToParentSpace(moveEvent.ScreenSpaceDelta);

            // but this will be corrected.
            moveSelectionInBounds();
            return true;
        }

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
                    slider.Path.Reverse(out Vector2 offset);
                    slider.Position += offset;
                }
            }

            return true;
        }

        public override bool HandleFlip(Direction direction, bool flipOverOrigin)
        {
            var hitObjects = selectedMovableObjects;

            var flipQuad = flipOverOrigin ? new Quad(0, 0, OsuPlayfield.BASE_SIZE.X, OsuPlayfield.BASE_SIZE.Y) : getSurroundingQuad(hitObjects);

            bool didFlip = false;

            foreach (var h in hitObjects)
            {
                var flippedPosition = GetFlippedPosition(direction, flipQuad, h.Position);

                if (!Precision.AlmostEquals(flippedPosition, h.Position))
                {
                    h.Position = flippedPosition;
                    didFlip = true;
                }

                if (h is Slider slider)
                {
                    didFlip = true;

                    foreach (var point in slider.Path.ControlPoints)
                    {
                        point.Position = new Vector2(
                            (direction == Direction.Horizontal ? -1 : 1) * point.Position.X,
                            (direction == Direction.Vertical ? -1 : 1) * point.Position.Y
                        );
                    }
                }
            }

            return didFlip;
        }

        public override bool HandleScale(Vector2 scale, Anchor reference)
        {
            adjustScaleFromAnchor(ref scale, reference);

            var hitObjects = selectedMovableObjects;

            // for the time being, allow resizing of slider paths only if the slider is
            // the only hit object selected. with a group selection, it's likely the user
            // is not looking to change the duration of the slider but expand the whole pattern.
            if (hitObjects.Length == 1 && hitObjects.First() is Slider slider)
                scaleSlider(slider, scale);
            else
                scaleHitObjects(hitObjects, reference, scale);

            moveSelectionInBounds();
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
                h.Position = RotatePointAroundOrigin(h.Position, referenceOrigin.Value, delta);

                if (h is IHasPath path)
                {
                    foreach (var point in path.Path.ControlPoints)
                        point.Position = RotatePointAroundOrigin(point.Position, Vector2.Zero, delta);
                }
            }

            // this isn't always the case but let's be lenient for now.
            return true;
        }

        private void scaleSlider(Slider slider, Vector2 scale)
        {
            referencePathTypes ??= slider.Path.ControlPoints.Select(p => p.Type).ToList();

            Quad sliderQuad = GetSurroundingQuad(slider.Path.ControlPoints.Select(p => p.Position));

            // Limit minimum distance between control points after scaling to almost 0. Less than 0 causes the slider to flip, exactly 0 causes a crash through division by 0.
            scale = Vector2.ComponentMax(new Vector2(Precision.FLOAT_EPSILON), sliderQuad.Size + scale) - sliderQuad.Size;

            Vector2 pathRelativeDeltaScale = new Vector2(
                sliderQuad.Width == 0 ? 0 : 1 + scale.X / sliderQuad.Width,
                sliderQuad.Height == 0 ? 0 : 1 + scale.Y / sliderQuad.Height);

            Queue<Vector2> oldControlPoints = new Queue<Vector2>();

            foreach (var point in slider.Path.ControlPoints)
            {
                oldControlPoints.Enqueue(point.Position);
                point.Position *= pathRelativeDeltaScale;
            }

            // Maintain the path types in case they were defaulted to bezier at some point during scaling
            for (int i = 0; i < slider.Path.ControlPoints.Count; ++i)
                slider.Path.ControlPoints[i].Type = referencePathTypes[i];

            // Snap the slider's length to the current beat divisor
            // to calculate the final resulting duration / bounding box before the final checks.
            slider.SnapTo(positionSnapProvider);

            //if sliderhead or sliderend end up outside playfield, revert scaling.
            Quad scaledQuad = getSurroundingQuad(new OsuHitObject[] { slider });
            (bool xInBounds, bool yInBounds) = isQuadInBounds(scaledQuad);

            if (xInBounds && yInBounds && slider.Path.HasValidLength)
                return;

            foreach (var point in slider.Path.ControlPoints)
                point.Position = oldControlPoints.Dequeue();

            // Snap the slider's length again to undo the potentially-invalid length applied by the previous snap.
            slider.SnapTo(positionSnapProvider);
        }

        private void scaleHitObjects(OsuHitObject[] hitObjects, Anchor reference, Vector2 scale)
        {
            scale = getClampedScale(hitObjects, reference, scale);
            Quad selectionQuad = getSurroundingQuad(hitObjects);

            foreach (var h in hitObjects)
                h.Position = GetScaledPosition(reference, scale, selectionQuad, h.Position);
        }

        private (bool X, bool Y) isQuadInBounds(Quad quad)
        {
            bool xInBounds = (quad.TopLeft.X >= 0) && (quad.BottomRight.X <= DrawWidth);
            bool yInBounds = (quad.TopLeft.Y >= 0) && (quad.BottomRight.Y <= DrawHeight);

            return (xInBounds, yInBounds);
        }

        private void moveSelectionInBounds()
        {
            var hitObjects = selectedMovableObjects;

            Quad quad = getSurroundingQuad(hitObjects);

            Vector2 delta = Vector2.Zero;

            if (quad.TopLeft.X < 0)
                delta.X -= quad.TopLeft.X;
            if (quad.TopLeft.Y < 0)
                delta.Y -= quad.TopLeft.Y;

            if (quad.BottomRight.X > DrawWidth)
                delta.X -= quad.BottomRight.X - DrawWidth;
            if (quad.BottomRight.Y > DrawHeight)
                delta.Y -= quad.BottomRight.Y - DrawHeight;

            foreach (var h in hitObjects)
                h.Position += delta;
        }

        /// <summary>
        /// Clamp scale for multi-object-scaling where selection does not exceed playfield bounds or flip.
        /// </summary>
        /// <param name="hitObjects">The hitobjects to be scaled</param>
        /// <param name="reference">The anchor from which the scale operation is performed</param>
        /// <param name="scale">The scale to be clamped</param>
        /// <returns>The clamped scale vector</returns>
        private Vector2 getClampedScale(OsuHitObject[] hitObjects, Anchor reference, Vector2 scale)
        {
            float xOffset = ((reference & Anchor.x0) > 0) ? -scale.X : 0;
            float yOffset = ((reference & Anchor.y0) > 0) ? -scale.Y : 0;

            Quad selectionQuad = getSurroundingQuad(hitObjects);

            //todo: this is not always correct for selections involving sliders. This approximation assumes each point is scaled independently, but sliderends move with the sliderhead.
            Quad scaledQuad = new Quad(selectionQuad.TopLeft.X + xOffset, selectionQuad.TopLeft.Y + yOffset, selectionQuad.Width + scale.X, selectionQuad.Height + scale.Y);

            //max Size -> playfield bounds
            if (scaledQuad.TopLeft.X < 0)
                scale.X += scaledQuad.TopLeft.X;
            if (scaledQuad.TopLeft.Y < 0)
                scale.Y += scaledQuad.TopLeft.Y;

            if (scaledQuad.BottomRight.X > DrawWidth)
                scale.X -= scaledQuad.BottomRight.X - DrawWidth;
            if (scaledQuad.BottomRight.Y > DrawHeight)
                scale.Y -= scaledQuad.BottomRight.Y - DrawHeight;

            //min Size -> almost 0. Less than 0 causes the quad to flip, exactly 0 causes scaling to get stuck at minimum scale.
            Vector2 scaledSize = selectionQuad.Size + scale;
            Vector2 minSize = new Vector2(Precision.FLOAT_EPSILON);

            scale = Vector2.ComponentMax(minSize, scaledSize) - selectionQuad.Size;

            return scale;
        }

        /// <summary>
        /// Returns a gamefield-space quad surrounding the provided hit objects.
        /// </summary>
        /// <param name="hitObjects">The hit objects to calculate a quad for.</param>
        private Quad getSurroundingQuad(OsuHitObject[] hitObjects) =>
            GetSurroundingQuad(hitObjects.SelectMany(h =>
            {
                if (h is IHasPath path)
                {
                    return new[]
                    {
                        h.Position,
                        // can't use EndPosition for reverse slider cases.
                        h.Position + path.Path.PositionAt(1)
                    };
                }

                return new[] { h.Position };
            }));

        /// <summary>
        /// All osu! hitobjects which can be moved/rotated/scaled.
        /// </summary>
        private OsuHitObject[] selectedMovableObjects => SelectedItems.OfType<OsuHitObject>()
                                                                      .Where(h => !(h is Spinner))
                                                                      .ToArray();
    }
}
