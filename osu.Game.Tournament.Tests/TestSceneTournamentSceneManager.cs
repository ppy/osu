// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Platform;

namespace osu.Game.Tournament.Tests
{
    public class TestSceneTournamentSceneManager : TournamentTestScene
    {
        [BackgroundDependencyLoader]
        private void load(Storage storage)
        {
            Add(new TournamentSceneManager());
        }
    }
}
