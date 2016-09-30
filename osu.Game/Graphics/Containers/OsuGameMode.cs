//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.GameModes;

namespace osu.Game.Graphics.Containers
{
    public class OsuGameMode : GameMode
    {
        public new OsuGame Game => base.Game as OsuGame;
    }
}
