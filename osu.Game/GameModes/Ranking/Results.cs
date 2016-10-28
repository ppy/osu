//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.GameModes;
using osu.Game.GameModes.Backgrounds;
using OpenTK.Graphics;

namespace osu.Game.GameModes.Ranking
{
    class Results : GameModeWhiteBox
    {
        protected override BackgroundMode CreateBackground() => new BackgroundModeCustom(@"Backgrounds/bg4");

        protected override void OnEntering(GameMode last)
        {
            base.OnEntering(last);
            Background.FadeColour(Color4.DarkGray, 500);
        }

        protected override bool OnExiting(GameMode next)
        {
            Background.FadeColour(Color4.White, 500);
            return base.OnExiting(next);
        }
    }
}
