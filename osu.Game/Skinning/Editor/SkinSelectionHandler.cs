// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
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
        [Resolved]
        private SkinEditor skinEditor { get; set; }

        public override bool HandleRotation(float angle)
        {
            // TODO: this doesn't correctly account for origin/anchor specs being different in a multi-selection.
            foreach (var c in SelectedBlueprints)
                ((Drawable)c.Item).Rotation += angle;

            return base.HandleRotation(angle);
        }

        public override bool HandleScale(Vector2 scale, Anchor anchor)
        {
            adjustScaleFromAnchor(ref scale, anchor);

            foreach (var c in SelectedBlueprints)
                // TODO: this is temporary and will be fixed with a separate refactor of selection transform logic.
                ((Drawable)c.Item).Scale += scale * 0.02f;

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

            // for now aspect lock scale adjustments that occur at corners.
            if (!reference.HasFlagFast(Anchor.x1) && !reference.HasFlagFast(Anchor.y1))
            {
                // TODO: temporary implementation - only dragging the corner handles across the X axis changes size.
                scale.Y = scale.X;
            }
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
