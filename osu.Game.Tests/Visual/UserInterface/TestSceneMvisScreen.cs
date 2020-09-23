// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Overlays;
using osu.Game.Screens;
using osu.Game.Rulesets.Osu;
using osu.Game.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneMvisScreen : ScreenTestScene
    {
        [Cached]
        private MusicController musicController = new MusicController();

        [Cached]
        private IdleTracker idle = new IdleTracker(6000);

        private MvisScreen mvisScreen;

        [Test]
        public void CreateMvisScreen()
        {
            AddStep("Create screen", () =>
            {
                if (Stack.CurrentScreen != null)
                    Stack?.Exit();

                LoadScreen( mvisScreen = new MvisScreen() );
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(idle);
            Add(musicController);
            Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
        }
    }
}
