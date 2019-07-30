// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings.Sections.Graphics;
using osu.Game.Screens;
using osu.Game.Screens.Backgrounds;
using osu.Game.Users;
using osuTK.Graphics;

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

            AddStep("Default Mode", () => background.BackgroundMode.Value = MainMenuBackgroundMode.Default);
            AddStep("Skin Mode (skin with no bg)", () => background.BackgroundMode.Value = MainMenuBackgroundMode.Skin);
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
            private OsuSpriteText userState;
            private OsuSpriteText modeState;
            private OsuSpriteText updateNotification;

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

            protected override void LoadComplete()
            {
                base.LoadComplete();

                AddInternal(new FillFlowContainer
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Margin = new MarginPadding { Left = 100 },
                    Children = new Drawable[]
                    {
                        userState = new OsuSpriteText(),
                        modeState = new OsuSpriteText(),
                    }
                });

                AddInternal(updateNotification = new OsuSpriteText
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Margin = new MarginPadding { Bottom = 100 },
                    Text = "Background has been updated",
                    Colour = Color4.Red,
                    Font = OsuFont.GetFont(size: 40),
                    Alpha = 0,
                });

                User.BindValueChanged(user => updateUserState(user.NewValue), true);
                BackgroundMode.BindValueChanged(mode => updateModeState(mode.NewValue), true);
            }

            private void updateUserState(User user) => userState.Text = "user state: " + ((user?.IsSupporter ?? false) ? "supporter" : "unSupporter");

            private void updateModeState(MainMenuBackgroundMode mode) => modeState.Text = $"mode state: {mode}";

            protected override void UpdateBackground()
            {
                base.UpdateBackground();
                updateNotification.FadeIn().Then().Delay(500).Then().FadeOut(500);
            }
        }
    }
}
