// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.Select
{
    public class PlaySongSelect : SongSelect
    {
        private OsuScreen player;

        protected override void OnResuming(Screen last)
        {
            player = null;
            base.OnResuming(last);
        }

        protected override void OnSelected(WorkingBeatmap beatmap)
        {
            if (player != null) return;

            (player = new PlayerLoader(new Player
            {
                Beatmap = Beatmap, //eagerly set this so it's present before push.
            })).LoadAsync(Game, l => Push(player));
        }
    }
}
