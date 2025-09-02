// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.Menu
{
    public partial class MatchmakingButton : MainMenuButton
    {
        public MatchmakingButton(string sampleName, Color4 colour, Action<MainMenuButton, UIEvent>? clickAction = null, params Key[] triggerKeys)
            : base("matchmaking", sampleName, FontAwesome.Solid.Newspaper, colour, clickAction, triggerKeys)
        {
        }
    }
}
