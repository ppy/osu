// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Screens;

namespace osu.Game.Tournament.Screens
{
    public class TournamentScreen : OsuScreen
    {
        public override void Hide()
        {
            this.FadeOut(200);
        }

        public override void Show()
        {
            this.FadeIn(200);
        }
    }
}
