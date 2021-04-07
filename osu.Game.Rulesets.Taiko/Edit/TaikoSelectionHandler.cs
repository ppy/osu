// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Taiko.Edit
{
    public class TaikoSelectionHandler : SelectionHandler
    {
        private readonly Bindable<TernaryState> selectionRimState = new Bindable<TernaryState>();
        private readonly Bindable<TernaryState> selectionStrongState = new Bindable<TernaryState>();

        [BackgroundDependencyLoader]
        private void load()
        {
            selectionStrongState.ValueChanged += state =>
            {
                switch (state.NewValue)
                {
                    case TernaryState.False:
                        SetStrongState(false);
                        break;

                    case TernaryState.True:
                        SetStrongState(true);
                        break;
                }
            };

            selectionRimState.ValueChanged += state =>
            {
                switch (state.NewValue)
                {
                    case TernaryState.False:
                        SetRimState(false);
                        break;

                    case TernaryState.True:
                        SetRimState(true);
                        break;
                }
            };
        }

        public void SetStrongState(bool state)
        {
            EditorBeatmap.PerformOnSelection(h =>
            {
                if (!(h is Hit taikoHit)) return;

                if (taikoHit.IsStrong != state)
                {
                    taikoHit.IsStrong = state;
                    EditorBeatmap.Update(taikoHit);
                }
            });
        }

        public void SetRimState(bool state)
        {
            EditorBeatmap.PerformOnSelection(h =>
            {
                if (h is Hit taikoHit) taikoHit.Type = state ? HitType.Rim : HitType.Centre;
            });
        }

        protected override IEnumerable<MenuItem> GetContextMenuItemsForSelection(IEnumerable<SelectionBlueprint> selection)
        {
            if (selection.All(s => s.HitObject is Hit))
                yield return new TernaryStateMenuItem("Rim") { State = { BindTarget = selectionRimState } };

            if (selection.All(s => s.HitObject is TaikoHitObject))
                yield return new TernaryStateMenuItem("Strong") { State = { BindTarget = selectionStrongState } };
        }

        public override bool HandleMovement(MoveSelectionEvent moveEvent) => true;

        protected override void UpdateTernaryStates()
        {
            base.UpdateTernaryStates();

            selectionRimState.Value = GetStateFromSelection(EditorBeatmap.SelectedHitObjects.OfType<Hit>(), h => h.Type == HitType.Rim);
            selectionStrongState.Value = GetStateFromSelection(EditorBeatmap.SelectedHitObjects.OfType<TaikoStrongableHitObject>(), h => h.IsStrong);
        }
    }
}
