// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class OsuSelectionRotationHandler : SelectionRotationHandler
    {
        [Resolved]
        private IEditorChangeHandler? changeHandler { get; set; }

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
            CanRotateSelectionOrigin.Value = quad.Width > 0 || quad.Height > 0;
            CanRotatePlayfieldOrigin.Value = selectedMovableObjects.Any();
        }

        private OsuHitObject[]? objectsInRotation;

        private Vector2? defaultOrigin;
        private Dictionary<OsuHitObject, Vector2>? originalPositions;
        private Dictionary<IHasPath, Vector2[]>? originalPathControlPointPositions;

        public override void Begin()
        {
            if (objectsInRotation != null)
                throw new InvalidOperationException($"Cannot {nameof(Begin)} a rotate operation while another is in progress!");

            changeHandler?.BeginChange();

            objectsInRotation = selectedMovableObjects.ToArray();
            defaultOrigin = GeometryUtils.GetSurroundingQuad(objectsInRotation).Centre;
            originalPositions = objectsInRotation.ToDictionary(obj => obj, obj => obj.Position);
            originalPathControlPointPositions = objectsInRotation.OfType<IHasPath>().ToDictionary(
                obj => obj,
                obj => obj.Path.ControlPoints.Select(point => point.Position).ToArray());
        }

        public override void Update(float rotation, Vector2? origin = null)
        {
            if (objectsInRotation == null)
                throw new InvalidOperationException($"Cannot {nameof(Update)} a rotate operation without calling {nameof(Begin)} first!");

            Debug.Assert(originalPositions != null && originalPathControlPointPositions != null && defaultOrigin != null);

            Vector2 actualOrigin = origin ?? defaultOrigin.Value;

            foreach (var ho in objectsInRotation)
            {
                ho.Position = GeometryUtils.RotatePointAroundOrigin(originalPositions[ho], actualOrigin, rotation);

                if (ho is IHasPath withPath)
                {
                    var originalPath = originalPathControlPointPositions[withPath];

                    for (int i = 0; i < withPath.Path.ControlPoints.Count; ++i)
                        withPath.Path.ControlPoints[i].Position = GeometryUtils.RotatePointAroundOrigin(originalPath[i], Vector2.Zero, rotation);
                }
            }
        }

        public override void Commit()
        {
            if (objectsInRotation == null)
                throw new InvalidOperationException($"Cannot {nameof(Commit)} a rotate operation without calling {nameof(Begin)} first!");

            changeHandler?.EndChange();

            objectsInRotation = null;
            originalPositions = null;
            originalPathControlPointPositions = null;
            defaultOrigin = null;
        }

        private IEnumerable<OsuHitObject> selectedMovableObjects => selectedItems.Cast<OsuHitObject>()
                                                                                 .Where(h => h is not Spinner);
    }
}
