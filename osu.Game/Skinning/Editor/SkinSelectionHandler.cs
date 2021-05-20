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
using osu.Game.Extensions;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Skinning.Editor
{
    public class SkinSelectionHandler : SelectionHandler<ISkinnableDrawable>
    {
        private Vector2? referenceOrigin;

        [Resolved]
        private SkinEditor skinEditor { get; set; }

        protected override void OnOperationEnded()
        {
            base.OnOperationEnded();
            referenceOrigin = null;
        }

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

                referenceOrigin ??= selectionQuad.Centre;

                foreach (var b in SelectedBlueprints)
                {
                    var drawableItem = (Drawable)b.Item;

                    drawableItem.Position = drawableItem.Parent.ToLocalSpace(RotatePointAroundOrigin(b.ScreenSpaceSelectionPoint, referenceOrigin.Value, angle)) - drawableItem.AnchorPosition;
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

            var selectionQuad = getSelectionQuad();

            // the selection quad is always upright, so use a rect to make mutating the values easier.
            var adjustedRect = selectionQuad.AABBFloat;

            // for now aspect lock scale adjustments that occur at corners.
            if (!anchor.HasFlagFast(Anchor.x1) && !anchor.HasFlagFast(Anchor.y1))
                scale.Y = scale.X / selectionQuad.Width * selectionQuad.Height;

            if (anchor.HasFlagFast(Anchor.x0))
            {
                adjustedRect.X -= scale.X;
                adjustedRect.Width += scale.X;
            }
            else if (anchor.HasFlagFast(Anchor.x2))
            {
                adjustedRect.Width += scale.X;
            }

            if (anchor.HasFlagFast(Anchor.y0))
            {
                adjustedRect.Y -= scale.Y;
                adjustedRect.Height += scale.Y;
            }
            else if (anchor.HasFlagFast(Anchor.y2))
            {
                adjustedRect.Height += scale.Y;
            }

            // scale adjust should match that of the quad itself.
            var scaledDelta = new Vector2(
                adjustedRect.Width / selectionQuad.Width,
                adjustedRect.Height / selectionQuad.Height
            );

            foreach (var b in SelectedBlueprints)
            {
                var drawableItem = (Drawable)b.Item;

                // each drawable's relative position should be maintained in the scaled quad.
                var screenPosition = b.ScreenSpaceSelectionPoint;

                var relativePositionInOriginal =
                    new Vector2(
                        (screenPosition.X - selectionQuad.TopLeft.X) / selectionQuad.Width,
                        (screenPosition.Y - selectionQuad.TopLeft.Y) / selectionQuad.Height
                    );

                var newPositionInAdjusted = new Vector2(
                    adjustedRect.TopLeft.X + adjustedRect.Width * relativePositionInOriginal.X,
                    adjustedRect.TopLeft.Y + adjustedRect.Height * relativePositionInOriginal.Y
                );

                drawableItem.Position = drawableItem.Parent.ToLocalSpace(newPositionInAdjusted) - drawableItem.AnchorPosition;
                drawableItem.Scale *= scaledDelta;
            }

            return true;
        }

        public override bool HandleFlip(Direction direction)
        {
            var selectionQuad = GetSurroundingQuad(SelectedBlueprints.Select(b => b.ScreenSpaceSelectionPoint));

            foreach (var b in SelectedBlueprints)
            {
                var drawableItem = (Drawable)b.Item;

                drawableItem.Position =
                    drawableItem.Parent.ToLocalSpace(GetFlippedPosition(direction, selectionQuad, b.ScreenSpaceSelectionPoint)) - drawableItem.AnchorPosition;

                drawableItem.Scale *= new Vector2(
                    direction == Direction.Horizontal ? -1 : 1,
                    direction == Direction.Vertical ? -1 : 1
                );
            }

            return true;
        }

        public override bool HandleMovement(MoveSelectionEvent<ISkinnableDrawable> moveEvent)
        {
            foreach (var c in SelectedBlueprints)
            {
                Drawable drawable = (Drawable)c.Item;
                drawable.Position += drawable.ScreenSpaceDeltaToParentSpace(moveEvent.ScreenSpaceDelta);
            }

            return true;
        }

        protected override void OnSelectionChanged()
        {
            base.OnSelectionChanged();

            SelectionBox.CanRotate = true;
            SelectionBox.CanScaleX = true;
            SelectionBox.CanScaleY = true;
            SelectionBox.CanReverse = false;
        }

        protected override void DeleteItems(IEnumerable<ISkinnableDrawable> items) =>
            skinEditor.DeleteItems(items.ToArray());

        protected override IEnumerable<MenuItem> GetContextMenuItemsForSelection(IEnumerable<SelectionBlueprint<ISkinnableDrawable>> selection)
        {
            yield return new OsuMenuItem("Anchor")
            {
                Items = createAnchorItems(d => d.Anchor, applyAnchor).ToArray()
            };

            yield return new OsuMenuItem("Origin")
            {
                Items = createAnchorItems(d => d.Origin, applyOrigin).ToArray()
            };

            foreach (var item in base.GetContextMenuItemsForSelection(selection))
                yield return item;

            IEnumerable<AnchorMenuItem> createAnchorItems(Func<Drawable, Anchor> checkFunction, Action<Anchor> applyFunction)
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
                    return new AnchorMenuItem(a, selection, _ => applyFunction(a))
                    {
                        State = { Value = GetStateFromSelection(selection, c => checkFunction((Drawable)c.Item) == a) }
                    };
                });
            }
        }

        private void applyOrigin(Anchor anchor)
        {
            foreach (var item in SelectedItems)
            {
                var drawable = (Drawable)item;

                var previousOrigin = drawable.OriginPosition;
                drawable.Origin = anchor;
                drawable.Position += drawable.OriginPosition - previousOrigin;
            }
        }

        /// <summary>
        /// A screen-space quad surrounding all selected drawables, accounting for their full displayed size.
        /// </summary>
        /// <returns></returns>
        private Quad getSelectionQuad() =>
            GetSurroundingQuad(SelectedBlueprints.SelectMany(b => b.Item.ScreenSpaceDrawQuad.GetVertices().ToArray()));

        private void applyAnchor(Anchor anchor)
        {
            foreach (var item in SelectedItems)
            {
                var drawable = (Drawable)item;

                var previousAnchor = drawable.AnchorPosition;
                drawable.Anchor = anchor;
                drawable.Position -= drawable.AnchorPosition - previousAnchor;
            }
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

        public class AnchorMenuItem : TernaryStateMenuItem
        {
            public AnchorMenuItem(Anchor anchor, IEnumerable<SelectionBlueprint<ISkinnableDrawable>> selection, Action<TernaryState> action)
                : base(anchor.ToString(), getNextState, MenuItemType.Standard, action)
            {
            }

            private static TernaryState getNextState(TernaryState state) => TernaryState.True;
        }
    }
}
