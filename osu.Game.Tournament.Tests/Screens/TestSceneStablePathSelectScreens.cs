// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Tournament.Screens;
using osu.Framework.Platform;

namespace osu.Game.Tournament.Tests.Screens
{
    public class TestSceneStablePathSelectScreens : TournamentTestScene
    {
        public TestSceneStablePathSelectScreens()
        {
            AddStep("Add screen", () => Add(new TestSceneStablePathSelectScreen()));
        }

        private class TestSceneStablePathSelectScreen : StablePathSelectScreen
        {
            protected override void ChangePath(Storage storage)
            {
                Expire();
            }

            protected override void AutoDetect()
            {
                Expire();
            }
        }
    }
}
