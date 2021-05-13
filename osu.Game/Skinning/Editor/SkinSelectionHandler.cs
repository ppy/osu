// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
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
    public class SkinSelectionHandler : SelectionHandler<ISkinnableComponent>
    {
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
            // TODO: this is temporary as well.
            foreach (var c in SelectedBlueprints)
            {
                ((Drawable)c.Item).Scale *= new Vector2(
                    direction == Direction.Horizontal ? -1 : 1,
                    direction == Direction.Vertical ? -1 : 1
                );
            }

            return true;
        }

        public override bool HandleMovement(MoveSelectionEvent<ISkinnableComponent> moveEvent)
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

        protected override void DeleteItems(IEnumerable<ISkinnableComponent> items)
        {
            foreach (var i in items)
            {
                ((Drawable)i).Expire();
                SelectedItems.Remove(i);
            }
        }

        protected override IEnumerable<MenuItem> GetContextMenuItemsForSelection(IEnumerable<SelectionBlueprint<ISkinnableComponent>> selection)
        {
            yield return new OsuMenuItem("Anchor")
            {
                Items = createAnchorItems().ToArray()
            };

            foreach (var item in base.GetContextMenuItemsForSelection(selection))
                yield return item;

            IEnumerable<AnchorMenuItem> createAnchorItems()
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
                    return new AnchorMenuItem(a, selection, _ => applyAnchor(a))
                    {
                        State = { Value = GetStateFromSelection(selection, c => ((Drawable)c.Item).Anchor == a) }
                    };
                });
            }
        }

        private void applyAnchor(Anchor anchor)
        {
            foreach (var item in SelectedItems)
                ((Drawable)item).Anchor = anchor;
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
                if (reference.HasFlagFast(Anchor.x0) || reference.HasFlagFast(Anchor.x2))
                    scale.Y = scale.X;
                else
                    scale.X = scale.Y;
            }
        }

        public class AnchorMenuItem : TernaryStateMenuItem
        {
            public AnchorMenuItem(Anchor anchor, IEnumerable<SelectionBlueprint<ISkinnableComponent>> selection, Action<TernaryState> action)
                : base(anchor.ToString(), getNextState, MenuItemType.Standard, action)
            {
            }

            private static TernaryState getNextState(TernaryState state) => TernaryState.True;
        }
    }
}
