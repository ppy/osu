// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osuTK.Graphics;

namespace osu.Game.Online.Chat
{
    public class ColouredDrawableLinkCompiler : DrawableLinkCompiler
    {
        private Color4 idleColour;

        public new Color4 IdleColour
        {
            get => idleColour;
            set => base.IdleColour = idleColour = value;
        }

        private Color4 hoverColour;

        public new Color4 HoverColour
        {
            get => hoverColour;
            set => base.HoverColour = hoverColour = value;
        }

        public ColouredDrawableLinkCompiler(List<Drawable> parts, string tooltipText, Action action)
            : base(parts, tooltipText, action)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            // the IdleColour has to always be set here to prevent the base class from overwriting our colour with the default blue
            IdleColour = idleColour == default ? colourProvider.Light2 : idleColour;

            if (hoverColour == default)
                HoverColour = colourProvider.Light1;
        }
    }
}
