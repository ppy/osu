// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Utils;
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
        /// <summary>
        /// Whether scaling anchored by the center of the playfield can currently be performed.
        /// </summary>
        public Bindable<bool> CanScaleFromPlayfieldOrigin { get; private set; } = new BindableBool();

        /// <summary>
        /// Whether a single slider is currently selected, which results in a different scaling behaviour.
        /// </summary>
        public Bindable<bool> IsScalingSlider { get; private set; } = new BindableBool();

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

            CanScaleX.Value = quad.Width > 0;
            CanScaleY.Value = quad.Height > 0;
            CanScaleDiagonally.Value = CanScaleX.Value && CanScaleY.Value;
            CanScaleFromPlayfieldOrigin.Value = selectedMovableObjects.Any();
            IsScalingSlider.Value = selectedMovableObjects.Count() == 1 && selectedMovableObjects.First() is Slider;
        }

        private Dictionary<OsuHitObject, OriginalHitObjectState>? objectsInScale;
        private Vector2? defaultOrigin;

        public override void Begin()
        {
            if (OperationInProgress.Value)
                throw new InvalidOperationException($"Cannot {nameof(Begin)} a scale operation while another is in progress!");

            base.Begin();

            changeHandler?.BeginChange();

            objectsInScale = selectedMovableObjects.ToDictionary(ho => ho, ho => new OriginalHitObjectState(ho));
            OriginalSurroundingQuad = objectsInScale.Count == 1 && objectsInScale.First().Key is Slider slider
                ? GeometryUtils.GetSurroundingQuad(slider.Path.ControlPoints.Select(p => slider.Position + p.Position))
                : GeometryUtils.GetSurroundingQuad(objectsInScale.Keys);
            defaultOrigin = OriginalSurroundingQuad.Value.Centre;
        }

        public override void Update(Vector2 scale, Vector2? origin = null, Axes adjustAxis = Axes.Both)
        {
            if (!OperationInProgress.Value)
                throw new InvalidOperationException($"Cannot {nameof(Update)} a scale operation without calling {nameof(Begin)} first!");

            Debug.Assert(objectsInScale != null && defaultOrigin != null && OriginalSurroundingQuad != null);

            Vector2 actualOrigin = origin ?? defaultOrigin.Value;

            // for the time being, allow resizing of slider paths only if the slider is
            // the only hit object selected. with a group selection, it's likely the user
            // is not looking to change the duration of the slider but expand the whole pattern.
            if (objectsInScale.Count == 1 && objectsInScale.First().Key is Slider slider)
            {
                var originalInfo = objectsInScale[slider];
                Debug.Assert(originalInfo.PathControlPointPositions != null && originalInfo.PathControlPointTypes != null);
                scaleSlider(slider, scale, originalInfo.PathControlPointPositions, originalInfo.PathControlPointTypes);
            }
            else
            {
                scale = ClampScaleToPlayfieldBounds(scale, actualOrigin);

                foreach (var (ho, originalState) in objectsInScale)
                {
                    ho.Position = GeometryUtils.GetScaledPosition(scale, actualOrigin, originalState.Position);
                }
            }

            moveSelectionInBounds();
        }

        public override void Commit()
        {
            if (!OperationInProgress.Value)
                throw new InvalidOperationException($"Cannot {nameof(Commit)} a rotate operation without calling {nameof(Begin)} first!");

            changeHandler?.EndChange();

            base.Commit();

            objectsInScale = null;
            OriginalSurroundingQuad = null;
            defaultOrigin = null;
        }

        private IEnumerable<OsuHitObject> selectedMovableObjects => selectedItems.Cast<OsuHitObject>()
                                                                                 .Where(h => h is not Spinner);

        private void scaleSlider(Slider slider, Vector2 scale, Vector2[] originalPathPositions, PathType?[] originalPathTypes)
        {
            scale = Vector2.ComponentMax(scale, new Vector2(Precision.FLOAT_EPSILON));

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
        /// <param name="origin">The origin from which the scale operation is performed</param>
        /// <param name="scale">The scale to be clamped</param>
        /// <returns>The clamped scale vector</returns>
        public Vector2 ClampScaleToPlayfieldBounds(Vector2 scale, Vector2? origin = null)
        {
            //todo: this is not always correct for selections involving sliders. This approximation assumes each point is scaled independently, but sliderends move with the sliderhead.
            if (objectsInScale == null)
                return scale;

            Debug.Assert(defaultOrigin != null && OriginalSurroundingQuad != null);

            if (objectsInScale.Count == 1 && objectsInScale.First().Key is Slider slider)
                origin = slider.Position;

            Vector2 actualOrigin = origin ?? defaultOrigin.Value;
            var selectionQuad = OriginalSurroundingQuad.Value;

            var tl1 = Vector2.Divide(-actualOrigin, selectionQuad.TopLeft - actualOrigin);
            var tl2 = Vector2.Divide(OsuPlayfield.BASE_SIZE - actualOrigin, selectionQuad.TopLeft - actualOrigin);
            var br1 = Vector2.Divide(-actualOrigin, selectionQuad.BottomRight - actualOrigin);
            var br2 = Vector2.Divide(OsuPlayfield.BASE_SIZE - actualOrigin, selectionQuad.BottomRight - actualOrigin);

            if (!Precision.AlmostEquals(selectionQuad.TopLeft.X - actualOrigin.X, 0))
                scale.X = selectionQuad.TopLeft.X - actualOrigin.X < 0 ? MathHelper.Clamp(scale.X, tl2.X, tl1.X) : MathHelper.Clamp(scale.X, tl1.X, tl2.X);
            if (!Precision.AlmostEquals(selectionQuad.TopLeft.Y - actualOrigin.Y, 0))
                scale.Y = selectionQuad.TopLeft.Y - actualOrigin.Y < 0 ? MathHelper.Clamp(scale.Y, tl2.Y, tl1.Y) : MathHelper.Clamp(scale.Y, tl1.Y, tl2.Y);
            if (!Precision.AlmostEquals(selectionQuad.BottomRight.X - actualOrigin.X, 0))
                scale.X = selectionQuad.BottomRight.X - actualOrigin.X < 0 ? MathHelper.Clamp(scale.X, br2.X, br1.X) : MathHelper.Clamp(scale.X, br1.X, br2.X);
            if (!Precision.AlmostEquals(selectionQuad.BottomRight.Y - actualOrigin.Y, 0))
                scale.Y = selectionQuad.BottomRight.Y - actualOrigin.Y < 0 ? MathHelper.Clamp(scale.Y, br2.Y, br1.Y) : MathHelper.Clamp(scale.Y, br1.Y, br2.Y);

            return Vector2.ComponentMax(scale, new Vector2(Precision.FLOAT_EPSILON));
        }

        private void moveSelectionInBounds()
        {
            Quad quad = GeometryUtils.GetSurroundingQuad(objectsInScale!.Keys);

            Vector2 delta = Vector2.Zero;

            if (quad.TopLeft.X < 0)
                delta.X -= quad.TopLeft.X;
            if (quad.TopLeft.Y < 0)
                delta.Y -= quad.TopLeft.Y;

            if (quad.BottomRight.X > OsuPlayfield.BASE_SIZE.X)
                delta.X -= quad.BottomRight.X - OsuPlayfield.BASE_SIZE.X;
            if (quad.BottomRight.Y > OsuPlayfield.BASE_SIZE.Y)
                delta.Y -= quad.BottomRight.Y - OsuPlayfield.BASE_SIZE.Y;

            foreach (var (h, _) in objectsInScale!)
                h.Position += delta;
        }

        private struct OriginalHitObjectState
        {
            public Vector2 Position { get; }
            public Vector2[]? PathControlPointPositions { get; }
            public PathType?[]? PathControlPointTypes { get; }

            public OriginalHitObjectState(OsuHitObject hitObject)
            {
                Position = hitObject.Position;
                PathControlPointPositions = (hitObject as IHasPath)?.Path.ControlPoints.Select(p => p.Position).ToArray();
                PathControlPointTypes = (hitObject as IHasPath)?.Path.ControlPoints.Select(p => p.Type).ToArray();
            }
        }
    }
}
