// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class OsuSelectionScaleHandler : SelectionScaleHandler
    {
        [Resolved]
        private IEditorChangeHandler? changeHandler { get; set; }

        [Resolved(CanBeNull = true)]
        private IDistanceSnapProvider? snapProvider { get; set; }

        private BindableList<HitObject> selectedItems { get; } = new BindableList<HitObject>();

        [BackgroundDependencyLoader]
        private void load(EditorBeatmap editorBeatmap)
        {
            selectedItems.BindTo(editorBeatmap.SelectedHitObjects);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            selectedItems.CollectionChanged += (_, __) => updateState();
            updateState();
        }

        private void updateState()
        {
            var quad = GeometryUtils.GetSurroundingQuad(selectedMovableObjects);
            CanScale.Value = quad.Width > 0 || quad.Height > 0;
        }

        private OsuHitObject[]? objectsInScale;

        private Vector2? defaultOrigin;
        private Dictionary<OsuHitObject, Vector2>? originalPositions;
        private Dictionary<IHasPath, Vector2[]>? originalPathControlPointPositions;
        private Dictionary<IHasPath, PathType?[]>? originalPathControlPointTypes;

        public override void Begin()
        {
            if (objectsInScale != null)
                throw new InvalidOperationException($"Cannot {nameof(Begin)} a scale operation while another is in progress!");

            changeHandler?.BeginChange();

            objectsInScale = selectedMovableObjects.ToArray();
            OriginalSurroundingQuad = objectsInScale.Length == 1 && objectsInScale.First() is Slider slider
                ? GeometryUtils.GetSurroundingQuad(slider.Path.ControlPoints.Select(p => p.Position))
                : GeometryUtils.GetSurroundingQuad(objectsInScale);
            defaultOrigin = OriginalSurroundingQuad.Value.Centre;
            originalPositions = objectsInScale.ToDictionary(obj => obj, obj => obj.Position);
            originalPathControlPointPositions = objectsInScale.OfType<IHasPath>().ToDictionary(
                obj => obj,
                obj => obj.Path.ControlPoints.Select(point => point.Position).ToArray());
            originalPathControlPointTypes = objectsInScale.OfType<IHasPath>().ToDictionary(
                obj => obj,
                obj => obj.Path.ControlPoints.Select(p => p.Type).ToArray());
        }

        public override void Update(Vector2 scale, Vector2? origin = null)
        {
            if (objectsInScale == null)
                throw new InvalidOperationException($"Cannot {nameof(Update)} a scale operation without calling {nameof(Begin)} first!");

            Debug.Assert(originalPositions != null && originalPathControlPointPositions != null && defaultOrigin != null && originalPathControlPointTypes != null && OriginalSurroundingQuad != null);

            Vector2 actualOrigin = origin ?? defaultOrigin.Value;

            // for the time being, allow resizing of slider paths only if the slider is
            // the only hit object selected. with a group selection, it's likely the user
            // is not looking to change the duration of the slider but expand the whole pattern.
            if (objectsInScale.Length == 1 && objectsInScale.First() is Slider slider)
                scaleSlider(slider, scale, originalPathControlPointPositions[slider], originalPathControlPointTypes[slider]);
            else
            {
                scale = getClampedScale(OriginalSurroundingQuad.Value, actualOrigin, scale);

                foreach (var ho in objectsInScale)
                {
                    ho.Position = GeometryUtils.GetScaledPositionMultiply(scale, actualOrigin, originalPositions[ho]);
                }
            }

            moveSelectionInBounds();
        }

        public override void Commit()
        {
            if (objectsInScale == null)
                throw new InvalidOperationException($"Cannot {nameof(Commit)} a rotate operation without calling {nameof(Begin)} first!");

            changeHandler?.EndChange();

            objectsInScale = null;
            OriginalSurroundingQuad = null;
            originalPositions = null;
            originalPathControlPointPositions = null;
            originalPathControlPointTypes = null;
            defaultOrigin = null;
        }

        private IEnumerable<OsuHitObject> selectedMovableObjects => selectedItems.Cast<OsuHitObject>()
                                                                                 .Where(h => h is not Spinner);

        private void scaleSlider(Slider slider, Vector2 scale, Vector2[] originalPathPositions, PathType?[] originalPathTypes)
        {
            // Maintain the path types in case they were defaulted to bezier at some point during scaling
            for (int i = 0; i < slider.Path.ControlPoints.Count; i++)
            {
                slider.Path.ControlPoints[i].Position = originalPathPositions[i] * scale;
                slider.Path.ControlPoints[i].Type = originalPathTypes[i];
            }

            // Snap the slider's length to the current beat divisor
            // to calculate the final resulting duration / bounding box before the final checks.
            slider.SnapTo(snapProvider);

            //if sliderhead or sliderend end up outside playfield, revert scaling.
            Quad scaledQuad = GeometryUtils.GetSurroundingQuad(new OsuHitObject[] { slider });
            (bool xInBounds, bool yInBounds) = isQuadInBounds(scaledQuad);

            if (xInBounds && yInBounds && slider.Path.HasValidLength)
                return;

            for (int i = 0; i < slider.Path.ControlPoints.Count; i++)
                slider.Path.ControlPoints[i].Position = originalPathPositions[i];

            // Snap the slider's length again to undo the potentially-invalid length applied by the previous snap.
            slider.SnapTo(snapProvider);
        }

        private (bool X, bool Y) isQuadInBounds(Quad quad)
        {
            bool xInBounds = (quad.TopLeft.X >= 0) && (quad.BottomRight.X <= OsuPlayfield.BASE_SIZE.X);
            bool yInBounds = (quad.TopLeft.Y >= 0) && (quad.BottomRight.Y <= OsuPlayfield.BASE_SIZE.Y);

            return (xInBounds, yInBounds);
        }

        /// <summary>
        /// Clamp scale for multi-object-scaling where selection does not exceed playfield bounds or flip.
        /// </summary>
        /// <param name="selectionQuad">The quad surrounding the hitobjects</param>
        /// <param name="origin">The origin from which the scale operation is performed</param>
        /// <param name="scale">The scale to be clamped</param>
        /// <returns>The clamped scale vector</returns>
        private Vector2 getClampedScale(Quad selectionQuad, Vector2 origin, Vector2 scale)
        {
            //todo: this is not always correct for selections involving sliders. This approximation assumes each point is scaled independently, but sliderends move with the sliderhead.

            var tl1 = Vector2.Divide(-origin, selectionQuad.TopLeft - origin);
            var tl2 = Vector2.Divide(OsuPlayfield.BASE_SIZE - origin, selectionQuad.TopLeft - origin);
            var br1 = Vector2.Divide(-origin, selectionQuad.BottomRight - origin);
            var br2 = Vector2.Divide(OsuPlayfield.BASE_SIZE - origin, selectionQuad.BottomRight - origin);

            scale.X = selectionQuad.TopLeft.X - origin.X < 0 ? MathHelper.Clamp(scale.X, tl2.X, tl1.X) : MathHelper.Clamp(scale.X, tl1.X, tl2.X);
            scale.Y = selectionQuad.TopLeft.Y - origin.Y < 0 ? MathHelper.Clamp(scale.Y, tl2.Y, tl1.Y) : MathHelper.Clamp(scale.Y, tl1.Y, tl2.Y);
            scale.X = selectionQuad.BottomRight.X - origin.X < 0 ? MathHelper.Clamp(scale.X, br2.X, br1.X) : MathHelper.Clamp(scale.X, br1.X, br2.X);
            scale.Y = selectionQuad.BottomRight.Y - origin.Y < 0 ? MathHelper.Clamp(scale.Y, br2.Y, br1.Y) : MathHelper.Clamp(scale.Y, br1.Y, br2.Y);

            return scale;
        }

        private void moveSelectionInBounds()
        {
            Quad quad = GeometryUtils.GetSurroundingQuad(objectsInScale!);

            Vector2 delta = Vector2.Zero;

            if (quad.TopLeft.X < 0)
                delta.X -= quad.TopLeft.X;
            if (quad.TopLeft.Y < 0)
                delta.Y -= quad.TopLeft.Y;

            if (quad.BottomRight.X > OsuPlayfield.BASE_SIZE.X)
                delta.X -= quad.BottomRight.X - OsuPlayfield.BASE_SIZE.X;
            if (quad.BottomRight.Y > OsuPlayfield.BASE_SIZE.Y)
                delta.Y -= quad.BottomRight.Y - OsuPlayfield.BASE_SIZE.Y;

            foreach (var h in objectsInScale!)
                h.Position += delta;
        }
    }
}
