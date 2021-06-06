// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Extensions;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;
using Humanizer;
using osu.Game.Localisation;

namespace osu.Game.Skinning.Editor
{
    public class SkinSelectionHandler : SelectionHandler<ISkinnableDrawable>
    {
        /// <summary>
        /// <p>Keeps track of whether a <see cref="Drawable"/> is using the closest <see cref="Drawable.Anchor">anchor point</see> within its <see cref="Drawable.Parent">parent</see>,
        /// or whether the user is overriding its anchor point.</p>
        /// <p>Each <see cref="KeyValuePair{TKey,TValue}.Key">key</see> is either a direct cast of an Anchor value, or it is equal to <see cref="hash_of_closest_anchor_menu_item"/>. This is done
        /// because the "Closest" menu item is not a valid anchor, so something other than an anchor must be used.</p>
        /// <p>Each <see cref="KeyValuePair{TKey,TValue}.Value">value</see> is a <see cref="BindableBool"/>. If the <see cref="Bindable{T}.Value"/> is <see langword="false"/>, the user has
        /// overridden the anchor point.
        /// If <see langword="true"/>, the closest anchor point is assigned to the Drawable when it is either dragged by the user via <see cref="HandleMovement"/>, or when "Closest" is assigned from
        /// the anchor context menu via <see cref="applyAnchor"/>.</p>
        /// </summary>
        /// <remarks>
        /// <p>A <see cref="ConditionalWeakTable{TKey,TValue}">ConditionalWeakTable</see> is preferable to a <see cref="Dictionary{TKey,TValue}">Dictionary</see> because a Dictionary will keep
        /// orphaned references to a Drawable forever, unless manually pruned</p>
        /// <p><see cref="BindableBool"/> is used as a thin wrapper around <see cref="Boolean">bool</see> because ConditionalWeakTable requires a reference type as both a key and a value.</p>
        /// </remarks>
        private readonly ConditionalWeakTable<Drawable, BindableBool> isDrawableUsingClosestAnchorLookup = new ConditionalWeakTable<Drawable, BindableBool>();

        /// <summary>
        /// The hash code of the "Closest" menu item in the anchor point context menu.
        /// </summary>
        /// <remarks>This does not need to change with locale; it need only be constant and distinct from any <see cref="Anchor"/> values.</remarks>
        private static readonly int hash_of_closest_anchor_menu_item = @"Closest".GetHashCode();

        /// <remarks>Used by <see cref="load"/> to populate <see cref="localisedAnchorMenuItems"/> and <see cref="localisedOriginMenuItems"/>.</remarks>
        private static readonly LocalisableString[] unbound_anchor_menu_items =
        {
            SkinEditorStrings.Closest,
            SkinEditorStrings.TopLeft,
            SkinEditorStrings.TopCentre,
            SkinEditorStrings.TopRight,
            SkinEditorStrings.CentreLeft,
            SkinEditorStrings.Centre,
            SkinEditorStrings.CentreRight,
            SkinEditorStrings.BottomLeft,
            SkinEditorStrings.BottomCentre,
            SkinEditorStrings.BottomRight,
        };

        /// <remarks>Used by <see cref="load"/> to populate <see cref="localisedAnchorMenuItems"/> and <see cref="localisedOriginMenuItems"/>.</remarks>
        private static readonly int[] anchor_menu_hashes =
            new[]
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
                }
                .Cast<int>()
                .Prepend(hash_of_closest_anchor_menu_item)
                .ToArray();

        private Dictionary<int, ILocalisedBindableString> localisedAnchorMenuItems;
        private Dictionary<int, ILocalisedBindableString> localisedOriginMenuItems;
        private ILocalisedBindableString localisedAnchor;
        private ILocalisedBindableString localisedOrigin;

