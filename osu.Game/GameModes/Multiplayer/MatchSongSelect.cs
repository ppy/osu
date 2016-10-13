//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.GameModes;
using osu.Game.GameModes.Backgrounds;
using OpenTK.Graphics;

namespace osu.Game.GameModes.Multiplayer
{
    class MatchSongSelect : GameModeWhiteBox
    {
        protected override BackgroundMode CreateBackground() => new BackgroundModeCustom(@"Backgrounds/bg4");
    }
}
