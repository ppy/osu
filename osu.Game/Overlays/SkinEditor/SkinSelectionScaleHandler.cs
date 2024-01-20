// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Skinning;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Overlays.SkinEditor
{
    public partial class SkinSelectionScaleHandler : SelectionScaleHandler
    {
        public Action<Drawable, Vector2> UpdatePosition { get; init; } = null!;

        public event Action<Axes>? PerformFlipFromScaleHandles;

        [Resolved]
        private IEditorChangeHandler? changeHandler { get; set; }

        private BindableList<ISerialisableDrawable> selectedItems { get; } = new BindableList<ISerialisableDrawable>();

        [BackgroundDependencyLoader]
        private void load(SkinEditor skinEditor)
        {
            selectedItems.BindTo(skinEditor.SelectedComponents);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            selectedItems.CollectionChanged += (_, __) => updateState();
            updateState();
        }

        private void updateState()
        {
            CanScaleX.Value = allSelectedSupportManualSizing(Axes.X);
            CanScaleY.Value = allSelectedSupportManualSizing(Axes.Y);
            CanScaleDiagonally.Value = true;
        }

        private bool allSelectedSupportManualSizing(Axes axis) => selectedItems.All(b => (b as CompositeDrawable)?.AutoSizeAxes.HasFlagFast(axis) == false);

        private Drawable[]? objectsInScale;

        private Vector2? defaultOrigin;
        private Dictionary<Drawable, float>? originalWidths;
        private Dictionary<Drawable, float>? originalHeights;
        private Dictionary<Drawable, Vector2>? originalScales;
        private Dictionary<Drawable, Vector2>? originalPositions;

        public override void Begin()
        {
            if (objectsInScale != null)
                throw new InvalidOperationException($"Cannot {nameof(Begin)} a scale operation while another is in progress!");

            changeHandler?.BeginChange();

            objectsInScale = selectedItems.Cast<Drawable>().ToArray();
            originalWidths = objectsInScale.ToDictionary(d => d, d => d.Width);
            originalHeights = objectsInScale.ToDictionary(d => d, d => d.Height);
            originalScales = objectsInScale.ToDictionary(d => d, d => d.Scale);
            originalPositions = objectsInScale.ToDictionary(d => d, d => d.ToScreenSpace(d.OriginPosition));
            OriginalSurroundingQuad = GeometryUtils.GetSurroundingQuad(objectsInScale.SelectMany(d => d.ScreenSpaceDrawQuad.GetVertices().ToArray()));
            defaultOrigin = OriginalSurroundingQuad.Value.Centre;
        }

        public override void Update(Vector2 scale, Vector2? origin = null)
        {
            if (objectsInScale == null)
                throw new InvalidOperationException($"Cannot {nameof(Update)} a scale operation without calling {nameof(Begin)} first!");

            Debug.Assert(originalWidths != null && originalHeights != null && originalScales != null && originalPositions != null && defaultOrigin != null && OriginalSurroundingQuad != null);

            var actualOrigin = origin ?? defaultOrigin.Value;

            Axes adjustAxis = scale.X == 0 ? Axes.Y : scale.Y == 0 ? Axes.X : Axes.Both;

            if ((adjustAxis == Axes.Y && !allSelectedSupportManualSizing(Axes.Y)) ||
                (adjustAxis == Axes.X && !allSelectedSupportManualSizing(Axes.X)))
                return;

            // the selection quad is always upright, so use an AABB rect to make mutating the values easier.
            var selectionRect = OriginalSurroundingQuad.Value.AABBFloat;

            // If the selection has no area we cannot scale it
            if (selectionRect.Area == 0)
                return;

            // copy to mutate, as we will need to compare to the original later on.
            var adjustedRect = selectionRect;

            // for now aspect lock scale adjustments that occur at corners..
            if (adjustAxis == Axes.Both)
            {
                // project scale vector along diagonal
                Vector2 diag = new Vector2(1, 1).Normalized();
                scale = Vector2.Dot(scale, diag) * diag;
            }
            // ..or if any of the selection have been rotated.
            // this is to avoid requiring skew logic (which would likely not be the user's expected transform anyway).
            else if (objectsInScale.Any(b => !Precision.AlmostEquals(b.Rotation % 90, 0)))
            {
                if (adjustAxis == Axes.Y)
                    // if dragging from the horizontal centre, only a vertical component is available.
                    scale.X = scale.Y / selectionRect.Height * selectionRect.Width;
                else
                    // in all other cases (arbitrarily) use the horizontal component for aspect lock.
                    scale.Y = scale.X / selectionRect.Width * selectionRect.Height;
            }

            adjustedRect.Location = GeometryUtils.GetScaledPosition(scale, actualOrigin, OriginalSurroundingQuad!.Value.TopLeft);
            adjustedRect.Size = OriginalSurroundingQuad!.Value.Size * scale;

            if (adjustedRect.Width <= 0 || adjustedRect.Height <= 0)
            {
                Axes toFlip = Axes.None;

                if (adjustedRect.Width <= 0) toFlip |= Axes.X;
                if (adjustedRect.Height <= 0) toFlip |= Axes.Y;

                PerformFlipFromScaleHandles?.Invoke(toFlip);
                return;
            }

            // scale adjust applied to each individual item should match that of the quad itself.
            var scaledDelta = new Vector2(
                adjustedRect.Width / selectionRect.Width,
                adjustedRect.Height / selectionRect.Height
            );

            foreach (var b in objectsInScale)
            {
                // each drawable's relative position should be maintained in the scaled quad.
                var screenPosition = originalPositions[b];

                var relativePositionInOriginal =
                    new Vector2(
                        (screenPosition.X - selectionRect.TopLeft.X) / selectionRect.Width,
                        (screenPosition.Y - selectionRect.TopLeft.Y) / selectionRect.Height
                    );

                var newPositionInAdjusted = new Vector2(
                    adjustedRect.TopLeft.X + adjustedRect.Width * relativePositionInOriginal.X,
                    adjustedRect.TopLeft.Y + adjustedRect.Height * relativePositionInOriginal.Y
                );

                UpdatePosition(b, newPositionInAdjusted);

                var currentScaledDelta = scaledDelta;
                if (Precision.AlmostEquals(MathF.Abs(b.Rotation) % 180, 90))
                    currentScaledDelta = new Vector2(scaledDelta.Y, scaledDelta.X);

                switch (adjustAxis)
                {
                    case Axes.X:
                        b.Width = originalWidths[b] * currentScaledDelta.X;
                        break;

                    case Axes.Y:
                        b.Height = originalHeights[b] * currentScaledDelta.Y;
                        break;

                    case Axes.Both:
                        b.Scale = originalScales[b] * currentScaledDelta;
                        break;
                }
            }
        }

        public override void Commit()
        {
            if (objectsInScale == null)
                throw new InvalidOperationException($"Cannot {nameof(Commit)} a scale operation without calling {nameof(Begin)} first!");

            changeHandler?.EndChange();

            objectsInScale = null;
            originalPositions = null;
            originalWidths = null;
            originalHeights = null;
            originalScales = null;
            defaultOrigin = null;
        }
    }
}
