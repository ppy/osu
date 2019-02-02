// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Tournament.Screens.Showcase
{
    public class ShowcaseScreen : BeatmapInfoScreen
    {
        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            AddInternal(new TournamentLogo());
        }
    }
}
