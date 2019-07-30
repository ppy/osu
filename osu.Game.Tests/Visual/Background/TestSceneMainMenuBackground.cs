// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings.Sections.Graphics;
using osu.Game.Screens;
using osu.Game.Screens.Backgrounds;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Background
{
    public class TestSceneMainMenuBackground : OsuTestScene
    {
        public TestSceneMainMenuBackground()
        {
            TestBackgroundScreen background;

            Child = new OsuScreenStack(background = new TestBackgroundScreen())
            {
                RelativeSizeAxes = Axes.Both,
            };

            AddStep("Trigger background update", () => background.UpdateBackground());
            AddStep("Default Mode", () => background.BackgroundMode.Value = MainMenuBackgroundMode.Default);
            AddStep("Skin Mode", () => background.BackgroundMode.Value = MainMenuBackgroundMode.Skin);
            AddStep("Beatmap Mode", () => background.BackgroundMode.Value = MainMenuBackgroundMode.Beatmap);
            AddStep("User change(not supporter)", () => background.User.Value = new User
            {
                Id = -1,
                IsSupporter = false
            });
            AddStep("User change(supporter)", () => background.User.Value = new User
            {
                Id = -2,
                IsSupporter = true
            });
        }

        private class TestBackgroundScreen : BackgroundScreenDefault
        {
            public TestBackgroundScreen()
            {
                var mc = new MusicController
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre
                };

                AddInternal(mc);

                mc.Show();
            }
        }
    }
}
