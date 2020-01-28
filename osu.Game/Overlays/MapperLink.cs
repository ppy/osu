// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Users;

namespace osu.Game.Overlays
{
    public class MapperLink : LinkFlowContainer
    {
        public MapperLink(User mapper, int fontSize, Action<SpriteText> textCreationParameters = null)
            : base(s =>
            {
                s.Shadow = false;
                s.Font = OsuFont.GetFont(size: fontSize);
            })
        {
            AutoSizeAxes = Axes.Both;
            AddText("mapped by ", textCreationParameters);
            AddUserLink(mapper, text => text.Font = text.Font.With(weight: FontWeight.Bold));
        }
    }
}
