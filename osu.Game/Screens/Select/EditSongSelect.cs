// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Screens.Edit;

namespace osu.Game.Screens.Select
{
    internal class EditSongSelect : SongSelect
    {
        protected override void OnSelected(WorkingBeatmap beatmap) => Push(new Editor(beatmap));
        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Footer.AddButton(@"random", colours.Green, SelectRandom, Key.F2);
            Footer.AddButton(@"options", colours.Blue, BeatmapOptions.ToggleVisibility, Key.F3);

            BeatmapOptions.AddButton(@"Delete", @"Beatmap", FontAwesome.fa_trash, colours.Pink, PromptDelete, Key.Number4);
        }
    }
}
