// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Allocation;
using osu.Game.Overlays;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens;
using osu.Game.Input;
using osu.Framework.Audio;
using osu.Game.Overlays.Toolbar;
using System.Linq;
using osu.Game.Screens.Mvis.UI;
using osu.Game.Screens.Mvis.Buttons;
using osu.Game.Screens.Mvis.SideBar;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneMvisScreen : TestSceneBeatSyncedContainer
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(BottomBar),
            typeof(MvisScreen),
            typeof(BottomBarButton),
            typeof(SideBarSettingsPanel),
            typeof(ToggleableButton),
            typeof(MusicControlButton),
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
        private Toolbar toolbar = new Toolbar();

        [Cached]
        private IdleTracker idleTracker = new IdleTracker(1000);


        [Resolved]
        private AudioManager audioManager { get; set; }

        [Test]
        public void Mvis()
        {
            OsuScreenStack stack;
            idleTracker = new IdleTracker(3000);

            AddStep("Run test", () =>
            {
                Child = stack = new OsuScreenStack
                {
                    RelativeSizeAxes = Axes.Both
                };

                stack.Push( new MvisScreen() );
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
        }

        
        protected override void LoadComplete()
        {
            base.LoadComplete();

            Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);

            Beatmap.Value.Track.Start();
            Beatmap.Value.Track.Seek(Beatmap.Value.Beatmap.HitObjects.First().StartTime - 1000);
        }
    }
}
