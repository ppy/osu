// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Utils;
using osu.Game.Extensions;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Skinning;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Overlays.SkinEditor
{
    public partial class SkinSelectionHandler : SelectionHandler<ISerialisableDrawable>
    {
        [Resolved]
        private SkinEditor skinEditor { get; set; } = null!;

        public override SelectionRotationHandler CreateRotationHandler() => new SkinSelectionRotationHandler
        {
            UpdatePosition = updateDrawablePosition
        };

        private bool allSelectedSupportManualSizing(Axes axis) => SelectedItems.All(b => (b as CompositeDrawable)?.AutoSizeAxes.HasFlagFast(axis) == false);

        public override bool HandleScale(Vector2 scale, Anchor anchor)
        {
            Axes adjustAxis;

            switch (anchor)
            {
                // for corners, adjust scale.
                case Anchor.TopLeft:
                case Anchor.TopRight:
                case Anchor.BottomLeft:
                case Anchor.BottomRight:
                    adjustAxis = Axes.Both;
                    break;

                // for edges, adjust size.
                // autosize elements can't be easily handled so just disable sizing for now.
                case Anchor.TopCentre:
                case Anchor.BottomCentre:
                    if (!allSelectedSupportManualSizing(Axes.Y))
                        return false;

                    adjustAxis = Axes.Y;
                    break;

                case Anchor.CentreLeft:
                case Anchor.CentreRight:
                    if (!allSelectedSupportManualSizing(Axes.X))
                        return false;

                    adjustAxis = Axes.X;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(anchor), anchor, null);
            }

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
            bool isRotated = false;

            // for now aspect lock scale adjustments that occur at corners..
            if (!anchor.HasFlagFast(Anchor.x1) && !anchor.HasFlagFast(Anchor.y1))
            {
                // project scale vector along diagonal
                Vector2 diag = (selectionRect.TopLeft - selectionRect.BottomRight).Normalized();
                scale = Vector2.Dot(scale, diag) * diag;
            }
            // ..or if any of the selection have been rotated.
            // this is to avoid requiring skew logic (which would likely not be the user's expected transform anyway).
            else if (SelectedBlueprints.Any(b => !Precision.AlmostEquals(((Drawable)b.Item).Rotation % 90, 0)))
            {
                isRotated = true;
                if (anchor.HasFlagFast(Anchor.x1))
                    // if dragging from the horizontal centre, only a vertical component is available.
                    scale.X = scale.Y / selectionRect.Height * selectionRect.Width;
                else
                    // in all other cases (arbitrarily) use the horizontal component for aspect lock.
                    scale.Y = scale.X / selectionRect.Width * selectionRect.Height;
            }

            if (anchor.HasFlagFast(Anchor.x0)) adjustedRect.X -= scale.X;
            if (anchor.HasFlagFast(Anchor.y0)) adjustedRect.Y -= scale.Y;

            // Maintain the selection's centre position if dragging from the centre anchors and selection is rotated.
            if (isRotated && anchor.HasFlagFast(Anchor.x1)) adjustedRect.X -= scale.X / 2;
            if (isRotated && anchor.HasFlagFast(Anchor.y1)) adjustedRect.Y -= scale.Y / 2;

            adjustedRect.Width += scale.X;
            adjustedRect.Height += scale.Y;

            if (adjustedRect.Width <= 0 || adjustedRect.Height <= 0)
            {
                Axes toFlip = Axes.None;

                if (adjustedRect.Width <= 0) toFlip |= Axes.X;
                if (adjustedRect.Height <= 0) toFlip |= Axes.Y;

                SelectionBox.PerformFlipFromScaleHandles(toFlip);
                return true;
            }

            // scale adjust applied to each individual item should match that of the quad itself.
            var scaledDelta = new Vector2(
                adjustedRect.Width / selectionRect.Width,
                adjustedRect.Height / selectionRect.Height
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

                var currentScaledDelta = scaledDelta;
                if (Precision.AlmostEquals(MathF.Abs(drawableItem.Rotation) % 180, 90))
                    currentScaledDelta = new Vector2(scaledDelta.Y, scaledDelta.X);

                switch (adjustAxis)
                {
                    case Axes.X:
                        drawableItem.Width *= currentScaledDelta.X;
                        break;

                    case Axes.Y:
                        drawableItem.Height *= currentScaledDelta.Y;
                        break;

                    case Axes.Both:
                        drawableItem.Scale *= currentScaledDelta;
                        break;
                }
            }

            return true;
        }

        public override bool HandleFlip(Direction direction, bool flipOverOrigin)
        {
            var selectionQuad = getSelectionQuad();
            Vector2 scaleFactor = direction == Direction.Horizontal ? new Vector2(-1, 1) : new Vector2(1, -1);

            foreach (var b in SelectedBlueprints)
            {
                var drawableItem = (Drawable)b.Item;

                var flippedPosition = GeometryUtils.GetFlippedPosition(direction, flipOverOrigin ? drawableItem.Parent!.ScreenSpaceDrawQuad : selectionQuad, b.ScreenSpaceSelectionPoint);

                updateDrawablePosition(drawableItem, flippedPosition);

                drawableItem.Scale *= scaleFactor;
                drawableItem.Rotation -= drawableItem.Rotation % 180 * 2;
            }

            return true;
        }

        public override bool HandleMovement(MoveSelectionEvent<ISerialisableDrawable> moveEvent)
        {
            foreach (var c in SelectedBlueprints)
            {
                var item = c.Item;
                Drawable drawable = (Drawable)item;

                drawable.Position += drawable.ScreenSpaceDeltaToParentSpace(moveEvent.ScreenSpaceDelta);

                if (item.UsesFixedAnchor) continue;

                ApplyClosestAnchor(drawable);
            }

            return true;
        }

        public static void ApplyClosestAnchor(Drawable drawable) => applyAnchor(drawable, getClosestAnchor(drawable));

        protected override void OnSelectionChanged()
        {
            base.OnSelectionChanged();

            SelectionBox.CanFlipX = true;
            SelectionBox.CanFlipY = true;
            SelectionBox.CanReverse = false;
        }

        protected override void DeleteItems(IEnumerable<ISerialisableDrawable> items) =>
            skinEditor.DeleteItems(items.ToArray());

        protected override IEnumerable<MenuItem> GetContextMenuItemsForSelection(IEnumerable<SelectionBlueprint<ISerialisableDrawable>> selection)
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

            yield return new OsuMenuItemSpacer();

            yield return new OsuMenuItem("Reset position", MenuItemType.Standard, () =>
            {
                foreach (var blueprint in SelectedBlueprints)
                    ((Drawable)blueprint.Item).Position = Vector2.Zero;
            });

            yield return new OsuMenuItem("Reset rotation", MenuItemType.Standard, () =>
            {
                foreach (var blueprint in SelectedBlueprints)
                    ((Drawable)blueprint.Item).Rotation = 0;
            });

            yield return new OsuMenuItem("Reset scale", MenuItemType.Standard, () =>
            {
                foreach (var blueprint in SelectedBlueprints)
                {
                    var blueprintItem = ((Drawable)blueprint.Item);
                    blueprintItem.Scale = Vector2.One;

                    if (blueprintItem.RelativeSizeAxes.HasFlagFast(Axes.X))
                        blueprintItem.Width = 1;
                    if (blueprintItem.RelativeSizeAxes.HasFlagFast(Axes.Y))
                        blueprintItem.Height = 1;
                }
            });

            yield return new OsuMenuItemSpacer();

            yield return new OsuMenuItem("Bring to front", MenuItemType.Standard, () => skinEditor.BringSelectionToFront());

            yield return new OsuMenuItem("Send to back", MenuItemType.Standard, () => skinEditor.SendSelectionToBack());

            yield return new OsuMenuItemSpacer();

            foreach (var item in base.GetContextMenuItemsForSelection(selection))
                yield return item;

            IEnumerable<TernaryStateMenuItem> createAnchorItems(Func<ISerialisableDrawable, Anchor, bool> checkFunction, Action<Anchor> applyFunction)
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
                drawable.Parent!.ToLocalSpace(screenSpacePosition) - drawable.AnchorPosition;
        }

        private void applyOrigins(Anchor origin)
        {
            OnOperationBegan();

            foreach (var item in SelectedItems)
            {
                var drawable = (Drawable)item;

                if (origin == drawable.Origin) continue;

                var previousOrigin = drawable.OriginPosition;
                drawable.Origin = origin;
                drawable.Position += drawable.OriginPosition - previousOrigin;

                if (item.UsesFixedAnchor) continue;

                ApplyClosestAnchor(drawable);
            }

            OnOperationEnded();
        }

        /// <summary>
        /// A screen-space quad surrounding all selected drawables, accounting for their full displayed size.
        /// </summary>
        /// <returns></returns>
        private Quad getSelectionQuad() =>
            GeometryUtils.GetSurroundingQuad(SelectedBlueprints.SelectMany(b => b.Item.ScreenSpaceDrawQuad.GetVertices().ToArray()));

        private void applyFixedAnchors(Anchor anchor)
        {
            OnOperationBegan();

            foreach (var item in SelectedItems)
            {
                var drawable = (Drawable)item;

                item.UsesFixedAnchor = true;
                applyAnchor(drawable, anchor);
            }

            OnOperationEnded();
        }

        private void applyClosestAnchors()
        {
            OnOperationBegan();

            foreach (var item in SelectedItems)
            {
                item.UsesFixedAnchor = false;
                ApplyClosestAnchor((Drawable)item);
            }

            OnOperationEnded();
        }

        private static Anchor getClosestAnchor(Drawable drawable)
        {
            var parent = drawable.Parent;

            if (parent == null)
                return drawable.Anchor;

            var screenPosition = drawable.ToScreenSpace(drawable.OriginPosition);

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
