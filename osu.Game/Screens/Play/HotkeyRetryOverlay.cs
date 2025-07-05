// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;

namespace osu.Game.Screens.Play
{
    public partial class HotkeyRetryOverlay : HoldToConfirmOverlay, IKeyBindingHandler<GlobalAction>
    {
        private KeyCombination[] quickRetryChords = null!;
        private InputManager inputManager = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager()!;

            quickRetryChords = GlobalActionContainer
                               .GetDefaultBindingsFor(GlobalActionCategory.InGame)
                               .Where(b => b.Action is GlobalAction.QuickRetry)
                               .Select(b => b.KeyCombination)
                               .ToArray();
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Action == GlobalAction.Back && isQuickRetryChordHeld())
                return true;

            if (e.Action == GlobalAction.QuickRetry && !e.Repeat)
            {
                BeginConfirm();
                return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
            if (e.Action == GlobalAction.QuickRetry)
                AbortConfirm();
        }

        private bool isQuickRetryChordHeld()
        {
            var pressed = inputManager.CurrentState.Keyboard.Keys.Select(k => (InputKey)k);
            return quickRetryChords.Any(chord => chord.Keys.All(pressed.Contains));
        }
    }
}
