// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Overlays;
using osu.Game.Screens;
using osu.Game.Input;
using osu.Game.Rulesets.Osu;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestScenePurePlayerScreen : ScreenTestScene
    {
        [Cached]
        private MusicController musicController = new MusicController();

        private PurePlayerScreen mvisScreen;

        [Test]
        public void CreatePurePlayerScreen()
        {
            AddStep("Create screen", () =>
            {
                if (Stack.CurrentScreen != null)
                    Stack?.Exit();

                LoadScreen( mvisScreen = new PurePlayerScreen() );
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(musicController);
            Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
        }
    }
}
