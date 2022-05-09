// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Overlays;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;
using osuTK.Input;
using osu.Game.Localisation;

namespace osu.Game.Screens.OnlinePlay
{
    public class FreeModSelectScreen : ModSelectScreen
    {
        protected override bool ShowTotalMultiplier => false;

        public new Func<Mod, bool> IsValidMod
        {
            get => base.IsValidMod;
            set => base.IsValidMod = m => m.UserPlayable && value.Invoke(m);
        }

        public FreeModSelectScreen()
            : base(OverlayColourScheme.Plum)
        {
            IsValidMod = _ => true;
        }

        protected override ModColumn CreateModColumn(ModType modType, Key[] toggleKeys = null) => new ModColumn(modType, true, toggleKeys);

        protected override IEnumerable<ShearedButton> CreateFooterButtons() => new[]
        {
            new ShearedButton(BUTTON_WIDTH)
            {
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                Text = CommonStrings.SelectAll,
                Action = SelectAll
            },
            new ShearedButton(BUTTON_WIDTH)
            {
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                Text = CommonStrings.DeselectAll,
                Action = DeselectAll
            }
        };
    }
}
