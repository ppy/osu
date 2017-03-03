// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Graphics;

namespace osu.Game.Screens.Select.Options
{
    public class BeatmapOptionsEditButton : BeatmapOptionsButton
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            ButtonColour = colour.Yellow;
        }

        public BeatmapOptionsEditButton()
        {
            Icon = FontAwesome.fa_pencil;
            FirstLineText = @"Edit";
            SecondLineText = @"Beatmap";
        }
    }
}