        [BackgroundDependencyLoader]
        private void load(LocalisationManager localisation)
        {
            localisedAnchor = localisation.GetLocalisedString(SkinEditorStrings.Anchor);
            localisedOrigin = localisation.GetLocalisedString(SkinEditorStrings.Origin);

            var boundAnchorMenuItems = unbound_anchor_menu_items.Select(localisation.GetLocalisedString).ToArray();

            var anchorPairs = anchor_menu_hashes.Zip(boundAnchorMenuItems, (k, v) => new KeyValuePair<int, ILocalisedBindableString>(k, v)).ToArray();
            localisedAnchorMenuItems = new Dictionary<int, ILocalisedBindableString>(anchorPairs);
            var originPairs = anchorPairs.Where(pair => pair.Key != hash_of_closest_anchor_menu_item);
            localisedOriginMenuItems = new Dictionary<int, ILocalisedBindableString>(originPairs);
        }

        private Anchor getClosestAnchorForDrawable(Drawable drawable)
        {
            var parent = drawable.Parent;
            if (parent == null)
                return drawable.Anchor;

            // If there is a better way to get this information, let me know. Was taken from LogoTrackingContainer.ComputeLogoTrackingPosition
            // I tried a lot of different things, such as just using Position / ChildSize, but none of them worked properly.
            var screenPosition = getOriginPositionFromQuad(drawable.ScreenSpaceDrawQuad, drawable.Origin);
            var absolutePosition = parent.ToLocalSpace(screenPosition);
            var factor = parent.RelativeToAbsoluteFactor;

            var result = default(Anchor);

            static Anchor getTieredComponent(float component, Anchor tier0, Anchor tier1, Anchor tier2) =>
                component >= 2 / 3f
                    ? tier2
                    : component >= 1 / 3f
                        ? tier1
                        : tier0;

            result |= getTieredComponent(absolutePosition.X / factor.X, Anchor.x0, Anchor.x1, Anchor.x2);
            result |= getTieredComponent(absolutePosition.Y / factor.Y, Anchor.y0, Anchor.y1, Anchor.y2);

            return result;
        }

        private Vector2 getOriginPositionFromQuad(in Quad quad, Anchor origin)
        {
            var result = quad.TopLeft;

            if (origin.HasFlagFast(Anchor.x2))
                result.X += quad.Width;
            else if (origin.HasFlagFast(Anchor.x1))
                result.X += quad.Width / 2f;

            if (origin.HasFlagFast(Anchor.y2))
                result.Y += quad.Height;
            else if (origin.HasFlagFast(Anchor.y1))
                result.Y += quad.Height / 2f;

            return result;
        }

        /// <remarks>Defaults to <see langword="true"/>, meaning anchors are closest by default.</remarks>
        private BindableBool isDrawableUsingClosestAnchor(Drawable drawable) => isDrawableUsingClosestAnchorLookup.GetValue(drawable, _ => new BindableBool(true));

        // There may be a more generalised form of this somewhere in the codebase. If so, use that.
        private static string getSentenceCaseLocalisedString(ILocalisedBindableString ls) => ls.Value.Transform(To.SentenceCase);

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

            bool shouldAspectLock =
                // for now aspect lock scale adjustments that occur at corners..
                (!anchor.HasFlagFast(Anchor.x1) && !anchor.HasFlagFast(Anchor.y1))
                // ..or if any of the selection have been rotated.
                // this is to avoid requiring skew logic (which would likely not be the user's expected transform anyway).
                || SelectedBlueprints.Any(b => !Precision.AlmostEquals(((Drawable)b.Item).Rotation, 0));

