// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public partial class EditorBlueprintContainer : BlueprintContainer<HitObject>
    {
        [Resolved]
        protected EditorClock EditorClock { get; private set; }

        [Resolved]
        protected EditorBeatmap EditorBeatmap { get; private set; }

        protected readonly HitObjectComposer Composer;

        private HitObjectUsageEventBuffer usageEventBuffer;

        protected InputManager InputManager { get; private set; }

        protected EditorBlueprintContainer(HitObjectComposer composer)
        {
            Composer = composer;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            SelectedItems.BindTo(EditorBeatmap.SelectedHitObjects);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            InputManager = GetContainingInputManager();

            EditorBeatmap.HitObjectAdded += AddBlueprintFor;
            EditorBeatmap.HitObjectRemoved += RemoveBlueprintFor;

            if (Composer != null)
            {
                foreach (var obj in Composer.HitObjects)
                    AddBlueprintFor(obj.HitObject);

                usageEventBuffer = new HitObjectUsageEventBuffer(Composer.Playfield);
                usageEventBuffer.HitObjectUsageBegan += AddBlueprintFor;
                usageEventBuffer.HitObjectUsageFinished += RemoveBlueprintFor;
                usageEventBuffer.HitObjectUsageTransferred += TransferBlueprintFor;
            }
        }

        protected override void BeginChange()
        {
            // If the editor beatmap has a change handler then this will automatically call BeginChange() on it.
            EditorBeatmap.BeginChange();
        }

        protected override void EndChange()
        {
            // If the editor beatmap has a change handler then this will automatically call EndChange() on it.
            EditorBeatmap.EndChange();
        }

        protected override void Update()
        {
            base.Update();
            usageEventBuffer?.Update();
        }

        protected override IEnumerable<SelectionBlueprint<HitObject>> SortForMovement(IReadOnlyList<SelectionBlueprint<HitObject>> blueprints)
            => blueprints.OrderBy(b => b.Item.StartTime);

        protected override bool ApplySnapResult(SelectionBlueprint<HitObject>[] blueprints, SnapResult result)
        {
            if (!base.ApplySnapResult(blueprints, result))
                return false;

            if (result.Time.HasValue)
            {
                // Apply the start time at the newly snapped-to position
                double offset = result.Time.Value - blueprints.First().Item.StartTime;

                if (offset != 0)
                {
                    EditorBeatmap.PerformOnSelection(obj =>
                    {
                        obj.StartTime += offset;
                        EditorBeatmap.Update(obj);
                    });
                }
            }

            return true;
        }

        protected override void AddBlueprintFor(HitObject item)
        {
            if (item is IBarLine)
                return;

            base.AddBlueprintFor(item);
        }

        /// <summary>
        /// Invoked when a <see cref="HitObject"/> has been transferred to another <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <param name="hitObject">The hit object which has been assigned to a new drawable.</param>
        /// <param name="drawableObject">The new drawable that is representing the hit object.</param>
        protected virtual void TransferBlueprintFor(HitObject hitObject, DrawableHitObject drawableObject)
        {
        }

        protected override void DragOperationCompleted()
        {
            base.DragOperationCompleted();

            // handle positional change etc.
            foreach (var blueprint in SelectionBlueprints)
                EditorBeatmap.Update(blueprint.Item);
        }

        protected override bool OnDoubleClick(DoubleClickEvent e)
        {
            if (!base.OnDoubleClick(e))
                return false;

            EditorClock?.SeekSmoothlyTo(ClickedBlueprint.Item.StartTime);
            return true;
        }

        protected override IEnumerable<SelectionBlueprint<HitObject>> ApplySelectionOrder(IEnumerable<SelectionBlueprint<HitObject>> blueprints) =>
            base.ApplySelectionOrder(blueprints)
                .OrderBy(b => Math.Min(Math.Abs(EditorClock.CurrentTime - b.Item.GetEndTime()), Math.Abs(EditorClock.CurrentTime - b.Item.StartTime)));

        protected override Container<SelectionBlueprint<HitObject>> CreateSelectionBlueprintContainer() => new HitObjectOrderedSelectionContainer { RelativeSizeAxes = Axes.Both };

        protected override SelectionHandler<HitObject> CreateSelectionHandler() => new EditorSelectionHandler();

        protected override void SelectAll()
        {
            Composer.Playfield.KeepAllAlive();
            SelectedItems.AddRange(EditorBeatmap.HitObjects.Except(SelectedItems).ToArray());
        }

        protected override void OnBlueprintSelected(SelectionBlueprint<HitObject> blueprint)
        {
            base.OnBlueprintSelected(blueprint);

            Composer.Playfield.SetKeepAlive(blueprint.Item, true);
        }

        protected override void OnBlueprintDeselected(SelectionBlueprint<HitObject> blueprint)
        {
            base.OnBlueprintDeselected(blueprint);

            Composer.Playfield.SetKeepAlive(blueprint.Item, false);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (EditorBeatmap != null)
            {
                EditorBeatmap.HitObjectAdded -= AddBlueprintFor;
                EditorBeatmap.HitObjectRemoved -= RemoveBlueprintFor;
            }

            usageEventBuffer?.Dispose();
        }
    }
}
