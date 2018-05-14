// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select;
using OpenTK.Graphics;

namespace osu.Game.Screens.Multi.Screens
{
    public class Match : ScreenWhiteBox
    {
        protected override IEnumerable<Type> PossibleChildren => new[] {
            typeof(MatchSongSelect),
            typeof(Player),
        };

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenCustom(@"Backgrounds/bg4");

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);

            Background.FadeColour(Color4.DarkGray, 500);
        }

        protected override bool OnExiting(Screen next)
        {
            Background.FadeColour(Color4.White, 500);
            return base.OnExiting(next);
        }
    }
}
