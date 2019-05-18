// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Game.Tests.Visual;
using osu.Game.Tournament.Screens;

namespace osu.Game.Tournament.Tests
{
    public class TestCaseSceneManager : OsuTestScene
    {
        [BackgroundDependencyLoader]
        private void load(Storage storage)
        {
            Add(new TournamentSceneManager());
        }
    }
}
