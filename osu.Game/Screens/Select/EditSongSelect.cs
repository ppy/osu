// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.Select
{
    public class EditSongSelect : SongSelect
    {
        protected override bool ShowFooter => false;

        public WorkingBeatmap SelectedBeatmap;

        protected override void OnSelected(InputState state)
        {
            SelectedBeatmap = Beatmap;
            Exit();
        }
    }
}
