//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Game.GameModes.Backgrounds;
using osu.Framework;

namespace osu.Game.GameModes.Play
{
    class PlaySongSelect : GameModeWhiteBox
    {
        private Bindable<PlayMode> playMode;

        protected override BackgroundMode CreateBackground() => new BackgroundModeCustom(@"Backgrounds/bg4");

        protected override IEnumerable<Type> PossibleChildren => new[] {
                typeof(ModSelect),
                typeof(Player)
        };

        public override void Load(BaseGame game)
        {
            base.Load(game);

            OsuGame osu = game as OsuGame;

            playMode = osu.PlayMode;
            playMode.ValueChanged += PlayMode_ValueChanged;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            playMode.ValueChanged -= PlayMode_ValueChanged;
        }

        private void PlayMode_ValueChanged(object sender, EventArgs e)
        {
        }
    }
}
