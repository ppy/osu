// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// A container for <see cref="SelectionBlueprint{HitObject}"/> ordered by their <see cref="HitObject"/> start times.
    /// </summary>
    public sealed partial class HitObjectOrderedSelectionContainer : Container<SelectionBlueprint<HitObject>>
    {
        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            editorBeatmap.BeatmapReprocessed += SortInternal;
        }

        public override void Add(SelectionBlueprint<HitObject> drawable)
        {
            SortInternal();
            base.Add(drawable);
        }

        public override bool Remove(SelectionBlueprint<HitObject> drawable, bool disposeImmediately)
        {
            SortInternal();
            return base.Remove(drawable, disposeImmediately);
        }

        protected override int Compare(Drawable x, Drawable y)
        {
            var xObj = ((SelectionBlueprint<HitObject>)x).Item;
            var yObj = ((SelectionBlueprint<HitObject>)y).Item;

            // Put earlier blueprints towards the end of the list, so they handle input first
            int result = yObj.StartTime.CompareTo(xObj.StartTime);
            if (result != 0) return result;

            // Fall back to end time if the start time is equal.
            result = yObj.GetEndTime().CompareTo(xObj.GetEndTime());
            if (result != 0) return result;

            // As a final fallback, use combo information if available.
            if (xObj is IHasComboInformation xHasCombo && yObj is IHasComboInformation yHasCombo)
            {
                result = yHasCombo.ComboIndex.CompareTo(xHasCombo.ComboIndex);
                if (result != 0) return result;

                result = yHasCombo.IndexInCurrentCombo.CompareTo(xHasCombo.IndexInCurrentCombo);
                if (result != 0) return result;
            }

            return CompareReverseChildID(x, y);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (editorBeatmap.IsNotNull())
                editorBeatmap.BeatmapReprocessed -= SortInternal;
        }
    }
}
