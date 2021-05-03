// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public class EditorBlueprintContainer : BlueprintContainer<HitObject>
    {
        [Resolved]
        protected EditorClock EditorClock { get; private set; }

        [Resolved]
        protected EditorBeatmap Beatmap { get; private set; }

        protected readonly HitObjectComposer Composer;

        private readonly BindableList<HitObject> selectedHitObjects = new BindableList<HitObject>();

        protected EditorBlueprintContainer(HitObjectComposer composer)
        {
            Composer = composer;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            // For non-pooled rulesets, hitobjects are already present in the playfield which allows the blueprints to be loaded in the async context.
            if (Composer != null)
            {
                foreach (var obj in Composer.HitObjects)
                    AddBlueprintFor(obj.HitObject);
            }

            selectedHitObjects.BindTo(Beatmap.SelectedHitObjects);
            selectedHitObjects.CollectionChanged += (selectedObjects, args) =>
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var o in args.NewItems)
                            SelectionBlueprints.FirstOrDefault(b => b.Item == o)?.Select();
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        foreach (var o in args.OldItems)
                            SelectionBlueprints.FirstOrDefault(b => b.Item == o)?.Deselect();

                        break;
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Beatmap.HitObjectAdded += AddBlueprintFor;
            Beatmap.HitObjectRemoved += RemoveBlueprintFor;

            if (Composer != null)
            {
                // For pooled rulesets, blueprints must be added for hitobjects already "current" as they would've not been "current" during the async load addition process above.
                foreach (var obj in Composer.HitObjects)
                    AddBlueprintFor(obj.HitObject);

                Composer.Playfield.HitObjectUsageBegan += AddBlueprintFor;
                Composer.Playfield.HitObjectUsageFinished += RemoveBlueprintFor;
            }
        }

        protected override IEnumerable<SelectionBlueprint<HitObject>> SortForMovement(IReadOnlyList<SelectionBlueprint<HitObject>> blueprints)
            => blueprints.OrderBy(b => b.Item.StartTime);

        protected override bool AllowDeselectionDuringDrag => !EditorClock.IsRunning;

        protected override bool ApplySnapResult(SelectionBlueprint<HitObject>[] blueprints, SnapResult result)
        {
            if (!base.ApplySnapResult(blueprints, result))
                return false;

            if (result.Time.HasValue)
            {
                // Apply the start time at the newly snapped-to position
                double offset = result.Time.Value - blueprints.First().Item.StartTime;

                if (offset != 0)
                    Beatmap.PerformOnSelection(obj => obj.StartTime += offset);
            }

            return true;
        }

        protected override void AddBlueprintFor(HitObject item)
        {
            if (item is IBarLine)
                return;

            base.AddBlueprintFor(item);
        }

        protected override void DragOperationCompleted()
        {
            base.DragOperationCompleted();

            // handle positional change etc.
            foreach (var blueprint in SelectionBlueprints)
                Beatmap.Update(blueprint.Item);
        }

        protected override bool OnDoubleClick(DoubleClickEvent e)
        {
            if (!base.OnDoubleClick(e))
                return false;

            EditorClock?.SeekSmoothlyTo(ClickedBlueprint.Item.StartTime);
            return true;
        }

        protected override Container<SelectionBlueprint<HitObject>> CreateSelectionBlueprintContainer() => new HitObjectOrderedSelectionContainer { RelativeSizeAxes = Axes.Both };

        protected override SelectionHandler<HitObject> CreateSelectionHandler() => new EditorSelectionHandler();

        protected override void SelectAll()
        {
            Composer.Playfield.KeepAllAlive();

            base.SelectAll();
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

            if (Beatmap != null)
            {
                Beatmap.HitObjectAdded -= AddBlueprintFor;
                Beatmap.HitObjectRemoved -= RemoveBlueprintFor;
            }

            if (Composer != null)
            {
                Composer.Playfield.HitObjectUsageBegan -= AddBlueprintFor;
                Composer.Playfield.HitObjectUsageFinished -= RemoveBlueprintFor;
            }
        }
    }
}