            if (shouldAspectLock)
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
                drawableItem.Scale *= scaledDelta;
            }

            return true;
        }

        public override bool HandleFlip(Direction direction)
        {
            var selectionQuad = getSelectionQuad();

            foreach (var b in SelectedBlueprints)
            {
                var drawableItem = (Drawable)b.Item;

                var flippedPosition = GetFlippedPosition(direction, selectionQuad, b.ScreenSpaceSelectionPoint);

                updateDrawablePosition(drawableItem, flippedPosition);

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

                updateDrawableAnchorIfUsingClosest(drawable);
            }

            return true;
        }

        private void updateDrawableAnchorIfUsingClosest(Drawable drawable)
        {
            if (!isDrawableUsingClosestAnchor(drawable).Value) return;

            var closestAnchor = getClosestAnchorForDrawable(drawable);

            if (closestAnchor == drawable.Anchor) return;

            updateDrawableAnchor(drawable, closestAnchor);
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
            int checkAnchor(Drawable drawable) =>
                isDrawableUsingClosestAnchor(drawable).Value
                    ? hash_of_closest_anchor_menu_item
                    : (int)drawable.Anchor;

            yield return new OsuMenuItem(getSentenceCaseLocalisedString(localisedAnchor))
            {
                Items = createAnchorItems(localisedAnchorMenuItems, checkAnchor, applyAnchor).ToArray()
            };

            yield return new OsuMenuItem(getSentenceCaseLocalisedString(localisedOrigin))
            {
                // Origins can't be "closest" so we just cast to int
                Items = createAnchorItems(localisedOriginMenuItems, d => (int)d.Origin, applyOrigin).ToArray()
            };

            foreach (var item in base.GetContextMenuItemsForSelection(selection))
                yield return item;

            IEnumerable<TernaryStateMenuItem> createAnchorItems(IDictionary<int, ILocalisedBindableString> items, Func<Drawable, int> checkFunction, Action<int> applyFunction) =>
                items.Select(pair =>
                {
                    var (hash, ls) = pair;

                    return new TernaryStateRadioMenuItem(getSentenceCaseLocalisedString(ls), MenuItemType.Standard, _ => applyFunction(hash))
                    {
                        State = { Value = GetStateFromSelection(selection, c => checkFunction((Drawable)c.Item) == hash) }
                    };
                });
        }

        private static void updateDrawablePosition(Drawable drawable, Vector2 screenSpacePosition)
        {
            drawable.Position =
                drawable.Parent.ToLocalSpace(screenSpacePosition) - drawable.AnchorPosition;
        }

        private void applyOrigin(int hash)
        {
            var anchor = (Anchor)hash;

            foreach (var item in SelectedItems)
            {
                var drawable = (Drawable)item;

                var previousOrigin = drawable.OriginPosition;
                drawable.Origin = anchor;
                drawable.Position += drawable.OriginPosition - previousOrigin;

                updateDrawableAnchorIfUsingClosest(drawable);
            }
        }

        /// <summary>
        /// A screen-space quad surrounding all selected drawables, accounting for their full displayed size.
        /// </summary>
        /// <returns></returns>
        private Quad getSelectionQuad() =>
            GetSurroundingQuad(SelectedBlueprints.SelectMany(b => b.Item.ScreenSpaceDrawQuad.GetVertices().ToArray()));

        private void applyAnchor(int hash)
        {
            foreach (var item in SelectedItems)
            {
                var drawable = (Drawable)item;

                var anchor = getAnchorFromHashAndDrawableAndRecordWhetherUsingClosestAnchor(hash, drawable);

                updateDrawableAnchor(drawable, anchor);
            }
        }

        private static void updateDrawableAnchor(Drawable drawable, Anchor anchor)
        {
            var previousAnchor = drawable.AnchorPosition;
            drawable.Anchor = anchor;
            drawable.Position -= drawable.AnchorPosition - previousAnchor;
        }

        private Anchor getAnchorFromHashAndDrawableAndRecordWhetherUsingClosestAnchor(int hash, Drawable drawable)
        {
            var isUsingClosestAnchor = isDrawableUsingClosestAnchor(drawable);

            if (hash == hash_of_closest_anchor_menu_item)
            {
                isUsingClosestAnchor.Value = true;
                return getClosestAnchorForDrawable(drawable);
            }

            isUsingClosestAnchor.Value = false;
            return (Anchor)hash;
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
