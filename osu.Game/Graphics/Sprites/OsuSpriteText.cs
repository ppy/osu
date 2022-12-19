﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics.Sprites
{
    public partial class OsuSpriteText : SpriteText
    {
        public OsuSpriteText()
        {
            Shadow = true;
            Font = OsuFont.Default;
        }
    }
}
