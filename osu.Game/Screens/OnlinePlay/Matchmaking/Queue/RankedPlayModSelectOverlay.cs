// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Queue
{
    public partial class RankedPlayModSelectOverlay : UserModSelectOverlay
    {
        public new Func<Mod, bool> IsValidMod
        {
            get => base.IsValidMod;
            set => base.IsValidMod = m => m.UserPlayable && value.Invoke(m);
        }

        public RankedPlayModSelectOverlay()
            : base(OverlayColourScheme.Plum)
        {
            IsValidMod = _ => true;
        }

        public override VisibilityContainer CreateFooterContent() => new RankedPlayModSelectFooterContent(this)
        {
            Beatmap = { BindTarget = Beatmap },
            ActiveMods = { BindTarget = ActiveMods },
            Ruleset = { BindTarget = Ruleset },
        };

        public partial class RankedPlayModSelectFooterContent : ModSelectFooterContent
        {
            protected override bool ShowModEffects => false;

            public RankedPlayModSelectFooterContent(RankedPlayModSelectOverlay overlay)
                : base(overlay)
            {
            }

            protected override IEnumerable<ShearedButton> CreateButtons() => [];
        }
    }
}
