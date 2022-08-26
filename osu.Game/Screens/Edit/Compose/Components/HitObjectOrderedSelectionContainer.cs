// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
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
    public sealed class HitObjectOrderedSelectionContainer : Container<SelectionBlueprint<HitObject>>
    {
        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            editorBeatmap.HitObjectUpdated += hitObjectUpdated;
        }

        private void hitObjectUpdated(HitObject _) => SortInternal();

        public override void Add(SelectionBlueprint<HitObject> drawable)
        {
            SortInternal();
            base.Add(drawable);
        }

        public override bool Remove(SelectionBlueprint<HitObject> drawable)
        {
            SortInternal();
            return base.Remove(drawable);
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

            return CompareReverseChildID(y, x);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (editorBeatmap != null)
                editorBeatmap.HitObjectUpdated -= hitObjectUpdated;
        }
    }
}
