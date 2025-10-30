// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Bindings;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Taiko.Edit
{
    public partial class TaikoSelectionHandler : EditorSelectionHandler
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
            if (SelectedItems.OfType<TaikoStrongableHitObject>().All(h => h.IsStrong == state))
                return;

            EditorBeatmap.PerformOnSelection(h =>
            {
                if (h is not TaikoStrongableHitObject strongable) return;

                if (strongable.IsStrong != state)
                    strongable.IsStrong = state;
            });
        }

        public void SetRimState(bool state)
        {
            if (SelectedItems.OfType<Hit>().All(h => h.Type == (state ? HitType.Rim : HitType.Centre)))
                return;

            EditorBeatmap.PerformOnSelection(h =>
            {
                if (h is Hit taikoHit)
                    taikoHit.Type = state ? HitType.Rim : HitType.Centre;
            });
        }

        protected override IEnumerable<MenuItem> GetContextMenuItemsForSelection(IEnumerable<SelectionBlueprint<HitObject>> selection)
        {
            if (selection.All(s => s.Item is Hit))
            {
                yield return new TernaryStateToggleMenuItem("Rim")
                {
                    State = { BindTarget = selectionRimState },
                    Hotkey = new Hotkey(new KeyCombination(InputKey.W), new KeyCombination(InputKey.R)),
                };
            }

            if (selection.All(s => s.Item is TaikoHitObject))
            {
                yield return new TernaryStateToggleMenuItem("Strong")
                {
                    State = { BindTarget = selectionStrongState },
                    Hotkey = new Hotkey(new KeyCombination(InputKey.E)),
                };
            }

            foreach (var item in base.GetContextMenuItemsForSelection(selection))
                yield return item;
        }

        public override bool HandleMovement(MoveSelectionEvent<HitObject> moveEvent) => true;

        protected override void UpdateTernaryStates()
        {
            base.UpdateTernaryStates();

            selectionRimState.Value = GetStateFromSelection(EditorBeatmap.SelectedHitObjects.OfType<Hit>(), h => h.Type == HitType.Rim);
            selectionStrongState.Value = GetStateFromSelection(EditorBeatmap.SelectedHitObjects.OfType<TaikoStrongableHitObject>(), h => h.IsStrong);
        }
    }
}
