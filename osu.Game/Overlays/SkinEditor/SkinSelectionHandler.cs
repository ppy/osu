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

        public override SelectionScaleHandler CreateScaleHandler()
        {
            var scaleHandler = new SkinSelectionScaleHandler
            {
                UpdatePosition = updateDrawablePosition
            };

            scaleHandler.PerformFlipFromScaleHandles += a => SelectionBox.PerformFlipFromScaleHandles(a);

            return scaleHandler;
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
    }
}
