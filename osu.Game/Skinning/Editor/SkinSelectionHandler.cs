// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Utils;
using osu.Game.Extensions;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Skinning.Editor
{
    public class SkinSelectionHandler : SelectionHandler<ISkinnableDrawable>
    {
        [Resolved]
        private SkinEditor skinEditor { get; set; }

        public override bool HandleRotation(float angle)
        {
            if (SelectedBlueprints.Count == 1)
            {
                // for single items, rotate around the origin rather than the selection centre.
                ((Drawable)SelectedBlueprints.First().Item).Rotation += angle;
            }
            else
            {
                var selectionQuad = getSelectionQuad();

                foreach (var b in SelectedBlueprints)
                {
                    var drawableItem = (Drawable)b.Item;

                    var rotatedPosition = RotatePointAroundOrigin(b.ScreenSpaceSelectionPoint, selectionQuad.Centre, angle);
                    updateDrawablePosition(drawableItem, rotatedPosition);

                    drawableItem.Rotation += angle;
                }
            }

            // this isn't always the case but let's be lenient for now.
            return true;
        }

        public override bool HandleScale(Vector2 scale, Anchor anchor)
        {
            // convert scale to screen space
            scale = ToScreenSpace(scale) - ToScreenSpace(Vector2.Zero);

            adjustScaleFromAnchor(ref scale, anchor);

            // the selection quad is always upright, so use an AABB rect to make mutating the values easier.
            var selectionRect = getSelectionQuad().AABBFloat;

            // If the selection has no area we cannot scale it
            if (selectionRect.Area == 0)
                return false;

            // copy to mutate, as we will need to compare to the original later on.
            var adjustedRect = selectionRect;

            // first, remove any scale axis we are not interested in.
            if (anchor.HasFlagFast(Anchor.x1)) scale.X = 0;
            if (anchor.HasFlagFast(Anchor.y1)) scale.Y = 0;

            // for now aspect lock scale adjustments that occur at corners..
            if (!anchor.HasFlagFast(Anchor.x1) && !anchor.HasFlagFast(Anchor.y1))
            {
                // project scale vector along diagonal
                Vector2 diag = (selectionRect.TopLeft - selectionRect.BottomRight).Normalized();
                scale = Vector2.Dot(scale, diag) * diag;
            }
            // ..or if any of the selection have been rotated.
            // this is to avoid requiring skew logic (which would likely not be the user's expected transform anyway).
            else if (SelectedBlueprints.Any(b => !Precision.AlmostEquals(((Drawable)b.Item).Rotation, 0)))
            {
                if (anchor.HasFlagFast(Anchor.x1))
                    // if dragging from the horizontal centre, only a vertical component is available.
                    scale.X = scale.Y / selectionRect.Height * selectionRect.Width;
                else
                    // in all other cases (arbitrarily) use the horizontal component for aspect lock.
                    scale.Y = scale.X / selectionRect.Width * selectionRect.Height;
            }

            if (anchor.HasFlagFast(Anchor.x0)) adjustedRect.X -= scale.X;
            if (anchor.HasFlagFast(Anchor.y0)) adjustedRect.Y -= scale.Y;

            adjustedRect.Width += scale.X;
            adjustedRect.Height += scale.Y;

            // scale adjust applied to each individual item should match that of the quad itself.
            var scaledDelta = new Vector2(
                MathF.Max(adjustedRect.Width / selectionRect.Width, 0),
                MathF.Max(adjustedRect.Height / selectionRect.Height, 0)
            );

            foreach (var b in SelectedBlueprints)
            {
                var drawableItem = (Drawable)b.Item;

                // each drawable's relative position should be maintained in the scaled quad.
                var screenPosition = b.ScreenSpaceSelectionPoint;

                var relativePositionInOriginal =
                    new Vector2(
                        (screenPosition.X - selectionRect.TopLeft.X) / selectionRect.Width,
                        (screenPosition.Y - selectionRect.TopLeft.Y) / selectionRect.Height
                    );

                var newPositionInAdjusted = new Vector2(
                    adjustedRect.TopLeft.X + adjustedRect.Width * relativePositionInOriginal.X,
                    adjustedRect.TopLeft.Y + adjustedRect.Height * relativePositionInOriginal.Y
                );

                updateDrawablePosition(drawableItem, newPositionInAdjusted);
                drawableItem.Scale *= scaledDelta;
            }

            return true;
        }

        public override bool HandleFlip(Direction direction)
        {
            var selectionQuad = getSelectionQuad();
            Vector2 scaleFactor = direction == Direction.Horizontal ? new Vector2(-1, 1) : new Vector2(1, -1);

            foreach (var b in SelectedBlueprints)
            {
                var drawableItem = (Drawable)b.Item;

                var flippedPosition = GetFlippedPosition(direction, selectionQuad, b.ScreenSpaceSelectionPoint);

                updateDrawablePosition(drawableItem, flippedPosition);

                drawableItem.Scale *= scaleFactor;
                drawableItem.Rotation -= drawableItem.Rotation % 180 * 2;
            }

            return true;
        }

        public override bool HandleMovement(MoveSelectionEvent<ISkinnableDrawable> moveEvent)
        {
            foreach (var c in SelectedBlueprints)
            {
                var item = c.Item;
                Drawable drawable = (Drawable)item;

                drawable.Position += drawable.ScreenSpaceDeltaToParentSpace(moveEvent.ScreenSpaceDelta);

                if (item.UsesFixedAnchor) continue;

                applyClosestAnchor(drawable);
            }

            return true;
        }

        private static void applyClosestAnchor(Drawable drawable) => applyAnchor(drawable, getClosestAnchor(drawable));

        protected override void OnSelectionChanged()
        {
            base.OnSelectionChanged();

            SelectionBox.CanRotate = true;
            SelectionBox.CanScaleX = true;
            SelectionBox.CanScaleY = true;
            SelectionBox.CanFlipX = true;
            SelectionBox.CanFlipY = true;
            SelectionBox.CanReverse = false;
        }

        protected override void DeleteItems(IEnumerable<ISkinnableDrawable> items) =>
            skinEditor.DeleteItems(items.ToArray());

        protected override IEnumerable<MenuItem> GetContextMenuItemsForSelection(IEnumerable<SelectionBlueprint<ISkinnableDrawable>> selection)
        {
            var closestItem = new TernaryStateRadioMenuItem("Closest", MenuItemType.Standard, _ => applyClosestAnchors())
            {
                State = { Value = GetStateFromSelection(selection, c => !c.Item.UsesFixedAnchor) }
            };

            yield return new OsuMenuItem("Anchor")
            {
                Items = createAnchorItems((d, a) => d.UsesFixedAnchor && ((Drawable)d).Anchor == a, applyFixedAnchors)
                        .Prepend(closestItem)
                        .ToArray()
            };

            yield return new OsuMenuItem("Origin")
            {
                Items = createAnchorItems((d, o) => ((Drawable)d).Origin == o, applyOrigins).ToArray()
            };

            foreach (var item in base.GetContextMenuItemsForSelection(selection))
                yield return item;

            IEnumerable<TernaryStateMenuItem> createAnchorItems(Func<ISkinnableDrawable, Anchor, bool> checkFunction, Action<Anchor> applyFunction)
            {
                var displayableAnchors = new[]
                {
                    Anchor.TopLeft,
                    Anchor.TopCentre,
                    Anchor.TopRight,
                    Anchor.CentreLeft,
                    Anchor.Centre,
                    Anchor.CentreRight,
                    Anchor.BottomLeft,
                    Anchor.BottomCentre,
                    Anchor.BottomRight,
                };
                return displayableAnchors.Select(a =>
                {
                    return new TernaryStateRadioMenuItem(a.ToString(), MenuItemType.Standard, _ => applyFunction(a))
                    {
                        State = { Value = GetStateFromSelection(selection, c => checkFunction(c.Item, a)) }
                    };
                });
            }
        }

        private static void updateDrawablePosition(Drawable drawable, Vector2 screenSpacePosition)
        {
            drawable.Position =
                drawable.Parent.ToLocalSpace(screenSpacePosition) - drawable.AnchorPosition;
        }

        private void applyOrigins(Anchor origin)
        {
            foreach (var item in SelectedItems)
            {
                var drawable = (Drawable)item;

                if (origin == drawable.Origin) continue;

                var previousOrigin = drawable.OriginPosition;
                drawable.Origin = origin;
                drawable.Position += drawable.OriginPosition - previousOrigin;

                if (item.UsesFixedAnchor) continue;

                applyClosestAnchor(drawable);
            }
        }

        /// <summary>
        /// A screen-space quad surrounding all selected drawables, accounting for their full displayed size.
        /// </summary>
        /// <returns></returns>
        private Quad getSelectionQuad() =>
            GetSurroundingQuad(SelectedBlueprints.SelectMany(b => b.Item.ScreenSpaceDrawQuad.GetVertices().ToArray()));

        private void applyFixedAnchors(Anchor anchor)
        {
            foreach (var item in SelectedItems)
            {
                var drawable = (Drawable)item;

                item.UsesFixedAnchor = true;
                applyAnchor(drawable, anchor);
            }
        }

        private void applyClosestAnchors()
        {
            foreach (var item in SelectedItems)
            {
                item.UsesFixedAnchor = false;
                applyClosestAnchor((Drawable)item);
            }
        }

        private static Anchor getClosestAnchor(Drawable drawable)
        {
            var parent = drawable.Parent;

            if (parent == null)
                return drawable.Anchor;

            var screenPosition = getScreenPosition();

            var absolutePosition = parent.ToLocalSpace(screenPosition);
            var factor = parent.RelativeToAbsoluteFactor;

            var result = default(Anchor);

            static Anchor getAnchorFromPosition(float xOrY, Anchor anchor0, Anchor anchor1, Anchor anchor2)
            {
                if (xOrY >= 2 / 3f)
                    return anchor2;

                if (xOrY >= 1 / 3f)
                    return anchor1;

                return anchor0;
            }

            result |= getAnchorFromPosition(absolutePosition.X / factor.X, Anchor.x0, Anchor.x1, Anchor.x2);
            result |= getAnchorFromPosition(absolutePosition.Y / factor.Y, Anchor.y0, Anchor.y1, Anchor.y2);

            return result;

            Vector2 getScreenPosition()
            {
                var quad = drawable.ScreenSpaceDrawQuad;
                var origin = drawable.Origin;

                var pos = quad.TopLeft;

                if (origin.HasFlagFast(Anchor.x2))
                    pos.X += quad.Width;
                else if (origin.HasFlagFast(Anchor.x1))
                    pos.X += quad.Width / 2f;

                if (origin.HasFlagFast(Anchor.y2))
                    pos.Y += quad.Height;
                else if (origin.HasFlagFast(Anchor.y1))
                    pos.Y += quad.Height / 2f;

                return pos;
            }
        }

        private static void applyAnchor(Drawable drawable, Anchor anchor)
        {
            if (anchor == drawable.Anchor) return;

            var previousAnchor = drawable.AnchorPosition;
            drawable.Anchor = anchor;
            drawable.Position -= drawable.AnchorPosition - previousAnchor;
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
    }
}
