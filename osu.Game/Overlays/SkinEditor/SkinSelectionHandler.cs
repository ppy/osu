// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Extensions;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Localisation;
using osu.Game.Skinning;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Overlays.SkinEditor
{
    public partial class SkinSelectionHandler : SelectionHandler<ISerialisableDrawable>
    {
        private OsuMenuItem? originMenu;

        private TernaryStateRadioMenuItem? closestAnchor;
        private AnchorMenuItem[]? fixedAnchors;

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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (ChangeHandler != null)
                ChangeHandler.OnStateChange += updateTernaryStates;
            SelectedItems.BindCollectionChanged((_, _) => updateTernaryStates());
        }

        private void updateTernaryStates()
        {
            var usingClosestAnchor = GetStateFromSelection(SelectedBlueprints, c => !c.Item.UsesFixedAnchor);

            if (closestAnchor != null)
                closestAnchor.State.Value = usingClosestAnchor;

            if (fixedAnchors != null)
            {
                foreach (var fixedAnchor in fixedAnchors)
                    fixedAnchor.State.Value = GetStateFromSelection(SelectedBlueprints, c => c.Item.UsesFixedAnchor && ((Drawable)c.Item).Anchor == fixedAnchor.Anchor);
            }

            if (originMenu != null)
            {
                foreach (var origin in originMenu.Items.OfType<AnchorMenuItem>())
                {
                    origin.State.Value = GetStateFromSelection(SelectedBlueprints, c => ((Drawable)c.Item).Origin == origin.Anchor);
                    origin.Action.Disabled = usingClosestAnchor == TernaryState.True;
                }
            }
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

                if (!item.UsesFixedAnchor)
                    ApplyClosestAnchorOrigin(drawable);

                drawable.Position += drawable.ScreenSpaceDeltaToParentSpace(moveEvent.ScreenSpaceDelta);
            }

            return true;
        }

        public static void ApplyClosestAnchorOrigin(Drawable drawable)
        {
            var closest = getClosestAnchor(drawable);

            applyAnchor(drawable, closest);
            applyOrigin(drawable, closest);
        }

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
            closestAnchor = new TernaryStateRadioMenuItem(SkinEditorStrings.Closest, MenuItemType.Standard, _ => applyClosestAnchors());
            fixedAnchors = createAnchorItems(applyFixedAnchors).ToArray();

            yield return new OsuMenuItem(SkinEditorStrings.Anchor)
            {
                Items = fixedAnchors.Prepend(closestAnchor).ToArray()
            };

            yield return originMenu = new OsuMenuItem(SkinEditorStrings.Origin);

            originMenu.Items = createAnchorItems(applyOrigins).ToArray();

            yield return new OsuMenuItemSpacer();

            yield return new OsuMenuItem(SkinEditorStrings.ResetPosition, MenuItemType.Standard, () =>
            {
                foreach (var blueprint in SelectedBlueprints)
                    ((Drawable)blueprint.Item).Position = Vector2.Zero;
            });

            yield return new OsuMenuItem(SkinEditorStrings.ResetRotation, MenuItemType.Standard, () =>
            {
                foreach (var blueprint in SelectedBlueprints)
                    ((Drawable)blueprint.Item).Rotation = 0;
            });

            yield return new OsuMenuItem(SkinEditorStrings.ResetScale, MenuItemType.Standard, () =>
            {
                foreach (var blueprint in SelectedBlueprints)
                {
                    var blueprintItem = ((Drawable)blueprint.Item);
                    blueprintItem.Scale = Vector2.One;

                    if (blueprintItem.RelativeSizeAxes.HasFlag(Axes.X))
                        blueprintItem.Width = 1;
                    if (blueprintItem.RelativeSizeAxes.HasFlag(Axes.Y))
                        blueprintItem.Height = 1;
                }
            });

            yield return new OsuMenuItemSpacer();

            yield return new OsuMenuItem(SkinEditorStrings.BringToFront, MenuItemType.Standard, () => skinEditor.BringSelectionToFront());

            yield return new OsuMenuItem(SkinEditorStrings.SendToBack, MenuItemType.Standard, () => skinEditor.SendSelectionToBack());

            yield return new OsuMenuItemSpacer();

            foreach (var item in base.GetContextMenuItemsForSelection(selection))
                yield return item;

            updateTernaryStates();
        }

        private IEnumerable<AnchorMenuItem> createAnchorItems(Action<Anchor> applyFunction)
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
                return new AnchorMenuItem(a, _ => applyFunction(a));
            });
        }

        private partial class AnchorMenuItem : TernaryStateRadioMenuItem
        {
            public readonly Anchor Anchor;

            public AnchorMenuItem(Anchor anchor, Action<Anchor> applyFunction)
                : base(anchor.ToString(), MenuItemType.Standard, _ => applyFunction(anchor))
            {
                Anchor = anchor;
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

                applyOrigin(drawable, origin);

                if (!item.UsesFixedAnchor)
                    ApplyClosestAnchorOrigin(drawable);
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
                ApplyClosestAnchorOrigin((Drawable)item);
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

        private static void applyOrigin(Drawable drawable, Anchor screenSpaceOrigin)
        {
            var boundingBox = drawable.ScreenSpaceDrawQuad.AABBFloat;

            var targetScreenSpacePosition = screenSpaceOrigin.PositionOnQuad(boundingBox);

            Anchor localOrigin = Anchor.TopLeft;
            float smallestDistanceFromTargetPosition = float.PositiveInfinity;

            void checkOrigin(Anchor originToTest)
            {
                Vector2 positionToTest = drawable.ToScreenSpace(originToTest.PositionOnQuad(drawable.DrawRectangle));
                float testedDistance = Vector2.Distance(targetScreenSpacePosition, positionToTest);

                if (testedDistance < smallestDistanceFromTargetPosition)
                {
                    localOrigin = originToTest;
                    smallestDistanceFromTargetPosition = testedDistance;
                }
            }

            checkOrigin(Anchor.TopLeft);
            checkOrigin(Anchor.TopCentre);
            checkOrigin(Anchor.TopRight);

            checkOrigin(Anchor.CentreLeft);
            checkOrigin(Anchor.Centre);
            checkOrigin(Anchor.CentreRight);

            checkOrigin(Anchor.BottomLeft);
            checkOrigin(Anchor.BottomCentre);
            checkOrigin(Anchor.BottomRight);

            Vector2 offset = drawable.ToParentSpace(localOrigin.PositionOnQuad(drawable.DrawRectangle)) - drawable.ToParentSpace(drawable.Origin.PositionOnQuad(drawable.DrawRectangle));

            drawable.Origin = localOrigin;
            drawable.Position += offset;
        }
    }
}
