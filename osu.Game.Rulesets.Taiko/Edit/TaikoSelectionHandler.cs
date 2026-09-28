// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
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
        [Resolved]
        private TaikoHitObjectComposer composer { get; set; } = null!;

        protected override IEnumerable<MenuItem> GetContextMenuItemsForSelection(IEnumerable<SelectionBlueprint<HitObject>> selection)
        {
            if (selection.All(s => s.Item is Hit))
            {
                yield return new TernaryStateToggleMenuItem("Rim")
                {
                    State = { BindTarget = composer.SelectionRimState },
                    Hotkey = new Hotkey(new KeyCombination(InputKey.W), new KeyCombination(InputKey.R)),
                };
            }

            if (selection.All(s => s.Item is TaikoHitObject))
            {
                yield return new TernaryStateToggleMenuItem("Strong")
                {
                    State = { BindTarget = composer.SelectionStrongState },
                    Hotkey = new Hotkey(new KeyCombination(InputKey.E)),
                };
            }

            foreach (var item in base.GetContextMenuItemsForSelection(selection))
                yield return item;
        }

        public override bool HandleMovement(MoveSelectionEvent<HitObject> moveEvent) => true;
    }
}
