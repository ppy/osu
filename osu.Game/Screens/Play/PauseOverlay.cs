// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;

namespace osu.Game.Screens.Play
{
    public partial class PauseOverlay : GameplayMenuOverlay
    {
        public override LocalisableString Header => GameplayMenuOverlayStrings.PausedHeader;

        protected override Action BackAction => () =>
        {
            if (Buttons.Any())
                Buttons.First().TriggerClick();
            else
                OnResume?.Invoke();
        };

        public override bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.PauseGameplay:
                    InternalButtons.First().TriggerClick();
                    return true;
            }

            return base.OnPressed(e);
        }
    }
}
