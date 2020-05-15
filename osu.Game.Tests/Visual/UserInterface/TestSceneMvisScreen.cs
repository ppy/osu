// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Overlays;
using osu.Game.Screens;
using osu.Game.Input;
using osu.Game.Screens.Mvis.UI;
using osu.Game.Screens.Mvis.Buttons;
using osu.Game.Screens.Mvis.SideBar;
using osu.Game.Screens.Mvis;
using osu.Game.Rulesets.Osu;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneMvisScreen : ScreenTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(BottomBar),
            typeof(MvisScreen),
            typeof(ClockContainer),
            typeof(BackgroundStoryBoard),
            typeof(BottomBarButton),
            typeof(SideBarSettingsPanel),
            typeof(ToggleableButton),
            typeof(ToggleableOverlayLockButton),
            typeof(HoverableProgressBarContainer)
        };

        private IReadOnlyList<Type> requiredGameDependencies => new[]
        {
            typeof(OsuGame),
            typeof(IdleTracker),
            typeof(OnScreenDisplay),
            typeof(NotificationOverlay),
            typeof(MusicController),
        };

        [Cached]
        private MusicController musicController = new MusicController();

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
            Add(musicController);
            Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
        }
    }
}
