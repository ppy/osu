// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public class CountdownOverlay : Container, ISkinnableDrawable
    {
        [Resolved]
        protected GameplayState GameplayState { get; private set; }

        [Resolved]
        protected DrawableRuleset DrawableRuleset { get; private set; }

        internal ISkinSource Skin;

        public CountdownOverlay()
        {
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            Skin = skin;
            Clock = DrawableRuleset.FrameStableClock;
        }

        public bool UsesFixedAnchor { get; set; }
    }
}
