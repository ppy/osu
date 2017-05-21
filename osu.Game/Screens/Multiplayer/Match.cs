// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Screens;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Play;
using OpenTK.Graphics;
using osu.Game.Screens.Select;

namespace osu.Game.Screens.Multiplayer
{
    internal class Match : ScreenWhiteBox
    {
        protected override IEnumerable<Type> PossibleChildren => new[] {
            typeof(MatchSongSelect),
            typeof(Player),
        };

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenCustom(@"Backgrounds/bg4");

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);

            Background.Schedule(() => Background.FadeColour(Color4.DarkGray, 500));
        }

        protected override bool OnExiting(Screen next)
        {
            Background.Schedule(() => Background.FadeColour(Color4.White, 500));
            return base.OnExiting(next);
        }
    }
}
