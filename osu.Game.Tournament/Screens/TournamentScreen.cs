// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Screens;

namespace osu.Game.Tournament.Screens
{
    public class TournamentScreen : OsuScreen
    {
        [Resolved]
        protected LadderInfo LadderInfo { get; private set; }

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
