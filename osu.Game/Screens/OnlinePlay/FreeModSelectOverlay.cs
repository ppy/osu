// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Overlays;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens.OnlinePlay
{
    public partial class FreeModSelectOverlay : ModSelectOverlay
    {
        protected override bool AllowCustomisation => false;

        public new Func<Mod, bool> IsValidMod
        {
            get => base.IsValidMod;
            set => base.IsValidMod = m => m.UserPlayable && value.Invoke(m);
        }

        private FreeModSelectFooterContent? currentFooterContent;

        protected override SelectAllModsButton? SelectAllModsButton => currentFooterContent?.SelectAllModsButton;

        public FreeModSelectOverlay()
            : base(OverlayColourScheme.Plum)
        {
            IsValidMod = _ => true;
        }

        protected override ModColumn CreateModColumn(ModType modType) => new ModColumn(modType, true);

        public override Drawable CreateFooterContent() => currentFooterContent = new FreeModSelectFooterContent(this)
        {
            Beatmap = { BindTarget = Beatmap },
            ActiveMods = { BindTarget = ActiveMods },
        };

        private partial class FreeModSelectFooterContent : ModSelectFooterContent
        {
            private readonly FreeModSelectOverlay overlay;

            protected override bool ShowModEffects => false;

            public SelectAllModsButton? SelectAllModsButton;

            public FreeModSelectFooterContent(FreeModSelectOverlay overlay)
                : base(overlay)
            {
                this.overlay = overlay;
            }

            protected override IEnumerable<ShearedButton> CreateButtons()
                => base.CreateButtons()
                       .Prepend(SelectAllModsButton = new SelectAllModsButton(overlay)
                       {
                           Anchor = Anchor.BottomLeft,
                           Origin = Anchor.BottomLeft,
                       });
        }
    }
}
