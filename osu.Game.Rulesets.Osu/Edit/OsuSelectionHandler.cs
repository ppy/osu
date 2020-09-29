// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class OsuSelectionHandler : SelectionHandler
    {
        public override ComposeSelectionBox CreateSelectionBox()
            => new ComposeSelectionBox
            {
                CanRotate = true,
                CanScaleX = true,
                CanScaleY = true,

                OperationStarted = onStart,
                OperationEnded = onEnd,

                OnRotation = handleRotation,
                OnScaleX = handleScaleX,
                OnScaleY = handleScaleY,
            };

        private void onEnd()
        {
            ChangeHandler.EndChange();
            centre = null;
        }

        private void onStart()
        {
            ChangeHandler.BeginChange();
        }

        private void handleScaleY(DragEvent e, Anchor reference)
        {
            int direction = (reference & Anchor.y0) > 0 ? -1 : 1;

            if (direction < 0)
            {
                // when resizing from a top drag handle, we want to move the selection first
                if (!moveSelection(new Vector2(0, e.Delta.Y)))
                    return;
            }

            scaleSelection(new Vector2(0, direction * e.Delta.Y));
        }

        private void handleScaleX(DragEvent e, Anchor reference)
        {
            int direction = (reference & Anchor.x0) > 0 ? -1 : 1;

            if (direction < 0)
            {
                // when resizing from a top drag handle, we want to move the selection first
                if (!moveSelection(new Vector2(e.Delta.X, 0)))
                    return;
            }

            scaleSelection(new Vector2(direction * e.Delta.X, 0));
        }

        private Vector2? centre;

        private void handleRotation(DragEvent e)
        {
            rotateSelection(e.Delta.X);
        }

        public override bool HandleMovement(MoveSelectionEvent moveEvent) =>
            moveSelection(moveEvent.InstantDelta);

        private bool rotateSelection(in float delta)
        {
            Quad quad = getSelectionQuad();

            centre ??= quad.Centre;

            foreach (var h in SelectedHitObjects.OfType<OsuHitObject>())
            {
                if (h is Spinner)
                {
                    // Spinners don't support position adjustments
                    continue;
                }

                h.Position = Rotate(h.Position, centre.Value, delta);

                if (h is IHasPath path)
                {
                    foreach (var point in path.Path.ControlPoints)
                    {
                        point.Position.Value = Rotate(point.Position.Value, Vector2.Zero, delta);
                    }
                }
            }

            return true;
        }

        private bool scaleSelection(Vector2 scale)
        {
            Quad quad = getSelectionQuad();

            Vector2 minPosition = quad.TopLeft;

            Vector2 size = quad.Size;
            Vector2 newSize = size + scale;

            foreach (var h in SelectedHitObjects.OfType<OsuHitObject>())
            {
                if (h is Spinner)
                {
                    // Spinners don't support position adjustments
                    continue;
                }

                if (scale.X != 1)
                    h.Position = new Vector2(minPosition.X + (h.X - minPosition.X) / size.X * newSize.X, h.Y);
                if (scale.Y != 1)
                    h.Position = new Vector2(h.X, minPosition.Y + (h.Y - minPosition.Y) / size.Y * newSize.Y);
            }

            return true;
        }

        private bool moveSelection(Vector2 delta)
        {
            Vector2 minPosition = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 maxPosition = new Vector2(float.MinValue, float.MinValue);

            // Go through all hitobjects to make sure they would remain in the bounds of the editor after movement, before any movement is attempted
            foreach (var h in SelectedHitObjects.OfType<OsuHitObject>())
            {
                if (h is Spinner)
                {
                    // Spinners don't support position adjustments
                    continue;
                }

                // Stacking is not considered
                minPosition = Vector2.ComponentMin(minPosition, Vector2.ComponentMin(h.EndPosition + delta, h.Position + delta));
                maxPosition = Vector2.ComponentMax(maxPosition, Vector2.ComponentMax(h.EndPosition + delta, h.Position + delta));
            }

            if (minPosition.X < 0 || minPosition.Y < 0 || maxPosition.X > DrawWidth || maxPosition.Y > DrawHeight)
                return false;

            foreach (var h in SelectedHitObjects.OfType<OsuHitObject>())
            {
                if (h is Spinner)
                {
                    // Spinners don't support position adjustments
                    continue;
                }

                h.Position += delta;
            }

            return true;
        }

        private Quad getSelectionQuad()
        {
            Vector2 minPosition = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 maxPosition = new Vector2(float.MinValue, float.MinValue);

            // Go through all hitobjects to make sure they would remain in the bounds of the editor after movement, before any movement is attempted
            foreach (var h in SelectedHitObjects.OfType<OsuHitObject>())
            {
                if (h is Spinner)
                {
                    // Spinners don't support position adjustments
                    continue;
                }

                // Stacking is not considered
                minPosition = Vector2.ComponentMin(minPosition, Vector2.ComponentMin(h.EndPosition, h.Position));
                maxPosition = Vector2.ComponentMax(maxPosition, Vector2.ComponentMax(h.EndPosition, h.Position));
            }

            Vector2 size = maxPosition - minPosition;

            return new Quad(minPosition.X, minPosition.Y, size.X, size.Y);
        }

        /// <summary>
        /// Returns rotated position from a given point.
        /// </summary>
        /// <param name="p">The point.</param>
        /// <param name="center">The center to rotate around.</param>
        /// <param name="angle">The angle to rotate (in degrees).</param>
        internal static Vector2 Rotate(Vector2 p, Vector2 center, float angle)
        {
            angle = -angle;

            p.X -= center.X;
            p.Y -= center.Y;

            Vector2 ret;
            ret.X = (float)(p.X * Math.Cos(angle / 180f * Math.PI) + p.Y * Math.Sin(angle / 180f * Math.PI));
            ret.Y = (float)(p.X * -Math.Sin(angle / 180f * Math.PI) + p.Y * Math.Cos(angle / 180f * Math.PI));

            ret.X += center.X;
            ret.Y += center.Y;

            return ret;
        }
    }
}
