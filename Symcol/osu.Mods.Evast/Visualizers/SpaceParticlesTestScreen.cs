// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Core.Screens.Evast;

namespace osu.Mods.Evast.Visualizers
{
    public class SpaceParticlesTestScreen : BeatmapScreen
    {
        public SpaceParticlesTestScreen()
        {
            Child = new SpaceParticlesContainer();
        }
    }
}
