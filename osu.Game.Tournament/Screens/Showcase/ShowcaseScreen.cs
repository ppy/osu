// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;

namespace osu.Game.Tournament.Screens.Showcase
{
    public class ShowcaseScreen : BeatmapInfoScreen
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(new TournamentLogo(false));
        }
    }
}
