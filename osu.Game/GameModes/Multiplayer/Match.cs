//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.GameModes;
using osu.Game.GameModes.Play;
using OpenTK.Graphics;

namespace osu.Game.GameModes.Multiplayer
{
    class Match : GameModeWhiteBox
    {
        protected override IEnumerable<Type> PossibleChildren => new[] {
            typeof(MatchSongSelect),
            typeof(Player),
        };

        protected override BackgroundMode CreateBackground() => new BackgroundModeCustom(@"Backgrounds/bg4");

        protected override void OnEntering(GameMode last)
        {
            base.OnEntering(last);
            Background.FadeColour(Color4.DarkGray, 500);
        }

        protected override void OnExiting(GameMode next)
        {
            base.OnExiting(next);
            Background.FadeColour(Color4.White, 500);
        }
    }
}
