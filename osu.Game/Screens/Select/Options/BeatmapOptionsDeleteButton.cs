// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Graphics;

namespace osu.Game.Screens.Select.Options
{
    public class BeatmapOptionsDeleteButton : BeatmapOptionsButton
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            ButtonColour = colour.Pink;
        }

        public BeatmapOptionsDeleteButton()
        {
            Icon = FontAwesome.fa_trash;
            FirstLineText = @"Delete";
            SecondLineText = @"Beatmap";
        }
    }
}
