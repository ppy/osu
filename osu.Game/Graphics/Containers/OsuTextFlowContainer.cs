// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics.Containers
{
    public partial class OsuTextFlowContainer : TextFlowContainer
    {
        public OsuTextFlowContainer(Action<SpriteText> defaultCreationParameters = null)
            : base(defaultCreationParameters)
        {
        }

        protected override SpriteText CreateSpriteText() => new OsuSpriteText();

        public ITextPart AddArbitraryDrawable(Drawable drawable) => AddPart(new TextPartManual(drawable.Yield()));

        public ITextPart AddIcon(IconUsage icon, Action<SpriteText> creationParameters = null) => AddText(icon.Icon.ToString(), creationParameters);
    }
}
