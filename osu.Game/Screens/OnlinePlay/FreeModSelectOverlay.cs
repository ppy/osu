// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Overlays;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;
using osuTK.Input;
using osu.Game.Localisation;

namespace osu.Game.Screens.OnlinePlay
{
    public class FreeModSelectOverlay : ModSelectOverlay, IKeyBindingHandler<PlatformAction>
    {
        protected override bool ShowTotalMultiplier => false;

        protected override bool AllowCustomisation => false;

        public new Func<Mod, bool> IsValidMod
        {
            get => base.IsValidMod;
            set => base.IsValidMod = m => m.UserPlayable && value.Invoke(m);
        }

        private ShearedButton selectAllButton;

        public FreeModSelectOverlay()
            : base(OverlayColourScheme.Plum)
        {
            IsValidMod = _ => true;
        }

        protected override ModColumn CreateModColumn(ModType modType, Key[] toggleKeys = null) => new ModColumn(modType, true, toggleKeys);

        protected override IEnumerable<ShearedButton> CreateFooterButtons() => base.CreateFooterButtons().Prepend(
            selectAllButton = new ShearedButton(BUTTON_WIDTH)
            {
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                Text = CommonStrings.SelectAll,
                Action = SelectAll
            });

        public bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
        {
            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case PlatformAction.SelectAll:
                    selectAllButton.TriggerClick();
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<PlatformAction> e)
        {
        }
    }
}
