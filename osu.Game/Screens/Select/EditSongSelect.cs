// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Screens.Edit;

namespace osu.Game.Screens.Select
{
    internal class EditSongSelect : SongSelect
    {
        protected override void OnSelected(WorkingBeatmap beatmap) => Push(new Editor(beatmap));
    }
}
