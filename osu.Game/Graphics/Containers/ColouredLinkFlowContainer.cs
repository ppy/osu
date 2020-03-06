// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Online.Chat;
using osuTK.Graphics;

namespace osu.Game.Graphics.Containers
{
    public class ColouredLinkFlowContainer : LinkFlowContainer
    {
        private Color4 idleColour;

        public Color4 IdleColour
        {
            get => idleColour;
            set
            {
                idleColour = value;
                foreach (var linkCompiler in InternalChildren.OfType<ColouredDrawableLinkCompiler>())
                    linkCompiler.IdleColour = value;
            }
        }

        private Color4 hoverColour;

        public Color4 HoverColour
        {
            get => hoverColour;
            set
            {
                hoverColour = value;
                foreach (var linkCompiler in InternalChildren.OfType<ColouredDrawableLinkCompiler>())
                    linkCompiler.HoverColour = value;
            }
        }

        protected override DrawableLinkCompiler CreateLinkCompiler(List<Drawable> parts, string tooltipText, Action action)
            => new ColouredDrawableLinkCompiler(parts, tooltipText, action);

        public ColouredLinkFlowContainer(Action<SpriteText> defaultCreationParameters = null)
            : base(defaultCreationParameters)
        {
        }
    }
}
