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

        private bool isFlippedX;
        private bool isFlippedY;

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
            OriginalSurroundingQuad = ToLocalSpace(GeometryUtils.GetSurroundingQuad(objectsInScale.SelectMany(d => d.ScreenSpaceDrawQuad.GetVertices().ToArray())));
            defaultOrigin = OriginalSurroundingQuad.Value.Centre;

            isFlippedX = false;
            isFlippedY = false;
        }

        public override void Update(Vector2 scale, Vector2? origin = null, Axes adjustAxis = Axes.Both)
        {
            if (objectsInScale == null)
                throw new InvalidOperationException($"Cannot {nameof(Update)} a scale operation without calling {nameof(Begin)} first!");

            Debug.Assert(originalWidths != null && originalHeights != null && originalScales != null && originalPositions != null && defaultOrigin != null && OriginalSurroundingQuad != null);

            var actualOrigin = ToScreenSpace(origin ?? defaultOrigin.Value);

            if ((adjustAxis == Axes.Y && !allSelectedSupportManualSizing(Axes.Y)) ||
                (adjustAxis == Axes.X && !allSelectedSupportManualSizing(Axes.X)))
                return;

            // If the selection has no area we cannot scale it
            if (OriginalSurroundingQuad.Value.Width == 0 || OriginalSurroundingQuad.Value.Height == 0)
                return;

            // for now aspect lock scale adjustments that occur at corners.
            if (adjustAxis == Axes.Both)
            {
                // project scale vector along diagonal
                scale = new Vector2((scale.X + scale.Y) * 0.5f);
            }
            // If any of the selection have been rotated and the adjust axis is not both,
            // we would require skew logic to achieve a correct image editor-like scale.
            // For now we just ignore, because it would likely not be the user's expected transform anyway.

            bool flippedX = scale.X < 0;
            bool flippedY = scale.Y < 0;
            Axes toFlip = Axes.None;

            if (flippedX != isFlippedX)
            {
                isFlippedX = flippedX;
                toFlip |= Axes.X;
            }

            if (flippedY != isFlippedY)
            {
                isFlippedY = flippedY;
                toFlip |= Axes.Y;
            }

            if (toFlip != Axes.None)
            {
                PerformFlipFromScaleHandles?.Invoke(toFlip);
                return;
            }

            foreach (var b in objectsInScale)
            {
                UpdatePosition(b, GeometryUtils.GetScaledPosition(scale, actualOrigin, originalPositions[b]));

                var currentScale = scale;
                if (Precision.AlmostEquals(MathF.Abs(b.Rotation) % 180, 90))
                    currentScale = new Vector2(scale.Y, scale.X);

                switch (adjustAxis)
                {
                    case Axes.X:
                        b.Width = MathF.Abs(originalWidths[b] * currentScale.X);
                        break;

                    case Axes.Y:
                        b.Height = MathF.Abs(originalHeights[b] * currentScale.Y);
                        break;

                    case Axes.Both:
                        b.Scale = originalScales[b] * currentScale;
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
