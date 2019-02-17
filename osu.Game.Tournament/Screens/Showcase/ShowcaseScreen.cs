// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;

namespace osu.Game.Tournament.Screens.Showcase
{
    public class ShowcaseScreen : BeatmapInfoScreen
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(new TournamentLogo());
        }
    }
}
