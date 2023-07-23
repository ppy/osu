// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Skinning;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Overlays.SkinEditor
{
    public class SkinSelectionRotationHandler : SelectionRotationHandler
    {
        private readonly IEditorChangeHandler? changeHandler;

        public BindableList<ISerialisableDrawable> SelectedItems { get; } = new BindableList<ISerialisableDrawable>();
        public Action<Drawable, Vector2> UpdatePosition { get; init; } = null!;

        public SkinSelectionRotationHandler(IEditorChangeHandler? changeHandler)
        {
            this.changeHandler = changeHandler;

            SelectedItems.CollectionChanged += (_, __) => updateState();
        }

        private void updateState()
        {
            CanRotate.Value = SelectedItems.Count > 0;
        }

        private Drawable[]? objectsInRotation;

        private Vector2? defaultOrigin;
        private Dictionary<Drawable, float>? originalRotations;
        private Dictionary<Drawable, Vector2>? originalPositions;

        public override void Begin()
        {
            if (objectsInRotation != null)
                throw new InvalidOperationException($"Cannot {nameof(Begin)} a rotate operation while another is in progress!");

            changeHandler?.BeginChange();

            objectsInRotation = SelectedItems.Cast<Drawable>().ToArray();
            originalRotations = objectsInRotation.ToDictionary(d => d, d => d.Rotation);
            originalPositions = objectsInRotation.ToDictionary(d => d, d => d.ToScreenSpace(d.OriginPosition));
            defaultOrigin = GeometryUtils.GetSurroundingQuad(objectsInRotation.SelectMany(d => d.ScreenSpaceDrawQuad.GetVertices().ToArray())).Centre;
        }

        public override void Update(float rotation, Vector2? origin = null)
        {
            if (objectsInRotation == null)
                throw new InvalidOperationException($"Cannot {nameof(Update)} a rotate operation without calling {nameof(Begin)} first!");

            Debug.Assert(originalRotations != null && originalPositions != null && defaultOrigin != null);

            if (objectsInRotation.Length == 1 && origin == null)
            {
                // for single items, rotate around the origin rather than the selection centre by default.
                objectsInRotation[0].Rotation = originalRotations.Single().Value + rotation;
                return;
            }

            var actualOrigin = origin ?? defaultOrigin.Value;

            foreach (var drawableItem in objectsInRotation)
            {
                var rotatedPosition = GeometryUtils.RotatePointAroundOrigin(originalPositions[drawableItem], actualOrigin, rotation);
                UpdatePosition(drawableItem, rotatedPosition);

                drawableItem.Rotation = originalRotations[drawableItem] + rotation;
            }
        }

        public override void Commit()
        {
            if (objectsInRotation == null)
                throw new InvalidOperationException($"Cannot {nameof(Commit)} a rotate operation without calling {nameof(Begin)} first!");

            changeHandler?.EndChange();

            objectsInRotation = null;
            originalPositions = null;
            originalRotations = null;
            defaultOrigin = null;
        }
    }
}
